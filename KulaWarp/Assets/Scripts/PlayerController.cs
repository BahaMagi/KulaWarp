using System.Collections;
using UnityEngine;

/**
 * Takes care of the core movement mechanics of the player. The player consists of two gameobjects.
 * An (invisible) box which is set as parent for a sphere. This allows using the box (which does not 
 * rotate) in animations.
 * This script is attached to the parent player, i.e. the box. Thus, to access the sphere use
 * the gameobject reference @player_sphere. 
 */
public class PlayerController : MonoBehaviour
{
    #region
    public GameObject mainCamera, player_sphere;

    public float speed        = 5f; // Maximum speed of the player
    public float easeInTime   = 0.02f; // Dampening strength of the EaseIn. 

    //[HideInInspector] public Vector3 world_direction, world_up; // Current World and Camera forwarwards/up direction
    [HideInInspector] public bool       isMoving;

    private SphereCollider   m_sphereCollider_player;
    private Rigidbody        m_rb;
    private Animator         m_animator;
    private CameraController m_cc;

    private int m_isMoving_Param_ID; // ID for parameter that triggers Idle -> Moving transition in animator. 

    private Vector3     m_RotationAngles; // Stores the current rotation angles of the sphere in world axis coordinates
    private Vector3     m_targetPosition;
    private float       m_remainingDistance; // Distance between the current position and the current target position. Used to smoothen the coninuation of movement.
    private float       m_sphereRadius, m_boxsize; // Radius of the player sphere and environment box size. 
    private float       m_circum,       m_rotConst; // Circumference of player sphere and precomputed constant to speed up computation
    private int         m_envLayerMask; // Layermask to only check layer 10, i.e. the Environment layer, for collisions. 

    #endregion

    void Start()
    {
        isMoving = false;

        m_sphereCollider_player = player_sphere.GetComponent<SphereCollider>();
        m_rb                    = GetComponent<Rigidbody>();
        m_cc                    = mainCamera.GetComponent<CameraController>();
        m_animator              = GetComponent<Animator>();

        m_isMoving_Param_ID = Animator.StringToHash("isMoving"); // @TODO Check if there is another way of doing this that is not string search based. 

        m_sphereRadius  = m_sphereCollider_player.radius * player_sphere.transform.lossyScale.x;
        m_circum        = 2 * Mathf.PI * m_sphereRadius;
        m_rotConst      = 360.0f / m_circum;
        m_boxsize       = 1.0f;
        m_envLayerMask  = 1 << 10;

        m_RotationAngles = Vector3.zero;
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

    /**
     * Checks if the player can move forwards and based on the topology of the level decides to go 
     * forwards or downwards. For more infromation see @CanMove()
     */
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
            m_targetPosition += m_cc.world_direction;
        // The player is already moving and there is no next block -> prepare go down
        //else if (isMoving && canMove && m_remainingDistance < (m_boxsize * m_boxsize * 0.20f) && !hitFront)
            //m_targetPosition += world_direction;
        else if (!isMoving && !canMove)
            OnCantMove();
    }

    /**
     * When intially called, this function assumes that there is a next block on the same level. 
     * Once the co-routine has started, the @m_targetPosition variable can be changed and the 
     * movement will be continued/adjusted accordingly until the target position is reached. 
     * 
     * The 
     */
    protected IEnumerator MoveForwards()
    {
        setisMoving(true);

        // Set target position to the next block in front of the player. 
        // The result is rounded to avoid small deviations from the theoretical integer coordinates of the blocks due to Unity physics
        // and the idle animation.
        Vector3 startPosition = transform.position;
        m_targetPosition      = SnapToGrid(transform.position + m_cc.world_direction);
        m_targetPosition.Round(Vector3.one-m_cc.world_up);

        m_remainingDistance   = (m_targetPosition - startPosition).sqrMagnitude;

        float t = 0.0f;
        bool arrived = false;

        while (!arrived)
        {
            m_rb.MovePosition(MyInterps.QuadEaseIn(startPosition, m_targetPosition, out arrived, t, easeInTime, speed));
            m_remainingDistance = (m_targetPosition - transform.position).sqrMagnitude;
            t += Time.deltaTime;

            RotatePlayerSphere();

            // Check with a ray cast whether the sphere has to move up. 
            Ray checkFront = new Ray(transform.position, m_cc.world_direction);

            if (Physics.Raycast(checkFront, m_sphereRadius * 1.1f, m_envLayerMask))
            {
                StartCoroutine(MoveUpwards());
                yield break;
            }

            yield return null;
        }

        setisMoving(false);
    }

