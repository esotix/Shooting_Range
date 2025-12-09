using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class Target : MonoBehaviour
{
    public string poolTag = "Target";
    public void Hit()
    {
        Debug.Log("Target Hit method called.");
        if (EffectManager.Instance != null)
        {
            Debug.Log("Target hit! Triggering hit effect.");
            EffectManager.Instance.HitEffect(transform.position);
        }
        if (TargetManager.Instance != null)
        {
            TargetManager.Instance.NotifyTargetHit(poolTag);
        }
        ReturnToPool();
    }
    private void ReturnToPool()
    {
        ObjectPooler.Instance.ReturnToPool(gameObject);
    }
    
}