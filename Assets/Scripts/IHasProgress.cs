using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//C#接口，为所有有过程的东西，制作进度条
public interface IHasProgress
{
    public event EventHandler<OnProgressChangedEventArgs> OnProgressChanged; //控制进度条的事件
    public class OnProgressChangedEventArgs : EventArgs
    {
        public float progressNormalized;
    }
}
