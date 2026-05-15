using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttacking : MonoBehaviour
{
    [SerializeField] float attackSpeed = 1f;
    [SerializeField] ParticleSystem attackParticles;

    bool canAttack = true;
    Animator myAnimator;
    PlayerMovement playerMoveController;

    void Start()
    {
        myAnimator = GetComponent<Animator>();
        playerMoveController = GetComponent<PlayerMovement>();
    }
    void OnAttack(InputValue value)
    {
        if (canAttack)
        {
            myAnimator.SetBool("isAttacking", true);
        }

    }

    public void StartAttack()
    {
        canAttack = false;
        playerMoveController.runSpeed = attackSpeed;
    }
    public void MiddleAttack()
    {
        playerMoveController.runSpeed = 0f;
        attackParticles.Play();
    }
    public void EndAttack()
    {
        canAttack = true;
        myAnimator.SetBool("isAttacking", false);
        playerMoveController.runSpeed = playerMoveController.normalSpeed;
    }
}
