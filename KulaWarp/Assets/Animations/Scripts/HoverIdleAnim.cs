using UnityEngine;

/**
 * Animation that rotates the object around the <up> vector with <rotSpeed> degree per second. 
 * Furthermore, the object will float up and down with frequency <hoverSpeed> and distance
 * <hoverAmont> in units from the center of the object to the extrems points of the movement. 
 * 
 * Start position and rotation are randomized. 
 */
public class HoverIdleAnim : MonoBehaviour
{
    public float   rotSpeed   = 45.0f; // Speed of the rotation in angle / sec 
    public float   hoverSpeed = 2.0f, hoverAmount = 0.01f; // Up/down cylces / sec and distance in units from center of the object
    public Vector3 up         = Vector3.up; // Rotation axis and float movement direction

    private float   m_t = 0.0f;
    private Vector3 m_idlePos;

    protected void Awake()
    {
        // Initialize the animations randomly to have each pickup start the animation at a different point
        m_idlePos   = transform.position;
        m_t         = Random.Range(0, 6); // Randomize start time. 

        // Initiate Object Rotation based on the given Up vector. This is just here so it does not have to
        // be done in the inspector for every single pickup. 
        transform.rotation = Quaternion.FromToRotation(Vector3.up, up);
        transform.Rotate(new Vector3(0, Random.Range(0, 180), 0));
    }

    protected void Update()
    {
        transform.Rotate(Vector3.up, rotSpeed * Time.deltaTime);
        transform.position = m_idlePos + up * Mathf.Sin(hoverSpeed * m_t) * hoverAmount;

        m_t += Time.deltaTime; // @ TODO make this % c to avoid loss of precision 
    }
}
