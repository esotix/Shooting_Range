using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class GunFire : MonoBehaviour
{
    [Tooltip("Le Prefab du projectile à tirer.")]
    public GameObject projectilePrefab;
    [Tooltip("L'objet vide positionné au bout du canon.")]
    public Transform firePoint;

    // NOUVEAU : Référence au système de particules enfant
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

        // Optionnel : Vérifier si le Muzzle Flash est assigné
        if (muzzleFlash == null && firePoint != null)
        {
            // Tente de trouver le composant directement sur le FirePoint si non assigné
            muzzleFlash = firePoint.GetComponentInChildren<ParticleSystem>();
        }
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.activated.RemoveListener(FireWeapon);
        }
    }

    // Fonction de tir mise à jour
    public void FireWeapon(ActivateEventArgs args)
    {
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogError("Projectile Prefab ou Fire Point manquant sur l'arme.");
            return;
        }

        // --- NOUVELLE ÉTAPE : Jouer l'effet de particules ---
        if (muzzleFlash != null)
        {
            // Joue l'effet si le système de particules est assigné
            muzzleFlash.Play();
        }

        // --- Ancienne logique : Lancement du projectile ---
        GameObject newProjectileGO = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        Projectile projectileScript = newProjectileGO.GetComponent<Projectile>();

        if (projectileScript != null)
        {
            projectileScript.Launch(firePoint.forward);
        }
    }
}