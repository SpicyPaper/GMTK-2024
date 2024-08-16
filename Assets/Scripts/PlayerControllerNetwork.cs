using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;     // Speed of the player movement
    public float jumpForce = 5f;     // Force applied when the player jumps
    public LayerMask groundLayer;    // Layer that defines what is considered ground
    public Transform groundCheck;    // A point used to check if the player is on the ground
    public float groundCheckRadius = 0.3f; // Radius of the ground check

    private Rigidbody rb;            // Reference to the player's Rigidbody
    private bool isGrounded;         // Whether the player is on the ground or not

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Get the Rigidbody component attached to the player
    }

    void Update()
    {
        // Handle movement
        float moveInputX = Input.GetAxis("Horizontal");
        float moveInputZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveInputX + transform.forward * moveInputZ;
        rb.velocity = new Vector3(move.x * moveSpeed, rb.velocity.y, move.z * moveSpeed);

        // Check if the player is on the ground
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        // Handle jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}
