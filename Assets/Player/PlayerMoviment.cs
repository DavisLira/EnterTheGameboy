using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Pega o input WASD
        movement.x = Input.GetAxisRaw("Horizontal");  // A e D
        movement.y = Input.GetAxisRaw("Vertical");    // W e S

        movement = movement.normalized; // evita andar mais rápido na diagonal
    }

    void FixedUpdate()
    {
        // Move o player fisicamente
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}
