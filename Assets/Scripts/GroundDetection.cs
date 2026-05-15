using UnityEngine;

public class GroundDetection : MonoBehaviour
{
    BoxCollider2D feetCollider;
    PlayerMovement playerController;
    int groundLayer;
    public bool isGrounded;

    void Start()
    {
        feetCollider = GetComponent<BoxCollider2D>();
        groundLayer = LayerMask.GetMask("Platforms");
    }
    
    void Update()
    {
        isGrounded = feetCollider.IsTouchingLayers(groundLayer);
    }
}
