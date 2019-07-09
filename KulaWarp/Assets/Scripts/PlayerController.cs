using System.Collections;
using UnityEngine;

/**
 * Takes care of the core movement mechanics of the player. The player consists of two gameobjects.
 * An (invisible) box which is set as parent for a sphere. This allows using the box (which does not 
 * rotate) in animations.
 * This script is attached to the parent player, i.e. the box. Thus, to access the sphere use
 * the gameobject reference @player_sphere. 
 */
public class PlayerController : ObjectBase
{
    public static PlayerController pc;

    public GameObject player_sphere;

    public float speed        = 5f; // Maximum speed of the player
    public float easeInTime   = 0.02f; // Dampening strength of the EaseIn. 

    [HideInInspector] public Vector3 world_direction, world_up; // Current World and Camera forwarwards/up direction
    [HideInInspector] public bool    isMoving, isWarping, isFalling, isGravityShifting;
    [HideInInspector] public float   sphereRadius;

    private SphereCollider   m_sphereCollider_player;
    private Rigidbody        m_rb;
    private Animator         m_animator;

    private Coroutine m_moveForCoro, m_moveDownCoro; // Need to store the started coroutines to be able to interrupt them

    private int m_isMoving_Param_ID; // ID for parameter that triggers Idle -> Moving transition in animator. 
    private int m_warp_trigger_ID;
    private int m_envLayerMask; // Layermask to only check layer 10, i.e. the Environment layer, for collisions. 

    private Vector3     m_RotationAngles; // Stores the current rotation angles of the sphere in world axis coordinates
    private Vector3     m_targetPosition;
    private float       m_remainingDistance; // Distance between the current position and the current target position. Used to smoothen the coninuation of movement.
    private float       m_circum, m_rotConst; // Circumference of player sphere and precomputed constant to speed up computation

#region Monobehavior
    void Awake()
    {
        // Make this a public singelton
        if (pc == null) pc = this;
        else if (pc != this) Destroy(gameObject);

        isMoving = false; isWarping = false; isFalling = false; isGravityShifting = false;

        LoadComponents();

        m_isMoving_Param_ID = Animator.StringToHash("isMoving"); // @TODO Check if there is another way of doing this that is not string search based. 
        m_warp_trigger_ID   = Animator.StringToHash("warp");
        m_envLayerMask      = 1 << 10;

        sphereRadius     = m_sphereCollider_player.radius * player_sphere.transform.lossyScale.x;
        m_circum         = 2 * Mathf.PI * sphereRadius;
        m_rotConst       = 360.0f / m_circum;
        m_RotationAngles = Vector3.zero;

        world_up        = LevelController.lc.startUp;
        world_direction = LevelController.lc.startDir;

        LevelController.lc.Register(this);
    }

    void FixedUpdate()
    {
        // Check whether the player is falling and nearing impact. 
        CheckImpact();

        int input = HandleInput();

        if (input == 1)
            AttemptMove();
        else if (input == 2)
            StartCoroutine(Warp());
    }
#endregion Monobehavior

    void AttemptMove()
    {
        bool canMove = CanMove(out int nextBlockLevel);

        if (!canMove) return;

        if (isMoving)
        {
            if (nextBlockLevel >= 0) SetTarget(nextBlockLevel);
            return;
        }
        else SetTarget(nextBlockLevel);

        switch (nextBlockLevel)
        {
            case -1:
                m_moveDownCoro = StartCoroutine(MoveDownwards());
                break;
            case 0:
            case 1:
                m_moveForCoro = StartCoroutine(MoveForwards());
                break;
            default:
                break;
        }
    }

