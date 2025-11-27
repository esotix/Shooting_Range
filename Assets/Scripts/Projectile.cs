using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Tooltip("Vitesse à laquelle le projectile sera tiré.")]
    public float launchSpeed = 20f;
    [Tooltip("Temps en secondes avant la destruction automatique du projectile.")]
    public float lifespan = 5f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Projectile requires a Rigidbody component!");
            enabled = false;
            return;
        }
        Destroy(gameObject, lifespan);
    }

    public void Launch(Vector3 direction)
    {
        rb.linearVelocity = direction.normalized * launchSpeed;
    }

    // NOUVELLE LOGIQUE DE DÉTECTION PAR TAG
    private void OnCollisionEnter(Collision collision)
    {
        // Debug pour voir si la collision est détectée du tout
        Debug.Log("Collision détectée avec : " + collision.gameObject.name);

        // 1. Vérifie si l'objet touché a le tag "Target"
        if (collision.gameObject.CompareTag("Target"))
        {
            // Debug pour confirmer la cible
            Debug.Log("Cible 'Target' touchée ! Destruction...");

            // 2. Tente de récupérer le script de cible (optionnel, mais bon de le garder)
            Target target = collision.gameObject.GetComponent<Target>();

            if (target != null)
            {
                // Si le script est trouvé, on l'appelle
                target.Hit();
            }
            else
            {
                // Si le script n'est pas trouvé (mais le tag l'est), on détruit l'objet quand même
                Destroy(collision.gameObject);
            }
        }

        // Détruit toujours le projectile après l'impact
        Destroy(gameObject);
    }
}