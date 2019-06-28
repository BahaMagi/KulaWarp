using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    #region
    public GameObject           player;
    private PlayerController    m_pc;

    [HideInInspector] public Vector3    world_direction, world_up;
    [HideInInspector] public bool       isMoving;

    public Vector3  offset          = new Vector3(-1.6f, 1.3f, 0);
    public float    offsetAngle     = 0.71762f, cameraSpeed = 0.5f;

    private float m_invCameraSpeed, m_boxsize, m_sphereRadius, m_updownSpeed;
    private bool  m_isTilting = false;

    #endregion

    void Start()
    {
        m_pc = player.GetComponent<PlayerController>();

        world_direction   = Vector3.right; // @TODO both have to be initializable by the Scene/Level
        world_up          = Vector3.up;

        transform.position = player.transform.position + offset;
        transform.LookAt(player.transform.position + offsetAngle * world_up);

        m_invCameraSpeed     = 1 / cameraSpeed;
        m_updownSpeed        = 2.0f;
        m_boxsize            = 1.0f; // @TODO make this initializable by the Scene/leven
        m_sphereRadius       = m_pc.player_sphere.GetComponent<SphereCollider>().radius * player.transform.lossyScale.x;
    }

    void LateUpdate()
    {
        int input = HandleInput();

        if (CanRotate() && input != 0)
            StartCoroutine(CameraRotate(input));

        if (!(isMoving || m_isTilting))
        {
            // The Idle Animation of the played causes a slight up and down when the play is not moving. So use the gridPos
            // rather than the real pos when moving the camera behind the player 
            Vector3 gridPos = SnapToGrid(player.transform.position);
            transform.position = gridPos + offset;

            // Look in the direction of the player at a point <offsetAngle> units above (w/r to the world up vector) the center of the player. 
            transform.LookAt(gridPos + offsetAngle * world_up, world_up);
        }
    }

    /**
    * The camera can only rotate if it is not already moving and while the play is not moving. 
    */
    protected bool CanRotate()
    {
        return !(isMoving || m_pc.isMoving || m_isTilting);
    }

    protected int HandleInput()
    {
        return (int)(Input.GetAxisRaw("Horizontal")); // For keyboard input this is in {-1, 0, 1}. Left is -1, right is 1.
    }

    /**
     * Rotate the camera to the left (-1) or to the right (1) with respect to the current direction
     * and up vector.
     */
    protected IEnumerator CameraRotate(int dir)
    {
        isMoving = true;

        // Turn Camera to the Left of the Player
        Vector3 target  = Quaternion.AngleAxis(90 * dir, world_up) * offset;
        Vector3 start   = offset;

        float t = 0;

        while(t * m_invCameraSpeed < 1)
        {
            t += Time.deltaTime;
            offset = Vector3.Slerp(start, target, t * m_invCameraSpeed);

            // Move and rotate the camera accordigly. 
            transform.position = player.transform.position + offset;
            transform.LookAt(player.transform.position + offsetAngle * world_up, world_up);

            yield return null;
        }
        world_direction = Quaternion.AngleAxis(90 * dir, world_up) * world_direction;
        m_pc.world_direction = world_direction;

        isMoving = false;
    }

    /**
     * Rotates the camera up(1) or down(-1).
     */
    public IEnumerator CameraUpDown(int dir)
    {
        m_isTilting = true; // @TODO use isMoving instead.
        // Turn Camera up/down when the player moves up a wall or down an edge.
        Vector3 target = Quaternion.AngleAxis(90, Vector3.Cross(world_up, world_direction)*-dir) * offset;
        Vector3 start  = offset;

        Vector3 upStart  = world_up;
        Vector3 upTarget = -dir * world_direction;
        Vector3 tmp      = world_up;
        world_direction  = dir * world_up;
        
        float t = 0;

        while (t * m_invCameraSpeed < 1)
        {
            t     += Time.deltaTime;
            offset = Vector3.Lerp(start, target, t * m_invCameraSpeed * m_updownSpeed);
            tmp    = Vector3.Lerp(upStart, upTarget, t * m_invCameraSpeed * m_updownSpeed);

            // Move and rotate the camera accordigly. 
            transform.position = player.transform.position + offset;
            transform.LookAt(player.transform.position + offsetAngle * tmp, tmp);

            yield return null;
        }
        world_up = tmp.Round(world_up);
        m_isTilting = false;
    }

    /**
     * This rounds the up component of @vec to valid multiples of m_boxsize/2 +/- m_sphere Radius.
     * 
     * This is mainly necessary because the Idle animation introduces small numerical changes in the
     * up component. To avoid jumping of the ball this has to be countered. 
     */ // @TODO implemented the same function twice now. Maybe refactor and put in one common location?
    private Vector3 SnapToGrid(Vector3 vec)
    {
        return vec.Round(world_up) - (m_boxsize * 0.5f - m_sphereRadius) * world_up;
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
        Debug.DrawRay(player.transform.position, world_up, Color.cyan);
        //Debug.DrawRay(transform.position, m_targetPosition - transform.position);
        //Debug.DrawRay(transform.position, Physics.gravity, Color.green);
        Debug.DrawRay(player.transform.position, world_direction, Color.red);
        //Debug.DrawRay(transform.position, world_direction * m_sphereRadius * 1.25f, Color.blue);
        //Debug.DrawRay(transform.position + 0.05f * world_direction, -world_up, Color.black);
    }
}
