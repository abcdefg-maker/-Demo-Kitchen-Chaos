using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetStaticDataManager : MonoBehaviour //由于静态对象在切换场景时候不会自动销毁，可能导致重复进入游戏的时候产生bug
                                                    //因此设置此类来进行静态数据管理
{
    private void Awake()
    {
        CuttingCounter.ResetStaticData();
        BaseCounter.ResetStaticData();
        TrashCounter.ResetStaticData();
    }
}
