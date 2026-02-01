using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TargetManager : MonoBehaviour
{
    public static TargetManager Instance;

    [Header("Configuration du Spawning")]
    [Tooltip("Liste des points de réapparition possibles.")]
    public List<Transform> spawnPoints;
    public float spawnInterval = 5f;
    public int maxTargets = 5;

    [Header("État")]
    private int currentTargets = 0;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        WaitForSeconds delay = new WaitForSeconds(spawnInterval);

        while (true)
        {
            yield return delay;

            if (currentTargets < maxTargets)
            {
                SpawnTarget("Target");
            }
        }
    }

    private void SpawnTarget(string tag)
    {
        GameObject targetGO = ObjectPooler.Instance.SpawnFromPool(tag);

        if (targetGO != null)
        {
            Vector3 spawnPosition = ChooseRandomSpawnPoint();
            targetGO.transform.position = spawnPosition;
            targetGO.SetActive(true);

            currentTargets++;
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.TargetActivated();
            }
        }
    }

    public void RegisterTargetDespawn()
    {
        if (currentTargets > 0)
        {
            currentTargets--;
        }
        if (GameplayManager.Instance != null)
        {
            GameplayManager.Instance.TargetDeactivated();
        }
    }

    private Vector3 ChooseRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogError("Aucun point de réapparition défini!");
            return Vector3.zero;
        }
        int randomIndex = Random.Range(0, spawnPoints.Count);
        return spawnPoints[randomIndex].position;
    }
}