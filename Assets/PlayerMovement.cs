using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    bool isFacingRight = true;
    [Header("Movement")]
    public float moveSpeed = 5f;
    float horizontalMovement;

    [Header("Jumping")]
    public float jumpPower = 10f;
    public int maxJumps = 2;
    int jumpsRemaining;

    [Header("GroundCheck")]
    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.45f, 0.04f);
    public LayerMask groundLayer;
    bool isGrounded;

    [Header("WallCheck")]
    public Transform wallCheckPos;
    public Vector2 wallCheckSize = new Vector2(0.05f, 0.5f);
    public LayerMask wallLayer;

    [Header("Gravity")]
    public float baseGravity = 2f;
    public float maxFallSpeed = 18f;
    public float fallSpeedMultiplier = 2f;

    [Header("WallMovement")]
    public float wallSlideSpeed = 2;
    bool isWallSliding;

    //Wall Jumping
    bool isWallJumping;
    float wallJumpDirection;
    float wallJumpTime = 0.5f;
    float wallJumpTimer;
    public Vector2 wallJumpPower = new Vector2(5f, 10f);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GroundCheck();
        ProcessGravity();
        ProcessWallSlide();
        ProcessWallJump();
        if (!isWallJumping) {
            rb.velocity = new Vector2(horizontalMovement * moveSpeed, rb.velocity.y);
            Flip();
        }
    }

  
    public void Move(InputAction.CallbackContext context) {
        horizontalMovement = context.ReadValue<Vector2>().x;
    } 
    public void Jump(InputAction.CallbackContext context) {
        if (jumpsRemaining > 0) {
            if (context.performed) {
                //hold down = full power
                rb.velocity = new Vector2(rb.velocity.x, jumpPower);
                jumpsRemaining--;
            } else if (context.canceled && rb.velocity.y > 0) {
                //light tap  = half power
                rb.velocity = new Vector2(rb.velocity.x, 0.5f * rb.velocity.y);
                jumpsRemaining--;
            }
        }

        //wall jump
        if(context.performed && wallJumpTimer > 0f) {
            isWallJumping = true;
            rb.velocity = new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y);
            Debug.Log(rb.velocity);//jump away from wall
            wallJumpTimer = 0;
            //Force flip
            if(transform.localScale.x != wallJumpDirection) {
                isFacingRight = !isFacingRight;
                Vector3 ls = transform.localScale;
                ls.x *= -1f;
                transform.localScale = ls;
            }

            Invoke(nameof(CancelWallJump), wallJumpTime + 0.1f); //wall jump = 0.5f -- jump again = 0.6f;
                
        }
    }

    private void GroundCheck() {
        if(Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer)) {
            Debug.Log("Onground");
            jumpsRemaining = maxJumps;
            isGrounded = true;
        } else {
            isGrounded = false;
        }
    }

    private bool WallCheck() {
        return Physics2D.OverlapBox(wallCheckPos.position, wallCheckSize, 0, wallLayer);
    }
    private void ProcessGravity() {
        if (rb.velocity.y < 0) {
            rb.gravityScale = baseGravity * fallSpeedMultiplier; //fall with acceleration
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
        } else {
            rb.gravityScale = baseGravity;
        }
    }

    private void ProcessWallSlide() {
        //Not grounded & on a wall & movement !=0
        if (!isGrounded && WallCheck() && horizontalMovement !=0 ) {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -wallSlideSpeed)); //caps fall rate
        } else {
            isWallSliding = false;
        }
    }

    private void ProcessWallJump() {
        if (isWallSliding) {
            isWallJumping = false;
            wallJumpDirection = -transform.localScale.x;
            wallJumpTimer = wallJumpTime;
            CancelInvoke(nameof(CancelWallJump));
        } else if (wallJumpTimer > 0f) {
            wallJumpTimer -= Time.deltaTime;
        }
    }

    private void CancelWallJump() {
        isWallJumping = false;
    }

    private void Flip() {
        if(isFacingRight && horizontalMovement < 0 || !isFacingRight && horizontalMovement > 0) {
            isFacingRight = !isFacingRight;
            Vector3 ls = transform.localScale;
            ls.x *= -1f;
            transform.localScale = ls;
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(wallCheckPos.position, wallCheckSize);
    }
}
