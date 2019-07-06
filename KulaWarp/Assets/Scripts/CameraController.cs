using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    #region
    public  GameObject game;
    private GameObject player;

    private PlayerController m_pc;
    private GameController   m_gc;

    [HideInInspector] public bool isMoving;

    public Vector3  offset      = new Vector3(-1.6f, 1.3f, 0);
    public float    offsetAngle = 0.71762f, cameraSpeed = 0.5f, tiltSpeed = 0.5f;

    private float    m_invCameraSpeed, m_boxsize, m_sphereRadius;
    private bool     m_isMovingUpDown = false;
    private float    m_upOff, m_dirOff;
    private Animator m_anim;

    #endregion

    void Start()
    {
        LoadComponents();       

        transform.position = player.transform.position + offset;
        transform.LookAt(player.transform.position + offsetAngle * m_pc.world_up); 

        m_invCameraSpeed     = 1 / cameraSpeed;
        m_boxsize            = 1.0f; // @TODO make this initializable by the Scene/level
        m_sphereRadius       = m_pc.player_sphere.GetComponent<SphereCollider>().radius * player.transform.lossyScale.x;

        m_upOff  = Mathf.Abs(offset.getComponent(m_pc.world_up));
        m_dirOff = Mathf.Abs(offset.getComponent(m_pc.world_direction));

        m_anim   = gameObject.GetComponent<Animator>();
        StartCoroutine(PlayIntro());
    }

    void LoadComponents()
    {
        m_gc   = game.GetComponent<GameController>();
        player = m_gc.player;
        m_pc   = player.GetComponent<PlayerController>();
    }

    void LateUpdate()
    {
        int input = HandleInput();

        if (CanRotate() && input != 0)
            StartCoroutine(CameraRotate(input));

        // The methods that move the camera also take care of their placement and lookat for these frames.
        // So only do that here if the camera is not moving.
        if (!(isMoving || m_isMovingUpDown || m_gc.isPaused))
        {
            // The Idle Animation of the played causes a slight up and down when the play is not moving. So use the gridPos
            // rather than the real pos when moving the camera behind the player 
            Vector3 gridPos    = m_pc.isFalling ? player.transform.position : SnapToGrid(player.transform.position);
            transform.position = gridPos + offset;

            // Look in the direction of the player at a point <offsetAngle> units above (w/r to the world up vector) the center of the player. 
            transform.LookAt(gridPos + offsetAngle * m_pc.world_up, m_pc.world_up);
        }
    }

    /**
    * The camera can only rotate if it is not already moving and while the play is not moving. 
    */
    bool CanRotate()
    {
        return !(isMoving || m_pc.isMoving || m_isMovingUpDown || m_pc.isWarping || m_pc.isFalling);
    }

    int HandleInput()
    {
        return (int)(Input.GetAxisRaw("Horizontal")); // For keyboard input this is in {-1, 0, 1}. Left is -1, right is 1.
    }

    /**
     * Rotate the camera to the left (-1) or to the right (1) with respect to the current direction
     * and up vector.
     */
    IEnumerator CameraRotate(int dir)
    {
        isMoving = true;

        // Turn Camera to the Left of the Player
        Vector3 target  = -m_dirOff * dir * Vector3.Cross(m_pc.world_up, m_pc.world_direction) + m_upOff * m_pc.world_up;
        Vector3 start   = offset;

        float t = 0;

        while(t * m_invCameraSpeed <= 1)
        {
            t     += Time.deltaTime;
            offset = Vector3.Slerp(start, target, t * m_invCameraSpeed);

            // Move and rotate the camera accordigly. 
            transform.position = player.transform.position + offset;
            transform.LookAt(player.transform.position + offsetAngle * m_pc.world_up, m_pc.world_up);

            yield return null;
        }
        m_pc.world_direction = dir*Vector3.Cross(m_pc.world_up, m_pc.world_direction);

        isMoving = false;
    }

    /**
     * Rotates the camera up(1) or down(-1).
     */
    public IEnumerator CameraUpDown(int dir)
    {
        m_isMovingUpDown = true;

        // Turn Camera up/down when the player moves up a wall or down an edge.
        Vector3 start  = offset;
        Vector3 target = -m_dirOff * m_pc.world_direction + m_upOff * m_pc.world_up;

        Vector3 upStart  = m_pc.world_direction * dir;
        Vector3 upTmp    = upStart;
        
        float t = 0;

        while (t * tiltSpeed <= 1)
        {
            t     += Time.deltaTime;
            offset = Vector3.Slerp(start, target, t * tiltSpeed);
            upTmp  = Vector3.Slerp(upStart, m_pc.world_up, t * tiltSpeed);

            // Move and rotate the camera accordigly. 
            transform.position = player.transform.position + offset;
            transform.LookAt(player.transform.position + offsetAngle * upTmp, upTmp);

            yield return null;
        }

        m_isMovingUpDown = false;
    }

    /**
     * dir and up are the target direction and up vectors. This has to be called before they are changed
     */
    public IEnumerator GravityChange(Vector3 dir, Vector3 up)
    {
        m_isMovingUpDown = true;

        float t = 0;
        Vector3 start  = offset;
        Vector3 target = -m_dirOff * dir + m_upOff * up;

        Vector3 upStart = m_pc.world_up;
        Vector3 upTmp   = upStart;

        while (t * tiltSpeed <= 1)
        {
            t += Time.deltaTime;
            offset = Vector3.Slerp(start, target, t * tiltSpeed);
            upTmp  = Vector3.Slerp(upStart, m_pc.world_up, t * tiltSpeed);

            // Move and rotate the camera accordigly. 
            transform.position = player.transform.position + offset;
            transform.LookAt(player.transform.position + offsetAngle * upTmp, upTmp);

            yield return null;
        }

        m_isMovingUpDown = false;
    }

    /**
     * This rounds the up component of @vec to valid multiples of m_boxsize/2 +/- m_sphere Radius.
     * 
     * This is mainly necessary because the Idle animation introduces small numerical changes in the
     * up component. To avoid jumping of the ball this has to be countered. 
     */ // @TODO implemented the same function twice now. Maybe refactor and put in one common location?
    Vector3 SnapToGrid(Vector3 vec)
    {
        return vec.Round(m_pc.world_up) - (m_boxsize * 0.5f - m_sphereRadius) * m_pc.world_up;
    }

    /**
  * Plays an intro animation. During this time @isMoving is set to true to avoid camera movements.
  */
    public IEnumerator PlayIntro()
    {
        isMoving = true;
        m_anim.enabled = true;

        m_anim.Play("CameraIntro");
        float animLength = m_anim.runtimeAnimatorController.animationClips[0].length;

        //@TODO this is very dirty for now with the wait. Check if there is a better solution
        yield return new WaitForSeconds(animLength);

        isMoving = false;
        m_anim.enabled = false;
    }

    public void ResetCamera()
    {
        offset = m_gc.startUp * m_upOff - m_gc.startDir * m_dirOff;
        transform.position = m_gc.startPosPlayer + offset;

        transform.LookAt(m_gc.startPosPlayer + offsetAngle * m_gc.startUp);

        StartCoroutine(PlayIntro());
    }

    public void PauseCamera()
    {
        transform.position = new Vector3(1.5f, 6.5f, -6); // @TODO make this adjustable in the inspector
        transform.rotation = Quaternion.Euler(54, 0, 0);
    }

    public void ResumeCamera()
    {

    }

    // Currently only used for debuf outputs. 
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
