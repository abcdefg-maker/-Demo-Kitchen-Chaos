using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CuttingCounter : BaseCounter , IHasProgress
{
    public static event EventHandler OnAnyCut; //控制切菜音效的事件

    new public static void ResetStaticData() //在切换到主菜单的时候，重置OnAnyCut这个静态事件
    {
        OnAnyCut = null;
    }

    public event EventHandler <IHasProgress.OnProgressChangedEventArgs> OnProgressChanged; //控制切菜进度条的事件

    public event EventHandler OnCut;    //控制切菜动画的事件

    [SerializeField] private CuttingRecipeSO[] cuttingRecipeSOArray;
    
    private int cuttingProgress;

    public override void Interact(Player player)
    {
        if (!HasKitchenObject())
        {
            //台面上没东西
            if (player.HasKitchenObject())
            {
                // 玩家手里有东西
                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO()))//东西是可以切片的
                {
                    player.GetKitchenObject().SetKitchenObjectParent(this); //把东西放在台面上
                    cuttingProgress = 0;

                    CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                    {
                        progressNormalized = (float) cuttingProgress / cuttingRecipeSO.cuttingProgressMax
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
                    }
                }
            }
            else
            {
                // 玩家手里没有东西
                GetKitchenObject().SetKitchenObjectParent(player);//把东西放在玩家手上
            }
        }
    }
    public override void InteractAlternate(Player player)
    {
       if(HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO())) //保证切片一次之后，Slice不再被切
        {
            //台面上有东西
            cuttingProgress++;

            OnCut?.Invoke(this,EventArgs.Empty);    //调动切菜动画的事件
            OnAnyCut?.Invoke(this,EventArgs.Empty); //调动切菜声音的事件

            CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
            {
                progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax
            });


            if (cuttingProgress >= cuttingRecipeSO.cuttingProgressMax) //如果切的次数到了，就变成片
            {
                KitchenObjectSO outputKitchenObjectSO = GetOutputForInput(GetKitchenObject().GetKitchenObjectSO()); //找切片后的SO

                this.GetKitchenObject().DestorySelf();


                KitchenObject.SpawnKitchenObject(outputKitchenObjectSO, this);//生成切片后SO对应的prefab
            }
        }
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO) //查找切片前和切片后对应的gameobject
    {
       CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
        if (cuttingRecipeSO != null)
        {
            return cuttingRecipeSO.output;
        }
        else
        {
            return null; 
        }
    }

    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO) //判断一个物品是否是可以切片的
    {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
        return cuttingRecipeSO != null;
    }

    private CuttingRecipeSO GetCuttingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)//查找物品是否能够切片，可以就返回recipe
    {
        foreach (CuttingRecipeSO cuttingRecipeSO in cuttingRecipeSOArray)
        {
            if (cuttingRecipeSO.input == inputKitchenObjectSO)
            {
                return cuttingRecipeSO;
            }
        }
        return null;
    }
}
