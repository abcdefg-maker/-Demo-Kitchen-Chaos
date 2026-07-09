using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : MonoBehaviour
{
    [SerializeField] private GameObject hasProgressGameObject;
    [SerializeField] private Image barImage;


    private IHasProgress hasProgress;
    private void Start()
    {
        hasProgress = hasProgressGameObject.GetComponent<IHasProgress>(); //这样实现是因为unity的inspector内不会显示interface
                                                                          //所以不能用序列化的方式初始化接口变量
        if(hasProgress == null )
        {
            Debug.LogError(hasProgressGameObject + "没有一个IHasProgreess接口类型的Component!");
        }

        hasProgress.OnProgressChanged += HasProgress_OnProgressChanged;
    
        barImage.fillAmount = 0f;

        Hide();
    }

    private void HasProgress_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        barImage.fillAmount = e.progressNormalized;

        if(e.progressNormalized == 0f || e.progressNormalized ==1f) //如果一点没切或者切完了，隐藏Bar
        {
            Hide();
        }
        else  //显示Bar 
        {
            Show();
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
