using System.Collections;
using UnityEngine;
public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;
    public string Pooltag = "HitEffect";

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

    }
    public void HitEffect(Vector3 position)
    {
        Debug.Log("Effet de hit à la position: " + position);
        GameObject EffectGo = ObjectPooler.Instance.SpawnFromPool(Pooltag);
        if (EffectGo != null)
        {
            EffectGo.transform.position = position;
            EffectGo.SetActive(true);
            ParticleSystem part = EffectGo.GetComponent<ParticleSystem>();
            part.Play();
            float duration = part.main.duration + part.main.startLifetime.constantMax;
            StartCoroutine(ReturnToPool(EffectGo, duration));
        }
    }
    IEnumerator ReturnToPool(GameObject Effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        ObjectPooler.Instance.ReturnToPool(Effect);
    }
}
