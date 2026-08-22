using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System;

public class KitchenGameMutiplayer : NetworkBehaviour
{

    private const int MAX_PLAYER_COUNT = 4; //最大玩家数量
    public static KitchenGameMutiplayer Instance { get; private set; }

    public event EventHandler OnTryingToJoinGame; //当有客户端尝试连接到服务器/主机时，触发这个事件
    public event EventHandler OnFailedToJoinGame; //当有客户端尝试连接到服务器/主机失败时，触发这个事件
    public event EventHandler OnPlayerDataNetworkListChanged; //当网络列表发生变化时，触发这个事件

    [SerializeField] private KitchenObjectListSO kitchenObjectListSO;
    [SerializeField] private List<Color> playerColorList;

    private NetworkList<PlayerData> playerDataNetworkList; //网络列表：存储所有玩家的数据（客户端和服务器都能访问）


    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject); //切换场景时不销毁这个对象

        playerDataNetworkList = new NetworkList<PlayerData>(); //初始化保存玩家id的网络列表
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
    }

    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty); //触发事件：网络列表发生变化
    }

    public void StartHost()
    {
        //当有客户端尝试连接到服务器/主机时， 
        //Netcode 会调用这个回调，让你决定：这个连接批不批准。
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback; 
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback; //当客户端连接时，调用这个回调
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback; //当客户端断开连接时，调用这个回调
        NetworkManager.Singleton.StartHost();
    }

    private void NetworkManager_Server_OnClientDisconnectCallback(ulong clientId)
    {
        for(int i = 0; i < playerDataNetworkList.Count; i++) //遍历网络列表
        {
            if(playerDataNetworkList[i].clientId == clientId) //如果这个玩家id和传入的id相同
            {
                //执行断线处理逻辑：从网络列表中移除这个玩家数据
                playerDataNetworkList.RemoveAt(i); //从网络列表中移除这个玩家数据
                return;
            }
        }
    }
    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {

        playerDataNetworkList.Add(new PlayerData {
                clientId = clientId ,
                colorId = GetFirstUnusedColorId()
        }); //把新连接的客户端id加入到网络列表中

    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        if(SceneManager.GetActiveScene().name != Loader.Scene.CharacterSelectScene.ToString()) //如果当前场景不是角色选择场景
        {
            connectionApprovalResponse.Approved = false; //不批准连接
            connectionApprovalResponse.Reason = "Game has already started. Cannot join now."; //拒绝原因
            return;
        }

        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_COUNT) //如果当前已连接的客户端数量>=最大玩家数量
        {
            connectionApprovalResponse.Approved = false; //不批准连接
            connectionApprovalResponse.Reason = "Game is full. Cannot join now."; //拒绝原因
            return;
        }
        connectionApprovalResponse.Approved = true;

        return;

    }

    public void StartClient()
    {
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty); //触发事件：有客户端尝试连接到服务器/主机

        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback; //当客户端断开连接时，调用这个回调
        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_Client_OnClientDisconnectCallback(ulong clientId)
    {
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty); //触发事件：有客户端尝试连接到服务器/主机失败
    }

    //传函数接口作为参数的原因：
    //  只要是实现了这个接口的父类，都可以作为parent，减少代码重载的次数
    public void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO,IKitchenObjectParent kitchenObjectParent) //封装好的生成kO的函数
    {
        SpawnKitchenObjectServerRpc(GetKitchenObjectSOIndex(kitchenObjectSO), kitchenObjectParent.GetNetworkObject());

    }

    /// <summary>
    /// 函数功能：在服务器上生成一个kitchenObject
    /// 
    ///真正的生成逻辑运行在【服务器】上
    ///RequireOwnership = false：允许非owner的客户端也能调用此RPC（否则默认只有owner能调）
    ///参数只能传【可序列化】的数据：所以SO用int索引传，parent用NetworkObjectReference传
    ///Netcode 给每个网络对象分配了一个全网统一的 NetworkObjectId。NetworkObjectReference 就是
    /// 一个轻量的、可序列化的「网络对象身份证」——它内部其实就存了那个 id。
    /// 传输时只发这个 id（一个数字），到了服务器端再用 id 查回本地对应的对象。
    /// </summary>
    /// <param name="kitchenObjectSOIndex"></param>
    /// <param name="kitchenObjectParentNetworkObjectReference"></param>

    [ServerRpc(RequireOwnership = false)]
    private void SpawnKitchenObjectServerRpc(int kitchenObjectSOIndex, NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        //根据索引还原出对应的KitchenObjectSO（配置数据）
        KitchenObjectSO kitchenObjectSO = GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);

        Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefabs);   //在unity内构造一个transform（视觉实现）
                                                                                   //把台面顶部作为（）番茄transfrom的parent
        NetworkObject kitchenObjectNetworkObject = kitchenObjectTransform.GetComponent<NetworkObject>();
        kitchenObjectNetworkObject.Spawn(true); //联网生成kO：由服务器生成，并自动同步到所有客户端

        //把网络引用还原成真正的NetworkObject（TryGet成功返回true，并通过out给出对象）
        //TryGet(...) 的作用：根据引用里存的 id，在当前这一端查找对应的 NetworkObject。
        //out NetworkObject kitchenObjectParentNetworkObject：
        // 查到的对象通过 out 参数「输出」出来，直接就地声明了这个变量接住结果。、

        kitchenObjectParentNetworkObjectReference.TryGet(out NetworkObject kitchenObjectParentNetworkObject);
        //从这个NetworkObject上拿到"持有物品"的接口组件（可能是Player，也可能是柜台）
        IKitchenObjectParent kitchenObjectParent = kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectParent>();

        kitchenObjectTransform.GetComponent<KitchenObject>().SetKitchenObjectParent(kitchenObjectParent);     //把物品放上来（逻辑实现）

        KitchenObject kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();
    }


    public int GetKitchenObjectSOIndex(KitchenObjectSO kitchenObjectSO)
    {
        return kitchenObjectListSO.kitchenObjectSOList.IndexOf(kitchenObjectSO);
    } 

    public KitchenObjectSO GetKitchenObjectSOFromIndex(int kitchenObjectSOIndex)
    {
        return kitchenObjectListSO.kitchenObjectSOList[kitchenObjectSOIndex];
    }

    public void DestoryKitchenObject(KitchenObject kitchenObject)
    {
        DestoryKitchenObjectServerRpc(kitchenObject.NetworkObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestoryKitchenObjectServerRpc(NetworkObjectReference kitchenObjectNetworkObjectReference)
    {
        kitchenObjectNetworkObjectReference.TryGet(out NetworkObject kitchenObjectNetworkObject);
        KitchenObject kitchenObject = kitchenObjectNetworkObject.GetComponent<KitchenObject>();

        ClearKitchenObjectOnParentClientRpc(kitchenObjectNetworkObjectReference); //把kO从parent上清除（逻辑上）

        kitchenObject.DestorySelf();//把一个GameObject在自身（视觉上）清除
    }

    [ClientRpc]
    private void ClearKitchenObjectOnParentClientRpc(NetworkObjectReference kitchenObjectNetworkObjectReference)
    {
        kitchenObjectNetworkObjectReference.TryGet(out NetworkObject kitchenObjectNetworkObject);
        KitchenObject kitchenObject = kitchenObjectNetworkObject.GetComponent<KitchenObject>();
        kitchenObject.ClearKitchenObjectOnParent();
    }

    public bool IsPlayerIndexConnected(int playerIndex)
    {
        return playerDataNetworkList.Count > playerIndex; //如果网络列表中有这个索引的玩家id，说明这个玩家已连接
    }

    public int GetPlayerDataIndexFromClientId(ulong clientId)
    {
        for(int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if(playerDataNetworkList[i].clientId == clientId) //如果这个玩家id和传入的id相同
            {
                return i; //返回这个玩家数据
            }
        }
        return -1; //如果没找到，返回默认值
    }

    public PlayerData GetPlayerDataFromClientId(ulong clientId)
    {
        foreach (PlayerData playerData in playerDataNetworkList) //遍历网络列表
        {
            if(playerData.clientId == clientId) //如果这个玩家id和传入的id相同
            {
                return playerData; //返回这个玩家数据
            }
        }
        return default; //如果没找到，返回默认值
    }
    public PlayerData GetPlayerData()
    {
        return GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId);//获取本地客户端的玩家数据
    }
    public PlayerData GetPlayerDataFromPlayerIndex(int playerIndex)
    {
        return playerDataNetworkList[playerIndex];
    }

    public Color GetPlayerColor(int playerIndex)
    {
        return playerColorList[playerIndex];
    }

    public void ChangePlayerColor(int colorId)
    {
        ChangePlayerColorServerRpc(colorId); //调用服务器RPC，传入本地客户端id和颜色id
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangePlayerColorServerRpc(int colorId, ServerRpcParams serverRpcParams = default)
    {
        if(!IsColorAvailable(colorId)) //如果这个颜色已被选中
        {
            return; //直接返回，不做任何处理
        }
        
        //因为List是网络变量，不能直接修改它里面的元素，所以要先取出来，修改后再写回去 
        //具体原因： 
        // 1.struct 是值拷贝所以原地改改的是废拷贝（语言限制） 
        // 2. NetworkList 只认自己的 setter 才会触发同步（框架设计），两个原因叠一起，只能"取出→改→写回"。 
        //这才是更关键的原因。NetworkList 要把变化同步到所有客户端、并触发 OnListChanged 事件，它必须知道哪个元素被改了。 
        //它是怎么知道的？——通过它自己的方法：Add()、Remove()、以及索引器的 setter list[i] = value。 
        // 只有走这些方法，NetworkList 才会把对应元素标记为"脏"（dirty），排队等着通过网络发出去。 
        //假如 C# 允许你原地改结构体字段，绕过了 setter， 
        // 那 NetworkList 根本不知道发生了变化 → 不会同步给客户端，也不会触发事件。等于改了个寂寞。 

        
        int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId); //获取这个玩家数据的索引 
        PlayerData playerData = playerDataNetworkList[playerDataIndex]; //获取这个玩家数据 
        playerData.colorId = colorId; //修改玩家数据的颜色id 
        playerDataNetworkList[playerDataIndex] = playerData; //把修改后的玩家数据写回网络列表 
    }

    private bool IsColorAvailable(int colorId)
    {
        foreach (PlayerData playerData in playerDataNetworkList) //遍历网络列表
        {
            if(playerData.colorId == colorId) //如果这个玩家的颜色id和传入的颜色id相同
            {
                return false; //说明这个颜色已被选中，返回false
            }
        }
        return true; //如果没找到，说明这个颜色未被选中，返回true
    }

    private int GetFirstUnusedColorId()
    {
        for(int i = 0; i < playerColorList.Count; i++) //遍历颜色列表
        {
            if(IsColorAvailable(i)) //如果这个颜色未被选中
            {
                return i; //返回这个颜色id
            }
        }
        return -1; //如果没找到，返回-1
    }

    public void KickPlayer(ulong clientId)
    {
        NetworkManager.Singleton.DisconnectClient(clientId); //踢掉这个客户端
        NetworkManager_Server_OnClientDisconnectCallback(clientId); //手动调用断线处理逻辑(出于未知原因，Netcode这里不会自动调用这个回调)
    }
}
