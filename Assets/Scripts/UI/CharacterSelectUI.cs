using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class CharacterSelectUI : MonoBehaviour
{
    [SerializeField] private Button MainMenuButton;
    [SerializeField] private Button ReadyButton;


    private void Awake()
    {
        MainMenuButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown(); //关闭网络管理器
            Loader.Load(Loader.Scene.MainMenuScene);
        });

        ReadyButton.onClick.AddListener(() =>
        {
            CharacterSelectReady.Instance.SetPlayerReady(); //通知服务器：本玩家已准备好，服务器据此判断是否所有人都ready
        });
    }
}
