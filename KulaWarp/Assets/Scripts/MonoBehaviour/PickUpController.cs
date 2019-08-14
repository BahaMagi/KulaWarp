using UnityEngine;

/**
* BaseClass that is attached to all objects which can be picked up by the player. It takes care of idle 
* animations, pick up animations (@TODO) and pick up behaviour.
* The class is used directly for items giving points. Energy has its own controller that inherits from this. 
*/
public class PickUpController : ObjectBase
{
    public float rotSpeed   = 45.0f; // Speed of the rotation in angle / sec 
    public float hoverSpeed = 2.0f, hoverAmount = 0.01f; // Up/down cylces / sec and distance in units from center of the pickup
    public int scoreValue   = 100; //Point value of this pickup
    public Vector3 up       = Vector3.up; // The up vector for a pick up has to be specified as depending on the position this could be ambiguous

    protected float   m_timeOffset = 0.0f; // Time offset to start animations at a different point in time
    protected Vector3 m_idlePos;

    // Base Classes ObjectBase and MonoBehaviour:

    protected void Awake()
    {
        // Initialize the animations randomly to have each pickup start the animation at a different point
        m_idlePos    = transform.position;
        m_timeOffset = Random.Range(0, 6);

        // Initiate Object Rotation based on the given Up vector. This is just here so it does not have to
        // be done in the inspector for every single pickup. 
        transform.rotation = Quaternion.FromToRotation(Vector3.up, up);
        transform.Rotate(new Vector3(0, Random.Range(0, 180), 0));

        // Register this object with the LevelController so it is reset on a restart
        LevelController.lc.Register(this);
    }

    protected void Update()
    {
        AnimObject();
    }

    public override void Reset()
    {
        gameObject.SetActive(true);
    }

    // PickUpController:

    protected void AnimObject()
    {
        transform.Rotate(Vector3.up, rotSpeed * Time.deltaTime);
        transform.position = m_idlePos + up * Mathf.Sin(hoverSpeed * m_timeOffset) * hoverAmount;

        m_timeOffset += Time.deltaTime;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        gameObject.SetActive(false);
        LevelController.lc.Score(scoreValue);
    }

    
}
