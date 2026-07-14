using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ContainerCounter : BaseCounter //功能：把东西从台面里拿到玩家手上
{
    public event EventHandler OnPlayerGrabbedObject;

    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    public override void Interact(Player player)
    {
        if (!player.HasKitchenObject())//如果玩家手里有东西就不能再给玩家东西了
        {
            KitchenObject.SpawnKitchenObject(kitchenObjectSO, player);

           InteractLogicServerRpc();  //触发事件
        }
    } 

    [ServerRpc(RequireOwnership = false)]
    public void InteractLogicServerRpc()
    {
        InteractLogicClientRpc();
    }

    [ClientRpc]
    public void InteractLogicClientRpc()
    {
         OnPlayerGrabbedObject?.Invoke(this, EventArgs.Empty);    //触发事件,播放动画（应该也有音效，不是很确定）
    }

}
