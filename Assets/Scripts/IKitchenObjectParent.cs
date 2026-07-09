using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//  C#接口
public interface IKitchenObjectParent      //C#接口（interface）像是一个合同，
                                           //他只会声明一些函数，但是不会实现这些函数
                                           //在一个类A以该接口作为基类的时候，
                                           //A必须在类内实现这个接口声明的所有函数
                                           //另外，
                                           //C#不支持类的多继承，但是一个类可以继承多个接口

                                           //现在，为了方便实现player抓取kO的代码
                                           //把cc这个具体的类，代替为接口，来实现这个功能，注释懒得更新了，原理不变
{
    public Transform GetKitchenObjectFollowTransform();  //获取kO应该被放置的位置，
                                                         //这里是配合转移kO位置而实现的函数接口，
                                                         //用于获取secondCC的物品放置位置



    public void SetKitchenObject(KitchenObject kitchenObjtect); //物品增

    public KitchenObject GetKitchenObject(); //物品查
    public void ClearKitchenObject(); //物品删


    public bool HasKitchenObject();  //查看kO是否被赋值
    
}
