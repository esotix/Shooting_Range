using UnityEngine;

public class WeaponEquip : MonoBehaviour
{
    [Header("References")]
    public GameObject weapon;
    public Transform weaponSocket;
    public Transform weaponHolster;

    [Header("Settings")]
    public KeyCode equipKey = KeyCode.E;

    private Animator animator;
    public bool isEquipped = false;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (weapon != null && weaponHolster != null)
        {
            weapon.transform.SetParent(weaponHolster);
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(equipKey))
        {
            ToggleEquip();
        }
    }

    public void ToggleEquip()
    {
        animator.SetTrigger("Equip");

        isEquipped = !isEquipped;
        animator.SetBool("IsEquipped", isEquipped);
        Invoke(nameof(AttachWeapon), 0.5f);
    }

    public void AttachWeapon()
    {
        if (weapon == null) return;

        if (isEquipped)
        {
            weapon.transform.SetParent(weaponSocket);
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;

            Debug.Log("Arme équipée !");
        }
        else
        {
            weapon.transform.SetParent(weaponHolster);
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;

            Debug.Log("Arme rangée !");
        }
    }
}