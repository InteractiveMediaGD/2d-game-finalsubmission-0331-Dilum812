using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    private Transform player;

    [SerializeField] private Rigidbody2D frontTireRB;
    [SerializeField] private Rigidbody2D backTireRB;

    private Rigidbody2D rb;

    [Header("Physics Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 3f;
    [SerializeField] private float alignmentSpeed = 2f; // Smoother alignment to ground
    [SerializeField] private float torqueMultiplier = 40f; // Lower still to match player speed

    private void Awake()
    {
        // Some prefabs have a separate physics-based head (HingeJoint2D + Rigidbody2D)
        // that causes jittery "fast rotation". We'll strip those off at runtime.
        Transform head = transform.Find("driver-head");
        if (head != null)
        {
            // Destroy components to make head a simple child of the car body
            HingeJoint2D hj = head.GetComponent<HingeJoint2D>();
            if (hj != null) Destroy(hj);
            
            Rigidbody2D headRb = head.GetComponent<Rigidbody2D>();
            if (headRb != null) Destroy(headRb);
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Freeze rotation so the car is stable and doesn't flip,
        // but we can still manually rotate it to match the ground
        // --- NEW: Physics Stabilization ---
        // 1. Joint Auto-Repair: Set suspension anchors at runtime based on wheel positions
        WheelJoint2D[] joints = GetComponents<WheelJoint2D>();
        foreach (WheelJoint2D joint in joints)
        {
            if (joint.connectedBody != null)
            {
                // Set the anchor on the body to the current local position of the wheel
                joint.anchor = transform.InverseTransformPoint(joint.connectedBody.transform.position);
                joint.connectedAnchor = Vector2.zero; // Attach to center of wheel
                
                // Don't enable motor here as the scripts use AddTorque directly on the tires
                joint.useMotor = false;
                
                JointSuspension2D suspension = joint.suspension;
                if (suspension.frequency == 0) suspension.frequency = 5f; // Add bounciness if missing
                if (suspension.dampingRatio == 0) suspension.dampingRatio = 0.7f;
                joint.suspension = suspension;
            }
        }

        // 2. Ignore collisions between tires and the car body/other tires
        Collider2D[] carColliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D colA in carColliders)
        {
            foreach (Collider2D colB in carColliders)
            {
                if (colA != colB)
                {
                    if (colA.gameObject.name.ToLower().Contains("tire") || colB.gameObject.name.ToLower().Contains("tire"))
                    {
                        Physics2D.IgnoreCollision(colA, colB);
                    }
                }
            }
        }

        GameObject playerObj = GameObject.Find("VEHICLE");
        if (playerObj == null) playerObj = GameObject.FindGameObjectWithTag("Player");
        
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // Set initial facing direction once to avoid joint glitches every frame
        transform.localScale = new Vector3(-1, 1, 1);
    }

    private void FixedUpdate()
    {
        if (player != null && rb != null)
        {
            // Removed: transform.localScale = new Vector3(-1, 1, 1); 
            // Flipping every frame causes physics joint glitches. Moved to Start.

            // --- 1. Movement Logic ---
            if (transform.position.x > player.position.x)
            {
                // Move Left using wheel torque
                float rotationTorque = speed * torqueMultiplier; 
                
                if (frontTireRB != null) frontTireRB.AddTorque(rotationTorque * Time.fixedDeltaTime);
                if (backTireRB != null) backTireRB.AddTorque(rotationTorque * Time.fixedDeltaTime);
            }

            // --- 2. Dynamic Ground Alignment (Smooth Control) ---
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
            if (hit.collider != null)
            {
                // Calculate target angle from ground normal
                float targetAngle = Mathf.Atan2(hit.normal.y, hit.normal.x) * Mathf.Rad2Deg - 90f;
                // Move rotation smoothly to match ground
                float nextRotation = Mathf.LerpAngle(rb.rotation, targetAngle, alignmentSpeed * Time.fixedDeltaTime);
                rb.MoveRotation(nextRotation);
            }
        }
    }
}
