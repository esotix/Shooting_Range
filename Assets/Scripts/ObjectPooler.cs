using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class PoolConfig
{
    public string tag;
    public GameObject prefab;
    public int size = 10;
    public bool canExpand = true;
}
public class ObjectPooler : MonoBehaviour
{

    public static ObjectPooler Instance;

    [Header("Configuration du Pool")]
    public List<PoolConfig> pools;
    private Dictionary<string, List<GameObject>> poolDictionary;

    private List<GameObject> activeProjectiles = new List<GameObject>();
    public Transform cleaningReferencePoint;
    public float maxProjectileDistance = 50f;
    public float cleaningInterval = 0.3f;

    void Awake()
    {
        Instance = this;
        poolDictionary = new Dictionary<string, List<GameObject>>();

        foreach (PoolConfig pool in pools)
        {
            List<GameObject> objectList = new List<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectList.Add(obj);
                obj.transform.parent = this.transform;
            }

            poolDictionary.Add(pool.tag, objectList);
        }
    }

    void Start()
    {
        StartCoroutine(CleanupRoutine());

        if (cleaningReferencePoint == null)
        {
            cleaningReferencePoint = transform;
        }
    }

    public GameObject SpawnFromPool(string tag)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return null;
        }

        List<GameObject> poolList = poolDictionary[tag];

        foreach (GameObject obj in poolList)
        {
            if (!obj.activeInHierarchy)
            {
                return obj;
            }
        }

        PoolConfig config = pools.Find(p => p.tag == tag);
        if (config != null && config.canExpand)
        {
            GameObject newObj = Instantiate(config.prefab);
            newObj.transform.parent = this.transform;

            poolList.Add(newObj);

            return newObj;
        }

        return null;
    }
    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
    }

    public void InitializePool(string tag, GameObject newPrefab, int initialSize)
    {
        if (poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Le pool '{tag}' existe. Remplacement du Prefab en cours.");

            foreach (GameObject obj in poolDictionary[tag])
            {
                Destroy(obj);
            }
            poolDictionary.Remove(tag);
        }

        List<GameObject> objectList = new List<GameObject>();

        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Instantiate(newPrefab);
            obj.SetActive(false);
            objectList.Add(obj);
            obj.transform.parent = this.transform;
        }

        poolDictionary.Add(tag, objectList);
        Debug.Log($"Pool '{tag}' initialisé avec succès. Taille: {initialSize}.");
    }

    IEnumerator CleanupRoutine()
    {
        WaitForSeconds delay = new WaitForSeconds(cleaningInterval);

        while (true)
        {
            yield return delay;

            for (int i = activeProjectiles.Count - 1; i >= 0; i--)
            {
                GameObject projectile = activeProjectiles[i];

                if (projectile == null || !projectile.activeInHierarchy)
                {
                    activeProjectiles.RemoveAt(i);
                    continue;
                }

                float distance = Vector3.Distance(projectile.transform.position, cleaningReferencePoint.position);

                if (distance > maxProjectileDistance)
                {
                    ReturnToPool(projectile);
                }
            }
        }
    }
}