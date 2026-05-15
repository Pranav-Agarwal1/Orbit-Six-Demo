using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float jumpSpeed = 20f;
    [SerializeField] float doubleJumpSpeed = 3f;
    [SerializeField] float squatSpeed = 1f;
    [SerializeField] public float normalSpeed = 10f;
    [SerializeField] ParticleSystem doubleJumpParticles;

    public float runSpeed;
    Rigidbody2D myRigidBody;
    Animator myAnimator;
    BoxCollider2D myBoxCollider;
    GroundDetection groundSensor;
    Vector2 moveInput;
    Vector2 originalSpriteSize;
    Vector2 originalSpriteOffset;
    bool canJump = true;
    bool canDoubleJump = false;
    bool isJumping = false;
    bool isRunning = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myRigidBody = GetComponent<Rigidbody2D>();
        myBoxCollider = GetComponent<BoxCollider2D>();
        myAnimator = GetComponent<Animator>();
        groundSensor = GetComponentInChildren<GroundDetection>(); // This is attached to the child object (ground detector)
        originalSpriteSize = myBoxCollider.size;
        originalSpriteOffset = myBoxCollider.offset;
        runSpeed = normalSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        Run();
        FlipSprite();
        UpdateJumpPermissions();
        UpdateAnimation();
    }

    void OnJump(InputValue value)
    {
        if (canJump && !isJumping)
        {
            StartCoroutine(JumpSquatRoutine());
        }
        else if (canDoubleJump && !canJump)
        {
            StartCoroutine(DoubleJumpRoutine());
        }
    }
    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
    void Run()
    {
        myRigidBody.linearVelocityX = moveInput.x * runSpeed;
        isRunning = Mathf.Abs(myRigidBody.linearVelocityX) > Mathf.Epsilon;
    }
    void FlipSprite()
    {
        bool hasHorizontalSpeed = Mathf.Abs(myRigidBody.linearVelocityX) > Mathf.Epsilon; // Compares abs(x velocity) to epsilon, the smallest possible value of a float. More stable than using 0
        if (hasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(myRigidBody.linearVelocityX), 1f); // Flips scale on x axis between -1 and 1 based on direction
        }
    }
    IEnumerator JumpSquatRoutine()
    {
        canJump = false;
        isJumping = true; // locks ground detection because you're mid-jump
        myAnimator.SetBool("isGrounded", false); // trying to hard code no shenanigans during jump squat

        // At the very start, resize for squat
        float newHeight = originalSpriteSize.y * (54f / 68f); // decreasing collider height by ratio of sprites
        float heightDifference = originalSpriteSize.y - newHeight;
        myBoxCollider.size = new Vector2(originalSpriteSize.x, newHeight); // Adjusts the size of the collider to be shorter
        // Move the collider UP by half the removed height
        transform.position = new Vector2(transform.position.x, transform.position.y -heightDifference / 2f); // Corrects for correct height, then adds tiny offset to stop from falling in floor
        myAnimator.SetTrigger("Jumping");
        runSpeed = squatSpeed;

        yield return new WaitUntil(() =>
        {
            AnimatorStateInfo state = myAnimator.GetCurrentAnimatorStateInfo(0); // defines a new variable "state" for animator info

            return state.IsName("Poncho_Jump") && state.normalizedTime >= 2f/9f; // confirms that jumping has been triggered and that right number of frames have passed
        });

        // After jump squat is over, carry out jump
        // Give velocity boost for jump
        myRigidBody.linearVelocityY = jumpSpeed;
        // Also restore original collider
        myBoxCollider.size = originalSpriteSize;
        myBoxCollider.offset = originalSpriteOffset;
        // Restore original player speed as well once squat ends
        runSpeed = normalSpeed;

        yield return new WaitForSeconds(.2f); // adds a small delay after actual jump portion before enabling ground detection

        isJumping = false;
        canJump = false;
    }
    IEnumerator DoubleJumpRoutine()
    {
        myAnimator.SetTrigger("DoubleJumping");
        yield return new WaitUntil(() =>
        {
            AnimatorStateInfo state = myAnimator.GetCurrentAnimatorStateInfo(0);

            return state.IsName("Poncho_DoubleJump") && state.normalizedTime >= 1f / 14f;
        });
        myRigidBody.linearVelocityY = doubleJumpSpeed;
        doubleJumpParticles.Play();
        canDoubleJump = false;
    }

    void UpdateAnimation()
    {
        if (!isJumping) // only consider ground detection if not actively jumping
        {
            myAnimator.SetBool("isGrounded", groundSensor.isGrounded); // if now grounded, sends jump or double jump back to idle
        }
        myAnimator.SetBool("isRunning", isRunning); // controls running vs idling
    }
    void UpdateJumpPermissions() // if touching the ground, refresh jumps
    {
        if (groundSensor.isGrounded)
        {
            canJump = true;
            canDoubleJump = true;
        }
        else
        {
            canJump = false; // being off ground eats first jump, but not double jump
        }
    }
}