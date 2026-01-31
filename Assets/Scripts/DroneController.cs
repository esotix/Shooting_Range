using UnityEngine;
using System.Collections;


public class DroneController : MonoBehaviour
{
    public GameObject following;
    public Vector3 offset = new Vector2(0, 3);
    public float damping = 0.95f;
    public float maxSpeed = 5f;
    public float k = 10f;

    private Rigidbody rb;
    private Vector3 p;
    private Vector3 r;
    private Vector3 F;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        transform.position = following.transform.position + offset;
        StartCoroutine(Follow());
    }

    IEnumerator Follow()
    {
        while (following != null)
        {
            p = transform.position;
            Vector3 rotatedOffset = following.transform.TransformDirection(offset);

            r = following.transform.position + rotatedOffset;
            F = -k * (p - r);

            rb.AddForce(F / rb.mass, ForceMode.Force);

            rb.linearVelocity *= damping;
            rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maxSpeed);

            yield return new WaitForFixedUpdate();
        }
    }
}
