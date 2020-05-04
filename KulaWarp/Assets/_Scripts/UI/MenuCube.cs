using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System;

public class MenuCube : MonoBehaviour
{
    public GameObject selector;

    public float rotationTime = 0.4f, selectorRotAmount = 500.0f;
    public int   defaultEntry = 0;

    [ReadOnlyWhenPlaying]
    public MenuEntry[] menuEntries;
    [ReadOnlyWhenPlaying]
    public bool invertMenuCntrl = false;

    private int   m_rotating    = 0, m_invertMenuCntrl = 1;
    private int   m_currentSide = 0, m_curMenuEntry    = 0;
    private float m_angle       = 0.0f;

    private Quaternion[]      m_cubeRotations;
    private TextMeshProUGUI[] m_tmpTexts;
    private WarpAnimation     m_selectorWarpAnim;
    private HoverAnim         m_selectorHoverAnim;
    private AudioSource[]     m_audioSources;


    // Base Class MonoBehaviour:

    void Awake()
    {
        m_curMenuEntry = defaultEntry;

        // Invert Menu Control. Having an int makes calculation easier.
        m_invertMenuCntrl = invertMenuCntrl ? -1 : 1;

        // Initialize cube rotations. Used to make up for inaccurate rotations.
        // Applying rotation with index i makes text i face the camera. 
        // Index 0 is at start facing the camera, 1 is on top, 2 facing away 
        // and 3 on the bottom. 
        m_cubeRotations = new Quaternion[4];
        m_cubeRotations[0] = transform.rotation;
        transform.RotateAround(transform.position, transform.right, -90);
        m_cubeRotations[1] = transform.rotation;
        transform.RotateAround(transform.position, transform.right, -90);
        m_cubeRotations[2] = transform.rotation;
        transform.RotateAround(transform.position, transform.right, -90);
        m_cubeRotations[3] = transform.rotation;
        transform.RotateAround(transform.position, transform.right, -90);

        // Load references to game objects and components
        LoadComponents();

        // Initialize the text fields
        InitTexts();
    }

    void FixedUpdate()
    {
        if (m_rotating != 0)
            RotateCube();
    }


    // MenuCube:

    void LoadComponents()
    {
        // Obtain references to the text fields.
        // Index 0 is at start facing the camera, 1 is on top, 2 facing away 
        // and 3 on the bottom. 
        m_tmpTexts    = new TextMeshProUGUI[4];
        m_tmpTexts[0] = transform.FindDeepChild("Text1").GetComponent<TextMeshProUGUI>();
        m_tmpTexts[1] = transform.FindDeepChild("Text2").GetComponent<TextMeshProUGUI>();
        m_tmpTexts[2] = transform.FindDeepChild("Text3").GetComponent<TextMeshProUGUI>();
        m_tmpTexts[3] = transform.FindDeepChild("Text4").GetComponent<TextMeshProUGUI>();

        // The order of the AudioSource objects in the array is the same as in the inspector.
        // There are 2 sources: 
        // [0] holds the SelectionChange sound effect
        // [1] holds the selection confirm sound effect
        m_audioSources = gameObject.GetComponents<AudioSource>();
    }

    /**
     * Rotates the menu cube by 90° as well as the sphere on top.
     * The rotation of the sphere is not physically exact but rather given by @selectorRotAmount.
     */
    void RotateCube()
    {
        // Calculate angle for rotating the cube
        float dAngle = m_invertMenuCntrl * m_rotating * 90.0f * (1 / rotationTime) * Time.deltaTime;
        m_angle     += Mathf.Abs(dAngle);

        // Calculate angle for rotating the sphere selector
        float dSelectorAngle = (m_invertMenuCntrl * m_rotating * selectorRotAmount) * Time.deltaTime;

        // Apply rotations
        transform.RotateAround(transform.position, transform.right, dAngle);
        selector.transform.RotateAround(selector.transform.position, Vector3.right, -dSelectorAngle);

        // If the cube rotated 90° stop rotating by setting @m_rotating back to 0.
        // Furthermore, to make up for small inaccuracies, the rotation is set once more
        // to the exact intended value. 
        if (m_angle >= 90.0f)
        {
            transform.rotation = m_cubeRotations[m_currentSide];
            m_angle    = 0.0f;
            m_rotating = 0;
        }
    }

    void InitTexts()
    {
        // Text facing the camera
        m_tmpTexts[m_currentSide].text = menuEntries[m_curMenuEntry].text;
        // Text on top
        int entryAbove = invertMenuCntrl ? menuEntries.Length-1 : 1;
        m_tmpTexts[1].text = menuEntries[entryAbove].text;
        // Text below
        int entryBelow = (!invertMenuCntrl) ? menuEntries.Length - 1 : 1;
        m_tmpTexts[3].text = menuEntries[entryBelow].text;
    }

    /**
     * Changes the selected menu entry and triggers the rotation of the cube 
     * and sphere. The rotation direction is affected by @dir \in {-1, 0, 1} 
     * and whether menu controls are inverted with @m_invertMenuCntrl.
     */
    public void ChangeSelectedEntry(int dir, bool audio = true)
    {
        if (m_rotating != 0)
            return;

        // Play the according sound effect
        if (audio)
            m_audioSources[0].Play();

        // Get side that is going to face the camera
        m_currentSide += m_invertMenuCntrl * -dir;
        m_currentSide  = (m_currentSide == -1) ? 3 : (m_currentSide % 4);

        // Activate rotation in Update()
        m_rotating = dir;

        // Get the menu text that is going to be shown
        m_curMenuEntry += m_invertMenuCntrl * dir;
        m_curMenuEntry  = m_curMenuEntry == -1 ? menuEntries.Length-1 : m_curMenuEntry % menuEntries.Length;

        // Set the text accordingly
        m_tmpTexts[m_currentSide].text = menuEntries[m_curMenuEntry].text;
    }

    public void Confirm()
    {
        // Play the according sound effect
        m_audioSources[1].Play();

        menuEntries[m_curMenuEntry].callBack.Invoke();
    }

    public MenuEntry getCurMenuEntry()
    {
        return menuEntries[m_curMenuEntry];
    }

    // MenuEntry Class:

    [Serializable]
    public class MenuEntry
    {
        public string     text;
        public MenuCube   subMenu;
        public UnityEvent callBack;

        public MenuEntry(string Text, UnityEvent cllBck)
        { text = Text; this.callBack = cllBck; }
    }
}
