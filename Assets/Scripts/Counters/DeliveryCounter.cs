using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryCounter : BaseCounter
{
    public static DeliveryCounter Instance {  get; private set; }
    private void Awake()
    {
        Instance = this;
    }
    public override void Interact(Player player)
    {
        if (player.HasKitchenObject())
        {
            if(player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
            {
                //只有玩家手里拿着盘子的时候才能够往传送带上放东西

                DeliveryManager.Instance.DeliveryRecipe(plateKitchenObject);

                KitchenObject.DestroyKitchenObject(player.GetKitchenObject()); //服务器销毁物品
            }
        }
    }
}
