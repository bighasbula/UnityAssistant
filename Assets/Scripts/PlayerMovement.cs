using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public float forwardSpeed = 10f;
    public float sideSpeed = 5f;
    public float jumpForce = 7f;
    public float rightLimit = 5.5f;
    public float leftLimit = -5.5f;

    private Rigidbody rb;
    private bool isGrounded = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        
        transform.Translate(Vector3.forward * Time.deltaTime * forwardSpeed, Space.World);

       
        if (Input.GetKey(KeyCode.A) && transform.position.x > leftLimit)
        {
            transform.Translate(Vector3.left * Time.deltaTime * sideSpeed);
        }
        if (Input.GetKey(KeyCode.D) && transform.position.x < rightLimit)
        {
            transform.Translate(Vector3.right * Time.deltaTime * sideSpeed);
        }

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    // Ground check
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}

