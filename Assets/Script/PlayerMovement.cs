
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Animator animator;

    private Vector2 movement;
    private Vector2 lastDirection = Vector2.down;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Membaca input keyboard
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Mencegah gerakan diagonal lebih cepat
        movement = movement.normalized;

        // Menentukan arah terakhir player
        if (movement != Vector2.zero)
        {
            if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
            {
                lastDirection = new Vector2(Mathf.Sign(movement.x), 0);
            }
            else
            {
                lastDirection = new Vector2(0, Mathf.Sign(movement.y));
            }
        }

        // Mengirim informasi ke Animator
        animator.SetBool("IsMoving", movement != Vector2.zero);

        animator.SetFloat("DirectionX", lastDirection.x);
        animator.SetFloat("DirectionY", lastDirection.y);
    }

    void FixedUpdate()
    {
        // Menggerakkan player
        rb.MovePosition(
            rb.position + movement * moveSpeed * Time.fixedDeltaTime
        );
    }
}
