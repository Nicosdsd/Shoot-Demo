using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    public GameObject[] enemyPrefabs; // Enemy prefab array
    public float spawnInterval;
    public int spawnAmount;

    public Vector3 enemySpawnAreaDimensions; // Enemy spawn area dimensions
    public Vector3 enemySpawnAreaOffset; // Offset relative to the center

    public Transform Center; // Center of the spawn area
    public float exclusionZoneRadius; // Radius of the exclusion zone

    private void Start()
    {
        InvokeRepeating(nameof(SpawnEnemies), 0, spawnInterval); // Spawn enemies at intervals
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

            // Draw exclusion zone
            Gizmos.color = new Color(0, 0, 1, 0.5f);
            Gizmos.DrawSphere(Center.position, exclusionZoneRadius);
        }
    }
}