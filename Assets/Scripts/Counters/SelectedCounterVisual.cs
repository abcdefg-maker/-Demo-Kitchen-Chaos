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
        Player.Instance.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
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
