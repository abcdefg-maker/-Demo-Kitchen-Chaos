using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class KitchenObjectListSO : ScriptableObject //为了方便RPC传参数（用int型序号代替直接传KitchenObjetcSO类型），创建的SOList
{
    public List<KitchenObjectSO> kitchenObjectSOList;
}
