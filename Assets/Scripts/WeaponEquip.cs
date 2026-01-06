using UnityEngine;

public class WeaponEquip : MonoBehaviour
{
    [Header("References")]
    public GameObject weapon;              // L'arme à équiper
    public Transform weaponSocket;         // Main droite
    public Transform weaponHolster;        // Dos/hanche

    [Header("Settings")]
    public KeyCode equipKey = KeyCode.E;   // Touche pour équiper (test)

    private Animator animator;
    private bool isEquipped = false;

    void Start()
    {
        animator = GetComponent<Animator>();

        // Place l'arme au holster au départ
        if (weapon != null && weaponHolster != null)
        {
            weapon.transform.SetParent(weaponHolster);
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;
        }
    }

    void Update()
    {
        // Test avec la touche E
        if (Input.GetKeyDown(equipKey))
        {
            ToggleEquip();
        }
    }

    public void ToggleEquip()
    {
        // Déclenche l'animation
        animator.SetTrigger("Equip");

        // Change l'état
        isEquipped = !isEquipped;
        animator.SetBool("IsEquipped", isEquipped);

        // Attache l'arme immédiatement (méthode simple)
        // Pour la méthode avec Animation Event, commente cette ligne
        Invoke(nameof(AttachWeapon), 0.5f); // Délai de 0.5s
    }

    // Appelée par Animation Event OU par Invoke
    public void AttachWeapon()
    {
        if (weapon == null) return;

        if (isEquipped)
        {
            // Équiper : attacher à la main
            weapon.transform.SetParent(weaponSocket);
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;

            Debug.Log("Arme équipée !");
        }
        else
        {
            // Ranger : remettre au holster
            weapon.transform.SetParent(weaponHolster);
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;

            Debug.Log("Arme rangée !");
        }
    }
}