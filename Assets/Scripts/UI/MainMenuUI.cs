using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;



    private void Awake()
    {
        Time.timeScale = 1f; //如果是从Pause状态过来的，那恢复一下时间流逝

        playButton.onClick.AddListener(() =>
        {
            //  ()=> 为Lambda表达式(匿名函数),
            //  ()内为参数表，
            //  =>表示执行后面{}的代码，
            //  {}内放置函数体
            Loader.Load(Loader.Scene.MainScene);
        });

        quitButton.onClick.AddListener(() =>
        {
            #if UNITY_EDITOR
                EditorApplication.isPlaying = false;   // 在 Unity 编辑器内停止播放
            #else
                Application.Quit();                    // 在打包后的游戏中退出
            #endif
        });
    }


}
