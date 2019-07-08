using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


[System.Serializable]
public class SaveData
{
    public int totalPoints          = 0;
    public int curLevel             = 0;
    public List<int> pointsPerLevel = new List<int>();
}

public class GameController : MonoBehaviour
{
    [HideInInspector] public static GameController gc;

    private SaveData m_saveData;
    private bool m_isPaused;

    public string savePath = Application.dataPath + "/save.mem";

    void Awake()
    {
        // Make this a public singelton
        if (gc == null)
        {
            DontDestroyOnLoad(gameObject);
            gc = this;
        }
        else if (gc != this) Destroy(gameObject);

        m_saveData = new SaveData();
    }

    public void SaveGame()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream      stream    = new FileStream(savePath, FileMode.Create);

        formatter.Serialize(stream, m_saveData);

        stream.Close();
    }

    public void LoadGame()
    {
        if (!File.Exists(savePath))
            return;

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream      stream    = new FileStream(savePath, FileMode.Open);

        m_saveData = formatter.Deserialize(stream) as SaveData;

        stream.Close();
    }

    public void HandleInput()
    { }

    public void Win()
    {
        // Stop input affecting the player or the camera.
        PlayerController.pc.Disable();

        // Stop time
        Time.timeScale = 0f;

        // Set a pose for the camera as background for the score screen. 
        CameraController.cc.PauseCamera();

        yield return new WaitForSecondsRealtime(3.0f); // @TODO Score screen in this time and continue on button press

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Lose()
    { }

    public void GameOver()
    { }

    public void Quit()
    { }


//----------------------------------------------------------
    #region
    public GameObject crystalImgBnWPrefab;
    public GameObject player;
    public GameObject mainCamera;
    public GameObject exit;
    public GameObject UI;
    public GameObject pauseMenu;

    [HideInInspector] public CameraController m_cc;
    [HideInInspector] public PlayerController m_pc;
    [HideInInspector] public ExitController   m_ec;
    [HideInInspector] public HUDController    m_uic;

    private int   m_score, m_totalScore, m_crystalCount;
    private float m_curTime;

    public Vector3 startPosPlayer, startUp, startDir;
    public int     targetCrystalCount = 1; // >= 0
    public float   timeLimit          = 120; // in Seconds

    [HideInInspector] public bool isPaused = false; //@TODO quite some of these things could be made static. 

    private Stack<GameObject> m_deactivatedPickUps;
    #endregion

    void Start()
    {
        // Init Game Variables to start the game and level.
        m_crystalCount  = 0;
        m_score         = 0;
        m_totalScore    = 0; // @TODO this has to be saved/loaded. 
        m_curTime       = 0;
        Physics.gravity = -9.81f * startUp;
        Time.timeScale = 1.0f;

        LoadComponents();

        m_deactivatedPickUps = new Stack<GameObject>();

        pauseMenu.SetActive(false);
    }

    void LoadComponents()
    {
        m_cc  = mainCamera.GetComponent<CameraController>();
        m_pc  = player.GetComponent<PlayerController>();
        m_ec  = exit.GetComponent<ExitController>();
        m_uic = UI.GetComponent<HUDController>();
    }

    void Update()
    {
        if (m_totalScore < 0) GameOver();  // @TODO This should be checked only on death (i.e. when points are lost). 
        if (m_curTime > timeLimit) RestartLevel();

        HandleInput();

        m_curTime += Time.deltaTime;
    }

    void HandleInput()
    {
        if (Input.GetButtonDown("Pause") &&  isPaused && !m_cc.isMoving && !m_pc.isMoving) Resume();
        if (Input.GetButtonDown("Pause") && !isPaused && !m_cc.isMoving && !m_pc.isMoving) Pause();
    }

    public void Pause()
    {
        isPaused       = true;
        Time.timeScale = 0.0f;

        m_cc.PauseCamera();
        pauseMenu.SetActive(true);
    }

    public void Resume()
    {
        isPaused       = false;
        Time.timeScale = 1.0f;

        m_cc.ResumeCamera();
        pauseMenu.SetActive(false);
    }

    public void Quit()
    {
        SceneManager.LoadScene(0); // @TODO make the MainMenu index a variable somewhere
    }

    public void PlayerDie()
    {
        // @TODO death animations are lacking here 
        RestartLevel();
    }

    /**
     * When the level is reset, e.g. due to death, reset the camera, player and game variables.
     */
    public void RestartLevel()
    {
        m_curTime       = 0;
        m_crystalCount  = 0;
        m_score         = 0;
        Physics.gravity = -9.81f * startUp;

        m_cc.ResetCamera();
        m_pc.ResetPlayer();

        // Reactivate all the pickups which were already collected.
        int c = m_deactivatedPickUps.Count;
        while (c-- > 0) m_deactivatedPickUps.Pop().SetActive(true);
    }

    public void GameOver()
    {
        // Back to main menue without saving. Arcade Mode can be continued from the last "big save". 
        // Auto-Save after each level, but "big saves" after each 10 levels or so. 
        // Having played a level in Arcade Mode unlocks it for single level game mode. 
        SceneManager.LoadScene(0);
    }

    public IEnumerator WinLevel() // @TODO load next level. 
    { 
        // Stop input affecting the player or the camera.
        player.SetActive(false);
        m_cc.isMoving = true;

        // Stop time
        Time.timeScale = 0f;

        // Set a pose for the camera as background for the score screen. 
        m_cc.PauseCamera();

        yield return new WaitForSecondsRealtime(3.0f); // @TODO Score screen in this time and continue on button press

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Score(int points, GameObject o)
    {
        m_score += points;
        m_uic.Score(m_score);
        m_deactivatedPickUps.Push(o);
    }

    /**
     * Increase crystal count. The player can have more crystals than @targetCrystalCount.
     * If more are collected an extra symbol is added to show that the collected key was "extra".
     * They are used to activate additional teleporters whithin the level.
     */
    public void CollectCrystal(GameObject o)
    {
        m_crystalCount++;
        m_deactivatedPickUps.Push(o);

        m_uic.ColorCrystal();

        if (m_crystalCount >= targetCrystalCount) m_ec.Activate(true);
        else                                      m_ec.Activate(false);
    }

    public int getCrystalCount()
    {
        return m_crystalCount;
    }
}
