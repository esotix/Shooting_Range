using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GameManager : MonoBehaviour
{
    public static string PlatformGroupPrefix;

    [Header("Adresses des FX")]
    [Tooltip("Adresse/Label de l'effet d'impact (ex: 'ImpactFX')")]
    public string impactFXAddress = "ImpactFX";

    [Tooltip("Tag du pool d'impact correspondant (ex: 'HitEffect')")]
    public string impactFXPoolTag = "HitEffect";


    void Awake()
    {
#if UNITY_ANDROID
            PlatformGroupPrefix = "FX_Quest";
#elif UNITY_STANDALONE_WIN
        PlatformGroupPrefix = "FX_PCVR";
#else
            PlatformGroupPrefix = "FX_PCVR";
#endif

        Debug.Log($"Plateforme détectée. Préfixe du groupe FX: {PlatformGroupPrefix}");
    }

    void Start()
    {
        LoadFXAsset(impactFXAddress, impactFXPoolTag);
    }

    private void LoadFXAsset(string baseAddress, string poolTag)
    {

        string assetAddress = baseAddress;

        Addressables.LoadAssetAsync<GameObject>(assetAddress).Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject loadedPrefab = handle.Result;

                int desiredSize = 1;

                Debug.Log($"SUCCESS: FX chargé ({loadedPrefab.name}) via l'adresse {assetAddress}");

                ObjectPooler.Instance.InitializePool(poolTag, loadedPrefab, desiredSize);
            }
            else
            {
                Debug.LogError($"ÉCHEC du chargement Addressables pour l'adresse: {assetAddress}");
            }
        };
    }
}