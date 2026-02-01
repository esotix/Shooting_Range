using UnityEngine;

// Controls player movement and rotation.
public class PlayerMove : MonoBehaviour
{
    public float speed = 2.0f; // Set player's movement speed.
    public float rotationSpeed = 10.0f; // Set player's rotation speed.

    private Rigidbody rb; // Reference to player's Rigidbody.

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody>(); // Access player's Rigidbody.
    }

    // Update is called once per frame
    void Update()
    {
        // Move player based on vertical input.
        float moveVertical = Input.GetAxis("Vertical");
        float moveHorizontal = Input.GetAxis("Horizontal");
        Vector3 movement = new Vector3(moveHorizontal, 0f, moveVertical) * speed * Time.deltaTime;
        rb.transform.position += movement;

        // Rotate player based on horizontal input.
        Vector3 rotation = new Vector3(0, Input.GetAxis("Mouse X"), 0) * Time.deltaTime * rotationSpeed;
        rb.transform.Rotate(rotation);

    }
}