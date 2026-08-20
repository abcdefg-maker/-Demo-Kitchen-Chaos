using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class CharacterSelectReady :NetworkBehaviour
{
    public static CharacterSelectReady Instance { get; private set; }

    private Dictionary<ulong, bool> playerReadyDictionary;

    private void Awake()
    {
        Instance = this;
        playerReadyDictionary = new Dictionary<ulong, bool>();
    }


    
    public void SetPlayerReady()
    {
        //调用服务器端的RPC方法，通知服务器本玩家已准备好
        //服务器会根据所有玩家的准备状态来判断是否可以开始游戏
        if (NetworkManager.Singleton.IsClient)
        {
            SetPlayerReadyServerRpc();
        }
    }

    /// <summary>
    /// 一个结构体参数。当客户端调用 SetPlayerReadyServerRpc() 时，
    ///  Netcode 会在服务器端自动往里面塞入调用方的信息
    /// </summary>
    /// <param name="serverRpcParams"></param>

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        //靠 serverRpcParams.Receive.SenderClientId 来区分到底是谁调用的
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allPlayersReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                allPlayersReady = false;
                break;
            }
        }
       if (allPlayersReady)
        {
            Loader.LoadNetwork(Loader.Scene.MainScene);
        }

    }
}
