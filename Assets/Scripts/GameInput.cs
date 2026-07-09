using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    private const string PLAYER_PRESS_BINDINGS = "InputBindings";
    public static GameInput Instance {  get; private set; }

    public event EventHandler OnInteractAction;//通常以On开头命名
    public event EventHandler OnInteractAlternateAction;
    public event EventHandler OnPauseAction;
    public event EventHandler OnBindingRebind;
    
    
    public enum Binding //按键绑定 的 枚举
    {
        Move_Up,
        Move_Down,
        Move_Left,
        Move_Right,
        Interact,
        InteractAlternate,
        Pause,
    }

    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        Instance = this;

        playerInputActions = new PlayerInputActions();

        if (PlayerPrefs.HasKey(PLAYER_PRESS_BINDINGS)) //载入（最新版的->如果修改过）输入按键
        {
            playerInputActions.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PLAYER_PRESS_BINDINGS));
        }

        playerInputActions.Player.Enable();

        playerInputActions.Player.Interact.performed += Interact_performed;
        //当玩家触发 Interact 动作的 performed 阶段时，调用你的方法 Interact_performed
        //Interact是我在unity的playerInputAction内设置的一个动作
        //在 Input System 中，动作有三种典型“阶段”(Callback phases)——其本质是三个事件（Event）:
        //started	    用户刚开始按键
        //performed     动作确认完成（例如按键按下时）
        //canceled      松开、取消
        //performed事件，用+=来分配一个监听者（这里是监听函数Interact_performed）
        //
        //运行顺序：
        //Interact（动作）触发（比如这里是按E键） -> performed（事件）被触发
        //-> 调用委托列表里的所有监听者（如 Interact_performed 方法）
        playerInputActions.Player.InteractAlternate.performed += InteractAlternate_performed;
        playerInputActions.Player.Pause.performed += Pause_performed;


    }

    private void OnDestroy()    //用来解决PlayerActionInput的销毁不同步问题（仍需细究）
    {
        playerInputActions.Player.Interact.performed -= Interact_performed;
        playerInputActions.Player.InteractAlternate.performed -= InteractAlternate_performed;
        playerInputActions.Player.Pause.performed -= Pause_performed;

        playerInputActions.Dispose();
    }

    private void Pause_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnPauseAction?.Invoke(this,EventArgs.Empty);     
    }

    private void InteractAlternate_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnInteractAlternateAction?.Invoke(this, EventArgs.Empty);
    }

    private void Interact_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)//+=后按“Tab”自动生成的事件委托（delegate）
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);//和以下代码功能相同
        //if (OnInteractAction != null)  
        //{
        //    OnInteractAction(this, EventArgs.Empty);
        //}
    }

    public Vector2 GetMovementVectorNormalized()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        //用以上Input System的组件，来代替自己实现WASD的输入 
        /*原实现模式：
         * 
         * 
        ////输入速度
        ////由于输入只有两个维度，所以这里用Vector3是不合适的
        //Vector2 inputVector = new Vector2(0, 0);
        //if (Input.GetKey(KeyCode.W))
        //{
        //    inputVector.y += 1;
        //}
        //if (Input.GetKey(KeyCode.A))
        //{
        //    inputVector.x -= 1;
        //}
        //if (Input.GetKey(KeyCode.S))
        //{
        //    inputVector.y -= 1;
        //}
        //if (Input.GetKey(KeyCode.D))
        //{
        //    inputVector.x += 1;
        //}

        */
        //速度赋值
        inputVector = inputVector.normalized; //归一化移动速度，对角线时候不会跑的更快

        return inputVector;
    }

    public string GetBindingText(Binding binding) //从行为绑定的按键，获得绑定按键的text，用于显示
    {
        switch(binding)
        {
            default:
            case Binding.Interact:
                return playerInputActions.Player.Interact.bindings[0].ToDisplayString(); //bings数组内存放所有绑定的按键
            case Binding.InteractAlternate:
                return playerInputActions.Player.InteractAlternate.bindings[0].ToDisplayString();
            case Binding.Pause:
                return playerInputActions.Player.Pause.bindings[0].ToDisplayString();
            case Binding.Move_Up:
                return playerInputActions.Player.Move.bindings[1].ToDisplayString();
            case Binding.Move_Down:
                return playerInputActions.Player.Move.bindings[2].ToDisplayString();
            case Binding.Move_Left:
                return playerInputActions.Player.Move.bindings[3].ToDisplayString();
            case Binding.Move_Right:
                return playerInputActions.Player.Move.bindings[4].ToDisplayString();
        }
    }

    public void RebingBinding(Binding binding, Action onActionRebound) //重新绑定按键的函数
    {
        playerInputActions.Player.Disable(); //首先，在切换的过程中，禁止调用这个input系统

        InputAction inputAction = null;
        int bindingIndex = 0;

        switch (binding)
        {
            case Binding.Interact:
                inputAction = playerInputActions.Player.Interact;
                bindingIndex = 0;
                break;
            case Binding.InteractAlternate:
                inputAction = playerInputActions.Player.InteractAlternate;
                bindingIndex = 0;
                break;
            case Binding.Pause:
                inputAction = playerInputActions.Player.Pause;
                bindingIndex = 0;
                break;
            case Binding.Move_Up:
                inputAction = playerInputActions.Player.Move;
                bindingIndex = 1;
                break;
            case Binding.Move_Down:
                inputAction = playerInputActions.Player.Move;
                bindingIndex = 2;
                break;
            case Binding.Move_Left:
                inputAction = playerInputActions.Player.Move;
                bindingIndex = 3;
                break;
            case Binding.Move_Right:
                inputAction = playerInputActions.Player.Move;
                bindingIndex = 4;
                break;
        }

        inputAction.PerformInteractiveRebinding(bindingIndex)
            .OnComplete(callback =>
            {
                callback.Dispose();
                playerInputActions.Player.Enable();
                onActionRebound();

                PlayerPrefs.SetString(PLAYER_PRESS_BINDINGS, playerInputActions.SaveBindingOverridesAsJson()); //保存按键设置
                PlayerPrefs.Save();


                OnBindingRebind?.Invoke(this, EventArgs.Empty);
            })
            .Start();
    }

}