    /**
     * Checks if the player can move forwards and based on the topology of the level decides to go 
     * forwards or downwards. For more infromation see @CanMove()
     */
    bool CanMove(out int nextBlockLevel)
    {
        float l = 1.6f; // length of the rays sent.

        // Send rays from a point above player position.
        Vector3 origin = transform.position.SnapToGridUp(world_up) + (1 - sphereRadius) * world_up;

        // Check the front first. Any hit allows us to move.
        // This covers both forward and forward-up movement.
        RaycastHit hitFront;
        Vector3 frontDown  = world_direction - world_up;
        bool    isHitFront = Physics.Raycast(origin, frontDown, out hitFront, l, m_envLayerMask);
        nextBlockLevel     = isHitFront ? hitFront.distance < 1 ? 1 : 0 : -1;

        // The player can't start moving until the previous movement is finished.
        if (isWarping || isFalling || CameraController.cc.isMoving) return false;

        // If there is something in front it is always possible to move. 
        if (isHitFront) return true;

        // If there is no hit, then we can only move if left and right are empty. 
        Vector3 world_left = Vector3.Cross(world_up, world_direction);

        Vector3 leftDown  = world_left - world_up;
        bool    isHitLeft = Physics.Raycast(origin, leftDown, l, m_envLayerMask);

        Vector3 rightDown  = -world_left - world_up;
        bool    isHitRight = Physics.Raycast(origin, rightDown, l, m_envLayerMask);

        return !isHitLeft && !isHitRight;
    }

    bool CanWarp()
    {
        if (isWarping || isFalling || CameraController.cc.isMoving || isGravityShifting) return false;

        // If the player is moving or forward is pressed when the player cannot move,
        // check two blocks in front of the player. 
        // The order of the hit items in the array is NOT guaranteed. So all have to be checked.
        // Also, only entry points are registered, no exit points. 
        // As such, if there is one further away than one box size it can only mean there is 
        // is box at the target 
        if (isMoving || (int)(Input.GetAxisRaw("Vertical")) == 1)
        {
            RaycastHit[] raycasthits;
            raycasthits = Physics.RaycastAll(transform.position, world_direction, 2.0f, m_envLayerMask);

            if (raycasthits.Length > 0) // Something was hit
                for (int i = 0; i < raycasthits.Length; i++) // Check all hits
                    if (raycasthits[i].distance > LevelController.lc.boxSize) return false;

            return true;
        }
        else // If the player is not moving and nothing is pressed, check 2 blocks above the player. 
        {
            RaycastHit[] raycasthits;
            float rayLength = 2.0f + 0.5f - sphereRadius;
            raycasthits = Physics.RaycastAll(transform.position, world_up, rayLength, m_envLayerMask);

            if (raycasthits.Length > 0) // Something was hit
                for (int i = 0; i < raycasthits.Length; i++) // Check all hits
                    if (raycasthits[i].distance > LevelController.lc.boxSize) return false;

            return true;
        }
    }

    /**
     * After a Warp the gravity axis (and up and dir vector along with it) are set to a box close to the target
     * of the warp. If there is none, the player just falls with the gravity as it was before the warp. 
     * Order in which existance of boxes is checked is (relative to dir/up before the warp):
     * -up -> dir -> dir X up -> up x dir -> -dir -> up
     */
    void CheckBoxAfterWarp()
    {
        Vector3 origin = m_targetPosition;
        if      (Physics.Raycast(origin, -world_up,        1.0f, m_envLayerMask)) SetGravityAfterWarp(-world_up);
        else if (Physics.Raycast(origin, world_direction,  1.0f, m_envLayerMask)) SetGravityAfterWarp(world_direction);
        else if (Physics.Raycast(origin, Vector3.Cross(world_direction, world_up), 1.0f, m_envLayerMask)) SetGravityAfterWarp(Vector3.Cross(world_direction, world_up));
        else if (Physics.Raycast(origin, Vector3.Cross(world_up, world_direction), 1.0f, m_envLayerMask)) SetGravityAfterWarp(Vector3.Cross(world_up, world_direction));
        else if (Physics.Raycast(origin, -world_direction, 1.0f, m_envLayerMask)) SetGravityAfterWarp(-world_direction);
        else if (Physics.Raycast(origin, world_up,         1.0f, m_envLayerMask)) SetGravityAfterWarp(world_up);
    }

