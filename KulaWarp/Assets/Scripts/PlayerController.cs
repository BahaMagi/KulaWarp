﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject mainCamera;
    private CameraController m_cc;

    public float speed = 3f; // Maximum speed of the player
    public float dampTime = 0.1f; // Dampening strength of the EaseIn and EaseOut. 

    [HideInInspector] public Vector3 world_direction;
    [HideInInspector] public Vector3 world_up;
    [HideInInspector] public bool isMoving;

    private SphereCollider m_sphereCollider;
    private Rigidbody m_rb;


    private Vector3 m_targetPosition;
    private float m_remainingDistance;
    private float m_circum;
    private float m_rotConst;

    void Start()
    {
        m_sphereCollider = GetComponent<SphereCollider>();
        m_rb = GetComponent<Rigidbody>();
        m_cc = mainCamera.GetComponent<CameraController>();

        world_direction = Vector3.right;
        world_up = Vector3.up;

        isMoving = false;

        m_circum = 2 * Mathf.PI * m_sphereCollider.radius * transform.lossyScale.x;
        m_rotConst = 360.0f / m_circum;
    }

    void FixedUpdate()
    {
        int input = HandleInput();

        if (input == 1)
            AttemptMove();
    }

    protected int HandleInput()
    {
        return (int)(Input.GetAxisRaw("Vertical")); // For keyboard input this is in {-1, 0, 1}
    }

    protected void AttemptMove()
    {
        // The player is not moving and the next block is valid.
        if (!isMoving && CanMove())
            StartCoroutine(SmoothMovemet());
        // The player is already moving and the block after the next is also valid.
        else if (isMoving && CanMove() && m_remainingDistance < 0.25f)
            m_targetPosition += world_direction;
        // The player is not moving and tries to move into invalid space. 
        else if (!isMoving && !CanMove())
            OnCantMove();
    }

    protected IEnumerator SmoothMovemet()
    {
        isMoving = true;

        // Set target position to the next block in front of the player. 
        // The result is rounded to avoid small deviations from the theoretical integer coordinates of the blocks due to Unity physics. 
        m_targetPosition = (transform.position + world_direction).Round(world_direction);
        m_remainingDistance = (Vector3.Scale(m_targetPosition, world_direction) - Vector3.Scale(transform.position, world_direction)).sqrMagnitude;
        Vector3 velocity = Vector3.zero;

        while (m_remainingDistance > 0.01f)
        {
            // Move to new position by using SmoothDamp to have an EaseIn and EaseOut effect. 
            // Note that SmoothDamp allows repositioning the target during the interpolation.
            m_rb.MovePosition(Vector3.SmoothDamp(transform.position, m_targetPosition, ref velocity, dampTime, speed));

            Vector3 pos = Vector3.Scale(transform.position, world_direction);
            Vector3 target = Vector3.Scale(m_targetPosition, world_direction) - pos;
            m_remainingDistance = target.x + target.y + target.z;

            // Rotate the sphere according to the movement. 
            float rot = (((pos.x + pos.y + pos.z) % m_circum) * m_rotConst) - 180;
            m_rb.MoveRotation(Quaternion.Euler(Vector3.Cross(world_direction * -rot, world_up)));

            yield return null;
        }

        isMoving = false;
    }

    protected bool CanMove()
    {
        return !m_cc.isMoving;
    }

    protected void OnCantMove()
    {
    }
}