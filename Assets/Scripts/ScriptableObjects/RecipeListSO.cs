using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//脚本，用来管理存储的recipeSO
//[CreateAssetMenu()] 我们只希望有一个recipeSOList，所以为了以防万一，我们不再允许创造这个类型的脚本文件
public class RecipeListSO : ScriptableObject
{
    public List<RecipeSO> recipeSOList;
}
