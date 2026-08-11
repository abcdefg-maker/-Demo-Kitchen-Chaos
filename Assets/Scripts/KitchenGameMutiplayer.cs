using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class KitchenGameMutiplayer : NetworkBehaviour
{
    public static KitchenGameMutiplayer Instance { get; private set; }

    [SerializeField] private KitchenObjectListSO kitchenObjectListSO;


    private void Awake()
    {
        Instance = this;
    }

    public void StartHost()
    {
        //当有客户端尝试连接到服务器/主机时， 
        //Netcode 会调用这个回调，让你决定：这个连接批不批准。
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback; 
        NetworkManager.Singleton.StartHost();
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        if (KitchenGameManager.Instance.IsWaitingToStart()) //如果游戏处于等待开始状态
        {
            connectionApprovalResponse.Approved = true;
            connectionApprovalResponse.CreatePlayerObject = true; //允许创建玩家对象
        }
        else
        {
            connectionApprovalResponse.Approved = false;
        }
        return;

    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
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
}
