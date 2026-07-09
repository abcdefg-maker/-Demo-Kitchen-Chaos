using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliverManagerUI : MonoBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private Transform recipeTemplate;

    private void Awake()
    {
        recipeTemplate.gameObject.SetActive(false); //隐藏这个模板
    }

    private void Start()
    {
        DeliveryManager.Instance.OnRecipeSpawned += Delivery_OnRecipeSpawned;
        DeliveryManager.Instance.OnRecipeCompleted += Delivery_OnRecipeCompleted;

        UpdateVisual(); //确保初始模板的recipeTemplate不要显示出来
    }

    private void Delivery_OnRecipeSpawned(object sender, System.EventArgs e)
    {
        UpdateVisual();
    }

    private void Delivery_OnRecipeCompleted(object sender, System.EventArgs e)
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        foreach (Transform child in container)
        {
            if (child == recipeTemplate) continue;//不删除模板
            Destroy(child.gameObject); //在添加新的图标之前，要把上一次的图标去掉（除了模板，否则script内的组件就为null了）
        }
        
        foreach (RecipeSO recipeSO in DeliveryManager.Instance.GetWaitingRecipeSOList())
        {
            Transform recipeTransform = Instantiate(recipeTemplate, container);
            recipeTransform.gameObject.SetActive(true);   //把除了模板外，新生成的icons设为可见
            recipeTransform.GetComponent<DeliverManagerSingleUI>().SetRecipeSO(recipeSO);
        }
    }
}
