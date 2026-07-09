using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//C#脚本
//它定义了一个 ScriptableObject 数据资源类型 —— KitchenObjectSO。
//这个类不是 MonoBehaviour，而是 用于存储数据的资源对象
//Unity 会根据这个类，在 Project 面板里生成一个.asset 文件，
//里面保存食物、道具等的静态数据。

[CreateAssetMenu()] //这是一个 Unity 的特性（Attribute），作用是：
                    //在 Unity 的 Assets → Create 菜单里添加创建该 ScriptableObject 的选项。
                    //可以生成.asset文件，用于存储你定义的以下信息e.g. prefabs sprites objectNames...
public class KitchenObjectSO : ScriptableObject //注意这里不是Monobehavior
{

    public Transform prefabs;
    public Sprite sprite;
    public string objectName;
}
