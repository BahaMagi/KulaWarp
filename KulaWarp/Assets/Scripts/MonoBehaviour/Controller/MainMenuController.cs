using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/**
* Handles MainMenu events. 
*/
public class MainMenuController : MonoBehaviour
{
    public MenuCube   menuCube;
    public GameObject selector;

    private MenuTransition  m_trans;
    private Stack<MenuCube> m_menuStack;
    private GameObject      m_curPanel;

    private WarpAnimation m_selectorWarpAnim;
    private HoverAnim     m_selectorHoverAnim;
    private FadeInAnim    m_panelFadeInAnim;

    // The 2nd boolean is necessary as the onClick event is resolved before
    // Update() but the submit button is still considered clicked in Update().
    // Just setting flags in the callback and resolving them in Update prevents 
    // 'double clicking'. 
    private bool m_panelIsOpen  = false, m_closePanel   = false;
    private bool m_waitToStart  = false;


    // Base Class MonoBehaviour:

    private void Awake()
    {
        // Initialize menu stack
        m_menuStack = new Stack<MenuCube>();

        // Load references to game objects and components
        LoadComponents();
    }

    private void Update()
    {
        // Only allow input if no animation is currently playing. 
        // Otherwise opening and closing panels repeatedly can lead to
        // bugs like the sphere falling off. 
        if (!IsAnimPlaying())
        {
            // The flag is set if the game is supposed to start but the menu is waiting
            // for animations to finish.
            if (m_waitToStart)
                Play();

            if (!m_panelIsOpen)
            {
                if (Input.GetButtonDown("Submit"))
                    menuCube.Confirm();
                else if (Input.GetButtonDown("Cancel"))
                    GoBack();
                else if (Input.GetButtonDown("Back"))
                    GoBack();
                else if (Input.GetAxisRaw("Vertical") == 1)
                    menuCube.ChangeSelectedEntry(1);
                else if (Input.GetAxisRaw("Vertical") == -1)
                    menuCube.ChangeSelectedEntry(-1);
            }
            else if (m_closePanel)
            {// Separate flags are used to avoid double registration of input due to events
                m_closePanel  = false;
                m_panelIsOpen = false;
            }
            else if (m_panelIsOpen)
            {
                if (Input.GetButtonDown("Cancel") || Input.GetButtonDown("Back"))
                    ClosePanel(m_curPanel);
            }
        }
    }

    // MainMenuController: 

    private void LoadComponents()
    {
        m_trans = GetComponent<MenuTransition>();

        // Find animation components
        m_selectorWarpAnim  = selector.GetComponent<WarpAnimation>();
        m_selectorHoverAnim = selector.GetComponent<HoverAnim>();
        m_panelFadeInAnim   = GameObject.Find("BGPanel_Fade").GetComponent<FadeInAnim>();
    }

    private bool IsAnimPlaying()
    {
        bool isStillPlaying = m_selectorHoverAnim.isPlaying()
                || m_selectorWarpAnim.isPlaying()
                || m_panelFadeInAnim.isPlaying()
                || m_trans.isPlaying();

        return isStillPlaying;
    }

    public void StartGame()
    {
        // Make selector sphere disappear
        m_selectorWarpAnim.PlayD(Vector3.up);

        // Fade out the screen
        m_panelFadeInAnim.Play();

        // Play the sound effect to start the game
        m_panelFadeInAnim.gameObject.GetComponent<AudioSource>().Play();

        // Set this to wait until animation is over before starting the game
        m_waitToStart = true;
    }

    private void Play()
    { SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); }

    public void Quit()
    {
        // @TODO Temp solution for testing in the editor
#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying needs to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    public void ChangeMenu()
    {
        MenuCube.MenuEntry curEntry = menuCube.getCurMenuEntry();

        if (curEntry.subMenu != null)
        {
            m_selectorHoverAnim.Play();

            MenuCube subMenuCube = curEntry.subMenu;
            m_trans.Shrink(menuCube, m_menuStack.Count);
            m_trans.FlyIn(subMenuCube);

            m_menuStack.Push(menuCube);

            menuCube = subMenuCube;
        }
        else
            Debug.Log("You shouldn't be here...");
    }

    public void GoBack()
    {
        if (m_menuStack.Count > 0)
        {
            m_selectorHoverAnim.Play();

            MenuCube subMenuCube = m_menuStack.Pop();

            m_trans.Unshrink(subMenuCube);
            m_trans.FlyOut(menuCube);
            menuCube.ChangeSelectedEntry(-1, false);

            menuCube = subMenuCube;
        }
        else
        {
            Quit();
        }
    }

    public void OpenPanel(GameObject panel)
    {
        // Open the panel passed by the current MenuCube
        panel.GetComponent<OpenMenu>().Open();
        m_panelIsOpen = true;
        m_curPanel    = panel;

        // Make selector sphere disappear
        m_selectorWarpAnim.PlayD(Vector3.up);

        // Shrink the current MenuCube
        m_trans.Shrink(menuCube, m_menuStack.Count);
        m_menuStack.Push(menuCube);

        // Avoid selectoring dropping off while cube is shrinking
        m_selectorWarpAnim.dissolveObj.GetComponent<Rigidbody>().useGravity = false;
        m_selectorWarpAnim.appearObj.GetComponent<Rigidbody>().useGravity   = false;
    }

    public void ClosePanel(GameObject panel)
    {
        // If an animation is still playing ignore input
        if (IsAnimPlaying())
            return;

        // Close panel
        panel.GetComponent<OpenMenu>().Close();
        m_closePanel = true;

        // Make selector sphere reappear
        m_selectorWarpAnim.PlayA(new Vector3(0, 0.7f, 0), Vector3.up);

        // Fly MenuCube back in
        menuCube = m_menuStack.Pop();
        m_trans.Unshrink(menuCube);

        // Turn gravity back on for the selector
        m_selectorWarpAnim.dissolveObj.GetComponent<Rigidbody>().useGravity = true;
        m_selectorWarpAnim.appearObj.GetComponent<Rigidbody>().useGravity   = true;

        m_curPanel = null;
    }
}