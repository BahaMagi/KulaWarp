using UnityEngine;
using System;

/**
* Provides a smooth, rotatable (in 90° steps) follow camera that can be tilted up and down to allow 
* for a different viewing angle. @TODO This class will be turned into a StateMachine like the PlayerController
*/
public class CameraController : ObjectBase
{
    public static CameraController cc;

    public float dirOffset = -1.6f, upOffset = 1.3f, lookAtUpOffset = 0.71762f;
    public float rotSpeed  = 0.5f, tiltSpeed = 0.5f, followSpeed    = 1.0f;

    // Offset in fully tilted camera position. The X component is the offset in
    // world_dir direction, the Y component in world_up direction.
    public Vector2 tiltDownOffset = new Vector2(-0.01f, 1.3f), tiltUpOffset = new Vector2(-0.7f, 0.2f);

    public enum CamState { Default, Rotate, GravChange, Pause, Warp, Anim, Falling }; //@TODO add impact state 
    [ReadOnly] public CamState state = CamState.Anim;

    private StateMachine sm;

    private Vector3 m_playerPos, m_targetPos, m_lookAt;
    private Vector3 m_up, m_dir;
    private float   m_dirOffset, m_upOffset;
    private bool    m_resetSM = false, m_gravChangeTrigger = false, m_endIntro = false;

    // Base Classes ObjectBase and MonoBehaviour:

    void Awake()
    {
        // Make this a public singelton
        if (cc == null) cc = this;
        else if (cc != this) Destroy(gameObject);

        // Setup State Machine
        InitStateMachine();

        // Initialize camera offsets. They are stored in separate variable as they will be changed 
        // repeatedly and the public versions act as reference. 
        m_dirOffset = dirOffset; m_upOffset = upOffset;
        m_dir = LevelController.lc.startDir; m_up = LevelController.lc.startUp;

        // Register this object with the LevelController so it is reset on a restart
        LevelController.lc.Register(this);
    }

    void LateUpdate()
    {
        sm.currentState.CheckTransitions();
        sm.Update();
    }

    public override void Reset()
    {
        //m_resetSM = true;
        // Reset Statemachine
        sm.Reset();
    }

    // CameraController:

    bool CanRotate()
    {// The Camera can rotate whenever the player is idle, i.e. not moving in any way
        return PlayerController.pc.state == PlayerController.PlayerState.Idle;
    }

