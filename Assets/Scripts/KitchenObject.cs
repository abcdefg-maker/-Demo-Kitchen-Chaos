using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KitchenObject : MonoBehaviour
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;
    
    
    private IKitchenObjectParent kitchenObjectParent;  //为了给当前的物品定位，确定它处于哪个位置

    public KitchenObjectSO GetKitchenObjectSO()
    {
        return kitchenObjectSO;
    }
    /// <summary>
    /// 函数功能：初始化或修改kitchenObject的位置
    /// </summary>
    /// <param name="kitchenObjectParent"></param>
    public void SetKitchenObjectParent(IKitchenObjectParent kitchenObjectParent)      
                                                                //由于KO.cs内保存了cc的位置，CC.cs内保存了kO的位置
                                                                //所以进行修改位置操作的时候，必须同时修改
                                                                //（当然这个函数也有“初始化kO对应的cc“的功能，但是这里主要讨论“修改”操作）
                                                                //这个函数已经进行了相当完善而安全的实现
                                                                //逻辑：
                                                                //删除原cc上的kO → kO.cc = new CC → 确保new CC上没有kO → new CC.kO = kO
    {
        if (this.kitchenObjectParent != null)   //如果kO之前已经在某个台面上放着
        {
            this.kitchenObjectParent.ClearKitchenObject(); //如果移动位置前的CC上有kO，删了它,
                                                    //如果这里不删，那this.cc被切换为新的cc后，新的cc.kO被赋值，
                                                    //就会出现问题：两个cc上都放着一个kO
        }

        this.kitchenObjectParent = kitchenObjectParent;       //初始化为某个CC/切换到新的CC

        if (kitchenObjectParent.HasKitchenObject())    //进行赋值前的检查
        {
            Debug.LogError("这个台面上已经放着一个物品了！你必须先移除旧物品！(in KitchenObject.cs)");
        }

        kitchenObjectParent.SetKitchenObject(this);    //把这个CC上的kO修改为this

        transform.parent = kitchenObjectParent.GetKitchenObjectFollowTransform();  //因为视觉和逻辑分离，所以这里获取的是EmptyObject,
                                                                            //也就是prefab（视觉）的parent
        transform.localPosition = Vector3.zero;                             //视觉上，让物品处于counterTopPoint的中心
    }

    public IKitchenObjectParent GetKitchenObjectParent()
    {
        return this.kitchenObjectParent;
    }
    /// <summary>
    /// 把一个GameObject在parent（逻辑上）和自身（视觉上）都清除
    /// </summary>
    public void DestorySelf() 
    {
        kitchenObjectParent.ClearKitchenObject();
        Destroy(gameObject);
    }

    public static KitchenObject SpawnKitchenObject(KitchenObjectSO kitchenObjectSO,IKitchenObjectParent kitchenObjectParent) //封装好的生成kO的函数
    {

        Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefabs);   //在unity内构造一个transform（视觉实现）
                                                                                   //把台面顶部作为番茄transfrom的parent
        kitchenObjectTransform.GetComponent<KitchenObject>().SetKitchenObjectParent(kitchenObjectParent);     //把物品放上来（逻辑实现）

        KitchenObject kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();

        return kitchenObject;
    }

    public bool TryGetPlate(out PlateKitchenObject plateKitchenObject) //C#不像Python支持多个返回值，out是函数返回多个值的方法
                                                                       //但是函数内部必须给这个参数赋值
                                                                       //并且传参的时候需要格式如下
                                                                       //TryGetPlate(out a);
    {
        if(this is PlateKitchenObject)
        {
            plateKitchenObject = this as PlateKitchenObject;
            return true;
        }
        else
        {
            plateKitchenObject = null;  //此处不给这个参数赋值，就会报错，原因见上方对于out的说明
            return false;
        }
    }
}
