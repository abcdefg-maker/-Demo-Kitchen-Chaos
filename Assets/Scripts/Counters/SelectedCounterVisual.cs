using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedCounterVisual : MonoBehaviour
{

    [SerializeField] private BaseCounter baseCounter;
    [SerializeField] private GameObject[] visualGameObjectArray;

    private void Start()    //此处必须为Start，否则比Player类的Awake函数先运行的话，Instance == null
                            //为了避免这种问题，有一个技巧：
                            //类内部的初始化都在Awake内进行，对外部变量的访问、赋值都在Start进行。
                            //因为所有的Awake的运行时间一定先于所有的Start
    {
        //因为Instance为静态成员，所以要通过类名进行访问,它属于类，不属于对象

        //网络玩家生成是异步的，难以避免每个玩家生成的时间不同
        //这里设计的逻辑是：
        // 联网里每个客户端只关心自己控制的那个玩家，也就是localInstance，在盯着哪个柜台，所以绑定loacalInstance的事件就可以了
        
        //1. 服务器调用 NetworkManager.SceneManager.LoadScene(...)加载场景
        //2. 服务器针对每个连接的客户端，为每个客户端生成一个玩家对象
        //1 2这两部操作的时间顺序不定，因此需要以下start函数的逻辑来保证事件订阅的正确性
        
        if (Player.LocalInstance != null)//如果玩家已经生成，直接订阅选择柜台的事件
        {
            Player.LocalInstance.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
        }
        else//如果玩家还没生成，先将订阅事件的操作放到玩家生成的事件里
        {
            Player.OnAnyPlayerSpawned += Player_OnAnyPlayerSpawned;
        }
    }

    private void Player_OnAnyPlayerSpawned(object sender, System.EventArgs e) //一个事件处理方法（而不是事件）
                                                                              //事件处理方法的命名规则是：事件源_事件名
    {
       if(Player.LocalInstance != null)//为了防止玩家生成的事件触发多次，导致重复订阅事件
        {
            Player.LocalInstance.OnSelectedCounterChanged -= Player_OnSelectedCounterChanged;
            Player.LocalInstance.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
        }
    }

    private void Player_OnSelectedCounterChanged(object sender, Player.OnSelectedCounterChangedEventArgs e)
    {
       if(e.selectedCounter ==  baseCounter)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        foreach (GameObject visualGameObject in visualGameObjectArray)
        {
            visualGameObject.SetActive(true);
        }
    }

    private void Hide()
    {
        foreach (GameObject visualGameObject in visualGameObjectArray)
        {
            visualGameObject.SetActive(false);
        }
    }
}
