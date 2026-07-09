using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI recipesDeliveriedText;

    private void Start()
    {
        KitchenGameManager.Instance.OnStateChanged += KitchenGameManager_OnStateChanged;
    }

    private void KitchenGameManager_OnStateChanged(object sender, System.EventArgs e) //控制是否显示倒计时的监听函数
    {
        if (KitchenGameManager.Instance.IsGameOver())
        {
            Show();
            recipesDeliveriedText.text = DeliveryManager.Instance.GetSuccessfulRecipesAmount().ToString();

        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }

    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }

}
