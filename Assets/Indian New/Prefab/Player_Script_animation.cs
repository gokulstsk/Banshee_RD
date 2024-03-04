using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Script_animation : MonoBehaviour
{
    public float walkSpeed = 10f;
    public float runSpeed = 25f;
    public float jumpForce = 10f;
    public float bulletForce = 5f;

    private Rigidbody2D rb;
    private bool isGrounded;
    private LayerMask groundLayer;
    private Animator animator;
    private bool isFacingRight = false;
    private bool isShooting = false;
    private bool IsJumping = false;

    public GameObject bulletPrefab;
    public Transform firePoint;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        groundLayer = LayerMask.GetMask("Ground");  // Replace "Ground" with the name of your ground layer
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Shoot();
        // Check if the player is grounded
        isGrounded = Physics2D.OverlapCircle(transform.position, 0.2f, groundLayer);

        // Player movement
        float horizontalInput = Input.GetAxis("Horizontal");
        bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // Adjust speed based on running
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        Vector2 moveDirection = new Vector2(horizontalInput, 0f);
        rb.velocity = new Vector2(moveDirection.x * currentSpeed, rb.velocity.y);

        // Set walking or running animation parameter
        animator.SetFloat("Speed", Mathf.Abs(moveDirection.x));

        // Set the correct animation based on the speed
        if (isRunning)
        {
            animator.SetBool("IsRunning", true);
            animator.SetBool("IsWalking", false);
            Shoot();
        }
        else
        {
            animator.SetBool("IsWalking", true);
            animator.SetBool("IsRunning", false);
            Shoot();
        }

        if (horizontalInput > 0 && !isFacingRight || horizontalInput < 0 && isFacingRight)
        {
            Flip();
        }

        Jump();
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    void Shoot()
    {
        // Set the shooting animation parameter only when transitioning from not shooting to shooting
        if (Input.GetKeyDown(KeyCode.F))
        {
            // Set the shooting animation trigger
            animator.SetBool("Shoot", true);
            
        }

        else if (Input.GetKeyUp(KeyCode.F))
        {
            // Reset shooting animation trigger (if needed)
            
            animator.SetBool("Shoot", false);

        }
    }
    void Jump()
    {
        // if (IsJumping)
        // {

        // Player jump
        if (Input.GetButtonDown("Jump"))
        {
            
            IsJumping = true;
            animator.SetBool("IsJumping", IsJumping);
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
        else
        {
            // Reset jumping animation parameter when not jumping
            IsJumping = false;
            animator.SetBool("IsJumping", IsJumping);
            
        }
        // }

    }
}


