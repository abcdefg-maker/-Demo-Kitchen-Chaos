using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

// 玩家数据，用值类型struct才能放进NetworkList做联机同步
public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
    public ulong clientId;
    public int colorId;

    // 定义两个PlayerData何时算相等（clientId相同即同一玩家），供NetworkList内部比对用
    public bool Equals(PlayerData other)
    {
        return clientId == other.clientId && colorId == other.colorId;
    }

    // 定义该数据如何在网络上收发：SerializeValue是双向的，发送时写入字节流、接收时读出赋值
    // 以后新增字段，在这里再加一行serializer.SerializeValue(ref 字段)，否则不会被同步
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref colorId);
    }
}
