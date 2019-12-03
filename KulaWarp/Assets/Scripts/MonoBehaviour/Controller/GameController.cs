using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.EventSystems;
using UnityEngine.UI;


/**
* Game data that is saved and reloaded to a file. 
*/
[System.Serializable]
public class SaveData
{
    public int totalPoints = 0;
    public int curLevel    = 0;

    public List<int> pointsPerLevel = new List<int>();
}

/**
* The GameController manages the overall game. That includes starting, ending and pausing the levels,
* keeping track of the total score and, thus, the winning and losing condition and saving the game 
* data to restore it later.
*/
public class GameController : MonoBehaviour
{
    [HideInInspector] public static GameController gc;

    public enum GameState { Default, Paused, GameOver, Won, Lost };
    [ReadOnly] public GameState m_gameState = GameState.Default;

    public string saveFileName;

    private SaveData  m_saveData;
    private string    m_savePath;

    // Base Class MonoBehaviour:
    void Awake()
    {
        // Make this a public singelton
        if (gc == null)
        {
            DontDestroyOnLoad(gameObject); // This object is scene persistent
            gc = this;
        }
        else if (gc != this) Destroy(gameObject);

        // Initialize save file path
        m_saveData = new SaveData();
        if(saveFileName.Length == 0) m_savePath = Application.dataPath + "/save.mem";
        else m_savePath = Application.dataPath + saveFileName;

        // Register callback that is called when a new scene has finished loading
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void LateUpdate()
    {
        HandleInput();
    }

    // GameController:

    public void GameOver()
    {
        // Stop input affecting the player or the camera.
        Time.timeScale = 0.0f;

        PlayerController.pc.Enable(false);

        m_gameState = GameState.GameOver;

        // @TODO there has to be a "Game Over screen" shown here
    }

    void HandleInput()
    {
        switch(m_gameState)
        {
            case GameState.Paused:
                if (Input.GetButtonDown("Pause")) Resume();
                break;
            case GameState.GameOver:
                if(Input.GetButtonDown("Submit")) SceneManager.LoadScene(0);
                break;
            case GameState.Won:
                if (Input.GetButtonDown("Submit")) NextLevel();
                break;
            case GameState.Lost:
                if (Input.GetButtonDown("Submit"))
                    { LevelController.lc.Restart(); Resume(); }
                break;
            default:
                if (Input.GetButtonDown("Pause") &&
                    CameraController.cc.state == CameraController.CamState.Default &&
                    PlayerController.pc.state == PlayerController.PlayerState.Idle)
                        Pause();
                break;
        }
    }

    public bool IsPaused()
    {
        return m_gameState == GameState.Paused;
    }

    public bool IsDefault()
    {
        return m_gameState == GameState.Default;
    }

    public void LoadGame()
    {
        if (!File.Exists(m_savePath))
            return;

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream      stream    = new FileStream(m_savePath, FileMode.Open);

        m_saveData = formatter.Deserialize(stream) as SaveData;

        stream.Close();
    }

    public void Lost()
    {
        // Stop input affecting the player or the camera.
        Time.timeScale = 0.0f;

        UIController.uic.PauseScreen(false);
        PlayerController.pc.Enable(false);
        PlayerController.pc.sm.Reset();

        m_gameState = GameState.Lost;

        // Apply the points penatly and check for game over
        m_saveData.totalPoints -= LevelController.lc.GetPoints();
        //if (m_saveData.totalPoints < 0) GameOver(); // DEBUG switched this off for debugging

        // else @TODO there has to be a "Died screen" shown here
    }

    public void NextLevel()
    {
        // Save stats
        m_saveData.pointsPerLevel.Add(LevelController.lc.GetPoints());
        m_saveData.totalPoints += LevelController.lc.GetPoints();
        m_saveData.curLevel++;
        SaveGame();

        // Load the next level if there is one. If there is not, return to the main menu.
        if (SceneManager.GetActiveScene().buildIndex < SceneManager.sceneCountInBuildSettings - 1)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        else
            SceneManager.LoadScene(0);

        
    }

    /**
     * This is called when a a new level finished loading.
     */
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Resume Game
        Resume();
    }


    public void Pause()
    {
        m_gameState    = GameState.Paused;
        Time.timeScale = 0.0f; // Freeze time such that everything that relies on Time.DeltaTime does not continue 

        PlayerController.pc.Enable(false);
        UIController.uic.PauseScreen(true);
    }

    public void Quit()
    {
        Resume();

        // Go back to the main menu
        SceneManager.LoadScene(0);
    }

    public void Resume()
    {
        m_gameState    = GameState.Default;
        Time.timeScale = 1.0f; // Resume time 

        PlayerController.pc.Enable(true);
        UIController.uic.PauseScreen(false);
    }

    public void SaveGame()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream         = new FileStream(m_savePath, FileMode.Create);

        formatter.Serialize(stream, m_saveData);

        stream.Close();
    }

    public void Win()
    {
        // Stop input affecting the player or the camera.
        Time.timeScale = 0.0f;

        PlayerController.pc.Enable(false);

        m_gameState = GameState.Won;
        
        // @TODO there has to be a "Win screen" shown here
    }
}