    void CheckImpact()
    {
        // Check with a ray cast whether the sphere is in the air.
        bool hit = Physics.Raycast(transform.position, -world_up, sphereRadius * 2.0f, m_envLayerMask);

        if (!isFalling) isFalling = !hit;
        else
            if (hit)
        {
            isFalling = false;
            m_animator.SetTrigger("impact");
        }
    }

    public void Die()
    {
        //@TODO play death animation
        StartCoroutine(GameController.gc.Lose());
    }

    /**
     * Returns an int which represents the input.
     * 0 : no input -> no movement
     * 1 : Forward was pressed -> rolling
     * -1: Backwards was pressed -> not used here
     * 2 : Jump was pressed -> warp
     */
    int HandleInput()
    {
        if (Input.GetButton("Warp"))
            return 2;

        return (int)(Input.GetAxisRaw("Vertical")); // For keyboard input this is in {-1, 0, 1} 
    }

    void LoadComponents()
    {
        m_sphereCollider_player = player_sphere.GetComponent<SphereCollider>();
        m_rb                    = GetComponent<Rigidbody>();
        m_animator              = GetComponent<Animator>();
    }

    /**
    * This assumes that the player can move and that there is no following block, i.e. the sphere 
    * has to roll down and change gravity. 
    * This is done in 3 steps:
    * 1) Roll to the edge of the current box.
    * 2) Rotate 90° around that edge.
    * 3) Move to the center of that new face of the box. 
    */
    IEnumerator MoveDownwards() //@TODO Try at some point to put this into MoveForwards like MoveUpwards
    {
        SetisMoving(true);

        // Phase 1: Move to the edge of the box.
        Vector3 startPosition = transform.position;
        m_remainingDistance   = LevelController.lc.boxSize * LevelController.lc.boxSize;

        float t      = 0.0f;
        bool arrived = false;

        while (!arrived)
        {
            m_rb.MovePosition(MyInterps.QuadEaseIn(startPosition, m_targetPosition, out arrived, t, easeInTime, speed));

            m_remainingDistance = (m_targetPosition - transform.position).sqrMagnitude;
            t += Time.deltaTime;

            RotatePlayerSphere();

            yield return null;
        }

        // Phase 2: Rotate around the edge.
        isGravityShifting  = true;
        t                  = 0.0f;
        float dt           = 0.0f;
        float angularSpeed = 90.0f * speed / (LevelController.lc.boxSize * 0.5f); // @TODO That 90 is arbitrary. Make this a public var so its editable from the inspector.
                                                                 // @TODO also: the m_boxsize*0.5 should be m_sphereRadius*2 in theory. Want to test that again.
        Vector3 contactPoint = transform.position - world_up * sphereRadius;
        m_rb.useGravity = false; // Turn off gravity while rotating to avoid sliding.

        Vector3 tmp = world_up;
        world_up = world_direction;
        world_direction = -tmp;

        // Start rotating the camera.
        StartCoroutine(CameraController.cc.CameraUpDown(-1));

        while (t < 90.0f)
        {
            t += angularSpeed * Time.deltaTime;
            dt = angularSpeed * Time.deltaTime - Mathf.Min(0, 90.0f - t);

            transform.RotateAround(contactPoint, Vector3.Cross(-world_direction, world_up), dt);

            yield return null;
        }

        // Turn gravity back on and change it to the new direction.
        Physics.gravity = -LevelController.lc.gravity * world_up;
        m_rb.useGravity = true;
        isGravityShifting = false;

        // Phase 3: Move to the center of the face of the box. 
        m_targetPosition = (transform.position + 0.5f * world_direction).SnapToGridAll(world_up);
        m_remainingDistance = (m_targetPosition - startPosition).sqrMagnitude;

        // Finish of the movement by moving forwards to the new target. 
        m_moveForCoro = StartCoroutine(MoveForwards());
    }

