using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;
    public string Pooltag = "HitEffect";

    public void HitEffect(Vector3 position)
    {
        Debug.Log("Effet de hit à la position: " + position);
        GameObject EffectGo = ObjectPooler.Instance.SpawnFromPool(Pooltag);
        if (EffectGo != null)
        {
            EffectGo.transform.position = position;
            EffectGo.SetActive(true);
            EffectGo.GetComponent<ParticleSystem>().Play();
            ReturnToPool(EffectGo);
        }
    }
    private void ReturnToPool(GameObject Effect)
    {
        ObjectPooler.Instance.ReturnToPool(Effect);
    }
}
