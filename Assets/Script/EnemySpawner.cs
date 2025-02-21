using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs; // 敌人预制体数组
    public float spawnInterval;
    public int spawnAmount;

    public Vector3 enemySpawnAreaDimensions; // 敌人生成区域尺寸
    public Vector3 enemySpawnAreaOffset; // 敌人生成区域相对于中心的偏移

    public GameObject weaponGift;
    public float weaponGiftInterval;

    public Vector3 weaponGiftSpawnAreaDimensions; // 武器生成区域尺寸
    public Vector3 weaponGiftSpawnAreaOffset; // 武器生成区域相对于中心的偏移

    public Transform Center; // 生成区域中心
    public float exclusionZoneRadius; // 排除区域的半径

    private void Start()
    {
        InvokeRepeating(nameof(SpawnEnemies), 0, spawnInterval); // 按照间隔生成敌人
        InvokeRepeating(nameof(SpawnWeapon), 0, weaponGiftInterval); // 按照间隔生成武器礼包
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
            
            GameObject randomEnemyPrefab = GetRandomEnemyPrefab();
            Instantiate(randomEnemyPrefab, randomPosition, Quaternion.identity);
        }
    }

    void SpawnWeapon()
    {
        Vector3 randomPosition;
        do
        {
            randomPosition = GetRandomPositionInWeaponArea();
        } while (IsInsideExclusionZone(randomPosition));
        
        Instantiate(weaponGift, randomPosition, Quaternion.identity);
    }

    private Vector3 GetRandomPositionInEnemyArea()
    {
        float halfWidth = enemySpawnAreaDimensions.x * 0.5f;
        float halfHeight = enemySpawnAreaDimensions.y * 0.5f;
        float halfDepth = enemySpawnAreaDimensions.z * 0.5f;
        
        Vector3 centerPosition = Center.position + enemySpawnAreaOffset;

        float randomX = Random.Range(centerPosition.x - halfWidth, centerPosition.x + halfWidth);
        float randomY = Random.Range(centerPosition.y - halfHeight, centerPosition.y + halfHeight);
        float randomZ = Random.Range(centerPosition.z - halfDepth, centerPosition.z + halfDepth);

        return new Vector3(randomX, randomY, randomZ);
    }

    private Vector3 GetRandomPositionInWeaponArea()
    {
        float halfWidth = weaponGiftSpawnAreaDimensions.x * 0.5f;
        float halfHeight = weaponGiftSpawnAreaDimensions.y * 0.5f;
        float halfDepth = weaponGiftSpawnAreaDimensions.z * 0.5f;

        Vector3 centerPosition = Center.position + weaponGiftSpawnAreaOffset;

        float randomX = Random.Range(centerPosition.x - halfWidth, centerPosition.x + halfWidth);
        float randomY = Random.Range(centerPosition.y - halfHeight, centerPosition.y + halfHeight);
        float randomZ = Random.Range(centerPosition.z - halfDepth, centerPosition.z + halfDepth);

        return new Vector3(randomX, randomY, randomZ);
    }

    private bool IsInsideExclusionZone(Vector3 position)
    {
        return Vector3.Distance(position, Center.position) < exclusionZoneRadius;
    }

    private GameObject GetRandomEnemyPrefab()
    {
        int randomIndex = Random.Range(0, enemyPrefabs.Length);
        return enemyPrefabs[randomIndex];
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        if (Center != null)
        {
            Gizmos.DrawCube(Center.position + enemySpawnAreaOffset, enemySpawnAreaDimensions);

            Gizmos.color = new Color(0, 1, 0, 0.5f);
            Gizmos.DrawCube(Center.position + weaponGiftSpawnAreaOffset, weaponGiftSpawnAreaDimensions);

            // 绘制排除区域
            Gizmos.color = new Color(0, 0, 1, 0.5f);
            Gizmos.DrawSphere(Center.position, exclusionZoneRadius);
        }
    }
}