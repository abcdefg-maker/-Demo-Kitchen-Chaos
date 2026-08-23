using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Sync.Relay;
using Unity.Sync.Relay.Lobby;
using Unity.Sync.Relay.Model;
using Unity.Sync.Relay.Transport.Netcode;

public class KitchenGameLobby : MonoBehaviour
{
    public static KitchenGameLobby Instance { get; private set; }

    private const string ROOM_NAMESPACE = "KitchenChaos";

    private RelayTransportNetcode relayTransport;
    private string playerUuid;

    // 防止重复点击创建或加入按钮。
    private bool isOperationInProgress;

    // Host 连接成功后再加载角色选择场景。
    private bool loadCharacterSelectAfterHost;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject); //在场景切换时不销毁这个对象
    }

    private void Start()
    {
        SetupRelayTransport();
    }

    private void SetupRelayTransport()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("[KitchenGameLobby] 找不到 NetworkManager。");
            return;
        }

        relayTransport = NetworkManager.Singleton.GetComponent<RelayTransportNetcode>();

        if (relayTransport == null)
        {
            Debug.LogError("[KitchenGameLobby] NetworkManager 上没有 RelayTransportNetcode。");
            return;
        }

        // 为当前玩家生成唯一 ID。
        playerUuid = Guid.NewGuid().ToString();

        // UOS 要求在创建或加入房间前设置玩家信息。
        relayTransport.SetPlayerData(
            playerUuid,
            "Player-" + playerUuid
        );

        // 注册 Relay 连接和房间状态相关回调。
        RelayCallbacks callbacks = new RelayCallbacks();
        callbacks.RegisterConnectToRelayServer(OnConnectToRelayServer);
        callbacks.RegisterPlayerEnterRoom(OnPlayerEnterRoom);
        callbacks.RegisterPlayerLeaveRoom(OnPlayerLeaveRoom);
        callbacks.RegisterMasterClientMigrate(OnMasterClientMigrate);
        callbacks.RegisterSetHeartbeat(OnSetHeartbeat);

        relayTransport.SetCallbacks(callbacks);

        Debug.Log("[KitchenGameLobby] UOS Relay 初始化完成。");
    }

    // 创建房间，并以 Host 身份启动游戏。
    public void CreateLobby(string lobbyName, bool isPrivate)
    {
        if (!CanStartOperation())
        {
            return;
        }

        // UOS 私有房间需要额外的 JoinCode。
        // 当前 LobbyUI 没有 JoinCode 输入框，所以暂时只支持公开房间。
        if (isPrivate)
        {
            Debug.LogError("[KitchenGameLobby] 当前暂不支持私有房间，请使用公开房间。");
            return;
        }

        isOperationInProgress = true;

        CreateRoomRequest request = new CreateRoomRequest
        {
            Name = lobbyName,
            Namespace = ROOM_NAMESPACE,
            MaxPlayers = KitchenGameMutiplayer.MAX_PLAYER_COUNT,
            Visibility = LobbyRoomVisibility.Public,
            OwnerId = playerUuid
        };

        // UOS 使用协程完成异步创建房间。
        StartCoroutine(LobbyService.AsyncCreateRoom(request, OnRoomCreated));
    }

    private void OnRoomCreated(CreateRoomResponse response)
    {
        if (response == null)
        {
            HandleOperationFailed("创建房间失败：没有收到服务器响应。");
            return;
        }

        if (response.Code != (uint)RelayCode.OK)
        {
            HandleOperationFailed("创建房间失败：" + response.ErrorMessage);
            return;
        }

        // Host 必须等待 UOS 分配 Relay 房间完成。
        if (response.Status != LobbyRoomStatus.ServerAllocated)
        {
            HandleOperationFailed("Relay 房间还没有分配完成，当前状态：" + response.Status);
            return;
        }

        // 将 UOS 返回的房间信息交给 Relay Transport。
        relayTransport.SetRoomData(response);

        loadCharacterSelectAfterHost = true;

        // 设置好房间数据后才能启动 NGO Host。
        KitchenGameMutiplayer.Instance.StartHost();

        Debug.Log("[KitchenGameLobby] 房间创建成功，正在启动 Host。");
    }

    // 快速加入一个公开房间，并以 Client 身份启动游戏。
    public void QuickJoin()
    {
        if (!CanStartOperation())
        {
            return;
        }

        isOperationInProgress = true;

        QuickJoinRequest request = new QuickJoinRequest
        {
            Namespace = ROOM_NAMESPACE,
            Status = LobbyRoomStatus.Ready
        };

        // UOS 自动查找一个可以加入的公开房间。
        StartCoroutine(LobbyService.QuickJoinRoom(request, OnRoomJoined));
    }

    private void OnRoomJoined(QuickJoinResponse response)
    {
        if (response == null)
        {
            HandleOperationFailed("加入房间失败：没有收到服务器响应。");
            return;
        }

        if (response.Code != (uint)RelayCode.OK)
        {
            HandleOperationFailed("加入房间失败：" + response.ErrorMessage);
            return;
        }

        // 将加入房间的信息交给 Relay Transport。
        relayTransport.SetRoomData(response);

        // 设置好房间数据后才能启动 NGO Client。
        KitchenGameMutiplayer.Instance.StartClient();

        Debug.Log("[KitchenGameLobby] 加入房间成功，正在启动 Client。");
    }

    private bool CanStartOperation()
    {
        if (isOperationInProgress)
        {
            Debug.LogWarning("[KitchenGameLobby] 当前已经有一个房间操作正在进行。");
            return false;
        }

        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("[KitchenGameLobby] 找不到 NetworkManager。");
            return false;
        }

        if (NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("[KitchenGameLobby] NetworkManager 已经在运行。");
            return false;
        }

        if (relayTransport == null)
        {
            Debug.LogError("[KitchenGameLobby] RelayTransportNetcode 尚未初始化。");
            return false;
        }

        if (KitchenGameMutiplayer.Instance == null)
        {
            Debug.LogError("[KitchenGameLobby] 找不到 KitchenGameMutiplayer.Instance。");
            return false;
        }

        return true;
    }

    private void HandleOperationFailed(string message)
    {
        isOperationInProgress = false;
        loadCharacterSelectAfterHost = false;
        Debug.LogError("[KitchenGameLobby] " + message);
    }

    private void OnConnectToRelayServer(uint code, RelayRoom room)
    {
        isOperationInProgress = false;

        if (code != (uint)RelayCode.OK)
        {
            loadCharacterSelectAfterHost = false;
            Debug.LogError("[KitchenGameLobby] Relay 连接失败，错误代码：" + code);
            return;
        }

        Debug.Log("[KitchenGameLobby] Relay 连接成功，房间：" + room.Name);

        // Host 真正连接 Relay 后，再让所有网络客户端进入角色选择场景。
        if (loadCharacterSelectAfterHost)
        {
            loadCharacterSelectAfterHost = false;
            Loader.LoadNetwork(Loader.Scene.CharacterSelectScene);
        }
    }

    private void OnPlayerEnterRoom(RelayPlayer player)
    {
        Debug.Log("[KitchenGameLobby] 玩家加入房间：" + player.TransportId);
    }

    private void OnPlayerLeaveRoom(RelayPlayer player)
    {
        Debug.Log("[KitchenGameLobby] 玩家离开房间：" + player.TransportId);
    }

    private void OnMasterClientMigrate(uint newMasterClient)
    {
        Debug.Log("[KitchenGameLobby] Host 转移，新 Host：" + newMasterClient);
    }

    private void OnSetHeartbeat(uint code, uint timeout)
    {
        Debug.Log("[KitchenGameLobby] 心跳设置结果：" + code);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
