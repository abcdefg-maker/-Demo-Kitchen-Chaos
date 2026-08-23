using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Sync.Relay;
using Unity.Sync.Relay.Lobby;
using Unity.Sync.Relay.Model;
using Unity.Sync.Relay.Transport.Netcode;
/// <summary>
/// Used in tandem with the ConnectModeButtons prefab asset in test project
/// </summary>
public class ConnectionModeScript : MonoBehaviour
{
    [SerializeField]
    private GameObject m_ConnectionModeButtons;

    [SerializeField]
    private GameObject m_AuthenticationButtons;

    [SerializeField]
    private GameObject m_JoinCodeInput;
    
    private CommandLineProcessor m_CommandLineProcessor;

    private string uid;

    internal void SetCommandLineHandler(CommandLineProcessor commandLineProcessor)
    {
        m_CommandLineProcessor = commandLineProcessor;
        if (m_CommandLineProcessor.AutoConnectEnabled())
        {
            StartCoroutine(WaitForNetworkManager());
        }
    }

    public delegate void OnNotifyConnectionEventDelegateHandler();

    public event OnNotifyConnectionEventDelegateHandler OnNotifyConnectionEventServer;
    public event OnNotifyConnectionEventDelegateHandler OnNotifyConnectionEventHost;
    public event OnNotifyConnectionEventDelegateHandler OnNotifyConnectionEventClient;

