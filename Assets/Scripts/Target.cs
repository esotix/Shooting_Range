using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class Target : MonoBehaviour
{
    public GameObject impactEffectPrefab;
    public string poolTag = "Target";
    public void Hit()
    {
        if (impactEffectPrefab != null)
        {
            Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
        }
        if (TargetManager.Instance != null)
        {
            EffectManager.Instance.HitEffect(transform.position);
            TargetManager.Instance.NotifyTargetHit(poolTag);
        }
        ReturnToPool();
    }
    private void ReturnToPool()
    {
        ObjectPooler.Instance.ReturnToPool(gameObject);
    }
    
}