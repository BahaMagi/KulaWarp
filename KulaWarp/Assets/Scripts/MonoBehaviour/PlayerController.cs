using UnityEngine;
using System;

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

    public float speed = 3.5f, easeInTime = 0.3f; // Max speed of the player and duration of the EaseIn to the rolling movement.

    [ReadOnly] public PlayerState state;
    [ReadOnly] public AnimState   animState;
    [ReadOnly] public Vector3     world_direction, world_up; // Current World and Camera forwarwards/up direction

    [HideInInspector] public StateMachine sm;
    [HideInInspector] public float        sphereRadius;

    private SphereCollider m_sphereCollider_player;
    private Animator       m_animator;
    private Rigidbody      m_rb;

    // Animator trigger IDs:
    private int m_isMoving_ID, m_impact_ID, m_warp_ID;

    private int     m_envLayerMask; // Layermask to only check layer 10, i.e. the Environment layer, for collisions. 
    private float   m_invCircum, m_angularSpeed; // Inverse circumference of player sphere and precomputed constant to speed up computation

    public enum PlayerState { Idle, Moving, Warping, Falling, GravityChange };
    public enum AnimState   { Idle, Moving, FadeOut, FadeIn, Impact };

    // Base Classes ObjectBase and MonoBehaviour:

    void Awake()
    {
        // Make this a public singelton
        if (pc == null) pc = this;
        else if (pc != this) Destroy(gameObject);

        // Load references to game objects and components
        LoadComponents();

        // Animator related constants
        m_isMoving_ID = Animator.StringToHash("isMoving"); // @TODO Check if there is another way of doing this that is not string search based. 
        m_impact_ID   = Animator.StringToHash("impact");
        m_warp_ID     = Animator.StringToHash("warp");

        // Precalculate constants
        m_envLayerMask = 1 << 10;
        sphereRadius   = m_sphereCollider_player.radius * player_sphere.transform.lossyScale.x;
        m_invCircum    = 1.0f / 2.0f * Mathf.PI * sphereRadius;
        m_angularSpeed = 90.0f * pc.speed / (LevelController.lc.boxSize * 0.5f); //@TODO forget theory and make this a parameter in the inspector....

        // Initiate the game state
        world_up        = LevelController.lc.startUp;
        world_direction = LevelController.lc.startDir;

        // Setup State Machine
        InitStateMachine();

        // Register this object with the LevelController so it is reset on a restart
        LevelController.lc.Register(this);
    }

    void Update()
    {
        sm.currentState.CheckTransitions();
        sm.Update();
    }

    public override void Reset()
    {
        // Reset game state
        world_up           = LevelController.lc.startUp;
        world_direction    = LevelController.lc.startDir;
        transform.position = LevelController.lc.startPos;

        // Reset Statemachine
        sm.Reset();

        // Reset physics
        m_rb.useGravity = true;
        m_rb.velocity   = Vector3.zero; m_rb.angularVelocity = Vector3.zero;

        // Re-enable gameobject
        player_sphere.SetActive(true);
    }

    // PlayerController:

    bool CanMove()
    {
        float l = 1.6f; // length of the rays sent.

        // Send rays from a point above player position.
        Vector3 origin = transform.position.SnapToGridUp(world_up) + (1 - sphereRadius) * world_up;

        // Check the front first. Any hit allows us to move.
        // This covers both forward and forward-up movement.
        Vector3 frontDown = world_direction - world_up;
        bool isHitFront   = Physics.Raycast(origin, frontDown, l, m_envLayerMask);

        // The player can't start moving if the previous movement isn't finished,
        // the camera is moving or the game is paused.
        if ((sm.currentState.stateName  == (int)PlayerState.GravityChange) ||
            (sm.currentState.stateName  == (int)PlayerState.Warping) ||
            (sm.currentState.stateName  == (int)PlayerState.Falling) ||
            !(CameraController.cc.state == CameraController.CamState.Default) || 
            GameController.gc.IsPaused())
            return false;

        // If there is something in front it is always possible to move. 
        if (isHitFront) return true;

        // If there is no hit, then we can only move if left and right are empty. 
        Vector3 world_left = Vector3.Cross(world_up, world_direction);

        Vector3 leftDown = world_left - world_up;
        bool isHitLeft   = Physics.Raycast(origin, leftDown, l, m_envLayerMask);

        Vector3 rightDown = -world_left - world_up;
        bool isHitRight   = Physics.Raycast(origin, rightDown, l, m_envLayerMask);

        return !isHitLeft && !isHitRight;
    }

    bool CanWarp()
    {
        // The player can't warp if the player is falling/warping/in a gravity transition,
        // the camera is moving or the game is paused.
        if (!(CameraController.cc.state == CameraController.CamState.Default) || 
            state == PlayerState.Warping || 
            state == PlayerState.Falling || 
            state == PlayerState.GravityChange ||
            GameController.gc.IsPaused())
            return false;

        // If the player is moving or forward is pressed when the player cannot move due to the 
        // level geometry, check if two blocks in front of the player is occupied. 
        // The order of the hit items in the array is NOT guaranteed. So all have to be checked.
        // Also, only entry points are registered, no exit points. 
        // As such, if there is one further away than one box size it can only mean there is 
        // is box at the target 
        if (state == PlayerState.Moving || (int)(Input.GetAxisRaw("Vertical")) == 1)
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

    public void Die()
    {
        //@TODO play death animation
        GameController.gc.Lost();
    }

    public void Enable(bool enable)
    {
        player_sphere.SetActive(enable);
    }

    void InitStateMachine()
    {
        sm = new StateMachine(gameObject);

        // Create states
        Idle idle          = new Idle(sm);
        Moving mov         = new Moving(sm);
        GravityChange grav = new GravityChange(sm);
        Warping warp       = new Warping(sm);
        Falling fall       = new Falling(sm);

        // Setup triggered transitions
        Func<bool> transIdle_Mov  = (() => (Input.GetAxisRaw("Vertical") == 1 && CanMove()));
        Func<bool> transIdle_Warp = (() => (Input.GetButtonDown("Warp") && CanWarp()));
        Func<bool> transMov_Idle  = (() => mov.Arrived());
        Func<bool> transFall_Idle = (() => fall.hit);

        // From Idle
        idle.AddTransition(mov, transIdle_Mov); // Idle -> Mov, Press Forward
        idle.AddTransition(warp, transIdle_Warp); // Idle -> Warp, Press Warp

        // From Falling
        fall.AddTransition(idle, transFall_Idle); // Fall -> Idle, impact

        // Fromm Moving
        mov.AddTransition(idle, transMov_Idle); // Mov -> Idle, movement finished
        mov.AddTransition(grav); // Mov -> GravChange, MoveUp/Down
        mov.AddTransition(warp, transIdle_Warp); // Mov -> Warp, Press Warp

        // From GravityChange
        grav.AddTransition(mov); // Grav -> Mov, MoveUp/Down
        grav.AddTransition(fall); // Grav -> Fall, if GravChange was after warp 

        // From Warping
        warp.AddTransition(grav); // Warp -> grav, After Warp is done

        // Put everything together
        sm.AddState(idle); // @TODO Check if it is necessary to store the states in the sm
        sm.AddState(mov);
        sm.AddState(warp);
        sm.AddState(fall);
        sm.AddState(grav);

        // Set the default state, i.e. the starting state of the sm
        sm.SetDefaultState(idle);
        sm.ChangeState(idle);
    }

    void LoadComponents()
    {
        m_sphereCollider_player = player_sphere.GetComponent<SphereCollider>();
        m_animator              = GetComponent<Animator>();
        m_rb                    = GetComponent<Rigidbody>();
    }

    // StateMachine:

    class Idle : State
    {
        public Idle(StateMachine sm) : base(sm)
        {
            stateName = (int)PlayerState.Idle;
        }

        public override void OnEnterState(State from)
        {
            pc.state = PlayerState.Idle;
            pc.m_animator.SetBool(pc.m_isMoving_ID, false);
        }

        public override void OnExitState(State to)
        {
            pc.m_animator.SetBool(pc.m_isMoving_ID, true);
        }

        public override void UpdateState()
        { }
    }

    class Warping : State
    {
        public Vector3 newDir, boxDir;

        private Vector3 m_target;
        private float   m_t, m_hoverTime = 0.3f;
        private bool    m_hovering;

        public Warping(StateMachine sm) : base(sm)
        {
            stateName = (int)PlayerState.Warping;
        }

        public override void OnEnterState(State from)
        {
            pc.state = PlayerState.Warping;

            m_t = 0.0f;
            newDir = pc.world_direction; boxDir = -pc.world_up;

            if (from.stateName == (int)PlayerState.Moving || (int)(Input.GetAxisRaw("Vertical")) == 1)
                m_target = pc.transform.position.SnapToGridAll(pc.world_up) + pc.world_direction * 2.0f + pc.world_up * (0.5f - pc.sphereRadius);
            else
                m_target = pc.transform.position.SnapToGridAll(pc.world_up) + pc.world_up * (2.0f * LevelController.lc.boxSize + 0.5f - pc.sphereRadius);

            pc.m_rb.useGravity = false;
            m_hovering         = false;
            pc.m_animator.SetTrigger(pc.m_warp_ID);
        }

        public override void OnExitState(State to)
        {
            Vector3 origin = pc.transform.position;

            if (Physics.Raycast(origin, -pc.world_up, 1.0f, pc.m_envLayerMask))
                boxDir = -pc.world_up;
            else if (Physics.Raycast(origin, pc.world_direction, 1.0f, pc.m_envLayerMask))
                boxDir = pc.world_direction;
            else if (Physics.Raycast(origin, Vector3.Cross(pc.world_direction, pc.world_up), 1.0f, pc.m_envLayerMask))
                boxDir = Vector3.Cross(pc.world_direction, pc.world_up);
            else if (Physics.Raycast(origin, Vector3.Cross(pc.world_up, pc.world_direction), 1.0f, pc.m_envLayerMask))
                boxDir = Vector3.Cross(pc.world_up, pc.world_direction);
            else if (Physics.Raycast(origin, -pc.world_direction, 1.0f, pc.m_envLayerMask))
                boxDir = -pc.world_direction;
            else if (Physics.Raycast(origin, pc.world_up, 1.0f, pc.m_envLayerMask))
                boxDir = pc.world_up;
            else // No box around the target location
            {
                pc.m_rb.useGravity = true;
                return;
            }

            // If the old world_direction is still a valid direction (i.e. the new gravity axis is not 
            // pointing in the same or opposit direction) then keep it. Otherwise set it to the old 
            // world_up direction. 
            newDir = pc.world_direction;
            if (Mathf.Abs(1 - Mathf.Abs(Vector3.Dot(pc.world_direction, boxDir))) < 0.01f) newDir = pc.world_up;
        }

        public override void UpdateState()
        {
            if (pc.animState != AnimState.FadeIn) return;

            if (!m_hovering)
            {
                pc.transform.position   = m_target;
                pc.m_rb.velocity        = Vector3.zero;
                pc.m_rb.angularVelocity = Vector3.zero;
                m_hovering              = true;
            }

            m_t += Time.deltaTime;

            if (m_t > m_hoverTime) transitions[0].Trigger();
        }
    }

    class Falling : State
    {
        public bool   hit { get; private set; }
        private float m_t, m_thres = 1.5f;

        public Falling(StateMachine sm) : base(sm)
        {
            stateName = (int)PlayerState.Falling;
        }

        public override void OnEnterState(State from)
        {
            pc.state = PlayerState.Falling;
            hit = false;
            m_t = 0.0f;
        }

        public override void OnExitState(State to)
        {
            pc.m_animator.SetTrigger(pc.m_impact_ID);
        }

        public override void UpdateState()
        {
            // Check with a ray cast whether the sphere is in the air.
            hit = Physics.Raycast(pc.transform.position, -pc.world_up, pc.sphereRadius * 1.25f, pc.m_envLayerMask);

            // Check if the player has fallen off the level
            if (!Physics.Raycast(pc.transform.position, -pc.world_up, 50.0f, pc.m_envLayerMask))
                m_t += Time.deltaTime;

            if (m_t > m_thres) pc.Die();
        }
    }

    class GravityChange : State
    {
        private bool    m_gradual; // Does the player BoxCollider have to be rotated gradually, i.e. all but moving up
        private Vector3 m_dir, m_up, m_contactPoint;
        private float   m_t,   m_rotTime;

        private State pred;

        public GravityChange(StateMachine sm) : base(sm)
        {
            stateName = (int)PlayerState.GravityChange;
            pred = null;
        }

        public override void OnEnterState(State from)
        {
            pc.state = PlayerState.GravityChange;
            pred = from;

            m_gradual = false;

            if (from.stateName == (int)PlayerState.Moving)
            {
                if ((from as Moving).nextBlockLevel == -1)
                {
                    m_dir          = -pc.world_up;
                    m_up           = pc.world_direction;
                    m_gradual      = true;
                    m_contactPoint = pc.transform.position - pc.world_up * pc.sphereRadius;
                    m_t            = 0.0f;
                    m_rotTime      = 1 / CameraController.cc.rotSpeed;

                    pc.m_rb.useGravity = false; // Turn off gravity while rotating to avoid sliding.
                }
                else
                {
                    m_dir = pc.world_up;
                    m_up  = -pc.world_direction;
                }


            }
            else if (from.stateName == (int)PlayerState.Warping)
            {
                m_dir = (pred as Warping).newDir;
                m_up  = (pred as Warping).boxDir * -1;
            }

            pc.world_up        = m_up;
            pc.world_direction = m_dir;
            Physics.gravity    = LevelController.lc.gravity * -pc.world_up;

            // If this was accesed from Warp gravity is still turned off. 
            // This is to avoid having gravity being evaluated for one frame before this is
            // entered. 
            if (!m_gradual) pc.m_rb.useGravity = true;

            CameraController.cc.TriggerGravChange();
        }

        public override void OnExitState(State to)
        {
            // Correct minor inaccuracies from the rotation
            pc.transform.rotation = Quaternion.FromToRotation(Vector3.up, pc.world_up);
        }

        public override void UpdateState()
        {
            if (!m_gradual)
            {
                if (pred.stateName == (int)PlayerState.Moving && CameraController.cc.state == CameraController.CamState.Default)
                    transitions[0].Trigger();
                else if (pred.stateName == (int)PlayerState.Warping)
                    transitions[1].Trigger();
            }
            else
            {
                m_t += Time.deltaTime;

                if (m_t < m_rotTime)
                    pc.transform.RotateAround(m_contactPoint, Vector3.Cross(-pc.world_direction, pc.world_up), (90f / m_rotTime) * Time.deltaTime);
                else if (m_t >= m_rotTime && CameraController.cc.state == CameraController.CamState.Default)
                {
                    pc.m_rb.useGravity = true;
                    transitions[0].Trigger();
                }
            }
        }
    }

    class Moving : State
    {
        public int nextBlockLevel;

        private bool    m_easeIn = true, m_arrived = false;
        private Vector3 m_start, m_target, m_posBeforeUpdate;
        private float   m_t, m_easeInTime;

        public Moving(StateMachine sm) : base(sm)
        {
            stateName = (int)PlayerState.Moving;
        }

        public override void OnEnterState(State from)
        {
            pc.state = PlayerState.Moving;

            // This is needed to calculate the traveled distance to rotate the sphere 
            m_posBeforeUpdate = pc.transform.position;

            if (from.stateName == (int)PlayerState.GravityChange)
            {
                nextBlockLevel = 0;
                m_start = pc.transform.position.SnapToGridUp(pc.world_up);
                m_target = (m_start + pc.world_direction * 0.5f * LevelController.lc.boxSize).SnapToGridAll(pc.world_up);
                m_easeInTime = 0.0f;
            }
            else
            {
                nextBlockLevel = NextBlockLevel();
                m_start = pc.transform.position.SnapToGridAll(pc.world_up);
                m_target = GetTarget();
                m_easeInTime = m_easeIn ? pc.easeInTime : 0.0f;
            }

            m_t = 0.0f;
            m_arrived = false;
        }

        public override void OnExitState(State to)
        {
            m_easeIn = true;
        }

        public override void UpdateState()
        {
            if (nextBlockLevel != 0) MoveUpDown();
            else if (nextBlockLevel == 0) MoveForward();

            if (m_arrived && (Input.GetAxisRaw("Vertical") == 1) && pc.CanMove()) Continue();

            m_t += Time.deltaTime;
        }

        public bool Arrived()
        {
            return m_arrived;
        }

        void Continue()
        {
            m_easeIn = false;

            OnEnterState(this);
        }

        Vector3 GetTarget()
        {
            float c = 1.0f; // Same level -> move one block
            if (nextBlockLevel == -1) c = 0.5f; // MoveDown -> move to the edge first 
            else if (nextBlockLevel == 1) c = 0.5f - pc.sphereRadius; //MoveUp -> Move until contacting next block

            return m_start + pc.world_direction * c;
        }

        void MoveForward()
        {
            pc.transform.position = MyInterps.QuadEaseIn(m_start, m_target, out m_arrived, m_t, m_easeInTime, pc.speed);

            RotatePlayerSphere();
        }

        void MoveUpDown()
        {
            MoveForward();

            if (m_arrived)
            {
                m_arrived = false;
                transitions[1].Trigger();
            }
        }

        int NextBlockLevel()
        {
            float l = 1.6f; // length of the rays sent.

            // Send rays from a point above player position.
            Vector3 origin = pc.transform.position.SnapToGridUp(pc.world_up) + (1 - pc.sphereRadius) * pc.world_up;

            // Check the front first. Any hit allows us to move.
            // This covers both forward and forward-up movement.
            RaycastHit hitFront;
            Vector3 frontDown = pc.world_direction - pc.world_up;
            bool isHitFront = Physics.Raycast(origin, frontDown, out hitFront, l, pc.m_envLayerMask);

            return isHitFront ? hitFront.distance < 1 ? 1 : 0 : -1;
        }

        void RotatePlayerSphere()
        {
            float dist  = (pc.transform.position - m_posBeforeUpdate).magnitude;
            float theta = 360.0f * dist * pc.m_invCircum;

            pc.player_sphere.transform.RotateAround(pc.player_sphere.transform.position, Vector3.Cross(pc.world_up, pc.world_direction), theta);

            // Set the position for the next frame
            m_posBeforeUpdate = pc.transform.position;
        }
    }
}