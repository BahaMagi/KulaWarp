using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentController : MonoBehaviour
{
    public GameObject mainCamera, player;

    public float speed        = 5f; // Maximum speed of the player
    public float easeInTime   = 0.02f; // Dampening strength of the EaseIn. 

    [HideInInspector] public Vector3    world_direction, world_up; // Current World and Camera forwarwards/up direction
    [HideInInspector] public bool       isMoving;

    private SphereCollider   m_sphereCollider_player;
    private Rigidbody        m_rb;
    private Animator         m_animator;
    private CameraController m_cc;

    private int m_isMoving_Param_ID; // ID for parameter that triggers Idle -> Moving transition in animator. 

    private Vector3     m_targetPosition;
    private float       m_remainingDistance;
    private float       m_sphereRadius, m_boxsize; // Radius of the player sphere andenvironment box size. 
    private float       m_circum,       m_rotConst; // Circumference of player sphere and precomputed constant to speed up computation
    private int         m_envLayerMask; // Layermask to only check layer 10, i.e. the Environment layer. 

    void Start()
    {
        m_sphereCollider_player = player.GetComponent<SphereCollider>();
        m_rb                    = GetComponent<Rigidbody>();
        m_cc                    = mainCamera.GetComponent<CameraController>();
        m_animator              = GetComponent<Animator>();

        m_isMoving_Param_ID = Animator.StringToHash("isMoving");

        world_direction = Vector3.right;
        world_up        = Vector3.up;

        isMoving = false;

        m_sphereRadius  = m_sphereCollider_player.radius * player.transform.lossyScale.x;
        m_circum        = 2 * Mathf.PI * m_sphereRadius;
        m_rotConst      = 360.0f / m_circum;
        m_boxsize       = 1.0f;

        m_envLayerMask = 1 << 10; 
    }

    void FixedUpdate()
    {
        int input = HandleInput();

        if (input == 1)
            AttemptMove();

        if(isMoving)
            CheckPathUpwards();
    }

    protected int HandleInput()
    {
        return (int)(Input.GetAxisRaw("Vertical")); // For keyboard input this is in {-1, 0, 1}
    }

    protected void AttemptMove()
    {
        bool canMove = CanMove(out bool hitFront);

        // The player is not moving and there is no next block -> go down
        if (!isMoving && canMove && !hitFront)
            StartCoroutine(MoveDownwards());
        // The player is not moving and there is a next block (up or the same level) -> go forward (and evtl. up)
        else if (!isMoving && canMove && hitFront)
            StartCoroutine(MoveForwards());
        // The player is already moving and there is a next block (up or the same level) -> go forward
        else if (isMoving && canMove && m_remainingDistance < (m_boxsize * m_boxsize * 0.20f) && hitFront)
            m_targetPosition += world_direction;
        // The player is already moving and there is no next block -> prepare go down
        //else if (isMoving && canMove && m_remainingDistance < (m_boxsize * m_boxsize * 0.20f) && !hitFront)
            //m_targetPosition += world_direction;
        else if (!isMoving && !canMove)
            OnCantMove();
    }

    protected IEnumerator MoveForwards()
    {
        isMoving = true;
        m_animator.SetBool(m_isMoving_Param_ID, true);

        // Set target position to the next block in front of the player. 
        // The result is rounded to avoid small deviations from the theoretical integer coordinates of the blocks due to Unity physics
        // and the idle animation.
        Vector3 startPosition = transform.position;
        m_targetPosition      = SnapToGrid(transform.position + world_direction);
        m_targetPosition.Round(Vector3.one-world_up);
        m_remainingDistance   = (m_targetPosition - startPosition).sqrMagnitude;

        float t = 0.0f;
        bool arrived = false;

        while (!arrived)
        {
            m_rb.MovePosition(MyInterps.QuadEaseIn(startPosition, m_targetPosition, out arrived, t, easeInTime, speed));
            m_remainingDistance = (m_targetPosition - transform.position).sqrMagnitude;
            t += Time.deltaTime;

            // Rotate the sphere according to the movement. 
            Vector3 pos = Vector3.Scale(transform.position, world_direction);
            float rot = (((pos.x + pos.y + pos.z) % m_circum) * m_rotConst) - 180;
            player.transform.rotation = Quaternion.Euler(Vector3.Cross(world_direction * -rot, world_up));

            yield return null;
        }

        isMoving = false;
        m_animator.SetBool(m_isMoving_Param_ID, false);
    }

    protected void CheckPathUpwards()
    {
        Ray checkFront = new Ray(transform.position, world_direction);

        if (Physics.Raycast(checkFront, m_sphereRadius * 1.25f, m_envLayerMask))
        {
            m_targetPosition = m_targetPosition + (-0.5f - m_sphereRadius) * world_direction + (0.5f - m_sphereRadius) * world_up;
            Physics.gravity = 9.81f * world_direction;
            Vector3 tmp = world_direction;
            world_direction = world_up;
            world_up = -tmp;

            transform.rotation = Quaternion.FromToRotation(Vector3.up, world_up);

            StartCoroutine(m_cc.CameraUpDown(1));
        }
    }

    protected IEnumerator MoveDownwards()
    {
        isMoving = true;
        m_animator.SetBool(m_isMoving_Param_ID, true);

        // Set target position to the next block in front of the player. 
        // The result is rounded to avoid small deviations from the theoretical integer coordinates of the blocks due to Unity physics
        // and the idle animation.
        Vector3 startPosition = transform.position;
        m_targetPosition      = SnapToGrid(startPosition);
        m_targetPosition.Round(Vector3.one - world_up);
        m_targetPosition     += world_direction * 0.5f;
        m_remainingDistance   = m_boxsize * m_boxsize;

        float t = 0.0f;
        bool arrived = false;

        while (!arrived)
        {
            m_rb.MovePosition(MyInterps.QuadEaseIn(startPosition, m_targetPosition, out arrived, t, easeInTime, speed));
            m_remainingDistance = (m_targetPosition - transform.position).sqrMagnitude;
            t += Time.deltaTime;

            // Rotate the sphere according to the movement. 
            Vector3 pos = Vector3.Scale(transform.position, world_direction);
            float rot = (((pos.x + pos.y + pos.z) % m_circum) * m_rotConst) - 180.0f;
            player.transform.rotation = Quaternion.Euler(Vector3.Cross(world_direction * -rot, world_up));

            yield return null;
        }

        t = 0.0f;
        float dt = 0.0f;
        float angularSpeed = 90.0f*speed / (m_boxsize*0.5f);
        Vector3 contactPoint = transform.position - world_up * m_sphereRadius;
        m_rb.useGravity = false;

        // Start rotating the camera.
        StartCoroutine(m_cc.CameraUpDown(-1));

        while (t < 90.0f)
        {
            t += angularSpeed * Time.deltaTime;
            dt = angularSpeed * Time.deltaTime - Mathf.Min(0, 90.0f-t);

            transform.RotateAround(contactPoint, Vector3.Cross(world_up, world_direction), dt);

            yield return null;
        }

        Physics.gravity = -9.81f * world_direction;
        m_rb.useGravity = true;
        Vector3 tmp     = world_up;
        world_up        = world_direction;
        world_direction = -tmp;

        startPosition       = transform.position;
        m_targetPosition    = SnapToGrid(startPosition + world_direction * 0.5f);
        m_remainingDistance = (m_targetPosition - startPosition).sqrMagnitude;
        m_targetPosition.Round(Vector3.one - world_up);

        t = 0.0f;
        arrived = false;

        while (!arrived)
        {
            m_rb.MovePosition(MyInterps.QuadEaseIn(startPosition, m_targetPosition, out arrived, t, easeInTime, speed));
            m_remainingDistance = (m_targetPosition - transform.position).sqrMagnitude;
            t += Time.deltaTime;

            // Rotate the sphere according to the movement. 
            Vector3 pos = Vector3.Scale(transform.position, world_direction);
            float rot = (((pos.x + pos.y + pos.z) % m_circum) * m_rotConst) - 180.0f;
            player.transform.rotation = Quaternion.Euler(Vector3.Cross(world_direction * -rot, world_up));

            yield return null;
        }

        isMoving = false;
        m_animator.SetBool(m_isMoving_Param_ID, false);
    }

    /* The player can move forwards if:
    // a) There is a block on the same level in front of it -> next move forwards
    // b) There are no blocks in front, left or right, i.e. the player is on a tip -> next move downwards
    // c) There is a block infront of the player one level above the current level -> next move up. 
    //
    // This assumes that the levels are designed such that no 2x2 square of blocks is completely filled, i.e.
    // there are no plateaus. 
    // 
    // The forwards ray is angled slightly ahead to capture the 2nd next block when called while the player is moving. 
    //
    // Also, the player is restriced to only move when the camera is not rotating. 
    */
    protected bool CanMove(out bool isHitFront)
    {
        float       l           = 1.5f; // length of the rays sent. 
        Vector3     origin      = transform.position + (1-m_sphereRadius) * world_up;

        // Check the front first. Any hit allows us to move.
        Ray  front      = new Ray(origin, 1.1f*(world_direction - world_up));
        isHitFront      = Physics.Raycast(front, l, m_envLayerMask);

        // If there is something in front it is always possible to move. 
        if (isHitFront)
            return !m_cc.isMoving;

        // If there is no hit, then we can only move if left and right are empty. 
        Vector3 crossProd = Vector3.Cross(world_up, world_direction);
        Ray left          = new Ray(origin, crossProd - world_up);
        Ray right         = new Ray(origin, -crossProd - world_up);
        bool isHitLeft    = Physics.Raycast(left, l, m_envLayerMask);
        bool isHitRight   = Physics.Raycast(right, l, m_envLayerMask);

        return !(isHitRight || isHitLeft || m_cc.isMoving);
    }

    protected void OnCantMove()
    {
    }

    private Vector3 SnapToGrid(Vector3 vec)
    {
        return vec.Round(world_up) - (m_boxsize * 0.5f - m_sphereRadius) * world_up;
    }


    // Debug Outputs
    void Update()
    {
        // Green : Gravity
        // White : Target 
        // Red: world diretion
        // cyan: up
        // Blue :Check Path front
        // Black: Check path down
        // Gray: can move front

        Vector3 origin = transform.position + (1 - m_sphereRadius) * world_up;
        //Debug.DrawRay(origin, (1.1f*(world_direction - world_up)), Color.grey);
        //Debug.DrawRay(transform.position, world_up, Color.cyan);
        Debug.DrawRay(transform.position, m_targetPosition - transform.position); // white
        //Debug.DrawRay(transform.position, Physics.gravity, Color.green);
        //Debug.DrawRay(transform.position, world_direction, Color.red);
        //Debug.DrawRay(transform.position, world_direction * m_sphereRadius * 1.25f, Color.blue);
        //Debug.DrawRay(transform.position + 0.05f * world_direction, -world_up, Color.black);
    }
}