using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour 
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
        //定期产生订单recipe
        spawnRecipeTimer -= Time.deltaTime;
        if(spawnRecipeTimer <= 0f)
        {
            spawnRecipeTimer = spawnRecipeTimerMax;

            if (KitchenGameManager.Instance.IsGamePlaying() && waitingRecipeSOList.Count < waitingRecipesMax)
            {
                RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count)];
                //recipeSOList是RecipeListSO类内的一个list，list类内部存储着所有的recipeSO
                waitingRecipeSOList.Add(waitingRecipeSO);

                OnRecipeSpawned?.Invoke(this,EventArgs.Empty);
            }
        }
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
                    waitingRecipeSOList.RemoveAt(i); //把提交成功的菜品从等待名单中去掉
                    successfulRecipeAmount++;
                    OnRecipeCompleted?.Invoke(this,EventArgs.Empty);  
                    OnRecipeSuccess?.Invoke(this,EventArgs.Empty); //播放成功音效
                    return;
                }
            }
        }
        //没有匹配成功的选项
        //玩家提交的recipe有问题
        OnRecipeFailed?.Invoke(this,EventArgs.Empty); //播放失败音效
        return;
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
