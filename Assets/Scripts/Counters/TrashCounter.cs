using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            player.GetKitchenObject().DestorySelf();

            OnAnyObjectTrashed?.Invoke(this, EventArgs.Empty); //调用丢垃圾音效的事件
        }
    }
}
