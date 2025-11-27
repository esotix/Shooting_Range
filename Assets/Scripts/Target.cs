using UnityEngine;

public class Target : MonoBehaviour
{
    [Tooltip("Effet visuel/sonore à jouer lors de l'impact.")]
    public GameObject impactEffectPrefab; // Optionnel : pour les effets de particules

    // Fonction appelée par le projectile lors de l'impact
    public void Hit()
    {
        // Optionnel : Instancie un effet visuel (e.g., explosion)
        if (impactEffectPrefab != null)
        {
            Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
        }

        // Détruit la cible
        Destroy(gameObject);

        Debug.Log(gameObject.name + " a été détruit par l'impact !");
    }
}