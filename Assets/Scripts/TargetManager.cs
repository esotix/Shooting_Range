using System.Collections.Generic;
using UnityEngine;

public class TargetManager : MonoBehaviour
{
    public static TargetManager Instance;

    [Header("Configuration du Respawn")]
    [Tooltip("Liste des points de réapparition possibles.")]
    public List<Transform> spawnPoints;
    public float respawnDelay = 3f;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

    }

    void Start()
    {
        StartCoroutine(RespawnTargetAfterDelay("Target"));
    }
    public void NotifyTargetHit(string targetTag)
    {
        StartCoroutine(RespawnTargetAfterDelay(targetTag));
    }

    private System.Collections.IEnumerator RespawnTargetAfterDelay(string tag)
    {
        Debug.Log("Démarrage du délai de réapparition pour la cible avec le tag: " + tag);
        yield return new WaitForSeconds(respawnDelay);

        GameObject targetGO = ObjectPooler.Instance.SpawnFromPool(tag);

        if (targetGO != null)
        {
            Vector3 spawnPosition = ChooseRandomSpawnPoint();
            targetGO.transform.position = spawnPosition;
            targetGO.SetActive(true);
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