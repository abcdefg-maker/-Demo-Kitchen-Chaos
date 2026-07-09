using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private enum Mode
    {
        LookAt,
        LookAtInverted,
        CameraForward,
        CameraForwardInverted
    }

    [SerializeField] private Mode mode;

    private void LateUpdate()
    {
        switch (mode)
        {
            case Mode.LookAt:
                transform.LookAt(Camera.main.transform); break; //以前的教程说不要用Camera.main，因为之前每调用这个变量，
                                                                //都要在unity遍历所有tranform来找到Camera.main，很损失性能
                                                                //但是现在在unity内有Camera.main的缓存了，所有不存在这个问题了
            case Mode.LookAtInverted:
                Vector3 dirFromCamera = transform.position - Camera.main.transform.position;
                transform.LookAt(dirFromCamera + transform.position);
                break;
            case Mode.CameraForward:
                transform.forward = Camera.main.transform.forward; break;
            case Mode.CameraForwardInverted:
                transform.forward = -Camera.main.transform.forward; break;   
        }
    }
}
