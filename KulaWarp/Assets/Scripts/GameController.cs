using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class SaveData
{
    public int totalPoints = 0;
    public int curLevel = 0;
    public List<int> pointsPerLevel = new List<int>();
}

public class GameController : MonoBehaviour
{
    [HideInInspector] public static GameController gc;

    private SaveData m_saveData;
    private bool     m_isPaused;
    private string   m_savePath;

    public string     saveFileName;
    public GameObject pauseMenu;

#region Monobehavior
    void Awake()
    {
        // Make this a public singelton
        if (gc == null)
        {
            DontDestroyOnLoad(gameObject);
            gc = this;
        }
        else if (gc != this) Destroy(gameObject);

        m_isPaused = false;
        pauseMenu.SetActive(false);

        m_saveData = new SaveData();
        if(saveFileName.Length == 0) m_savePath = Application.dataPath + "/save.mem";
        else m_savePath = Application.dataPath + saveFileName;
    }

    void Update()
    {
        HandleInput();
    }
#endregion Monobehavior

    public  IEnumerator GameOver()
    {
        // @TODO there has to be a "Game Over screen" shown here

        while (!Input.GetButtonDown("Warp")) yield return null;

        // Return to the main menu
        SceneManager.LoadScene(0);
    }

    private void HandleInput()
    {
        if (Input.GetButtonDown("Pause") && m_isPaused) Resume();
        else if (Input.GetButtonDown("Pause") && !m_isPaused && 
           !CameraController.cc.isMoving && 
           !PlayerController.pc.isMoving) Pause();
    }

    public  bool IsPaused()
    {
        return m_isPaused;
    }

    public  void LoadGame()
    {
        if (!File.Exists(m_savePath))
            return;

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream      stream    = new FileStream(m_savePath, FileMode.Open);

        m_saveData = formatter.Deserialize(stream) as SaveData;

        stream.Close();
    }

    public  IEnumerator Lose()
    {
        // Stop input affecting the player or the camera.
        Pause(false);
        PlayerController.pc.Disable();

        // Stop time
        Time.timeScale = 0f;

        // Set a pose for the camera as background for the score screen. 
        CameraController.cc.PauseCamera();

        // Apply the points penatly and check for game over
        m_saveData.totalPoints -= LevelController.lc.GetPoints();
        if (m_saveData.totalPoints < 0) { StartCoroutine(GameOver()); yield break; }

        // @TODO there has to be a "Died screen" shown here

        while (!Input.GetButtonDown("Warp")) yield return null;

        Resume();

        // Restart the level
        LevelController.lc.Restart();
    }

    public  void Pause(bool showMenu = true)
    {
        m_isPaused     = true;
        Time.timeScale = 0.0f;

        CameraController.cc.PauseCamera();
        if(showMenu) pauseMenu.SetActive(true);
    }

    public  void Quit()
    {
        // Go back to the main menu
        SceneManager.LoadScene(0);
    }

    public  void Resume()
    {
        m_isPaused     = false;
        Time.timeScale = 1.0f;

        CameraController.cc.ResumeCamera();
        pauseMenu.SetActive(false);
    }

    public  void SaveGame()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(m_savePath, FileMode.Create);

        formatter.Serialize(stream, m_saveData);

        stream.Close();
    }

    public  IEnumerator Win()
    {
        // Stop input affecting the player or the camera.
        Pause(false);
        PlayerController.pc.Disable();

        // Stop time
        Time.timeScale = 0f;

        // Set a pose for the camera as background for the score screen. 
        CameraController.cc.PauseCamera();

        // @TODO there has to be a "Win screen" shown here

        while (!Input.GetButtonDown("Warp")) yield return null;

        // Save stats
        m_saveData.pointsPerLevel.Add(LevelController.lc.GetPoints());
        m_saveData.totalPoints += LevelController.lc.GetPoints();
        m_saveData.curLevel++;
        SaveGame();

        Resume();

        // Load the next level if there is one. If there is not, return to the main menu.
        if (SceneManager.GetActiveScene().buildIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        else
            SceneManager.LoadScene(0);
    }
}
