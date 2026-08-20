using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public static class Loader //静态类的所有成员都必须是静态的
{
    public enum Scene
    {
        MainMenuScene,
        MainScene,
        LoadingScene,
        LobbyScene,
        CharacterSelectScene, 
    }
    private static Scene targetScene;

    public static void Load(Scene targetScene) //单机版本的加载场景逻辑
    {
        Loader.targetScene = targetScene;

        SceneManager.LoadScene(Scene.LoadingScene.ToString());


    } 

    public static void LoadNetwork(Scene targetScene)//联网版本的加载场景逻辑
    {
        //由主机 / 服务器调用后，会通知所有已连接的客户端同时切到这个场景
        //客户端会自动同步加载，保证大家在同一个场景里，网络对象（NetworkObject）也会正确地在新场景中生成/迁移
        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
    }

    public static void LoaderCallback()
    {
        SceneManager.LoadScene(targetScene.ToString());
    }
}