    /**
     * When intially called, this function assumes that there is a next block on the same level. 
     * Once the co-routine has started, the @m_targetPosition variable can be changed and the 
     * movement will be continued/adjusted accordingly until the target position is reached. 
     * 
     * The 
     */
    IEnumerator MoveForwards()
    {
        SetisMoving(true);

        Vector3 startPosition = transform.position;
        m_remainingDistance = (m_targetPosition - startPosition).sqrMagnitude;

        float t = 0.0f;
        bool arrived = false;

        while (!arrived)
        {
            m_rb.MovePosition(MyInterps.QuadEaseIn(startPosition, m_targetPosition, out arrived, t, easeInTime, speed).SnapToGridUp(world_up));
            m_remainingDistance = (m_targetPosition - transform.position).sqrMagnitude;
            t += Time.deltaTime;

            RotatePlayerSphere();

            // Check with a ray cast whether the sphere has to move up. 
            Ray checkFront = new Ray(transform.position, world_direction);
            if (Physics.Raycast(checkFront, sphereRadius * 1.1f, m_envLayerMask))
            {
                MoveUpwards();
                startPosition = transform.position;
                t = 0.0f;
            }

            yield return null;
        }

        // In case MoveUpwards was called the parent player object needs to be reoriented. 
        transform.rotation = Quaternion.FromToRotation(Vector3.up, world_up);
        SetisMoving(false);
    }

    void MoveUpwards()
    {
        Physics.gravity = LevelController.lc.gravity * world_direction;

        m_targetPosition = m_targetPosition - world_direction * (0.5f * LevelController.lc.boxSize + sphereRadius) + world_up * (-sphereRadius + 0.5f * LevelController.lc.boxSize);
        m_remainingDistance = (m_targetPosition - transform.position).sqrMagnitude;

        Vector3 tmp = world_up;
        world_up = -world_direction;
        world_direction = tmp;

        StartCoroutine(CameraController.cc.CameraUpDown(1));
    }

    /**
     * Triger sounds and animations when the player is asked to move without valid move. 
     */
    void OnCantMove()
    {
        //@TODO Play OnCan't Move Animation if the player is not moving but asked to move in a direction it cannot.
    }

    /**
     * Rotates the player sphere according to its current position in world space according to the world direction.
     */
    void RotatePlayerSphere()
    {
        float targetTheta = ((transform.position.getComponent(world_direction) % m_circum) * m_rotConst) - 180.0f;
        float dTheta = targetTheta - m_RotationAngles.getComponent(world_direction);
        m_RotationAngles.setComponent(world_direction, targetTheta);

        player_sphere.transform.RotateAround(player_sphere.transform.position, Vector3.Cross(world_up, world_direction), dTheta * world_direction.getComponent(world_direction));
    }

    void SetGravityAfterWarp(Vector3 boxDir)
    {
        // If the old world_direction is still a valid direction (i.e. the new gravity axis is not 
        // pointing in the same or opposit direction) then keep it. Otherwise set it to the old 
        // world_up direction. 
        Vector3 newDir = world_direction;
        if (Mathf.Abs(1 - Mathf.Abs(Vector3.Dot(world_direction, boxDir))) < 0.01f) newDir = world_up;

        StartCoroutine(CameraController.cc.GravityChange(newDir, -boxDir));

        world_direction = newDir;
        world_up        = -boxDir;
        Physics.gravity = LevelController.lc.gravity * boxDir;
    }

    /**
     * The isMoving trigger for the idle animation has to be set together with the @isMoving 
     * variable of this class.
     */
    void SetisMoving(bool moving)
    {
        isMoving = moving;
        m_animator.SetBool(m_isMoving_Param_ID, moving);
    }

    void SetTarget(int nextBlockLevel)
    {
        Vector3 pos       = transform.position;
        m_targetPosition  = pos.SnapToGridAll(world_up);
        m_targetPosition += world_direction * ((nextBlockLevel == -1) ? 0.5f : 1.0f);
    }
    
