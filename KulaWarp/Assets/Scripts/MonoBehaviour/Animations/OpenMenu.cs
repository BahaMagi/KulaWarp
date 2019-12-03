using UnityEngine;
using UnityEngine.UI;

public class OpenMenu : MonoBehaviour
{
    public GameObject bgPanel;
    public GameObject settingsPanel;
    public GameObject selectedButton;

    public float minWidth = 410.0f, minHeight = 410.0f;
    public float duration = 0.5f;
    public AnimationCurve animCurve;

    private RectTransform m_rect;
    private Button        m_selectedButton;
    private float         m_maxWidth, m_maxHeight;
    private bool          m_animPlaying = false, m_closing = false;
    private float         m_timer = 0;
        
    private void Awake()
    {
        LoadComponents();

        m_maxHeight = m_rect.rect.height;
        m_maxWidth  = m_rect.rect.width;
    }

    private void Update()
    {
        if (m_animPlaying)
        {
            m_timer += Time.deltaTime;

            float w, h;
            if (m_closing)
            {
                w = Mathf.Lerp(minWidth,  m_maxWidth,  1 - animCurve.Evaluate(Mathf.InverseLerp(0, duration, m_timer)));
                h = Mathf.Lerp(minHeight, m_maxHeight, 1 - animCurve.Evaluate(Mathf.InverseLerp(0, duration, m_timer)));
            }
            else
            {
                w = Mathf.Lerp(minWidth,  m_maxWidth,  animCurve.Evaluate(Mathf.InverseLerp(0, duration, m_timer)));
                h = Mathf.Lerp(minHeight, m_maxHeight, animCurve.Evaluate(Mathf.InverseLerp(0, duration, m_timer)));
            }
            m_rect.sizeDelta = new Vector2(w, h);

            if (m_timer >= duration)
            {
                m_timer       = 0.0f;
                m_animPlaying = false;

                if (m_closing)
                {
                    bgPanel.SetActive(false);
                    gameObject.SetActive(false);
                }
                else
                {
                    // After the open animation is over make the 
                    // SettingsPanel visible as well and set the selection 
                    // to a button so controler input works right away. 
                    settingsPanel.SetActive(true);
                    m_selectedButton.Select();
                    m_selectedButton.OnSelect(null);
                }
            }
        }
    }

    void LoadComponents()
    {
        m_rect           = GetComponent<RectTransform>();
        m_selectedButton = selectedButton.GetComponent<Button>();
    }

    public void Open()
    {
        // Activate background panel to darken the scene
        bgPanel.SetActive(true);

        // Activate the menu panel but down size it
        gameObject.SetActive(true);
        m_rect.sizeDelta = new Vector2(minWidth, minHeight);
        m_animPlaying    = true;
        m_closing        = false;
    }

    public void Close()
    {
        // Make settings immediately invisible but wait with 
        // background and menu panel until after the animation.
        settingsPanel.SetActive(false);

        m_animPlaying = true;
        m_closing     = true;
    }
}
