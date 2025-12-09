using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;

public class TreeSpawner : MonoBehaviour
{
    [Tooltip("Liste des emplacements où les arbres doivent apparaître.")]
    public List<Transform> spawnLocations;

    private const string TREE_ADDRESS = "Tree_Pine";

    void Start()
    {
        LoadAndSpawnTrees();
    }

    void LoadAndSpawnTrees()
    {
        string assetAddress = TREE_ADDRESS;

        Debug.Log($"Chargement du modèle d'arbre: {assetAddress}");

        Addressables.LoadAssetAsync<GameObject>(assetAddress).Completed += OnTreeLoaded;
    }

    private void OnTreeLoaded(AsyncOperationHandle<GameObject> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject treePrefab = handle.Result;

            foreach (Transform location in spawnLocations)
            {
                Instantiate(treePrefab, location.position, location.rotation, location);
            }
            Debug.Log($"Instanciation de {spawnLocations.Count} arbres ({treePrefab.name}) terminée.");
        }
        else
        {
            Debug.LogError($"Échec du chargement de l'arbre Addressable. Adresse recherchée: {TREE_ADDRESS}");
        }
    }
}