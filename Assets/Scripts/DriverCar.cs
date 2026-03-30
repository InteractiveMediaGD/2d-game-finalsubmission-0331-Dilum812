using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriveCar : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _frontTireRB;
    [SerializeField] private Rigidbody2D _backTireRB;
    [SerializeField] private Rigidbody2D _carRb;
    [SerializeField] private float _speed = 150f;
    [SerializeField] private float _rotationSpeed = 300f;

    private float _moveInput;

    private void Start()
    {
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
                if (suspension.frequency == 0) suspension.frequency = 5f;
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
    }

    private void Update()
    {
        _moveInput = Input.GetAxisRaw("Horizontal");

        // --- NEW: Falling Check (Game Over) ---
        if (transform.position.y < -15f)
        {
            if (MenuManager.Instance != null)
            {
                MenuManager.Instance.ShowGameOver();
            }
        }
    }

    private void FixedUpdate()
    {
        _frontTireRB.AddTorque(-_moveInput * _speed * Time.fixedDeltaTime);
        _backTireRB.AddTorque(-_moveInput * _speed * Time.fixedDeltaTime);
        _carRb.AddTorque(_moveInput * _rotationSpeed * Time.fixedDeltaTime);
    }
}