using UnityEngine;

public class HoverAnim : MonoBehaviour
{
    public float          duration = 0.5f;
    public float          height   = 1.0f;
    public AnimationCurve animCurve;

    private bool      m_isPlaying = false;
    private float     m_timer     = 0.0f;
    private Rigidbody m_rb;
    private Vector3   m_pos;

    void Awake()
    {
        m_rb = GetComponent<Rigidbody>();
    }

    public void  Play()
    {
        m_rb.useGravity = false;
        m_timer         = 0;
        m_isPlaying     = true;
        m_pos           = transform.position;
    }

    private void EvalCurves()
    {
        transform.position = m_pos + Vector3.up * height * animCurve.Evaluate(Mathf.InverseLerp(0, duration, m_timer));
    }

    private void EndAnim()
    {
        m_rb.useGravity = true;
    }

    private void FixedUpdate()
    {
        if (m_isPlaying)
        {
            m_timer += Time.deltaTime;

            EvalCurves();

            if (m_timer >= duration)
            {
                EndAnim();
                m_timer     = 0.0f;
                m_isPlaying = false;
            }
        }
    }

    public bool isPlaying()
    { return m_isPlaying; }
}