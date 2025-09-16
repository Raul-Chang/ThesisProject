using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;   
    public Transform[] spawnPoints;  
    public int enemiesToSpawn = 5;

    void Start()
    {
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        
        Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];

       
        NavMeshHit hit;
        if (NavMesh.SamplePosition(spawn.position, out hit, 2f, NavMesh.AllAreas))
        {
            Instantiate(enemyPrefab, hit.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("No se encontró NavMesh en este spawn point.");
        }
    }
}