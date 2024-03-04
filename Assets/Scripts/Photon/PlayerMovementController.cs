using Photon.Pun;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A component that handles all player movement.
/// </summary>
public class PlayerMovementController : MonoBehaviour
{
    
    [Header("Movement")]
    public float MovementSpeed = 100.0f;
    public float GroundedBufferTime = 0.15f;
    public float walkSpeed = 10f;
    public float runSpeed = 25f;
    // public ParticleSystem FootstepParticles;

    [Header("Jumping")]
    public float JumpBufferTime = 0.5f;
    public float JumpForce = 400.0f;
    private bool IsJumping = false;
    public float GravityScale = 100.0f;
    public float FallGravityMultiplier = 3.0f;
   // public ParticleSystem LandingParticles;

    [Header("Ground Collision")]
    public Transform FeetPosition;
    public LayerMask GroundLayer;

    [Header("Projectile Collision")]
  //  public ParticleSystem BloodParticles;
    public float KnockbackForce = 2000f;
    public float KnockbackTime = 0.25f;

    private Rigidbody2D r;
    public Animator PlayerAnimator;

    public AnimatorController IdleAnimatorController;
    public AnimatorController GunHoldAnimatorController;
    public AnimatorController katanaHoldAnimatorController;

    private float horizontalMovement;
    private int direction = 1;
    private bool jump;
    private bool jumpHeld;
    private bool isGrounded;
    private float jumpTimer;
    private float groundedTimer;
    private float knockbackTimer = 0f;
    private bool falling;
    private PhotonView photonView;

    public float ResetTimeAmount = 1f;
    private BansheePlayer BP;
   // private ParticleSystem.EmissionModule footstepEmission;

    /// <summary>
    /// Called by Unity when this GameObject starts.
    /// </summary>
    private void Start()
    {
        r = GetComponent<Rigidbody2D>();
        photonView = GetComponent<PhotonView>();
        BP = GetComponent<BansheePlayer>();
        PlayerAnimator.runtimeAnimatorController = IdleAnimatorController;
       // footstepEmission = FootstepParticles.emission;

    }
    public void SetAnimator()
    {
        //SetAnimator
        if (BP.isIdle == true && BP.isGunEquipped == false && BP.isSwordEquipped == false)
        {
            PlayerAnimator.runtimeAnimatorController = IdleAnimatorController;
        }
        else if (BP.isIdle == false && BP.isGunEquipped == true && BP.isSwordEquipped == false)
        {
            PlayerAnimator.runtimeAnimatorController = GunHoldAnimatorController;
        }
        else if (BP.isIdle == false && BP.isGunEquipped == false && BP.isSwordEquipped == true)
        {
            PlayerAnimator.runtimeAnimatorController = katanaHoldAnimatorController;
        }
        else
        {
            PlayerAnimator.runtimeAnimatorController = IdleAnimatorController;
        }
    }
    /// <summary>
    /// Called by Unity every frame.
    /// </summary>
    private void Update()
    {
        if(photonView.IsMine)
        {

            
                //bool isRunning = Mathf.Abs(horizontalMovement) > 0.1f;
                //PlayerAnimator.SetBool("isMoving", isRunning);
                //PlayerAnimator.SetFloat("Horizontal", Mathf.Abs(horizontalMovement));

                // Player movement
                float horizontalInput = Input.GetAxis("Horizontal");
            //bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool isRunning = true;
            // Adjust speed based on running
            //float currentSpeed = isRunning ? runSpeed : walkSpeed; // 0 OR 1
            float currentSpeed = runSpeed;
            Vector2 moveDirection = new Vector2(horizontalInput, 0f);
            r.velocity = new Vector2(moveDirection.x * currentSpeed, r.velocity.y);

            // Set walking or running animation parameter
            PlayerAnimator.SetFloat("Speed", Mathf.Abs(moveDirection.x));
            Debug.Log(moveDirection.x * currentSpeed + "MD");

            // Set the correct animation based on the speed
            if (isRunning)
            {
                PlayerAnimator.SetBool("IsRunning", true);
                PlayerAnimator.SetBool("IsWalking", false);
                //Shoot();
            }
            else
            {
                PlayerAnimator.SetBool("IsWalking", true);
                PlayerAnimator.SetBool("IsRunning", false);
                //Shoot();
            }

            //idle
            if (horizontalInput == 0)
            {
                PlayerAnimator.SetBool("IsWalking", false);
                PlayerAnimator.SetBool("IsRunning", false);
            }

            if(IsJumping)
            {
                PlayerAnimator.SetBool("IsJumping", IsJumping);
                PlayerAnimator.SetBool("IsWalking", false);
                PlayerAnimator.SetBool("IsRunning", false);
            }
            else
            {
                PlayerAnimator.SetBool("IsJumping", IsJumping);
            }

            if (jump)
            {
                jumpTimer = Time.time + JumpBufferTime;
                
            }
            

            // footstepEmission.rateOverTime = 0f;

            if (horizontalMovement != 0)
            {
                direction = horizontalMovement < 0 ? -1 : 1;

                if (isGrounded)
                {
                    // footstepEmission.rateOverTime = 20f;
                }
            }
        }
    }

