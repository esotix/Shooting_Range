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
        //if (TargetManager.Instance != null)
        //{
        //    TargetManager.Instance.NotifyTargetHit(poolTag);
        //}
        GameplayManager.Instance.AddScore(10);
        ReturnToPool();
    }
    private void ReturnToPool()
    {
        TargetManager.Instance.RegisterTargetDespawn();
        ObjectPooler.Instance.ReturnToPool(gameObject);
    }
    
}