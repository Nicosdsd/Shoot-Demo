using System.Collections;
using UnityEngine;
//敌人在规定范围内生成
public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab; // 将敌人预制体拖到脚本对应位置
    public float spawnInterval;
    public float spawnAmount;
    public float spawnArea;

    private void Start()
    {
        InvokeRepeating(nameof(SpawnEnemies), 0, spawnInterval); // 每隔一定时间调用SpawnEnemies函数
    }

    private void SpawnEnemies()
    {
        for (int i = 0; i < spawnAmount; i++)
        {
            Vector3 randomPosition = GetRandomPositionInSpawningArea(); // 在范围内生成随机位置
            Instantiate(enemyPrefab, randomPosition, Quaternion.identity); // 实例化敌人预制体
        }
    }

    private Vector3 GetRandomPositionInSpawningArea()
    {
        float halfArea = spawnArea * 0.5f;
        Vector3 centerPosition = transform.position;
        float randomX = Random.Range(centerPosition.x - halfArea, centerPosition.x + halfArea);
        float randomY = centerPosition.y;
        float randomZ = Random.Range(centerPosition.z - halfArea, centerPosition.z + halfArea);

        return new Vector3(randomX, randomY, randomZ);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawCube(transform.position, new Vector3(spawnArea, 1, spawnArea)); // 以绿色方块显示生成区域
    }
}