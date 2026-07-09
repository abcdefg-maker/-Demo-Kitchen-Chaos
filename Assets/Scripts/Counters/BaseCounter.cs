using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCounter : MonoBehaviour, IKitchenObjectParent //利用接口来实现多继承（C#类不支持多继承）
{
    public static event EventHandler OnAnyObjectPlacedHere; //调用放置物品音效的事件

    public static void ResetStaticData() //在切换到主菜单的时候，重置OnAnyObjectPlacedHere这个静态事件
    {
        OnAnyObjectPlacedHere = null;
    }

    [SerializeField] private Transform counterTopPoint;

    private KitchenObject kitchenObject;

    public virtual void Interact(Player player)
    {
        Debug.LogError("BaseCounter Interact();");
    }

    public virtual void InteractAlternate(Player player)
    {
        //Debug.LogError("BaseCounter InteractAlternate();");
    }

    public Transform GetKitchenObjectFollowTransform()  //获取kO应该被放置的位置，
                                                        //这里是配合转移kO位置而实现的函数接口，
                                                        //用于获取secondCC的物品放置位置
    {
        return counterTopPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObjtect) //物品增
    {
        this.kitchenObject = kitchenObjtect;

        if (kitchenObjtect != null)
        {
            OnAnyObjectPlacedHere?.Invoke(this,EventArgs.Empty);  //调用放置物品音效的事件
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