    /**
    * This assumes that the player can move and that there is no following block, i.e. the sphere 
    * has to roll down and change gravity. 
    * This is done in 3 steps:
    * 1) Roll to the edge of the current box.
    * 2) Rotate 90° around that edge.
    * 3) Move to the center of that new face of the box. 
    */
    protected IEnumerator MoveDownwards()
    {
        setisMoving(true);

        // Phase 1: Move to the edge of the box.

        // Set target position to the next block in front of the player. 
        // The result is rounded to avoid small deviations from the theoretical integer coordinates of the blocks due to Unity physics
        // and the idle animation.
        Vector3 startPosition = transform.position;
        m_targetPosition      = SnapToGrid(startPosition);
        m_targetPosition.Round(Vector3.one - m_cc.world_up);
        m_targetPosition     += m_cc.world_direction * 0.5f;
        m_remainingDistance   = m_boxsize * m_boxsize;

        float t      = 0.0f;
        bool arrived = false;

        while (!arrived)
        {
            m_rb.MovePosition(MyInterps.QuadEaseIn(startPosition, m_targetPosition, out arrived, t, easeInTime, speed));

            m_remainingDistance = (m_targetPosition - transform.position).sqrMagnitude;
            t                  += Time.deltaTime;

            RotatePlayerSphere();

            yield return null;
        }

        // Phase 2: Rotate around the edge.

        t                  = 0.0f;
        float dt           = 0.0f;
        float angularSpeed = 90.0f * speed / (m_boxsize * 0.5f); // @TODO That 90 is arbitrary. Make this a public var so its editable from the inspector.
                                                                 // @TODO also: the m_boxsize*0.5 should be m_sphereRadius*2 in theory. Want to test that again.
        Vector3 contactPoint = transform.position - m_cc.world_up * m_sphereRadius;
        m_rb.useGravity      = false; // Turn off gravity while rotating to avoid sliding.

        // We need to calculate the new dir and up values here as the co-routine is not guaranteed to finish before values are used.
        Vector3 tmpUp  = m_cc.world_direction;
        Vector3 tmpDir = -m_cc.world_up;
        // Start rotating the camera.
        StartCoroutine(m_cc.CameraUpDown(-1));

        while (t < 90.0f)
        {
            t += angularSpeed * Time.deltaTime;
            dt = angularSpeed * Time.deltaTime - Mathf.Min(0, 90.0f - t);

            transform.RotateAround(contactPoint, Vector3.Cross(-tmpDir, tmpUp), dt);

            yield return null;
        }

        // Phase 3: Move to the center of the face of the box. 

        // Turn gravity back on and change it to the new direction.
        Physics.gravity = -9.81f * tmpUp;
        m_rb.useGravity = true;

        startPosition       = transform.position;
        m_targetPosition    = SnapToGrid(startPosition + tmpDir * 0.5f, tmpUp);
        m_remainingDistance = (m_targetPosition - startPosition).sqrMagnitude;
        m_targetPosition.Round(Vector3.one - tmpUp);

        t       = 0.0f;
        arrived = false;

        while (!arrived)
        {
            m_rb.MovePosition(MyInterps.QuadEaseIn(startPosition, m_targetPosition, out arrived, t, easeInTime, speed));

            m_remainingDistance = (m_targetPosition - transform.position).sqrMagnitude;
            t                  += Time.deltaTime;

            RotatePlayerSphere(tmpDir, tmpUp);

            yield return null;
        }

        setisMoving(false);
    }

    protected IEnumerator MoveUpwards()
    {
        Vector3 startPosition = transform.position;
        m_targetPosition      = m_targetPosition - m_cc.world_direction * (0.5f * m_boxsize + m_sphereRadius) + m_cc.world_up * (-m_sphereRadius + 0.5f * m_boxsize);
        Physics.gravity       = 9.81f * m_cc.world_direction;

        m_remainingDistance = (m_targetPosition - startPosition).sqrMagnitude;

        Vector3 tmpUp  = -m_cc.world_direction;
        Vector3 tmpDir = -m_cc.world_up;

        StartCoroutine(m_cc.CameraUpDown(1));
        
        float t      = 0.0f;
        bool arrived = false;

        while (!arrived)
        {
            m_rb.MovePosition(MyInterps.QuadEaseIn(startPosition, m_targetPosition, out arrived, t, easeInTime, speed));
            m_remainingDistance = (m_targetPosition - transform.position).sqrMagnitude;
            t += Time.deltaTime;

            RotatePlayerSphere(tmpDir, tmpUp);

            yield return null;
        }

        transform.rotation = Quaternion.FromToRotation(Vector3.up, tmpUp);
        setisMoving(false);
    }

