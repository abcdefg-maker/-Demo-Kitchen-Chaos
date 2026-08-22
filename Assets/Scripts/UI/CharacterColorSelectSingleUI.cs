using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterColorSelectSingleUI : MonoBehaviour
{
    [SerializeField] private int colorId;
    [SerializeField] private Image image;
    [SerializeField] private GameObject selectedGameObject;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => {
            KitchenGameMutiplayer.Instance.ChangePlayerColor(colorId); //设置玩家颜色
        });
    }
    private void Start()
    {
        KitchenGameMutiplayer.Instance.OnPlayerDataNetworkListChanged += KitchenGameMutiplayer_OnPlayerDataNetworkListChanged;
        image.color = KitchenGameMutiplayer.Instance.GetPlayerColor(colorId); //设置图片的颜色为玩家颜色
        UpdateIsSelected(); 
    }

    private void KitchenGameMutiplayer_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e) 
    {
        UpdateIsSelected();
    }
    private void UpdateIsSelected()
    {
        if(KitchenGameMutiplayer.Instance.GetPlayerData().colorId == colorId) //如果当前玩家的颜色id等于这个颜色id，说明这个颜色已被选中
        {
            selectedGameObject.SetActive(true); //显示选中图标
        }
        else
        {
            selectedGameObject.SetActive(false); //隐藏选中图标
        }
    }

    private void OnDestroy()
    {
        KitchenGameMutiplayer.Instance.OnPlayerDataNetworkListChanged -= KitchenGameMutiplayer_OnPlayerDataNetworkListChanged;
    }
}