    void InitStateMachine()
    {
        sm = new StateMachine(gameObject);

        // Create states
        Default    def   = new Default(sm);
        Rotate     rot   = new Rotate(sm);
        Warp       warp  = new Warp(sm);
        Anim       anim  = new Anim(sm);
        GravChange grav  = new GravChange(sm);
        Pause      pause = new Pause(sm);
        Falling    fall  = new Falling(sm);

        //Setup triggered transitions
        Func<bool> transDef_Rot    = (() => (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") == -1) && CanRotate());
        Func<bool> transDef_Warp   = (() => PlayerController.pc.state == PlayerController.PlayerState.Warping);
        Func<bool> transDef_Grav   = (() => m_gravChangeTrigger);
        Func<bool> transDef_Anim   = (() => m_resetSM);
        Func<bool> transDef_Fall   = (() => PlayerController.pc.state == PlayerController.PlayerState.Falling);
        Func<bool> transFall_Def   = (() => PlayerController.pc.state != PlayerController.PlayerState.Falling);
        Func<bool> transDef_Pause  = (() => !GameController.gc.IsDefault());
        Func<bool> transPause_Def  = (() =>  GameController.gc.IsDefault());
        Func<bool> transWarp_Grav  = (() => PlayerController.pc.state != PlayerController.PlayerState.Warping);
        Func<bool> transAnim_Def   = (() => m_endIntro);
        Func<bool> transWarp_Pause = (() => !GameController.gc.IsDefault());
        Func<bool> transFall_Pause = (() => !GameController.gc.IsDefault());
        Func<bool> transGrav_Pause = (() => !GameController.gc.IsDefault());

        // From Def
        def.AddTransition(rot,   transDef_Rot);
        def.AddTransition(warp,  transDef_Warp);
        def.AddTransition(anim,  transDef_Anim);
        def.AddTransition(grav,  transDef_Grav);
        def.AddTransition(pause, transDef_Pause);
        def.AddTransition(fall,  transDef_Fall);

        // From Rot
        rot.AddTransition(def);

        // From Warp
        warp.AddTransition(grav, transWarp_Grav);
        warp.AddTransition(pause, transWarp_Pause);

        // From GravChange
        grav.AddTransition(def);
        grav.AddTransition(pause, transGrav_Pause);

        // From Pause
        pause.AddTransition(def,  transPause_Def);

        // From Anim
        anim.AddTransition(def, transAnim_Def);

        // From Fall
        fall.AddTransition(def, transFall_Def);
        fall.AddTransition(pause, transFall_Pause);

        // Set the default state, i.e. the starting state of the sm
        sm.SetDefaultState(anim);
        sm.ChangeState(anim);
    }

    void Tilt()
    {
        int tilt = 0;

        if (Input.GetButton("LookUp")) tilt = 1;
        else if (Input.GetButton("LookDown")) tilt = -1;

        // @TODO make the tilt positions inspector variables as well
        float target_dir = dirOffset, target_up = upOffset;

        if (tilt == -1) { target_dir = tiltDownOffset .x; target_up = tiltDownOffset.y; } // Tilt down
        else if (tilt == 1) { target_dir = tiltUpOffset.x; target_up = tiltUpOffset.y; } // Tilt Up

        // Move the camera towards the new position. No rotation has to be done as the LookAt will 
        // ensure that the camera keeps facing the LookAt position above the player. 
        m_dirOffset = Mathf.MoveTowards(m_dirOffset, target_dir, Time.deltaTime * tiltSpeed);
        m_upOffset  = Mathf.MoveTowards(m_upOffset, target_up, Time.deltaTime * tiltSpeed);
    }

    public void TriggerGravChange()
    {
        m_gravChangeTrigger = true;
    }

    public void EndIntro()
    {
        m_endIntro = true;
    }

    // State Machine: 

    class Default : State
    {
        public Default(StateMachine sm) : base(sm)
        {
            stateName = (int)CamState.Default;
        }

        public override void OnEnterState(State from)
        {
            cc.state = CamState.Default;

            cc.m_up  = PlayerController.pc.world_up;
            cc.m_dir = PlayerController.pc.world_direction;
        }

        public override void OnExitState(State to)
        { }

        public override void UpdateState()
        {
            // Set new target camera and lookAt positions
            cc.m_playerPos = PlayerController.pc.gameObject.transform.position.SnapToGridUp(PlayerController.pc.world_up);
            cc.m_targetPos = cc.m_playerPos + cc.m_dir * cc.m_dirOffset + cc.m_upOffset * cc.m_up;
            cc.m_lookAt    = cc.m_playerPos + cc.lookAtUpOffset * cc.m_up;

            // Tilt if a tilt button is pressed or interpolate back to normal position
            cc.Tilt();

            // Update position and orientation
            cc.transform.position = Vector3.MoveTowards(cc.transform.position, cc.m_targetPos, Time.deltaTime * cc.followSpeed);
            cc.transform.LookAt(cc.m_lookAt, cc.m_up);
        }
    }

    class Rotate : State
    {
        private int   m_direction   = 0; // -1 = left, 0 = 180°, 1 = right
        private float m_rotProgress = 0.0f;

        public Rotate(StateMachine sm) : base(sm)
        {
            stateName = (int)CamState.Rotate;
        }

        public override void OnEnterState(State from)
        {
            cc.state = CamState.Rotate;

            // Check the input which rotation should be executed
            if (Input.GetAxisRaw("Horizontal") == -1)     m_direction = -1;
            else if (Input.GetAxisRaw("Horizontal") == 1) m_direction = 1;

            // Player cannot move during rotation so the position wont change
            cc.m_playerPos = PlayerController.pc.gameObject.transform.position.SnapToGridUp(PlayerController.pc.world_up);
            cc.m_lookAt    = cc.m_playerPos + cc.lookAtUpOffset * cc.m_up;
        }

        public override void OnExitState(State to)
        {
            // Reset state
            m_rotProgress = 0.0f; m_direction = 0;

            // Set the new world_direction
            PlayerController.pc.world_direction = Vector3Int.RoundToInt(cc.m_dir);
        }

        public override void UpdateState()
        {
            m_rotProgress += Time.deltaTime * cc.rotSpeed;
            m_rotProgress = Mathf.Clamp(m_rotProgress, 0.0f, 1.0f);

            float targetAngle = 0.0f;
            switch (m_direction)
            {
                case -1: targetAngle = -90.0f; break;
                case 1:  targetAngle =  90.0f; break;
                default: targetAngle = 180.0f; break;
            }

            // Apply interpolated rotation for one step. This will be called once per Update() 
            // until the target angle has been reached. 
            Quaternion q = Quaternion.AngleAxis(m_rotProgress * targetAngle, PlayerController.pc.world_up);
            cc.m_dir = q * PlayerController.pc.world_direction;

            // Set new target camera positions
            cc.m_targetPos = cc.m_playerPos + cc.m_dir * cc.m_dirOffset + cc.m_upOffset * cc.m_up;

            // Tilt if a tilt button is pressed or interpolate back to normal position
            cc.Tilt();

            // Update position and orientation
            cc.transform.position = cc.m_targetPos;
            cc.transform.LookAt(cc.m_lookAt, cc.m_up);

            // If the new position has been reached, go back to Default state
            if (m_rotProgress >= 1.0f)
            {
                transitions[0].Trigger();
            }
        }
    }

    class Warp : State
    {
        GameObject sphere_Appear;

        public Warp(StateMachine sm) : base(sm)
        {
            stateName = (int)CamState.Warp;

            sphere_Appear = GameObject.Find("Sphere_Appear");
        }

        public override void OnEnterState(State from)
        {
            cc.state = CamState.Warp;

            cc.m_up  = PlayerController.pc.world_up;
            cc.m_dir = PlayerController.pc.world_direction;
        }

        public override void OnExitState(State to)
        { }

        public override void UpdateState()
        {
            // Set new target camera but skip lookAt positions
            cc.m_playerPos = sphere_Appear.transform.position;
            //cc.m_playerPos = PlayerController.pc.gameObject.transform.position;
            cc.m_targetPos = cc.m_playerPos + cc.m_dir * cc.m_dirOffset + cc.m_upOffset * cc.m_up;

            // Tilt if a tilt button is pressed or interpolate back to normal position
            cc.Tilt();

            // Update position
            cc.transform.position = Vector3.MoveTowards(cc.transform.position, cc.m_targetPos, Time.deltaTime * cc.followSpeed * 0.75f);
        }
    }

    class GravChange : State
    {
        float   m_rotProgress;
        Vector3 m_targetUp, m_targetDir;
        Vector3 m_startUp, m_startDir;

        public GravChange(StateMachine sm) : base(sm)
        {
            stateName = (int)CamState.GravChange;
        }

        public override void OnEnterState(State from)
        {
            cc.state = CamState.GravChange;

            m_rotProgress = 0;
            m_targetUp    = PlayerController.pc.world_up;
            m_targetDir   = PlayerController.pc.world_direction;
            m_startDir    = cc.m_dir;
            m_startUp     = cc.m_up;
        }

        public override void OnExitState(State to)
        {
            cc.m_gravChangeTrigger = false;
        }

        public override void UpdateState()
        {
            m_rotProgress += Time.deltaTime * cc.rotSpeed;
            m_rotProgress  = Mathf.Clamp(m_rotProgress, 0.0f, 1.0f);

            // Use spheretic interpolation between the old and the new
            // direction and up vectors to preserve magintude. 
            cc.m_dir = Vector3.Slerp(m_startDir, m_targetDir, m_rotProgress);
            cc.m_up  = Vector3.Slerp(m_startUp,  m_targetUp,  m_rotProgress);

            // Set new target camera and lookAt positions
            cc.m_playerPos = PlayerController.pc.gameObject.transform.position;
            cc.m_targetPos = cc.m_playerPos + cc.m_dir * cc.m_dirOffset + cc.m_upOffset * cc.m_up;
            cc.m_lookAt    = cc.m_playerPos + cc.lookAtUpOffset * cc.m_up;

            // Tilt if a tilt button is pressed or interpolate back to normal position
            cc.Tilt();

            // Update position and orientation
            cc.transform.position = cc.m_targetPos;
            cc.transform.LookAt(cc.m_lookAt, cc.m_up);

            // If the new position has been reached, go back to Default state
            if (m_rotProgress >= 1.0f)
            {
                transitions[0].Trigger();
            }
        }
    }

    class Anim : State
    {
        private Animator m_anim;
        private AnimatorOverrideController m_animOverrideCtrl;
        private int m_reset_trigger_ID;

        public Anim(StateMachine sm) : base(sm)
        {
            stateName = (int)CamState.Anim;

            // Set the intro animation of that level
            m_anim             = cc.gameObject.GetComponent<Animator>();
            m_animOverrideCtrl = new AnimatorOverrideController(m_anim.runtimeAnimatorController);
            m_anim.runtimeAnimatorController      = m_animOverrideCtrl;
            m_animOverrideCtrl["CameraIntroBase"] = LevelController.lc.cameraIntroAnimation;

            // Get ID of transition trigger to reset the into animation
            m_reset_trigger_ID = Animator.StringToHash("reset"); 
        }

        public override void OnEnterState(State from)
        {
            cc.state = CamState.Anim;

            cc.m_resetSM = false;

            // Restart the intro animation
            m_anim.SetTrigger(m_reset_trigger_ID);
        }

        public override void OnExitState(State to)
        {
            cc.m_endIntro = false;
        }

        public override void UpdateState()
        { }
    }

    class Pause : State
    {
        public Pause(StateMachine sm) : base(sm)
        {
            stateName = (int)CamState.Pause;
        }

        public override void OnEnterState(State from)
        {
            cc.state = CamState.Pause;

            cc.m_lookAt    = LevelController.lc.pauseCamLookAt;
            cc.m_targetPos = LevelController.lc.pauseCamPos;

            cc.transform.position = cc.m_targetPos;
            cc.transform.LookAt(cc.m_lookAt, cc.m_up);
        }

        public override void OnExitState(State to)
        {
            // Set the new target camera and lookAt positions back to the original values
            cc.m_playerPos = PlayerController.pc.gameObject.transform.position.SnapToGridUp(PlayerController.pc.world_up);
            cc.m_targetPos = cc.m_playerPos + cc.m_dir * cc.m_dirOffset + cc.m_upOffset * cc.m_up;
            cc.m_lookAt = cc.m_playerPos + cc.lookAtUpOffset * cc.m_up;
            
            // Update position and orientation
            cc.transform.position = cc.m_targetPos;
            cc.transform.LookAt(cc.m_lookAt, cc.m_up);
        }

        public override void UpdateState()
        {
            //@TODO Pause Camera Animation
        }
    }

    class Falling : State
    {
        public Falling(StateMachine sm) : base(sm)
        {
            stateName = (int)CamState.Falling;
        }

        public override void OnEnterState(State from)
        {
            cc.state = CamState.Falling;

            cc.m_up  = PlayerController.pc.world_up;
            cc.m_dir = PlayerController.pc.world_direction;
        }

        public override void OnExitState(State to)
        { }

        public override void UpdateState()
        {
            // Set new target camera and lookAt positions
            cc.m_playerPos = PlayerController.pc.gameObject.transform.position;
            cc.m_targetPos = cc.m_playerPos + cc.m_dir * cc.m_dirOffset + cc.m_upOffset * cc.m_up;
            cc.m_lookAt    = cc.m_playerPos + cc.lookAtUpOffset * cc.m_up;

            // Tilt if a tilt button is pressed or interpolate back to normal position
            cc.Tilt();

            // Update position and orientation
            cc.transform.position = Vector3.MoveTowards(cc.transform.position, cc.m_targetPos, Time.deltaTime * cc.followSpeed);
            cc.transform.LookAt(cc.m_lookAt, cc.m_up);
        }
    }
}
