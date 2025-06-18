using UnityEngine;
using System.Collections.Generic;

public class RandomSafeSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject prefabToSpawn;
    [SerializeField] private Vector2 spawnAreaMin;
    [SerializeField] private Vector2 spawnAreaMax;
    [SerializeField] private float checkRadius = 0.5f;
    [SerializeField] private LayerMask onWayLayer;
    [SerializeField] private int maxSpawnCount = 4;
    [SerializeField] private float minDistanceBetweenSpawns = 1.5f; 

    [Header("Timing")]
    [SerializeField] private float spawnInterval = 1.5f;
    private float spawnTimer;

    private int currentSpawnCount = 0;
    private List<GameObject> spawnedObjects = new List<GameObject>(); 

    private void Update()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            if (currentSpawnCount < maxSpawnCount)
            {
                SpawnRandomSafe();
            }
            spawnTimer = spawnInterval;
        }
    }

    private void SpawnRandomSafe()
    {
        Vector2 randomPos;
        int attempts = 0;
        const int maxAttempts = 30;

        do
        {
            randomPos = new Vector2(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                Random.Range(spawnAreaMin.y, spawnAreaMax.y)
            );
            attempts++;
        }
        while ((Physics2D.OverlapCircle(randomPos, checkRadius, onWayLayer) != null || !IsFarEnoughFromOthers(randomPos)) 
               && attempts < maxAttempts);

        GameObject obj = Instantiate(prefabToSpawn, randomPos, Quaternion.identity);
        obj.AddComponent<AutoDestroyOnPlayer>().Init(this, obj);
        spawnedObjects.Add(obj);

        currentSpawnCount++;
    }

    private bool IsFarEnoughFromOthers(Vector2 pos)
    {
        foreach (GameObject go in spawnedObjects)
        {
            if (go != null)
            {
                if (Vector2.Distance(pos, go.transform.position) < minDistanceBetweenSpawns)
                    return false;
            }
        }
        return true;
    }

    // Giảm số lượng khi object bị phá hủy
    public void NotifyObjectDestroyed(GameObject obj)
    {
        spawnedObjects.Remove(obj);
        currentSpawnCount = Mathf.Max(0, currentSpawnCount - 1);
    }

    // Class con xử lý tự huỷ khi chạm Player
    private class AutoDestroyOnPlayer : MonoBehaviour
    {
        private RandomSafeSpawner spawner;
        private GameObject self;

        public void Init(RandomSafeSpawner spawnerRef, GameObject obj)
        {
            spawner = spawnerRef;
            self = obj;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Weapon"))
            {
                spawner?.NotifyObjectDestroyed(self);
                Destroy(gameObject);
            }
        }
    }
}
