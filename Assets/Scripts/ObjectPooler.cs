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

        // Ajouter la nouvelle liste au dictionnaire
        poolDictionary.Add(tag, objectList);
        Debug.Log($"Pool '{tag}' initialisé avec succès. Taille: {initialSize}.");
    }
}