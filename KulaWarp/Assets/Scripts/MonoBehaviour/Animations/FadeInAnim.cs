using UnityEngine;
using UnityEngine.UI;

public class FadeInAnim : MonoBehaviour
{
    public float          duration = 0.5f;
    public AnimationCurve animCurve;

    private bool  m_isPlaying = false;
    private float m_timer     = 0.0f, m_alpha;
    private Image m_img;

    void Awake()
    {
        m_img = gameObject.GetComponent<Image>();
    }

    public void Play()
    {
        m_timer         = 0;
        m_isPlaying     = true;
        m_img.color     = new Color(m_img.color.r, m_img.color.g, m_img.color.b, 0f);
    }

    private void EvalCurves()
    {
        float a     = animCurve.Evaluate(Mathf.InverseLerp(0, duration, m_timer));
        m_img.color = new Color(m_img.color.r, m_img.color.g, m_img.color.b, a);
    }

    private void Update()
    {
        if (m_isPlaying)
        {
            m_timer += Time.deltaTime;

            EvalCurves();

            if (m_timer >= duration)
            {
                m_timer     = 0.0f;
                m_isPlaying = false;
            }
        }
    }

    public bool isPlaying()
    { return m_isPlaying; }
}
