using System;
using UnityEngine;
using UnityEngine.UI;

public class SpawnerManager : MonoBehaviour
{
    [Serializable]
    public class SpawnStage
    {
        public string stageName;
        public float startTime;
        public float spawnInterval;
        public int spawnAmount;
        public EnemyType[] enemyTypes;
    }

    [Serializable]
    public class EnemyType
    {
        public string name;
        public GameObject[] enemyPrefabs;
        public float spawnChance;
    }

    [Header("阶段设置")]
    public SpawnStage[] spawnStages;

    [Header("Boss 设置")]
    public GameObject[] bossPrefabs; // Boss 预制体数组
    public float bossSpawnInterval = 300f; // Boss 生成间隔时间（秒）
    public Vector3 bossSpawnOffset = new Vector3(0, 5, 0); // Boss 生成相对于主角的偏移量

    [Header("生成区域设置")]
    public Vector3 enemySpawnAreaDimensions;
    public Vector3 enemySpawnAreaOffset;
    public Transform center;
    public float exclusionZoneRadius;
    
    [Header("预警设置")]
    public GameObject spawnAlertPrefab; // 用于生成提示的预制件

    [Header("生成限制")]
    public Transform enemyParent; // 用于存放所有生成敌人的父物体
    public int maxEnemies ;     // 最大敌人总数限制
    
    [Header("UI 设置")]
    public Text timerText; // 用于显示时间
    public GameObject stageStartUI; // 用于显示阶段开始信息的UI物体
    public Text stageStartText;     // 阶段开始信息的文本组件
    private bool isFirstStage = true;


    private float gameStartTime;
    private SpawnStage currentStage;

    private void Start()
    {
        gameStartTime = Time.time;
        UpdateCurrentStage();

        // 启动小怪生成逻辑
        if (maxEnemies > 0)
        {
            InvokeRepeating(nameof(SpawnEnemies), 0, currentStage.spawnInterval);
        }
        

        // 启动 Boss 生成逻辑
        StartCoroutine(SpawnBosses());
    }

    private void Update()
    {
        UpdateCurrentStage();
        UpdateTimer();
    }
    
    //计时器
    private void UpdateTimer()
    {
        float elapsedTime = Time.time - gameStartTime;
        // 格式化时间为 "分钟:秒"
        string formattedTime = string.Format("{0:D2}:{1:D2}", (int)(elapsedTime) / 60, (int)(elapsedTime) % 60);
        if (timerText != null)
        {
            timerText.text = formattedTime;
        }
    }

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
            CancelInvoke(nameof(SpawnEnemies));
            if (maxEnemies > 0)
            {
                InvokeRepeating(nameof(SpawnEnemies), 0, currentStage.spawnInterval);
            }
            Debug.Log($"切换到阶段: {currentStage.stageName}" + currentStage);
            
   
            // 仅当不是第一波时才显示波次警告
            if (!isFirstStage)
            {
                stageStartUI.SetActive(true);
                stageStartText.text = currentStage.stageName;
                Invoke("StageStartUI", 2f);
            }

            // 第一波完成后，标记为 false
            isFirstStage = false;
        }
    }
    //持续几秒后隐藏
    private void StageStartUI()
    {
        stageStartUI.SetActive(false);
    }

    private void SpawnEnemies()
    {
        if (maxEnemies <= 0) return;

        int availableSlots = maxEnemies - enemyParent.childCount;
        if (availableSlots <= 0) return;

        int enemiesToSpawn = Mathf.Min(currentStage.spawnAmount, availableSlots);

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            StartCoroutine(SpawnWithAlert());
        }
    }

    //预警1秒后生成敌人
    private System.Collections.IEnumerator SpawnWithAlert()
    {
        Vector3 randomPosition;
        do
        {
            randomPosition = GetRandomPositionInEnemyArea();
        } while (IsInsideExclusionZone(randomPosition));

        GameObject alertObject = Instantiate(spawnAlertPrefab, randomPosition, Quaternion.identity);
    
        // 等待一秒后销毁预警物体并生成敌人
        yield return new WaitForSeconds(1f);

        Destroy(alertObject);

        GameObject selectedEnemy = GetRandomEnemyPrefabForCurrentStage();
        if (selectedEnemy != null)
        {
            Instantiate(selectedEnemy, randomPosition, Quaternion.identity, enemyParent);
        }
    }

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

    private bool IsInsideExclusionZone(Vector3 position)
    {
        return Vector3.Distance(position, center.position) < exclusionZoneRadius;
    }

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

    private System.Collections.IEnumerator SpawnBosses()
    {
        while (true)
        {
            // 等待 Boss 生成间隔时间
            yield return new WaitForSeconds(bossSpawnInterval);

            // 检查是否有空位
            if (maxEnemies > 0 && enemyParent.childCount < maxEnemies)
            {
                // 随机选择一个 Boss 预制体
                if (bossPrefabs.Length > 0)
                {
                    GameObject bossPrefab = bossPrefabs[UnityEngine.Random.Range(0, bossPrefabs.Length)];

                    // 获取随机位置
                    Vector3 randomPosition = GetRandomPositionInEnemyArea();
                    while (IsInsideExclusionZone(randomPosition))
                    {
                        randomPosition = GetRandomPositionInEnemyArea();
                    }

                    // 实例化 Boss
                    GameObject BossObj = Instantiate(bossPrefab, randomPosition, Quaternion.identity, enemyParent);
                    BossObj.transform.position = center.position + bossSpawnOffset;
                    Debug.Log($"生成 Boss: {bossPrefab.name}");
                    
                    
                    /*//波次警告
                    stageStartUI.SetActive(true);
                    stageStartText.text = "出现了Boss";
                    Invoke("StageStartUI",2f);*/
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (enemyParent != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.5f);
            Gizmos.DrawCube(enemyParent.position + enemySpawnAreaOffset, enemySpawnAreaDimensions);

            Gizmos.color = new Color(0, 0, 1, 0.5f);
            Gizmos.DrawSphere(enemyParent.position, exclusionZoneRadius);
        }
    }
}