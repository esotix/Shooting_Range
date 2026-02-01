using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class GunFire : MonoBehaviour
{
    [Tooltip("Le Prefab du projectile à tirer.")]
    public GameObject projectilePrefab;
    [Tooltip("L'objet vide positionné au bout du canon.")]
    public Transform FirePoint;

    [Tooltip("Le composant Particle System pour le flash de bouche.")]
    public ParticleSystem muzzleFlash;

    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (grabInteractable != null)
        {
            grabInteractable.activated.AddListener(FireWeapon);
        }
        else
        {
            Debug.LogError("GunFire requires an XRGrabInteractable component on the same GameObject!");
        }

        if (muzzleFlash == null && FirePoint != null)
        {
            muzzleFlash = FirePoint.GetComponentInChildren<ParticleSystem>();
        }
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.activated.RemoveListener(FireWeapon);
        }
    }

    public void FireWeapon(ActivateEventArgs args)
    {
        if (projectilePrefab == null || FirePoint == null)
        {
            Debug.LogError("Projectile Prefab ou Fire Point manquant sur l'arme.");
            return;
        }

        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        GameObject projectileGO = ObjectPooler.Instance.SpawnFromPool("Bullet");

        if (projectileGO != null)
        {
            Projectile projectileComponent = projectileGO.GetComponent<Projectile>();

            projectileComponent.Launch(
                FirePoint.position,
                FirePoint.rotation,
                FirePoint.forward
            );
        }
    } 
}