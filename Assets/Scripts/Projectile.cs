using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Tooltip("Vitesse à laquelle le projectile sera tiré.")]
    public float launchSpeed = 20f;
    [Tooltip("Temps en secondes avant la destruction automatique du projectile.")]
    public float lifespan = 5f;
    private float timeElapsed;

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
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed >= lifespan)
        {
            ReturnToPool();
        }
    }

    public void Launch(Vector3 position, Quaternion rotation, Vector3 direction)
    {
        timeElapsed = 0f;
        transform.position = position;
        transform.rotation = rotation;
        gameObject.SetActive(true);
        rb.linearVelocity = direction * launchSpeed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision détectée avec : " + collision.gameObject.name);

        if (collision.gameObject.CompareTag("Target"))
        {
            Debug.Log("Cible 'Target' touchée ! Destruction...");

            Target target = collision.gameObject.GetComponent<Target>();

            if (target != null)
            {
                target.Hit();
            }
            else
            {
                Destroy(collision.gameObject);
            }
        }
        ReturnToPool();
    }
    private void ReturnToPool()
    {
        ObjectPooler.Instance.ReturnToPool(gameObject);
    }
}