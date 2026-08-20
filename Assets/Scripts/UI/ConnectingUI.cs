using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectingUI : MonoBehaviour
{

    private void Start()
    {
        KitchenGameMutiplayer.Instance.OnTryingToJoinGame += KitchenGameMutiplayer_OnTryingToJoinGame;
        KitchenGameMutiplayer.Instance.OnFailedToJoinGame += KitchenGameMutiplayer_OnFailedToJoinGame;

        Hide();
    }

    private void KitchenGameMutiplayer_OnTryingToJoinGame(object sender, System.EventArgs e)
    {
        Show();
    }

    private void KitchenGameMutiplayer_OnFailedToJoinGame(object sender, System.EventArgs e)
    {
        Hide();
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
        KitchenGameMutiplayer.Instance.OnTryingToJoinGame -= KitchenGameMutiplayer_OnTryingToJoinGame;
        KitchenGameMutiplayer.Instance.OnFailedToJoinGame -= KitchenGameMutiplayer_OnFailedToJoinGame;
    }
}
