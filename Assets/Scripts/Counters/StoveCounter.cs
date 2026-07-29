using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;
using static CuttingCounter;

public class StoveCounter :BaseCounter, IHasProgress  
                                        //由于肉饼需要被煎炸成不同的状态，因此对应的stove也需要有不同的状态
                                        //为了实现这种情况，这个stovecounter类被内置了一个状态机，
                                        //用enum来枚举状态，生命周期函数管理状态
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged; //控制进度条的事件

    public event EventHandler<OnStateChangedEventArgs> OnStateChanged; //用来给StoveCounterVisual传递状态变化的事件
    public class OnStateChangedEventArgs : EventArgs
    {
        public State state;
    }

    public enum State
    {
        Idle,
        Frying,
        Fried,
        Burned,
    }

    [SerializeField] private FryingRecipeSO[] fryingRecipeSOArray;
    [SerializeField] private BurningRecipeSO[] burningRecipeSOArray;

    //网络变量,默认只有服务器可以访问
    //当网络变量的值变化的时候也会触发一个事件
    private NetworkVariable<State> state = new NetworkVariable<State>(State.Idle); //currentState
    private NetworkVariable<float> fryingTimer = new NetworkVariable<float>(0f); 
    private NetworkVariable<float> burningTimer = new NetworkVariable<float>(0f); 

    private FryingRecipeSO fryingRecipeSO;
    private BurningRecipeSO burningRecipeSO;


    //不再需要在 Start() 里初始化 state：
    //1. NetworkVariable<State> 构造时已默认 State.Idle
    //2. Start() 会在所有实例（含客户端）运行，客户端写 state.Value 属于非法写入会抛异常

    public override void OnNetworkSpawn()//unity netcode for Object的生命周期回调
                                         //当这个NetworkObject在网络上被Spawn的时候才调用
                                         //在联机游戏的时候，不应该在start/Awake进行对象的生成/赋值
                                         //而应该在这个函数内部进行操作
    {
        fryingTimer.OnValueChanged += FryingTimer_OnValueChanged;
        burningTimer.OnValueChanged += BurningTimer_OnValueChanged;
        state.OnValueChanged += State_OnValueChanged;
    }

    private void FryingTimer_OnValueChanged(float previousValue,float newValue)
    {
        float fryingTimerMax = fryingRecipeSO != null ? fryingRecipeSO.fryingTimerMax : 1f;

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = fryingTimer.Value / fryingTimerMax
        });
    }

      private void BurningTimer_OnValueChanged(float previousValue,float newValue)
    {
        float burningTimerMax = burningRecipeSO != null ? burningRecipeSO.burningTimerMax : 1f;

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = burningTimer.Value / burningTimerMax
        });
    }

    private void State_OnValueChanged (State previousState,State newState)
    {
        OnStateChanged?.Invoke(this,new OnStateChangedEventArgs{
                            state = this.state.Value,
        });

        if(state.Value == State.Burned || state.Value == State.Idle)
        {
            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
            {
                progressNormalized = 0f
            });
        }
    }

    private void Update()   //状态机的核心逻辑，管理状态进入和切换
    {
        if (!IsServer)
        {
            return;
        }

        if (HasKitchenObject())
        {
            switch (state.Value)
            {
                case State.Idle:
                    break;
                case State.Frying:
                    fryingTimer.Value += Time.deltaTime;

                  

                    if (fryingTimer.Value >= fryingRecipeSO.fryingTimerMax)
                    {
                        //煎炸好了
                        fryingTimer.Value = 0f;

                       KitchenObject.DestoryKitchenObject(GetKitchenObject());

                        KitchenObject.SpawnKitchenObject(fryingRecipeSO.output, this);

                        state.Value = State.Fried;
                        burningTimer.Value = 0f;

                        SetBurningRecipeSOClientRpc(
                            KitchenGameMutiplayer.Instance.GetKitchenObjectSOIndex(GetKitchenObject().GetKitchenObjectSO())
                        );

                        
                    }
                    break;
                case State.Fried:
                    burningTimer.Value += Time.deltaTime;

                  

                    if (burningTimer.Value >= burningRecipeSO.burningTimerMax)
                    {
                        //煎炸过头了
                        KitchenObject.DestoryKitchenObject(GetKitchenObject());

                        KitchenObject.SpawnKitchenObject(burningRecipeSO.output, this);

                        state.Value = State.Burned;


                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                        {
                            progressNormalized = 0f
                        });


                    }                 
                    break;
                case State.Burned:
                    break;
            }

        }
    }

    public override void Interact(Player player)
    {
        if (!HasKitchenObject())
        {
            //台面上没东西
            if (player.HasKitchenObject())
            {
                // 玩家手里有东西
                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO()))//东西是可以煎炸的
                {
                    KitchenObject kitchenObject = player.GetKitchenObject();
                    kitchenObject.SetKitchenObjectParent(this); //把东西放在台面上

                    InteractLogicPlaceObjectOnCounterServerRpc(
                        KitchenGameMutiplayer.Instance.GetKitchenObjectSOIndex(kitchenObject.GetKitchenObjectSO())
                    );

                   
                }
            }
            else
            {
                // 玩家手里没有东西
            }
        }
        else
        {
            //台面有东西
            if (player.HasKitchenObject())
            {
                // 玩家手里有东西
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    //玩家手里拿着盘子（这时候我们希望把clearcounter上的东西放到盘子里）
                    //as是安全类型转换
                    //返回 obj 强转后的对象，如果不能转换，就返回 null，不会抛异常
                    //反之，如果用强制类型转换(Type)a,这样转换失败的时候会抛出异常
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))//判断这个盘子里是否已经添加过这个食材
                                                                                                     //如果添加过相同的食材那就不能再把这个东西放进去
                                                                                                     //这样设计是因为我们设定的食物配方
                                                                                                     //在同一个菜品内不会出现相同的食材使用两次的情况
                                                                                                     //（e.g. double cheese ...）
                                                                                                     //所以如果后续需要拓展这种玩法的话，这块的代码是需要改的
                    {
                        KitchenObject.DestoryKitchenObject(GetKitchenObject());
                        SetStateIdleServerRpc(); //玩家拿起肉饼后，重置为初始状态
                                                 //注意不能直接写 state.Value（NetworkVariable 只有服务器可写），
                                                 //客户端必须走 ServerRpc，否则状态不同步、进度条不消失

                    }
                }
            }
            else
            {
                // 玩家手里没有东西
                GetKitchenObject().SetKitchenObjectParent(player);//把东西放在玩家手上
                
                SetStateIdleServerRpc();



            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetStateIdleServerRpc()
    {
        fryingTimer.Value = 0f;
        burningTimer.Value = 0f;
        state.Value = State.Idle;
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO) //查找煎炸前和煎炸后对应的gameobject
    {
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
        if (fryingRecipeSO != null)
        {
            return fryingRecipeSO.output;
        }
        else
        {
            return null;
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceObjectOnCounterServerRpc(int kitchenObjectSOIndex)
    {
        fryingTimer.Value = 0f; //client 无权赋值网络变量
        state.Value = State.Frying;

        SetFryingRecipeSOClientRpc(kitchenObjectSOIndex);
    }
    [ClientRpc]
    private void SetFryingRecipeSOClientRpc(int kitchenObjectSOIndex)
    {
        KitchenObjectSO kitchenObjectSO = KitchenGameMutiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        fryingRecipeSO = GetFryingRecipeSOWithInput(kitchenObjectSO);
    }
    [ClientRpc]
    private void SetBurningRecipeSOClientRpc(int kitchenObjectSOIndex)
    {
        KitchenObjectSO kitchenObjectSO = KitchenGameMutiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        burningRecipeSO = GetBurningRecipeSOWithInput(kitchenObjectSO);
    }

    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO) //判断一个物品是否是可以煎炸的
    {
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
        return fryingRecipeSO != null;
    }



    private FryingRecipeSO GetFryingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)//查找物品是否能够煎炸，可以就返回recipe
    {
        foreach (FryingRecipeSO fryingRecipeSO in fryingRecipeSOArray)
        {
            if (fryingRecipeSO.input == inputKitchenObjectSO)
            {
                return fryingRecipeSO;
            }
        }
        return null;
    }

    private BurningRecipeSO GetBurningRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)//查找物品是否能够煎炸过头，可以就返回recipe
    {
        foreach (BurningRecipeSO buringRecipeSO in burningRecipeSOArray)
        {
            if (buringRecipeSO.input == inputKitchenObjectSO)
            {
                return buringRecipeSO;
            }
        }
        return null;
    }

    public bool isFried()
    {
        return state.Value == State.Fried;
    }
}
