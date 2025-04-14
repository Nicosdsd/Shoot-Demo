using System;
using HighlightPlus;
using UnityEngine;
using UnityEngine.UI;

public class SpawnerManager : MonoBehaviour
{
    [Serializable]
    public class SpawnStage
    {
        public string stageName;           // 当前阶段的名称 
        public float startTime;            // 该阶段开始的时间（相对于游戏开始的秒数）
        public float spawnInterval;        // 敌人生成的时间间隔 
        public int spawnAmount;            // 每次生成的敌人数量
        public EnemyType[] enemyTypes;     // 该阶段中可用的敌人种类
    }

    [Serializable]
    public class EnemyType
    {
        public string name;                   // 敌人类型的名称
        public GameObject[] enemyPrefabs;     // 当前类型敌人的预制体数组
        public float spawnChance;             // 敌人生成的概率
    }

    [Header("阶段设置")]
    public SpawnStage[] spawnStages;

    [Header("Boss 设置")]
    public GameObject[] bossPrefabs;       // Boss预制体数组
    public float bossSpawnInterval = 300f; // Boss生成的时间间隔（以秒为单位）
    public Vector3 bossSpawnOffset = new Vector3(0, 5, 0); // Boss生成时相对于中心点的偏移

    [Header("生成区域设置")]
    public Vector3 enemySpawnAreaDimensions; // 敌人生成区域的大小
    public Vector3 enemySpawnAreaOffset;     // 敌人生成区域的偏移
    public Transform center;                 // 中心点参考（比如玩家的位置）
    public float exclusionZoneRadius;        // 排除的安全区域半径（生成时不会靠近此区域）

    [Header("预警设置")]
    public GameObject spawnAlertPrefab;      // 生成提示效果的预制件

    [Header("生成限制")]
    public Transform enemyParent;           // 用于存放所有敌人的父物体
    public int maxEnemies;                  // 最大敌人数量限制

    [Header("UI 设置")]
    public Text timerText;                  // UI组件，用于显示游戏计时器
    public GameObject stageStartUI;         // 阶段开始的UI提示物体
    public Text stageStartText;             // 显示阶段开始信息的文本组件

    private bool isFirstStage = true;       // 标志是否是第一阶段

    private float gameStartTime;            // 游戏开始的时间戳
    private SpawnStage currentStage;        // 当前活跃的阶段

    private void Start()
    {
        gameStartTime = Time.time;          // 记录游戏开始时间
        UpdateCurrentStage();               // 初始化并更新当前阶段

        // 如果设置了最大敌人数量，则启动定期生成敌人的逻辑
        if (maxEnemies > 0)
        {
            InvokeRepeating(nameof(SpawnEnemies), 0, currentStage.spawnInterval);
        }

        // 启动Boss生成的协程
        StartCoroutine(SpawnBosses());
    }

    private void Update()
    {
        UpdateCurrentStage();  // 根据游戏进行时间更新当前阶段
        UpdateTimer();         // 更新计时器并显示在UI上
    }
    
    // 更新游戏计时器，并将其显示在屏幕上的UI文本上
    private void UpdateTimer()
    {
        float elapsedTime = Time.time - gameStartTime;
        // 将时间格式化为 "分钟:秒" 的形式
        string formattedTime = string.Format("{0:D2}:{1:D2}", (int)(elapsedTime) / 60, (int)(elapsedTime) % 60);
        if (timerText != null)
        {
            timerText.text = formattedTime;
        }
    }

    // 根据当前时间更新所处阶段，并切换到合适的阶段逻辑 
    private void UpdateCurrentStage()
    {
        float elapsedTime = Time.time - gameStartTime;
        SpawnStage nextStage = null;

        foreach (var stage in spawnStages)
        {
            if (elapsedTime >= stage.startTime)
            {
                if (nextStage == null || stage.startTime > nextStage.startTime)
                {
                    nextStage = stage;
                }
            }
        }

        if (nextStage != null && nextStage != currentStage)
        {
            currentStage = nextStage;
            CancelInvoke(nameof(SpawnEnemies)); // 停止之前阶段的敌人生成逻辑
            if (maxEnemies > 0)
            {
                InvokeRepeating(nameof(SpawnEnemies), 0, currentStage.spawnInterval);
            }
            Debug.Log($"切换到阶段: {currentStage.stageName}" + currentStage);
            
            // 如果不是第一波，则显示阶段开始预警UI
            if (!isFirstStage)
            {
                stageStartUI.SetActive(true);
                stageStartText.text = currentStage.stageName;
                Invoke("HideStageStartUI", 2f); // 延迟隐藏UI
            }

            isFirstStage = false; // 第一波完成后标记为false
        }
    }
    
