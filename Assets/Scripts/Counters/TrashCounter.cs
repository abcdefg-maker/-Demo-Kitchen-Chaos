using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TrashCounter : BaseCounter
{
    public static event EventHandler OnAnyObjectTrashed; //调用丢垃圾音效的事件

    new public static void ResetStaticData() //在切换到主菜单的时候，重置OnAnyObjectTrashed这个静态事件
    {
        OnAnyObjectTrashed = null;
    }
    public override void Interact(Player player)
    {
        if (player.HasKitchenObject())
        {
            KitchenObject.DestoryKitchenObject(player.GetKitchenObject()); //服务器清除ko物品

            InteractLogicServerRpc();  //触发动画/音效事件
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicServerRpc()
    {
        InteractLogicClientRpc();
    }

    [ClientRpc]
    private void InteractLogicClientRpc()
    {
        OnAnyObjectTrashed?.Invoke(this, EventArgs.Empty); //调用丢垃圾动画/音效的事件
    }
}
