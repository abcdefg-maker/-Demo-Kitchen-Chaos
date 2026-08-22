using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

// 玩家数据，用值类型struct才能放进NetworkList做联机同步
// IEquatable<PlayerData>：让NetworkList能判断元素是否变化；INetworkSerializable：让该数据能在网络上收发
public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
    public ulong clientId;  // 客户端唯一ID，由Netcode分配，用来标识这条数据属于哪个玩家
    public int colorId;     // 玩家选择的颜色编号（对应颜色列表的下标）

    // 定义两个PlayerData何时算相等（clientId和colorId都相同才算相等），供NetworkList内部比对用
    public bool Equals(PlayerData other)
    {
        return clientId == other.clientId && colorId == other.colorId;
    }

    // 定义该数据如何在网络上收发：SerializeValue是双向的，发送时写入字节流、接收时读出赋值
    // 以后新增字段，在这里再加一行serializer.SerializeValue(ref 字段)，否则不会被同步
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        // 1. SerializeValue 是"双向"的一个方法
        // 大多数序列化框架会有 Write（写）和 Read（读）两个方法，而 Netcode 把它俩合成了一个 SerializeValue。它内部会看当前 BufferSerializer<T> 处于哪种模式：

        // 发送方（IsWriter）：把 clientId 的当前值写进字节流。
        // 接收方（IsReader）：从字节流里读出值，写回到 clientId 变量里。
        // 所以同一行代码，在发送端和接收端跑的是相反的操作。这就是为什么你只需要写一遍，不用维护两份"读/写"逻辑——省事且不会读写字段顺序不一致。

        // 2. 为什么必须用 ref
        // ref 表示按引用传递这个字段，而不是传值拷贝。原因就在上面第 1 点：

        // 接收方需要把读出来的值赋回到你的字段 clientId 上。如果不用 ref，方法内部改的只是一份拷贝，出了方法你的字段还是原样，同步就失效了。
        // 发送方其实只读不写，用不用 ref 都行，但因为同一个方法要兼顾接收方，所以签名强制要求 ref。
        // 一句话：ref 是为了让"接收时把值写回你的变量"这件事能成立。
        serializer.SerializeValue(ref clientId); 
        serializer.SerializeValue(ref colorId); 
    }
}
