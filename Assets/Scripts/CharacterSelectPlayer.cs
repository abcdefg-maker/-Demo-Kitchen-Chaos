using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;      
using UnityEngine.UI;
using Unity.Netcode;             

public class CharacterSelectPlayer : MonoBehaviour
{
    [SerializeField] private int playerIndex;
    [SerializeField] private GameObject readyGameObject;
    [SerializeField] private PlayerVisual playerVisual;
    [SerializeField] private Button kickButton;


    private void Awake()
    {
        kickButton.onClick.AddListener(() => {
            PlayerData playerData = KitchenGameMutiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex); //获取这个索引的玩家数据
            KitchenGameMutiplayer.Instance.KickPlayer(playerData.clientId); //踢掉这个
        });
    }
    private void Start()
    {
        KitchenGameMutiplayer.Instance.OnPlayerDataNetworkListChanged += KitchenGameMutiplayer_OnPlayerDataNetworkListChanged;
        CharacterSelectReady.Instance.OnReadyChanged += CharacterSelectReady_OnReadyChanged;

        kickButton.gameObject.SetActive(NetworkManager.Singleton.IsServer); //只有服务器端显示踢人按钮
        UpdatePlayer();
    }

    private void CharacterSelectReady_OnReadyChanged(object sender, EventArgs e) //玩家准备状态发生变化时，触发这个订阅
    {
        UpdatePlayer();
    }

    private void KitchenGameMutiplayer_OnPlayerDataNetworkListChanged(object sender, EventArgs e) //网络列表发生变化时，触发这个订阅
    {
        UpdatePlayer();
    }


    private void UpdatePlayer()
    {
        if(KitchenGameMutiplayer.Instance.IsPlayerIndexConnected(playerIndex)) //如果这个索引的玩家已连接
        {
            Show();

            PlayerData playerData = KitchenGameMutiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex); //获取这个索引的玩家数据
            
            readyGameObject.SetActive(CharacterSelectReady.Instance.IsPlayerReady(playerData.clientId)); //根据这个玩家的准备状态，显示或隐藏ready图标

            playerVisual.SetPlayerColor(KitchenGameMutiplayer.Instance.GetPlayerColor(playerData.colorId)); //设置玩家的颜色
        }
        else
        {
            Hide();
        }
    }
    private void Show()
    {
        gameObject.SetActive(true);
    }   

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if(KitchenGameMutiplayer.Instance != null)
        {
            KitchenGameMutiplayer.Instance.OnPlayerDataNetworkListChanged -= KitchenGameMutiplayer_OnPlayerDataNetworkListChanged;
        }

    }
}
