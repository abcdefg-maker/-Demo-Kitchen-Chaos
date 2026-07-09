using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter : BaseCounter  
{
    
    [SerializeField] private KitchenObjectSO kitchenObjectSO;


    public override void Interact(Player player)
    {
        if (!HasKitchenObject())
        {
            //台面上没东西
            if (player.HasKitchenObject())
            {
                // 玩家手里有东西
                player.GetKitchenObject().SetKitchenObjectParent(this); //把东西放在台面上
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
                if(player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
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
                else
                {
                    //玩家手里有东西，但是玩家手里的东西不是一个盘子
                    if(GetKitchenObject().TryGetPlate(out  plateKitchenObject)) //看看ClearCounter上面是否有盘子
                    {
                        if (plateKitchenObject.TryAddIngredient(player.GetKitchenObject().GetKitchenObjectSO()))
                        {
                            //把玩家手里的东西放到盘子上
                            player.GetKitchenObject().DestorySelf();
                        }
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


}
