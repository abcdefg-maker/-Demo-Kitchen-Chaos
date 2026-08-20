using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class ConnectRespinseMessageUI : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI messageText;

    [SerializeField] private Button closeButton;

    private void Awake()
    {
        closeButton.onClick.AddListener(Hide);
    }

    private void Start()
    {
        KitchenGameMutiplayer.Instance.OnFailedToJoinGame += KitchenGameMutiplayer_OnFailedToJoinGame;

        Hide();
    }
    private void KitchenGameMutiplayer_OnFailedToJoinGame(object sender, System.EventArgs e)
    {
        Show();

        messageText.text = NetworkManager.Singleton.DisconnectReason;

        if (string.IsNullOrEmpty(messageText.text))
        {
            messageText.text = "Failed to connect.";
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
        KitchenGameMutiplayer.Instance.OnFailedToJoinGame -= KitchenGameMutiplayer_OnFailedToJoinGame;
    }
}
