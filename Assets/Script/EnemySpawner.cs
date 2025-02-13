using UnityEngine;

// 敌人在规定范围内生成
public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab; // 拖入敌人预制体
    public float spawnInterval;
    public float spawnAmount;
    public float enemySpawnArea; // 敌人生成范围

    public GameObject weaponGift;
    public float weaponGiftInterval;
    public Transform Center; // 生成区域中心
    public float weaponGiftSpawnArea; // 武器生成范围半径

    private void Start()
    {
        InvokeRepeating(nameof(SpawnEnemies), 0, spawnInterval); // 按照间隔生成敌人
        InvokeRepeating(nameof(SpawnWeapon), 0, weaponGiftInterval); // 按照间隔生成武器礼包
    }

    private void SpawnEnemies()
    {
        for (int i = 0; i < spawnAmount; i++)
        {
            Vector3 randomPosition = GetRandomPositionInEnemyArea();
            Instantiate(enemyPrefab, randomPosition, Quaternion.identity);
        }
    }

    void SpawnWeapon()
    {
        Vector3 randomPosition = GetRandomPositionInWeaponArea();
        Instantiate(weaponGift, randomPosition, Quaternion.identity);
    }

    private Vector3 GetRandomPositionInEnemyArea()
    {
        float halfArea = enemySpawnArea * 0.5f;
        Vector3 centerPosition = Center.position; // 使用 Center 作为中心
        float randomX = Random.Range(centerPosition.x - halfArea, centerPosition.x + halfArea);
        float randomY = centerPosition.y;
        float randomZ = Random.Range(centerPosition.z - halfArea, centerPosition.z + halfArea);

        return new Vector3(randomX, randomY, randomZ);
    }

    private Vector3 GetRandomPositionInWeaponArea()
    {
        float halfArea = weaponGiftSpawnArea * 0.5f;
        Vector3 centerPosition = Center.position;
        float randomX = Random.Range(centerPosition.x - halfArea, centerPosition.x + halfArea);
        float randomY = centerPosition.y;
        float randomZ = Random.Range(centerPosition.z - halfArea, centerPosition.z + halfArea);

        return new Vector3(randomX, randomY, randomZ);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        if (Center != null)
        {
            Gizmos.DrawCube(Center.position, new Vector3(enemySpawnArea, 1, enemySpawnArea)); // 显示敌人生成区域
            Gizmos.color = new Color(0, 1, 0, 0.5f);
            Gizmos.DrawCube(Center.position, new Vector3(weaponGiftSpawnArea, 1, weaponGiftSpawnArea)); // 显示武器生成区域
        }
    }
}