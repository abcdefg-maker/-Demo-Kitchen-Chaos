using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour   //正常情况下，画面、音效、逻辑，这三个应该分别在三个类内实现
                                            //这样来降低各个类之间的耦合度，提高代码的可扩展性
                                            //这也是为什么我们几乎所有的音效都是用事件来出发，并且只在SoundManager内进行管理的
                                            //但是在这个类内，我们希望尝试一种其他的方法来实现音效，并且这个类将只处理音效
                                            //所以这样实现算是无伤大雅
{
    private Player player;
    private float footstepTimer;
    private float footstepTimerMax = .1f;


    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void Update()
    {
        footstepTimer -= Time.deltaTime;
        if (footstepTimer < 0f)
        {
            footstepTimer = footstepTimerMax;
            if (player.IsWalking())
            {
                float footstepsVolume = 1f;
                SoundManager.Instance.PlayFootstepsSound(player.transform.position, footstepsVolume);
            }
        }
    }


}
