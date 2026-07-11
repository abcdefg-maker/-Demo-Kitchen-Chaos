using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour, IKitchenObjectParent
{
    public static Player Instance { get; private set; } //属性（property）要用帕斯卡尔命名


    public event EventHandler OnPickedSomething; //捡起物品时候调用音效的事件 
    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;//泛型(Generics)的事件
    public class OnSelectedCounterChangedEventArgs : EventArgs //这是一个事件参数类，用来装事件需要传出去的数据。
    {
        public BaseCounter selectedCounter;
    }

    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private LayerMask counterLayerMask;
    [SerializeField] private Transform kitchenObjectHoldPoint;


    private bool isWalking;
    private Vector3 lastInteractDir;
    private BaseCounter selectedCounter;
    private KitchenObject kitchenObject;


    private void Awake()
    {
        if(Instance != null)//单例模式的安全检查
        {
            Debug.Log("出现超过一个玩家");
        }

        Instance = this; 
    }

    private void Start()
    {
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
        GameInput.Instance.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;
    }

    

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        HandleMovement();
        HandleInteractions();
    }

    private void GameInput_OnInteractAction(object sender, System.EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return; //如果游戏不是GamePlaying的状态，不允许进行交互

        if(selectedCounter!=null)
        {
            selectedCounter.Interact(this);
        }
    }

    private void GameInput_OnInteractAlternateAction(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying()) return; //如果游戏不是GamePlaying的状态，不允许进行交互

        if (selectedCounter != null)
        {
            selectedCounter.InteractAlternate(this);
        }
    }

    private void HandleInteractions()//处理与玩家互动对象逻辑的函数
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        if (moveDir != Vector3.zero)
        {
            lastInteractDir = moveDir; 
            //在raycast函数内用这个参数代替moveDir。
            //这样即使停止移动（停止input方向，这样moveDir为vector3.zero）
            //raycast函数也不会因为没有输入方向，而即使面对着某个可互动物体而无法互动
        }

        float interactDistance = 2f;

        RaycastHit raycastHit;//用这种包含out的raycast函数重构，可以获取碰撞到物体的信息
                               //raycast传参layermask，可以只检测固定layer的碰撞
        if (Physics.Raycast(transform.position, lastInteractDir, out raycastHit,interactDistance,counterLayerMask))
        {
            if(raycastHit.transform.TryGetComponent(out BaseCounter baseCounter))
            {
                //TryGetComponent类似GetComponent函数，只不过它自动处理空的情况
                //代表有ClearCounter
                if(baseCounter != selectedCounter)//把raycast检测到的counter设置为被选中的counter
                {
                    SetSelectedCounter(baseCounter);
                }
            }
            else
            {
                SetSelectedCounter(null);//如果没找到，选中counter设为空
            }
            
        }
        else
        {
            SetSelectedCounter(null);//如果没找到，选中counter设为空
        }

        //Debug.Log(selectedCounter);
    }

    

    private void HandleMovement()//处理玩家移动逻辑的函数
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        float moveDistance = moveSpeed * Time.deltaTime;
        float playerRadius = .7f;
        float playerHeight = 2f;
        bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDir, moveDistance);
        if (!canMove)
        {
            //如果只在一个方向（X/Z轴）有碰撞限制，那么按多个方向键时，在另外的方向应该能够移动
            //以下会给canMove进行判断赋值，这样才能进入下面的给transform.position赋值的分支里。
            //所以在这个大分支里的小分支的判断条件都要是canMove，而不能改成别的。


            //看X轴能否移动
            Vector3 movDirX = new Vector3(moveDir.x, 0, 0).normalized;//归一化，取消对角线移动的减速。下同
            canMove = moveDir.x != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, movDirX, moveDistance);
            if (canMove)
            {
                //只能在x轴移动
                moveDir = movDirX;
                //Debug.Log("在Z轴不能移动，在X轴可以移动");

            }
            else
            {
                //在X轴不能移动，看在Z轴能否移动

                Vector3 movDirZ = new Vector3(0, 0, moveDir.z).normalized;
                canMove = moveDir.z != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, movDirZ, moveDistance);
                if (canMove)
                {
                    //在X轴不能移动，在Z轴能移动
                    moveDir = movDirZ;
                    //Debug.Log("在X轴不能移动，在Z轴可以移动");
                }
                else
                {
                    //在X和Z轴都不能移动
                    //Debug.Log("在X和Z轴都不能移动");
                }
            }



        }

        if (canMove)
        {
            transform.position += (Vector3)moveDir * moveSpeed * Time.deltaTime; //× Time.deltaTime的原因：
                                                                                 //为了控制移动速度不随帧率变化
        }
        isWalking = moveDir != Vector3.zero;

        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed); //控制转身的方向
                                                                                                     //Slerp函数用于丝滑转向
    }

    public bool IsWalking()
    {
        return isWalking;
    }

    private void SetSelectedCounter(BaseCounter selectedCounter) //触发 OnSelectedCounterChanged 事件，并把当前 Player 选中的 Counter 信息，通过事件参数传递给所有事件监听者。
    {
        this.selectedCounter = selectedCounter;

        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs
        //触发 OnSelectedCounterChanged 事件，
        //并把当前 Player 选中的 Counter 信息，通过事件参数传递给所有事件监听者。
        {
            selectedCounter = selectedCounter   //虽然这俩名字一样，但是光标移上去就知道各自是什么了。
                                                //第一个是EventArgs要传递的参数，
                                                //第二个是Player类的成员变量selectCounter
        });
    }

    public Transform GetKitchenObjectFollowTransform()  //获取kO应该被放置的位置，
                                                        //这里是配合转移kO位置而实现的函数接口，
                                                        //用于获取secondCC的物品放置位置
    {
        return kitchenObjectHoldPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObjtect) //物品增
    {
        this.kitchenObject = kitchenObjtect;

        if(kitchenObjtect != null)
        {
            OnPickedSomething?.Invoke(this, EventArgs.Empty); //捡起物品时候调用音效的事件
        }
    }
    public KitchenObject GetKitchenObject() { return kitchenObject; } //物品查
    public void ClearKitchenObject() //物品删
    {
        kitchenObject = null;
    }

    public bool HasKitchenObject()  //查看kO是否被赋值
    {
        return kitchenObject != null;
    }
}