   // 隐藏阶段开始的UI提示 
   private void HideStageStartUI()
   {
       stageStartUI.SetActive(false);
   }

   // 定期生成普通敌人的逻辑
   private void SpawnEnemies()
   {
       if (maxEnemies <= 0) return;

       int availableSlots = maxEnemies - enemyParent.childCount; // 剩余可用空间
       if (availableSlots <= 0) return;

       int enemiesToSpawn = Mathf.Min(currentStage.spawnAmount, availableSlots);

       for (int i = 0; i < enemiesToSpawn; i++)
       {
           StartCoroutine(SpawnWithAlert());
       }
       
    
   }

   // 带生存预警效果的敌人生成逻辑（协程）
   private System.Collections.IEnumerator SpawnWithAlert()
   {
       Vector3 randomPosition;
       do
       {
           randomPosition = GetRandomPositionInEnemyArea();
       } while (IsInsideExclusionZone(randomPosition)); // 确保随机位置不在安全区域内

       GameObject alertObject = Instantiate(spawnAlertPrefab, randomPosition, Quaternion.identity);

       yield return new WaitForSeconds(1f);  // 等待1秒后销毁预警并生成敌人

       Destroy(alertObject);
       
       //GetComponent<HighlightEffect>().Refresh();
       GameObject selectedEnemy = GetRandomEnemyPrefabForCurrentStage();
       if (selectedEnemy != null)
       {
           Instantiate(selectedEnemy, randomPosition, Quaternion.identity, enemyParent);
       }
   }

   // 获取敌人区域内的一个随机位置，且满足条件（不在安全区范围内）
   private Vector3 GetRandomPositionInEnemyArea()
   {
       float halfWidth = enemySpawnAreaDimensions.x * 0.5f;
       float halfHeight = enemySpawnAreaDimensions.y * 0.5f;
       float halfDepth = enemySpawnAreaDimensions.z * 0.5f;

       Vector3 centerPosition = center.position + enemySpawnAreaOffset;

       float randomX = UnityEngine.Random.Range(centerPosition.x - halfWidth, centerPosition.x + halfWidth);
       float randomY = UnityEngine.Random.Range(centerPosition.y - halfHeight, centerPosition.y + halfHeight);
       float randomZ = UnityEngine.Random.Range(centerPosition.z - halfDepth, centerPosition.z + halfDepth);

       return new Vector3(randomX, randomY, randomZ);
   }

   // 判断某个位置是否在安全区域内
   private bool IsInsideExclusionZone(Vector3 position)
   {
       return Vector3.Distance(position, center.position) < exclusionZoneRadius;
   }

   // 随机获取当前阶段中的一个敌人Prefab，基于概率和种类匹配选择
   private GameObject GetRandomEnemyPrefabForCurrentStage()
   {
       foreach (var enemyType in currentStage.enemyTypes)
       {
           if (UnityEngine.Random.value <= enemyType.spawnChance && enemyType.enemyPrefabs.Length > 0)
           {
               return enemyType.enemyPrefabs[UnityEngine.Random.Range(0, enemyType.enemyPrefabs.Length)];
           }
       }

       return null;
   }

   // Boss生成逻辑（协程，每隔一定时间执行一次）
   private System.Collections.IEnumerator SpawnBosses()
   {
       while (true)
       {
           yield return new WaitForSeconds(bossSpawnInterval);

           if (maxEnemies > 0 && enemyParent.childCount < maxEnemies) // 检查是否有空位
           {
               if (bossPrefabs.Length > 0)
               {
                   GameObject bossPrefab = bossPrefabs[UnityEngine.Random.Range(0, bossPrefabs.Length)];

                   Vector3 randomPosition = GetRandomPositionInEnemyArea();
                   while (IsInsideExclusionZone(randomPosition))
                   {
                       randomPosition = GetRandomPositionInEnemyArea();
                   }

                   GameObject BossObj = Instantiate(bossPrefab, randomPosition, Quaternion.identity, enemyParent);
                   BossObj.transform.position = center.position + bossSpawnOffset;
                   Debug.Log($"生成 Boss: {bossPrefab.name}");
               }
           }
       }
   }

   // 在编辑器模式下可视化生成区域和安全区域（仅用于调试）
   private void OnDrawGizmosSelected()
   {
       if (enemyParent != null)
       {
           Gizmos.color = new Color(1, 0, 0, 0.5f);  // 敌人生成区域为红色
           Gizmos.DrawCube(enemyParent.position + enemySpawnAreaOffset, enemySpawnAreaDimensions);

           Gizmos.color = new Color(0, 0, 1, 0.5f);  // 安全区域为蓝色圆圈
           Gizmos.DrawSphere(enemyParent.position, exclusionZoneRadius);
       }
   }
}