using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class DeliveryManager : NetworkBehaviour 
    //存储顾客等待的菜单
{
    public event EventHandler OnRecipeSpawned;      //产生recipe的时候，控制ui显示的事件
    public event EventHandler OnRecipeCompleted;    //完成recipe的时候，控制ui消失的事件
    public event EventHandler OnRecipeSuccess;      //成功完成recipe的时候，播放成功音效的事件
    public event EventHandler OnRecipeFailed;       //没能完成recipe的时候，播放失败音效的事件


    public static DeliveryManager Instance { get; private set; }
   
    
    [SerializeField] private RecipeListSO recipeListSO;

    private List<RecipeSO> waitingRecipeSOList;
    private float spawnRecipeTimer;
    private float spawnRecipeTimerMax = 4f;
    private int waitingRecipesMax = 4;
    private int successfulRecipeAmount = 0;

    private void Awake()
    {
        Instance = this;
        waitingRecipeSOList = new List<RecipeSO>();
    }
    private void Update()
    {
        //只在server端运行，server端负责产生新的recipeSO，并通过ClientRpc传给所有客户端
        if (!IsServer)
        {
            return;
        }
        

        //定期产生订单recipe
        spawnRecipeTimer -= Time.deltaTime;
        if(spawnRecipeTimer <= 0f)
        {
            spawnRecipeTimer = spawnRecipeTimerMax;

            if (KitchenGameManager.Instance.IsGamePlaying() && waitingRecipeSOList.Count < waitingRecipesMax)
            {
                int waitingRecipeSOIndex = UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count);
                //recipeSOList是RecipeListSO类内的一个list，list类内部存储着所有的recipeSO
                SpawnNewWaitingRecipeClientRpc(waitingRecipeSOIndex);
            }
        }
    }

    //这里考虑到游戏性质（和作者偷懒hhh），没有使用网络变量来同步菜单，所以如果玩家进入游戏时间不同，可能会导致菜单不同步，玩家看到的菜单可能不一样
    //后续通过控制玩家同时进入游戏来解决这个问题

    //只在server端调用，server端负责产生新的recipeSO，并通过ClientRpc传给所有客户端
    [ClientRpc]//代码在所有客户端上运行，服务器将消息广播给客户端
    private void SpawnNewWaitingRecipeClientRpc(int waitingRecipeSOIndex)
    {
        RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[waitingRecipeSOIndex];

        waitingRecipeSOList.Add(waitingRecipeSO);

        OnRecipeSpawned?.Invoke(this,EventArgs.Empty);
    }

    /// <summary>
    /// 负责检查传送带接收的菜品，检查菜品是否合格
    /// </summary>
    /// <param name="plateKitchenObject"></param>
    public void DeliveryRecipe(PlateKitchenObject plateKitchenObject)
                                                                      //这里似乎要用到3重循环，
                                                                      //那这部分就是这个项目目前时间复杂度最高的函数了hhh
                                                                      //2025.11.29 课程第七个小时
    {
        for(int i = 0; i < waitingRecipeSOList.Count; i++)//随便传waitingRecipeSOList内的任何一个recipe，
                                                          //只要有一个能够匹配，就算通过
        {
            RecipeSO waitingRecipeSO = waitingRecipeSOList[i];

            if(waitingRecipeSO.kitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count)
            {
                bool plateContentsMatchesRecipe = true; 

                //如果两个配方的ingredient数量相同
                foreach(KitchenObjectSO recipeKitchenObejctSO in waitingRecipeSO.kitchenObjectSOList)
                {
                    bool ingredientFound = false;
                    //遍历 配方内的每一个元素
                    foreach(KitchenObjectSO plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList())
                    {
                        //遍历 提交的盘子内的每一个元素
                        if(plateKitchenObjectSO == recipeKitchenObejctSO)
                        {
                            //匹配成功
                            ingredientFound = true;
                            break;
                        }
                    }
                    if (!ingredientFound)
                    {
                        //有元素没有匹配成功
                        plateContentsMatchesRecipe = false;
                    }
                }
                if (plateContentsMatchesRecipe)
                {
                    //两个recipe的元素个数相同 且 所有元素都匹配成功了
                    DeliverCorrectRecipeServerRpc(i);
                    return;
                }
            }
        }
        //没有匹配成功的选项
        //玩家提交的recipe有问题
        DeliverRecipeIncorrectServerRpc();
        return;
    }

    //关于下面这段嵌套关系的解释：
    //本地匹配成功
    //→ ServerRpc（上报给服务器，因为客户端无权广播）
    //→ ClientRpc（服务器广播给所有人，大家一起删）

    //提交成功的RPC网络同步逻辑
    //代码在服务器上运行，客户端调用给服务器同步状态
    [ServerRpc(RequireOwnership = false)]//RequireOwnership = false的意思是，
                                         //客户端可以调用ServerRpc函数，而不需要拥有这个对象的所有权
    public void DeliverCorrectRecipeServerRpc(int waitingRecipeSOIndex) 
    {
        DeliverCorrectRecipeClientRpc(waitingRecipeSOIndex);
    }
    [ClientRpc]
    private void DeliverCorrectRecipeClientRpc(int waitingRecipeSOIndex)
    {
        waitingRecipeSOList.RemoveAt(waitingRecipeSOIndex); //把提交成功的菜品从等待名单中去掉
        successfulRecipeAmount++;
        OnRecipeCompleted?.Invoke(this,EventArgs.Empty);  
        OnRecipeSuccess?.Invoke(this,EventArgs.Empty); //播放成功音效
    }

    //提交失败的RPC网络同步逻辑
    [ServerRpc(RequireOwnership = false)]
    private void DeliverRecipeIncorrectServerRpc()
    {
        DeliverRecipeFailedClientRpc();
    }
    [ClientRpc]
    private void DeliverRecipeFailedClientRpc()
    {
        OnRecipeFailed?.Invoke(this,EventArgs.Empty); //播放失败音效
    }

    public List<RecipeSO> GetWaitingRecipeSOList()
    {
        return waitingRecipeSOList;
    }

    public int GetSuccessfulRecipesAmount()
    {
        return successfulRecipeAmount;
    }
}
