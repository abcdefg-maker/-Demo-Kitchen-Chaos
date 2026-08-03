using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class KitchenGameManager : NetworkBehaviour
{
    public static KitchenGameManager Instance {  get; private set; }



    public event EventHandler OnStateChanged;   //管理游戏开始状态变化 的事件
    public event EventHandler OnGamePaused;     //管理游戏暂停的事件（下同）
    public event EventHandler OnGameUnpaused;
    public event EventHandler OnLocalPlayerReadyChanged;

    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToStart);  //游戏状态的网络变量 
    private bool isLocalPlayerReady;
    //private float waitingToStartTimer = 1f;
    private NetworkVariable<float> countdownToStartTimer = new NetworkVariable<float>(3f);
    private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>(0f);
    private float gamePlayingTimerMax = 90f;
    private bool isGamePaused = false;
    private Dictionary<ulong, bool> playerReadyDictionary;

    private void Awake()
    {
        Instance = this;
        state.Value = State.WaitingToStart;

        playerReadyDictionary = new Dictionary<ulong, bool>();
    }
    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;

    }

    public override void OnNetworkSpawn()
    {
       state.OnValueChanged += State_OnValueChanged;
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
         OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        
        if(state.Value == State.WaitingToStart)
        {

            isLocalPlayerReady = true;
            OnLocalPlayerReadyChanged?.Invoke(this,EventArgs.Empty);

            SetPlayerReadyServerRpc();   //通知服务器：本玩家已准备好，服务器据此判断是否所有人都ready
        }
    }

    //一个结构体参数。当客户端调用 SetPlayerReadyServerRpc() 时，
    // Netcode 会在服务器端自动往里面塞入调用方的信息
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
            state.Value = State.CountdownToStart;
        }

    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        TogglePauseGame();
    }

    private void Update()
    {
        if(!IsServer)
        {
            return;
        }

        switch(state.Value)
        {
            case State.WaitingToStart:
                
                break;
            case State.CountdownToStart:
                countdownToStartTimer.Value -= Time.deltaTime;
                if (countdownToStartTimer.Value < 0f)
                {
                    state.Value = State.GamePlaying;
                    gamePlayingTimer.Value = gamePlayingTimerMax;
                   
                }
                break;
            case State.GamePlaying:
                gamePlayingTimer.Value -= Time.deltaTime;
                if (gamePlayingTimer.Value < 0f)
                {
                    state.Value = State.GameOver;
                }
                break;
            case State.GameOver:
                break;
        }

    }

    public  bool IsGamePlaying()
    {
        //判断游戏是否在GamePlaying状态
        return state.Value == State.GamePlaying;
    }

    public bool IsCountDownToStartActive()
    {
        return state.Value == State.CountdownToStart;
    }

    public float GetCountDownToStartTimer()
    {
        return countdownToStartTimer.Value;
    }

    public bool IsGameOver()
    {
        return state.Value == State.GameOver;
    }

    public bool IsLoaclPlayerReady()
    {
        return isLocalPlayerReady;
    }

    public float GetGamePlayingTimerNormalized()
    {
        return 1 - gamePlayingTimer.Value / gamePlayingTimerMax;
    }

    public void TogglePauseGame()
    {
        isGamePaused = !isGamePaused;
        if (isGamePaused) 
        {
            Time.timeScale = 0f;    //由于游戏内的诸多逻辑都是通过Time.deltaTime实现的
                                    //这样控制时间流速为0，以实现对deltaTime的控制，也就实现了游戏暂停的效果 
            OnGamePaused?.Invoke(this,EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1f;
            OnGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }
}
