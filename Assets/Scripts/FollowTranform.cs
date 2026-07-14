using System.Collections;
using System.Collections.Generic;
using UnityEngine;




/// <summary>
/// 因为networkObject无法静态嵌套（例如prefab内父子都为networkObject）
/// 只能Spawn之后由服务器动态生成reparent
/// </summary>
public class FollowTranform : MonoBehaviour
{
    private Transform targetTransform;

    public void SetTarget(Transform targetTransform)
    {
        this.targetTransform = targetTransform;
    }

    private void LateUpdate()
    {
        if (targetTransform == null)
        {
            return;
        }
        transform.position = targetTransform.position;
        transform.rotation = targetTransform.rotation;
    
    }
}