    private IEnumerator WaitForNetworkManager()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            try
            {
                if (NetworkManager.Singleton && !NetworkManager.Singleton.IsListening)
                {
                    m_ConnectionModeButtons.SetActive(false);
                    m_CommandLineProcessor.ProcessCommandLine();
                    break;
                }
            }
            catch { }
        }
        yield return null;
    }

    private bool UseUSync()
    {
        return true;
    }

    // Start is called before the first frame update
    private void Start()
    {
        //If we have a NetworkManager instance and we are not listening and m_ConnectionModeButtons is not null then show the connection mode buttons
        if (m_ConnectionModeButtons && m_AuthenticationButtons)
        {
            m_JoinCodeInput.SetActive(false);
            m_AuthenticationButtons.SetActive(false);
            m_ConnectionModeButtons.SetActive(NetworkManager.Singleton && !NetworkManager.Singleton.IsListening);
        }
        
        if (UseUSync())
        {
            //需要在创建或者加入房间之前，设置好玩家信息
            uid = Guid.NewGuid().ToString();
            var props = new Dictionary<string, string>();
            props.Add("icon", "unity");
            NetworkManager.Singleton.GetComponent<RelayTransportNetcode>().SetPlayerData(uid, "Player-" + uid, props);

            //需要在创建或者加入房间之前，配置好回调函数
            var callbacks = new RelayCallbacks();

            callbacks.RegisterConnectToRelayServer(OnConnectToRelayServer);//当连接到 Relay 服务器
            callbacks.RegisterPlayerEnterRoom(OnPlayerEnterRoom);//当有玩家加入房间
            callbacks.RegisterPlayerLeaveRoom(OnPlayerLeaveRoom);//当有玩家离开房间
            callbacks.RegisterMasterClientMigrate(OnMasterClientMigrate);//在退出房间或掉线时会触发
            callbacks.RegisterSetHeartbeat(OnSetHeartBeat);//设置心跳超时时间

            NetworkManager.Singleton.GetComponent<RelayTransportNetcode>().SetCallbacks(callbacks);
        }
    }

    public void OnConnectToRelayServer(uint code, RelayRoom room)
    {
        Debug.Log("OnConnectToRelayServer Called");
        if (code == (uint)RelayCode.OK)
        {
            Debug.LogFormat("Connect To Relay Server Succeed. ( Room : {0} )", room.Name);
        }
        else
        {
            Debug.LogFormat("Connect To Relay Server Failed with Code {0}.", code);
        }
    }
    
    public void OnRoomInfoUpdate(RelayRoom room)
    {
        Debug.Log("OnRoomInfoUpdate Called");
        Debug.LogFormat("Name : {0} ( Custom Properties Count {1} )", room.Name, room.CustomProperties.Count);
        foreach (var item in room.CustomProperties)
        {
            Debug.LogFormat("{0} - {1}", item.Key, item.Value);
        }
    }

    public void OnMasterClientMigrate(uint newMasterClient)
    {
        Debug.Log("OnMasterClientMigrate Called");
        Debug.LogFormat("New Master Client {0}", newMasterClient);
    }
    
    public void OnPlayerEnterRoom(RelayPlayer player)
    {
        Debug.Log("OnPlayerEnterRoom Called");
        Debug.LogFormat("Player {0} Enter Room", player.TransportId);
    }
    
    public void OnPlayerLeaveRoom(RelayPlayer player)
    {
        Debug.Log("OnPlayerLeaveRoom Called");
        Debug.LogFormat("Player {0} Leave Room", player.TransportId);
    }

    public void OnSetHeartBeat(uint code, uint timeout)
    {
        Debug.Log("OnSetHeartBeat Called");
        Debug.LogFormat("Code {0} Timeout {1}", code, timeout);
    }
    
    /// <summary>
    /// Handles starting netcode in server mode
    /// </summary>
    public void OnStartServerButton()
    {
        if (NetworkManager.Singleton && !NetworkManager.Singleton.IsListening && m_ConnectionModeButtons)
        {
            if (UseUSync())
            {
                StartCoroutine(LobbyService.AsyncCreateRoom(new CreateRoomRequest()
                {
                    Name = "Demo",
                    Namespace = "Unity",
                    MaxPlayers = 20,
                    Visibility = LobbyRoomVisibility.Public,
                    OwnerId = uid
                },( resp) =>
                {
                    if ( resp.Code == (uint)RelayCode.OK )
                    {
                        Debug.Log("Create Room succeed.");
                        if (resp.Status == LobbyRoomStatus.ServerAllocated)
                        {
                            NetworkManager.Singleton.GetComponent<RelayTransportNetcode>().SetRoomData(resp);
                            StartServer();
                        }
                        else
                        {
                            Debug.Log("Room Status Exception : " + resp.Status.ToString());
                        }
                    }
                    else
                    {
                        Debug.Log("Create Room Fail By Lobby Service");
                    }
                }));
            }
            else
            {
                StartServer();
            }
        }
    }

    private void StartServer()
    {
        NetworkManager.Singleton.StartServer();
        OnNotifyConnectionEventServer?.Invoke();
        m_ConnectionModeButtons.SetActive(false);
    }

    /// <summary>
    /// Handles starting netcode in host mode
    /// </summary>
    public void OnStartHostButton()
    {
        if (NetworkManager.Singleton && !NetworkManager.Singleton.IsListening && m_ConnectionModeButtons)
        {
            if (UseUSync())
            {
                StartCoroutine(LobbyService.AsyncCreateRoom(new CreateRoomRequest()
                {
                    Name = "Demo",
                    Namespace = "Unity",
                    MaxPlayers = 20,
                    Visibility = LobbyRoomVisibility.Public,
                    OwnerId = uid,
                    CustomProperties = new Dictionary<string, string>()
                    {
                        {"a", "b"},
                    }
                }, ( resp) =>
                {
                    if (resp.Code == (uint)RelayCode.OK)
                    {
                        Debug.Log("Create Room succeed.");
                        if (resp.Status == LobbyRoomStatus.ServerAllocated)
                        {
                            NetworkManager.Singleton.GetComponent<RelayTransportNetcode>().SetRoomData(resp);
                            StartHost();
                        }
                        else
                        {
                            Debug.Log("Room Status Exception : " + resp.Status.ToString());
                        }
                    }
                    else
                    {
                        Debug.Log("Create Room Fail By Lobby Service");
                    }
                }));
            }
            else
            {
                StartHost();
            }
        }
    }

    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        OnNotifyConnectionEventHost?.Invoke();
        m_ConnectionModeButtons.SetActive(false);
    }

    /// <summary>
    /// Handles starting netcode in client mode
    /// </summary>
    public void OnStartClientButton()//以 client 身份加入游戏
    {
        if (NetworkManager.Singleton && !NetworkManager.Singleton.IsListening && m_ConnectionModeButtons)
        {
            if (UseUSync())
            {
                
                StartCoroutine(LobbyService.AsyncListRoom(new ListRoomRequest()
                {
                    Namespace = "Unity",
                    Start = 0,
                    Count = 10,
                }, ( resp) =>
                {
                    if (resp.Code == (uint)RelayCode.OK)
                    {
                        Debug.Log("List Room succeed.");
                        if (resp.Items.Count > 0)
                        {
                            foreach (var item in resp.Items)
                            {
                                if (item.Status == LobbyRoomStatus.Ready)
                                {
                                    StartCoroutine(LobbyService.AsyncQueryRoom(item.RoomUuid,
                                        ( _resp) =>
                                        {
                                            if (_resp.Code == (uint)RelayCode.OK)
                                            {
                                                Debug.Log("Query Room succeed.");
                                                NetworkManager.Singleton.GetComponent<RelayTransportNetcode>()
                                                    .SetRoomData(_resp);
                                                StartClient();
                                            }
                                            else
                                            {
                                                Debug.Log("Query Room Fail By Lobby Service");
                                            }
                                        }));
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("List Room Fail By Lobby Service");
                    }
                }));
            }
            else
            {
                StartClient();
            }
        }
    }
    
    /// <summary>
    /// Handles starting netcode in host mode
    /// </summary>
    public void OnJoinAsHostButton()
    {
        if (NetworkManager.Singleton && !NetworkManager.Singleton.IsListening && m_ConnectionModeButtons)
        {
            if (UseUSync())
            {
                
                StartCoroutine(LobbyService.AsyncListRoom(new ListRoomRequest()
                {
                    Namespace = "Unity",
                    Start = 0,
                    Count = 10,
                }, ( resp) =>
                {
                    if (resp.Code == (uint)RelayCode.OK)
                    {
                        Debug.Log("List Room succeed.");
                        if (resp.Items.Count > 0)
                        {
                            foreach (var item in resp.Items)
                            {
                                if (item.Status == LobbyRoomStatus.Ready)
                                {
                                    StartCoroutine(LobbyService.AsyncQueryRoom(item.RoomUuid,
                                        ( _resp) =>
                                        {
                                            if (_resp.Code == (uint)RelayCode.OK)
                                            {
                                                Debug.Log("Query Room succeed.");
                                                NetworkManager.Singleton.GetComponent<RelayTransportNetcode>()
                                                    .SetRoomData(_resp);
                                                StartHost();
                                            }
                                            else
                                            {
                                                Debug.Log("Query Room Fail By Lobby Service");
                                            }
                                        }));
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("List Room Fail By Lobby Service");
                    }
                }));
            }
            else
            {
                StartHost();
            }
        }
    }

    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        OnNotifyConnectionEventClient?.Invoke();
        m_ConnectionModeButtons.SetActive(false);
    }
    
    public void Reset()
    {
        if (NetworkManager.Singleton && !NetworkManager.Singleton.IsListening && m_ConnectionModeButtons)
        {
            m_ConnectionModeButtons.SetActive(true);
        }
    }
}
