using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//脚本,用于存储应该提交的订单的食谱
[CreateAssetMenu()]
public class RecipeSO : ScriptableObject
{
    public List<KitchenObjectSO> kitchenObjectSOList;
    public string recipeName;
}
