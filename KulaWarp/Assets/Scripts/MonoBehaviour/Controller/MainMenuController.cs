using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/**
* Handles MainMenu events. 
*/
public class MainMenuController : MonoBehaviour
{
    public MenuCube menuCube;

    private MenuTransition m_trans;
    private Stack<MenuCube> m_menuStack;

    // Base Class MonoBehaviour:

    void Awake()
    {
        // Initialize menu stack
        m_menuStack = new Stack<MenuCube>();

        // Load references to game objects and components
        LoadComponents();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Submit"))
            menuCube.Confirm();
        else if (Input.GetAxisRaw("Vertical") == 1)
            menuCube.ChangeSelectedEntry(1);
        else if (Input.GetAxisRaw("Vertical") == -1)
            menuCube.ChangeSelectedEntry(-1);
    }

    // MainMenuController: 

    void LoadComponents()
    {
        m_trans = GetComponent<MenuTransition>();
    }

    public void Play()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

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
            MenuCube subMenuCube = curEntry.subMenu;
            m_trans.Shrink(menuCube, m_menuStack.Count);
            m_trans.FlyIn(subMenuCube);

            m_menuStack.Push(menuCube);

            menuCube = subMenuCube;
        }
        else
            Debug.Log("Open different menu here.");
    }

    public void GoBack()
    {
        MenuCube subMenuCube = m_menuStack.Pop();

        m_trans.Unshrink(subMenuCube);
        m_trans.FlyOut(menuCube);
        menuCube.ChangeSelectedEntry(-1);

        menuCube = subMenuCube;
    }
}