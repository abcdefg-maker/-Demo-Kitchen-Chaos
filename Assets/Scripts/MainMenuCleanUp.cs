using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MainMenuCleanUp : MonoBehaviour
{
    private void Awake()
    {
        //确保在主菜单场景中，所有的网络相关对象都被销毁
        if (NetworkManager.Singleton != null)
        {
            Destroy(NetworkManager.Singleton.gameObject);
        }

        if (KitchenGameMutiplayer.Instance != null)
        {
            Destroy(KitchenGameMutiplayer.Instance.gameObject);
        }

        if (KitchenGameLobby.Instance != null)
        {
            Destroy(KitchenGameLobby.Instance.gameObject);
        }
    }
}