    IEnumerator Warp()
    {
        if (!CanWarp()) yield break;

        // Warp forwards
        if (isMoving || (int)(Input.GetAxisRaw("Vertical")) == 1)
        {
            if (isMoving)
            {
                if (m_moveForCoro  != null) StopCoroutine(m_moveForCoro);
                if (m_moveDownCoro != null) StopCoroutine(m_moveDownCoro);
            }
            
            isWarping             = true;
            m_rb.useGravity       = false;
            m_rb.detectCollisions = false;

            m_animator.SetTrigger(m_warp_trigger_ID);

            float t = 0.0f;
            Vector3 startPosition = transform.position;
            m_targetPosition      = startPosition.SnapToGridAll(world_up);
            m_targetPosition     += world_direction * 2.0f + world_up * (0.5f - sphereRadius);

            bool arrived = false;
            while (!arrived)
            {
                m_rb.MovePosition(MyInterps.QuadEaseIn(startPosition, m_targetPosition, out arrived, t, easeInTime, speed*1.5f));
                m_remainingDistance = (m_targetPosition - transform.position).sqrMagnitude;
                t += Time.deltaTime;

                RotatePlayerSphere();

                yield return null;
            }
            transform.position = m_targetPosition;

            //while (!m_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) yield return null;

            SetisMoving(false);
            isWarping             = false;
            m_rb.useGravity       = true;
            m_rb.detectCollisions = true;
        }
        else // Warp Upwards
        {
            isWarping = true;

            m_animator.SetTrigger(m_warp_trigger_ID);

            while (!m_animator.GetCurrentAnimatorStateInfo(0).IsName("FadeIn")) yield return null; //@TODO get the ID 

            m_rb.useGravity = false;
            transform.position += world_up * (2.0f *LevelController.lc.boxSize + 0.5f - sphereRadius);
            m_targetPosition = transform.position;

            yield return new WaitForSeconds(0.3f); //@TODO make this time a public var to be adjustable as "hover time"

            m_rb.useGravity = true;
            isWarping = false;
        }

        // Check if the gravity axis has to change.
        CheckBoxAfterWarp();

        // If the player is falling without anything below wait for 2 seconds and test again (moving platform or 
        // something might have appeared). If there is still nothing, trigger death by falling. 
        if (isFalling)
        {
            if (!Physics.Raycast(transform.position, -world_up, 50.0f)) yield return new WaitForSeconds(1.5f);
            else yield break;

            if (!Physics.Raycast(transform.position, -world_up, 50.0f)) Die();

            yield break;
        }
    }

    public void Disable()
    {
        player_sphere.SetActive(false);
    }

    public override void Reset()
    {
        if (m_moveForCoro  != null) StopCoroutine(m_moveForCoro);
        if (m_moveDownCoro != null) StopCoroutine(m_moveDownCoro);

        world_up           = LevelController.lc.startUp;
        world_direction    = LevelController.lc.startDir;
        transform.position = LevelController.lc.startPos;

        isMoving = false; isWarping = false; isFalling = false; isGravityShifting = false;

        player_sphere.SetActive(true);
    }

    //---------------------------------------------------//
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

        Vector3 origin = transform.position + (1 - sphereRadius) * world_up;
        //Debug.DrawRay(origin, (1.1f*(world_direction - world_up)), Color.grey);
        //Debug.DrawRay(transform.position, world_up, Color.cyan);
        //Debug.DrawRay(transform.position, world_direction, Color.red);
        //Debug.DrawRay(transform.position, m_targetPosition - transform.position, Color.red);
        //Debug.DrawRay(transform.position, Physics.gravity, Color.green);
        //Debug.DrawRay(transform.position- 0.1f * world_up, world_direction, Color.white);
        //Debug.DrawRay(transform.position, world_direction * m_sphereRadius * 1.1f, Color.blue);
    }
}