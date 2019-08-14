using UnityEngine;

/**
* Provides a smooth, rotatable (in 90° steps) follow camera that can be tilted up and down to allow 
* for a different viewing angle. @TODO This class will be turned into a StateMachine like the PlayerController
*/
public class CameraController : ObjectBase
{
    public static CameraController cc;

    public float dirOffset = -1.6f, upOffset  = 1.3f, lookAtUpOffset = 0.71762f;
    public float rotSpeed  = 0.5f,  tiltSpeed = 0.5f, followSpeed    = 1.0f;

    public enum CamState {Default, RotLeft, RotRight, RotBack, GravChange, Pause, Anim};
    [ReadOnly] public CamState camState = CamState.Anim;

    private Animator m_anim;
    private AnimatorOverrideController m_animOverrideCtrl;
    private int m_reset_trigger_ID;

    [ReadOnly] public Vector3 m_playerPos, m_up, m_lookAt, m_dir;
    private int     m_tilt = 0;
    private float   m_dirOffset, m_upOffset;
    private float   m_rotProgress = 0.0f;

    private bool m_keyDown = false; // Axis Input does not provide GetXXDown() so this acts as replacement

    // Base Classes ObjectBase and MonoBehaviour:

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

        // Initialize camera offsets. They are stored in separate variable as they will be changed 
        // repeatedly and the public versions act as reference. 
        m_dirOffset = dirOffset; m_upOffset = upOffset;
        m_dir       = LevelController.lc.startDir; m_up = LevelController.lc.startUp;

        // Register this object with the LevelController so it is reset on a restart
        LevelController.lc.Register(this);
    }

    void LateUpdate()
    {
        HandleInput();

        // Get the current player position
        m_playerPos = PlayerController.pc.gameObject.transform.position;

        // If the player is not falling, compensate for movement in the Up component 
        // due to animations by snapping it back to the grid. 
        if (!(PlayerController.pc.state == PlayerController.PlayerState.Falling) &&
            camState != CamState.GravChange)
            m_playerPos = m_playerPos.SnapToGridUp(PlayerController.pc.world_up);

        // Set m_dir, m_up and m_pos depening on 
        if (camState == CamState.Default)         Follow();
        else if (camState <= CamState.RotBack)    Rotate();
        else if (camState == CamState.GravChange) GravityChange();
        else if (camState == CamState.Pause)      Pause();
        else if (camState == CamState.Anim)       return;

        // Tilt if a tilt button is pressed or interpolate back to normal position
        Tilt();

        // Set the new camera position and LookAt
        Vector3 target = m_playerPos + m_dir * m_dirOffset + m_upOffset * m_up;
        if (PlayerController.pc.state != PlayerController.PlayerState.Warping)
        {
            if (camState > CamState.Default && camState <= CamState.RotBack)
                transform.position = target;
            else if (camState == CamState.GravChange)
                transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * 1.5f * followSpeed);
            else
                transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * followSpeed);

            transform.LookAt(m_lookAt, m_up);
        }
    }

    public override void Reset()
    {
        // Play the camera intro animation again when level is reset
        camState = CamState.Anim;
        m_anim.SetTrigger(m_reset_trigger_ID); 
    }

    // CameraController:

    bool CanRotate()
    {// The Camera can rotate whenever the player is NOT moving in any way
        return IsDefault() && (PlayerController.pc.state == PlayerController.PlayerState.Idle);
    }

    bool CanTilt()
    {// The Camera can tilt whenever the player is not falling. That means, the camera can tile while the player
     // is moving!
        return !(PlayerController.pc.state == PlayerController.PlayerState.Falling);
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

        // Rotate current direction and up vector one step towards the target vectors. 
        // This will be called once per Update() and is finished once the distance to the target vectors is 
        // smaller than epsilon.
        // @TODO This will be replaced with Quaternion rotation. Also this will be linked to the 
        //       PlayerController State to prevent the camera from blocking input.
        m_dir    = Vector3.RotateTowards(m_dir, target_dir, Time.deltaTime * rotSpeed, 0.0f);
        m_up     = Vector3.RotateTowards(m_up, target_up, Time.deltaTime * rotSpeed, 0.0f);
        m_lookAt = m_playerPos + lookAtUpOffset * m_up;

        // As this is triggered by the PlayerController the pc also takes care of the world_dir/up changes. 
        if ((m_dir - target_dir).sqrMagnitude < epsilon && 
            (m_up - target_up).sqrMagnitude < epsilon)
            camState = CamState.Default;
    }

    void HandleInput()
    {
        if      (Input.GetAxisRaw("Horizontal") == -1 && CanRotate() && !m_keyDown)
            { camState = CamState.RotLeft; m_keyDown = true; }
        else if (Input.GetAxisRaw("Horizontal") ==  1 && CanRotate() && !m_keyDown)
            { camState = CamState.RotRight; m_keyDown = true; }
        else if (Input.GetAxisRaw("Vertical")   == -1 && CanRotate() && !m_keyDown)
            { camState = CamState.RotBack; m_keyDown = true; }
        else if (Input.GetAxisRaw("Horizontal") == 0 &&
                 Input.GetAxisRaw("Vertical") == 0 && IsDefault())
            m_keyDown = false;

        // Rotation is possible while tilting, hence the separate if/else
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
        m_rotProgress += Time.deltaTime * rotSpeed;
        m_rotProgress  = Mathf.Clamp(m_rotProgress, 0.0f, 1.0f);

        float targetAngle = 0.0f;
        switch (camState)
        {
            case CamState.RotLeft:  targetAngle = -90.0f; break;
            case CamState.RotRight: targetAngle =  90.0f; break;
            case CamState.RotBack:  targetAngle = 180.0f; break;
        }

        // Apply interpolated rotation for one step. This will be called once per Update() 
        // until the target angle has been reached. 
        Quaternion q = Quaternion.AngleAxis(m_rotProgress * targetAngle, PlayerController.pc.world_up);
        m_dir        = q * PlayerController.pc.world_direction;

        // If the new position has been reached, go back to Default state
        if (m_rotProgress >= 1.0f)
        {
            PlayerController.pc.world_direction = Vector3Int.RoundToInt(m_dir);

            camState    = CamState.Default;
            m_rotProgress = 0.0f;
        }
    }

    void Tilt()
    { // @TODO make the tilt positions inspector variables as well
        float target_dir = dirOffset, target_up = upOffset;

        if (m_tilt == -1)   { target_dir = -0.01f; target_up = upOffset; } // Tilt down
        else if(m_tilt == 1){ target_dir = -0.7f;  target_up = 0.2f; } // Tilt Up

        // Move the camera towards the new position. No rotation has to be done as the LookAt will 
        // ensure that the camera keeps facing the LookAt position above the player. 
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