    /**
     * Rotates the player sphere according to its current position in world space according to the world direction.
     */
    protected void RotatePlayerSphere()
    {
        float targetTheta = ((transform.position.getComponent(m_cc.world_direction) % m_circum) * m_rotConst) - 180.0f;
        float dTheta      = targetTheta - m_RotationAngles.getComponent(m_cc.world_direction);
        m_RotationAngles.setComponent(m_cc.world_direction, targetTheta);
        
        player_sphere.transform.RotateAround(player_sphere.transform.position, Vector3.Cross(m_cc.world_up, m_cc.world_direction), dTheta* m_cc.world_direction.getComponent(m_cc.world_direction));
    }

    protected void RotatePlayerSphere(Vector3 dir, Vector3 up)
    {
        float targetTheta = ((transform.position.getComponent(dir) % m_circum) * m_rotConst) - 180.0f;
        float dTheta = targetTheta - m_RotationAngles.getComponent(dir);
        m_RotationAngles.setComponent(dir, targetTheta);

        player_sphere.transform.RotateAround(player_sphere.transform.position, Vector3.Cross(up, dir), dTheta * dir.getComponent(dir));
    }

    /** The player can move forwards if:
     * a) There is a block on the same level in front of it -> next move forwards
     * b) There are no blocks in front, left or right, i.e. the player is on a tip -> next move downwards
     * c) There is a block infront of the player one level above the current level -> next move up. 
     *
     * This assumes that the levels are designed such that no 2x2 square of blocks is completely filled, i.e.
     * there are no plateaus. 
     * 
     * The forwards ray is angled slightly ahead to capture the 2nd next block when called while the player is moving. 
     *
     * Also, the player is restriced to only move when the camera is not rotating. 
     */
    protected bool CanMove(out bool isHitFront)
    {
        float       l           = 1.5f; // length of the rays sent. 
        Vector3     origin      = transform.position + (1-m_sphereRadius) * m_cc.world_up;

        // Check the front first. Any hit allows us to move.
        Ray  front      = new Ray(origin, 1.1f*(m_cc.world_direction - m_cc.world_up));
        isHitFront      = Physics.Raycast(front, l, m_envLayerMask);

        // If there is something in front it is always possible to move. 
        if (isHitFront)
            return !m_cc.isMoving;

        // If there is no hit, then we can only move if left and right are empty. 
        Vector3 crossProd = Vector3.Cross(m_cc.world_up, m_cc.world_direction);
        Ray left          = new Ray(origin, crossProd - m_cc.world_up);
        Ray right         = new Ray(origin, -crossProd - m_cc.world_up);
        bool isHitLeft    = Physics.Raycast(left, l, m_envLayerMask);
        bool isHitRight   = Physics.Raycast(right, l, m_envLayerMask);

        return !(isHitRight || isHitLeft || m_cc.isMoving);
    }

    /**
     * Triger sounds and animations when the player is asked to move without valid move. 
     */
    protected void OnCantMove()
    {
    }

    protected void setisMoving(bool moving)
    {
        isMoving = moving;
        m_animator.SetBool(m_isMoving_Param_ID, moving);
    }

    /**
     * This rounds the up component of @vec to valid multiples of m_boxsize/2 +/- m_sphere Radius.
     * 
     * This is mainly necessary because the Idle animation introduces small numerical changes in the
     * up component. To avoid jumping of the ball this has to be countered. 
     */
    private Vector3 SnapToGrid(Vector3 vec)
    {
        return vec.Round(m_cc.world_up) - (m_boxsize * 0.5f - m_sphereRadius) * m_cc.world_up;
    }

    private Vector3 SnapToGrid(Vector3 vec, Vector3 up)
    {
        return vec.Round(up) - (m_boxsize * 0.5f - m_sphereRadius) * up;
    }

    // Currently only used for debug outputs
    void Update()
    {
        // Green : Gravity
        // White : Target 
        // Red: world diretion
        // cyan: up
        // Blue :Check Path front
        // Black: Check path down
        // Gray: can move front

        Vector3 origin = transform.position + (1 - m_sphereRadius) * m_cc.world_up;
        //Debug.DrawRay(origin, (1.1f*(world_direction - world_up)), Color.grey);
        Debug.DrawRay(transform.position, m_cc.world_up, Color.cyan);
        Debug.DrawRay(transform.position, m_targetPosition - transform.position, Color.red);
        Debug.DrawRay(transform.position, Physics.gravity, Color.green);
        Debug.DrawRay(transform.position- 0.1f * m_cc.world_up, m_cc.world_direction, Color.white);
        Debug.DrawRay(transform.position, m_cc.world_direction * m_sphereRadius * 1.1f, Color.blue);
    }
}