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

    [Header("生成区域设置")]
    public Vector3 enemySpawnAreaDimensions;
    public Vector3 enemySpawnAreaOffset;
    public Transform center;
    public float exclusionZoneRadius;

    [Header("生成限制")]
    public Transform enemyParent; // 用于存放所有生成敌人的父物体
    public int maxEnemies = 0;     // 最大敌人总数限制

    private float gameStartTime;
    private SpawnStage currentStage;

    private void Start()
    {
        gameStartTime = Time.time;
        UpdateCurrentStage();
        if (maxEnemies > 0)
        {
            InvokeRepeating(nameof(SpawnEnemies), 0, currentStage.spawnInterval);
        }
    }

    private void Update()
    {
        UpdateCurrentStage();
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
            InvokeRepeating(nameof(SpawnEnemies), 0, currentStage.spawnInterval);
            Debug.Log($"切换到阶段: {currentStage.stageName}");
        }
    }

    private void SpawnEnemies()
    {
        if (maxEnemies <= 0) return; // 如果没有数量限制，直接返回

        // 计算可用的空位
        int availableSlots = maxEnemies - enemyParent.childCount;
        if (availableSlots <= 0) return; // 超过最大数量，不再生成

        int enemiesToSpawn = Mathf.Min(currentStage.spawnAmount, availableSlots);

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Vector3 randomPosition;
            do
            {
                randomPosition = GetRandomPositionInEnemyArea();
            } while (IsInsideExclusionZone(randomPosition));

            GameObject selectedEnemy = GetRandomEnemyPrefabForCurrentStage();
            if (selectedEnemy != null)
            {
                // 实例化敌人并将其设置为父物体的子对象
                GameObject newEnemy = Instantiate(selectedEnemy, randomPosition, Quaternion.identity, enemyParent);
                // 可选：设置敌人层级或其他初始化操作
            }
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