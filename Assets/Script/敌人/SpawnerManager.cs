using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class SpawnerManager : MonoBehaviour
{
    [Header("普通敌人设置")]
    public GameObject[] normalEnemyPrefabs;  // 普通敌人预制体数组
    [Header("精英敌人设置")]
    public GameObject[] eliteEnemyPrefabs;   // 精英敌人预制体数组
    public float eliteSpawnDelay = 30f;      // 游戏开始后多少秒开始出现精英敌人
    [Range(0, 1)] public float eliteSpawnChance = 0.2f; // 精英敌人生成概率

    [Header("生成设置")]
    public float spawnInterval = 3f;         // 敌人生成间隔
    public int spawnAmount = 5;              // 每波生成数量
    
    [Header("生成区域设置")]
    public Vector3 enemySpawnAreaDimensions; // 生成区域尺寸
    public Vector3 enemySpawnAreaOffset;     // 生成区域偏移位置
    public Transform center;                 // 生成中心点
    public float exclusionZoneRadius = 2f;   // 中心禁止生成区域半径

    private float gameStartTime;             // 游戏开始时间记录
    public Text timerText;

    private void Start()
    {
        gameStartTime = Time.time;
        InvokeRepeating(nameof(SpawnEnemies), 0, spawnInterval);
    }

    private void Update()
    {
        float elapsedTime = Time.time - gameStartTime;
        int minutes = (int)(elapsedTime / 60);
        int seconds = (int)(elapsedTime % 60);

        timerText.text = string.Format("时间：{0:D2}:{1:D2}", minutes, seconds);
    }

    private void SpawnEnemies()
    {
        for (int i = 0; i < spawnAmount; i++)
        {
            Vector3 randomPosition;
            do
            {
                randomPosition = GetRandomPositionInEnemyArea();
            } while (IsInsideExclusionZone(randomPosition));
            
            GameObject selectedEnemy = GetRandomEnemyPrefab();
            Instantiate(selectedEnemy, randomPosition, Quaternion.identity);
        }
    }


    private Vector3 GetRandomPositionInEnemyArea()
    {
        float halfWidth = enemySpawnAreaDimensions.x * 0.5f;
        float halfHeight = enemySpawnAreaDimensions.y * 0.5f;
        float halfDepth = enemySpawnAreaDimensions.z * 0.5f;
        
        Vector3 centerPosition = center.position + enemySpawnAreaOffset;

        float randomX = Random.Range(centerPosition.x - halfWidth, centerPosition.x + halfWidth);
        float randomY = Random.Range(centerPosition.y - halfHeight, centerPosition.y + halfHeight);
        float randomZ = Random.Range(centerPosition.z - halfDepth, centerPosition.z + halfDepth);

        return new Vector3(randomX, randomY, randomZ);
    }

    private bool IsInsideExclusionZone(Vector3 position)
    {
        return Vector3.Distance(position, center.position) < exclusionZoneRadius;
    }

    // 获取随机敌人类型（包含精英敌人逻辑）
    private GameObject GetRandomEnemyPrefab()
    {
        bool canSpawnElite = Time.time - gameStartTime >= eliteSpawnDelay;

        if (canSpawnElite && eliteEnemyPrefabs.Length > 0 && Random.value <= eliteSpawnChance)
        {
            return eliteEnemyPrefabs[Random.Range(0, eliteEnemyPrefabs.Length)];
        }
        else
        {
            return normalEnemyPrefabs[Random.Range(0, normalEnemyPrefabs.Length)];
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        if (center != null)
        {
            Gizmos.DrawCube(center.position + enemySpawnAreaOffset, enemySpawnAreaDimensions);

            // Draw exclusion zone
            Gizmos.color = new Color(0, 0, 1, 0.5f);
            Gizmos.DrawSphere(center.position, exclusionZoneRadius);
        }
    }
}