using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class KitchenObject : NetworkBehaviour
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;
    
    
    private IKitchenObjectParent kitchenObjectParent;  //为了给当前的物品定位，确定它处于哪个位置
    private FollowTranform followTranform;  //为了让物品跟随玩家移动，必须有一个FollowTransform组件


    protected virtual void Awake() //Awake()在继承类(其实也就是PlateKitchenObject子类 )中也会被调用，所以这里用protected
    {
        followTranform = GetComponent<FollowTranform>();
    }
    public KitchenObjectSO GetKitchenObjectSO()
    {
        return kitchenObjectSO;
    }
    /// <summary>
    /// 函数功能：初始化或修改kitchenObject的位置
    /// 
    /// 传函数接口作为参数的原因：
    ///  只要是实现了这个接口的父类，都可以作为parent，减少代码重载的次数
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
        SetKitchenObjectParentServerRpc(kitchenObjectParent.GetNetworkObject());

       /* 从前单机的实现
        transform.parent = kitchenObjectParent.GetKitchenObjectFollowTransform();  //因为视觉和逻辑分离，所以这里获取的是EmptyObject,
                                                                            //也就是prefab（视觉）的parent
        transform.localPosition = Vector3.zero;                             //视觉上，让物品处于counterTopPoint的中心
        */
    }

    [ServerRpc(RequireOwnership = false)] 
    private void SetKitchenObjectParentServerRpc(NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
          SetKitchenObjectParentClientRpc(kitchenObjectParentNetworkObjectReference);
    }

    [ClientRpc]
     private void SetKitchenObjectParentClientRpc(NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        kitchenObjectParentNetworkObjectReference.TryGet(out NetworkObject kitchenObjectParentNetworkObject);
        IKitchenObjectParent kitchenObjectParent = kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectParent>();
        
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

        followTranform.SetTarget(kitchenObjectParent.GetKitchenObjectFollowTransform());
    }


    public IKitchenObjectParent GetKitchenObjectParent()
    {
        return this.kitchenObjectParent;
    }
    /// <summary>
    /// 把一个GameObject在自身（视觉上）清除
    /// </summary>
    public void DestorySelf() 
    {
        Destroy(gameObject);
    }

    public void ClearKitchenObjectOnParent() //把kO从parent上清除（逻辑上）
    {
        kitchenObjectParent.ClearKitchenObject();
    }

    public static void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO,IKitchenObjectParent kitchenObjectParent) //封装好的生成kO的函数
    {
        KitchenGameMutiplayer.Instance.SpawnKitchenObject(kitchenObjectSO, kitchenObjectParent); //联网生成kO
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

    //static(包括上面的spawn函数的static)，都是不希望用一个具体的实体，来调用这个生成/删除实体的函数，这样感觉有点奇怪，逻辑上也说不通)
    public static void DestroyKitchenObject(KitchenObject kitchenObject) 
    {
        KitchenGameMutiplayer.Instance.DestoryKitchenObject(kitchenObject); //联网删除kO
    }


}
