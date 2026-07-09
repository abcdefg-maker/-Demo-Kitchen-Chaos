using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateKitchenObject : KitchenObject
{

    public event EventHandler<OnIngredinetAddEventArgs> OnIngredientAdd; //视觉上显示食材添加的事件
    public class OnIngredinetAddEventArgs: EventArgs
    {
        public KitchenObjectSO kitchenObjectSO;
    }

    [SerializeField] private List<KitchenObjectSO> validKitchenObjectList;  //我们不希望什么物品都可以被放进盘子里
                                                                            //只希望在最终配方的食材可以放在盘子里面，
                                                                            //比如整颗的番茄，我们是不希望可以放进去的

    private List<KitchenObjectSO> kitchenObjectSOList;

    private void Awake()
    {
        kitchenObjectSOList = new List<KitchenObjectSO>();
    }

    /// <summary>
    /// 把一个物品移动到盘子上面
    /// </summary>
    /// <param name="kitchenObjectSO"></param>
    public bool TryAddIngredient(KitchenObjectSO kitchenObjectSO) 
    {
        if(!validKitchenObjectList.Contains(kitchenObjectSO))
        {
            //不是可以放进盘子里的食物
            return false;
        }
        if (kitchenObjectSOList.Contains(kitchenObjectSO))
        {
            //上面已经放着这个食材
            return false;
        }
        else
        {

            kitchenObjectSOList.Add(kitchenObjectSO); //逻辑上把物品的so加入盘子的list

            OnIngredientAdd?.Invoke(this, new OnIngredinetAddEventArgs //视觉上激活这个ingredient的组件的事件
            {
                kitchenObjectSO = kitchenObjectSO,
            });

            return true;
        }
    }

    public List<KitchenObjectSO> GetKitchenObjectSOList()
    {
        return kitchenObjectSOList;
    }
}
