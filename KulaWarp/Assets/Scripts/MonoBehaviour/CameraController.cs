using UnityEngine;


public class CameraController : ObjectBase
{
    public static CameraController cc;

    public float dirOffset = -1.6f, upOffset  = 1.3f, lookAtUpOffset = 0.71762f;
    public float rotSpeed  = 0.5f,  tiltSpeed = 0.5f, followSpeed    = 1.0f;

    public enum CamState {Default, RotLeft, RotRight, RotBack, GravChange, Pause, Anim};
    [HideInInspector] public CamState camState = CamState.Default;

    private Animator m_anim;
    private AnimatorOverrideController m_animOverrideCtrl;
    private int m_reset_trigger_ID;

    private Vector3 m_playerPos, m_up, m_lookAt, m_dir;
    private int     m_tilt = 0;
    private float   m_dirOffset, m_upOffset;

#region Base_Classes
    void Awake()
    {
        // Make this a public singelton
        if (cc == null) cc = this;
        else if (cc != this) Destroy(gameObject);

        // Set the intro animation of that level
        m_anim             = gameObject.GetComponent<Animator>();
        m_animOverrideCtrl = new AnimatorOverrideController(m_anim.runtimeAnimatorController);
        m_anim.runtimeAnimatorController      = m_animOverrideCtrl;
        m_animOverrideCtrl["CameraIntroBase"] = LevelController.lc.cameraIntroAnimation;

        m_reset_trigger_ID = Animator.StringToHash("reset");

        m_dirOffset = dirOffset; m_upOffset = upOffset;

        LevelController.lc.Register(this); // This makes sure Reset() is called upon a level restart.
    }

    void LateUpdate()
    {
        HandleInput();

        // Get the current player position
        m_playerPos = PlayerController.pc.gameObject.transform.position;

        // If the player is not falling, compensate for movement in the Up component 
        // due to animations by snapping it back to the grid. 
        if (!PlayerController.pc.isFalling) m_playerPos = m_playerPos.SnapToGridUp(PlayerController.pc.world_up);

        // Set m_dir, m_up and m_pos depening on 
        if (camState == CamState.Default)         Follow();
        else if (camState <= CamState.RotBack)    Rotate();
        else if (camState == CamState.GravChange) GravityChange();
        else if (camState == CamState.Pause)      Pause();
        else if (camState == CamState.Anim)       return;

        // Tilt if a tilt button is pressed or interpolate back to normal position
        Tilt();

        transform.position = m_playerPos + m_dir * m_dirOffset + m_upOffset * m_up;
        transform.LookAt(m_lookAt, m_up);
    }

    public override void Reset()
    {
        m_anim.SetTrigger(m_reset_trigger_ID);
    }

#endregion Base_Classes

    bool CanRotate()
    {
        return IsDefault() && !PlayerController.pc.isMoving && !PlayerController.pc.isWarping && !PlayerController.pc.isFalling;
    }

    bool CanTilt()
    {
        return !PlayerController.pc.isFalling;
    }

    void Follow()
    {
        m_up     = PlayerController.pc.world_up;
        m_dir    = PlayerController.pc.world_direction;
        m_lookAt = m_playerPos + lookAtUpOffset * m_up;
    }

    void GravityChange()
    {
        float epsilon = 0.001f;

        Vector3 target_up  = PlayerController.pc.world_up;
        Vector3 target_dir = PlayerController.pc.world_direction;

        m_dir    = Vector3.RotateTowards(m_dir, target_dir, Time.deltaTime * rotSpeed * 2, 0.0f);
        m_up     = Vector3.RotateTowards(m_up, target_up, Time.deltaTime * rotSpeed * 2, 0.0f);
        m_lookAt = m_playerPos + lookAtUpOffset * m_up;

        // As this is triggered by the PlayerController the pc also takes care of the world_dir/up changes. 
        if (Vector3.Distance(m_dir, target_dir) < epsilon && Vector3.Distance(m_up, target_up) < epsilon)
            camState = CamState.Default;
    }

    void HandleInput()
    {
        if      (Input.GetButtonDown("Horizontal") && Input.GetAxisRaw("Horizontal") == -1 && CanRotate())
            camState = CamState.RotLeft;
        else if (Input.GetButtonDown("Horizontal") && Input.GetAxisRaw("Horizontal") ==  1 && CanRotate())
            camState = CamState.RotRight;
        else if (Input.GetButtonDown("Vertical")   && Input.GetAxisRaw("Vertical")   == -1 && CanRotate())
            camState = CamState.RotBack;

        // Rotation is possible while tilting
        if      (Input.GetButton("LookUp")   && CanTilt()) m_tilt = 1;
        else if (Input.GetButton("LookDown") && CanTilt()) m_tilt = -1;
        else m_tilt = 0;
    }

    public bool IsDefault()
    {
        return camState == CamState.Default;
    }

    public void Pause()
    {
        camState    = CamState.Pause;
        m_lookAt    = LevelController.lc.pauseCamLookAt;
        m_playerPos = LevelController.lc.pauseCamPos;
    }

    public void Resume()
    {
        camState = CamState.Default;
    }

    void Rotate()
    {
        float epsilon = 0.001f;
        Vector3 target;

        if (camState == CamState.RotLeft || camState == CamState.RotRight)
        { 
            int     dir    = camState == CamState.RotLeft ? -1 : 1;
            target = dir * Vector3.Cross(PlayerController.pc.world_up, PlayerController.pc.world_direction);
            m_dir          = Vector3.RotateTowards(m_dir, target, Time.deltaTime * rotSpeed, 0.0f);
        }
        else // Camstate.RotBack
        {
            target = -PlayerController.pc.world_direction;
            m_dir          = Vector3.RotateTowards(m_dir, target, Time.deltaTime * 2*rotSpeed, 0.0f);
        }

        // If the new position has been reached, go back to Default state
        if (Vector3.Distance(m_dir, target) < epsilon)
        {
            PlayerController.pc.world_direction = target;
            camState = CamState.Default;
        }
    }

    void Tilt()
    { // @TODO make the tilt positions inspector variables as well
        float target_dir = dirOffset, target_up = upOffset;
        if (m_tilt == -1)
        { target_dir = -0.01f; target_up = upOffset; }
        else if(m_tilt == 1)
        { target_dir = -0.7f; target_up = 0.2f; }

        m_dirOffset = Mathf.MoveTowards(m_dirOffset, target_dir, Time.deltaTime * tiltSpeed);
        m_upOffset  = Mathf.MoveTowards(m_upOffset,  target_up,  Time.deltaTime * tiltSpeed);
    }

    //---------------------------------------------------//
    // Currently only used for debug outputs. 
    void Update()
    {
        // Green : Gravity
        // White : Target 
        // Red: world diretion
        // cyan: up
        // Blue :Check Path front
        // Black: Check path down
        // Green: Offset

        //Debug.DrawRay(player.transform.position, offset, Color.green);
        //Debug.DrawRay(player.transform.position, world_up, Color.cyan);
        //Debug.DrawRay(transform.position, m_targetPosition - transform.position);
        //Debug.DrawRay(transform.position, Physics.gravity, Color.green);
        //Debug.DrawRay(player.transform.position, world_direction, Color.red);
        //Debug.DrawRay(transform.position, world_direction * m_sphereRadius * 1.25f, Color.blue);
        //Debug.DrawRay(transform.position + 0.05f * world_direction, -world_up, Color.black);
    }
}
