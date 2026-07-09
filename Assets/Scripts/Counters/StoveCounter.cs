using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    private State state; //currentState
    private float fryingTimer;
    private float burningTimer;
    private FryingRecipeSO fryingRecipeSO;
    private BurningRecipeSO burningRecipeSO;


    private void Start()
    {
        state = State.Idle; //初始化状态机状态
    }

    private void Update()   //状态机的核心逻辑，管理状态进入和切换
    {
        if (HasKitchenObject())
        {
            switch (state)
            {
                case State.Idle:
                    break;
                case State.Frying:
                    fryingTimer += Time.deltaTime;

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                    {
                        progressNormalized = fryingTimer / fryingRecipeSO.fryingTimerMax
                    });

                    if (fryingTimer >= fryingRecipeSO.fryingTimerMax)
                    {
                        //煎炸好了
                        fryingTimer = 0f;

                        GetKitchenObject().DestorySelf();

                        KitchenObject.SpawnKitchenObject(fryingRecipeSO.output, this);

                        state = State.Fried;
                        burningTimer = 0f;
                        burningRecipeSO = GetBurningRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

                        OnStateChanged?.Invoke(this,new OnStateChangedEventArgs{
                            state = this.state,
                        });
                    }
                    break;
                case State.Fried:
                    burningTimer += Time.deltaTime;

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                    {
                        progressNormalized = burningTimer / burningRecipeSO.burningTimerMax
                    });

                    if (burningTimer >= burningRecipeSO.burningTimerMax)
                    {
                        //煎炸过头了

                        GetKitchenObject().DestorySelf();

                        KitchenObject.SpawnKitchenObject(burningRecipeSO.output, this);

                        state = State.Burned;

                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                        {
                            state = this.state,
                        });

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
                    player.GetKitchenObject().SetKitchenObjectParent(this); //把东西放在台面上


                    fryingRecipeSO = GetFryingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());


                    state = State.Frying;
                    fryingTimer = 0f;
                    OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                    {
                        state = this.state,
                    });

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                    {
                        progressNormalized = fryingTimer / fryingRecipeSO.fryingTimerMax
                    });
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
                        GetKitchenObject().DestorySelf();
                        state = State.Idle; //玩家拿起肉饼后，重置为初始状态
                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                        {
                            state = this.state,
                        });

                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                        {
                            progressNormalized = 0f
                        });
                    }
                }
            }
            else
            {
                // 玩家手里没有东西
                GetKitchenObject().SetKitchenObjectParent(player);//把东西放在玩家手上
                
                state = State.Idle; //玩家拿起肉饼后，重置为初始状态
                OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                {
                    state = this.state,
                });

                OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                {
                    progressNormalized = 0f
                });
            }
        }
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
        return state == State.Fried;
    }
}