    public void Shoot()
    {
        // Set the shooting animation parameter only when transitioning from not shooting to shooting
        if (Input.GetKeyDown(KeyCode.F))
        {
            // Set the shooting animation trigger
            PlayerAnimator.SetBool("Shoot", true);

        }

        else if (Input.GetKeyUp(KeyCode.F))
        {
            // Reset shooting animation trigger (if needed)

            PlayerAnimator.SetBool("Shoot", false);

        }
    }
    /// <summary>
    /// Called by Unity at a fixed tick rate based on the physics settings.
    /// </summary>
    private void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            CheckIfFalling();
            CheckIfGrounded();
           // HandleMovement();
            HandleJumping();
            ModifyPhysics();
        }
    }

   
    public void PlayerHitsbyBullet(int direction)
    {
        // Add a knockback force to this player based on the direction the projectile was travelling.
        r.velocity = new Vector2(0, r.velocity.y);
        r.AddForce(new Vector2(direction * KnockbackForce, 0), ForceMode2D.Impulse);
        knockbackTimer = Time.time + KnockbackTime;

        // Play the blood particle effect.
       // BloodParticles.Play();

        // Trigger the hit animation.
        //PlayerAnimator.SetTrigger("Hit");
    }

    /// <summary>
    /// Checks to see if the player is falling and sets the falling variable.
    /// </summary>
    private void CheckIfFalling()
    {
        // Cache the current state of the falling variable.
        var wasFalling = falling == true;

        // Set the new falling state based on whether the player's y velocity is below 0.
        falling = r.velocity.y < 0;

        // If the player is not falling, trigger the Land animation event.
        // We don't check if they were already falling here due to a bug with the animation controller that sometimes locks the player in a falling animation
        // if they hold left/right when landing on a platform.
        if (!falling)
        {
           // PlayerAnimator.SetTrigger("Land");
            
        }

        // If the player wasn't falling but is now falling, trigger the Fall animation event.
        if (!wasFalling && falling)
        {
           // PlayerAnimator.SetTrigger("Fall");
           
        }
    }

    /// <summary>
    /// Checks to see if a player is on the ground and sets the isGrounded variable.
    /// </summary>
    private void CheckIfGrounded()
    {
       
        // Check if the player was grounded.
        var wasGrounded = isGrounded == true;

        // Perform a physics circle overlap from the FeetPosition to check if there is a collision with the ground.
        var collider = Physics2D.OverlapCircle(FeetPosition.transform.position, 0.5f, GroundLayer);

        // Set the isGrounded value.
        isGrounded = collider != null;
       // Debug.Log(isGrounded + " :isgrounded");

        // If the player is grounded, set the groundedTimer to the maximum grounded buffer time - this controls Hang Time.
        // If the player is not grounded, start reducing the groundedTimer.
        if (isGrounded)
        {
            groundedTimer = GroundedBufferTime;
        }
        else
        {
            groundedTimer -= Time.deltaTime;
        }

        // If the player wasn't grounded but now is, play the landing particle effect.
        if (!wasGrounded && isGrounded)
        {
           // LandingParticles.Play();
        }
    }

    /// <summary>
    /// Sets the player's x velocity based on horizontal input.
    /// </summary>
    private void HandleMovement()
    {
        // If the time has not surpassed the existing knockback timer, don't allow the player to move horizontally yet.
        if (Time.time < knockbackTimer)
        {
            return;
        }

        // Set the player's new velocity based on their horizontal input and their existing y velocity.
        r.velocity = new Vector2(horizontalMovement * MovementSpeed, r.velocity.y);
    }

    /// <summary>
    /// Sets the player's y velocity based on whether the player has initated a jump.
    /// </summary>
    private void HandleJumping()
    {
        // If the `jumpTimer` is greater than the current time and the groundedTimer is greater than 0 then the player has requested to jump and they're still allowed to
        // so activate the jump.
        if (jumpTimer > Time.time && groundedTimer > 0)
        {
            // Set the player's new velocity based on their existing x velocity and the JumpForce.
            r.velocity = new Vector2(r.velocity.x, JumpForce);

            ResetTimeAmount -= Time.deltaTime;

            if (ResetTimeAmount <= 0)
            {
                // Reset the jumpTimer and groundedTimer.
                jumpTimer = 0;
                groundedTimer = 0;
                ResetTimeAmount = 0.2f;

            }


            IsJumping = true;
            // Play the jump animation.
            //PlayerAnimator.SetTrigger("Jump");
            

        }
        else
        {
            IsJumping = false;
            
        }
    }

    /// <summary>
    /// Modifies the player's RigidBody2D `gravityScale` based on their current state.
    /// </summary>
    private void ModifyPhysics()
    {
        // If the player is grounded, set their gravityScale to 0.
        if (isGrounded)
        {
            r.gravityScale = 0;
        }
        else
        {
            // Otherwise, reset the gravity scale.
            r.gravityScale = GravityScale;

            // If the player is falling, apply the fall gravity multiplier.
            if (r.velocity.y < 0)
            {
                r.gravityScale = GravityScale * FallGravityMultiplier;
            }
            // If the player is jumping and holding the jump key, apply half the default gravity scale to allow for a higher jump.
            else if (r.velocity.y > 0 && jumpHeld)
            {
                r.gravityScale = GravityScale / 2;
            }
        }
    }

    /// <summary>
    /// Sets the player's `horizontalMovement` value.
    /// </summary>
    /// <param name="value">The new value.</param>
    public void SetHorizontalMovement(float value)
    {
        horizontalMovement = value;
    }

    /// <summary>
    /// Sets the player's `jump` value.
    /// </summary>
    /// <param name="value">The new value.</param>
    public void SetJump(bool value)
    {
        jump = value;
    }


    /// <summary>
    /// Sets the player's `jumpHeld` value.
    /// </summary>
    /// <param name="value">The new value.</param>
    public void SetJumpHeld(bool value)
    {
        jumpHeld = value;
    }

    /// <summary>
    /// Gets the player's current direction.
    /// </summary>
    /// <returns>The direction where -1 is left and 1 is right.</returns>
    public int GetDirection()
    {
        return direction;
    }

    /// <summary>
    /// Plays the death animation.
    /// </summary>
    public void PlayDeathAnimation()
    {
        PlayerAnimator.SetBool("Dead", true);
    }
}