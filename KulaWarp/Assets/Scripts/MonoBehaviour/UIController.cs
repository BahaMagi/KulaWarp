using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UIController : ObjectBase
{
    [HideInInspector] public static UIController uic;

    public GameObject energyImgPrefab; // A prefab that holds the UI.Image component with the energy sprite in red.
    public GameObject energyPanel; // The panel with the layout component that holds the enegery sprites
    public Sprite     energyImgGreen; // The sprite (= image file) of the green energy. 
    public Sprite     energyImgRed; // The sprite (= image file) of the red energy. 
    public GameObject scoreTMP, timeTMP;

    private EventSystem m_es;
    private GameObject  m_pauseMenuContinueBtn, m_pauseMenuQuitBtn, m_pauseMenuRestartBtn, m_pauseMenu;
    private bool        m_continueClicked, m_quitClicked, m_restartClicked;

    private TextMeshProUGUI  m_scoreText, m_timeText;
    private List<GameObject> m_energy;

    void Awake()
    {
        if (uic == null) uic = this;
        else if (uic != this) Destroy(gameObject);

        LoadComponents();

        // Instantiate Energy sprites at the bottom left corner of the screen
        m_energy = new List<GameObject>();
        for (int i = 0; i < LevelController.lc.targetCryCount; i++)
        {
            m_energy.Add(Instantiate(energyImgPrefab, Vector3.zero, Quaternion.identity));
            m_energy[i].transform.SetParent(energyPanel.transform, false);
        }

        // Add Listeners to the button clicks. These are just setting flags such that the actual game logic can 
        // happen in LateUpdate(). Otherwise, GetButtonDown affects more than intended. 
        m_pauseMenuContinueBtn.GetComponent<Button>().onClick.AddListener(delegate { m_continueClicked = true; });
        m_pauseMenuQuitBtn.GetComponent<Button>().onClick.AddListener(delegate { m_quitClicked = true; });
        m_pauseMenuRestartBtn.GetComponent<Button>().onClick.AddListener(delegate { m_restartClicked = true; });

        // Make sure the Continue Button is immediately selected in the pause screen
        m_pauseMenuContinueBtn.GetComponent<Button>().Select();
        m_pauseMenuContinueBtn.GetComponent<Button>().OnSelect(null);

        // Disable the Pause Screen
        m_pauseMenu.SetActive(false);

        LevelController.lc.Register(this);
    }

    void LateUpdate()
    {
        if (m_continueClicked) GameController.gc.Resume();
        if (m_quitClicked)     GameController.gc.Quit();
        if (m_restartClicked)  LevelController.lc.Restart();

        m_continueClicked = false;
        m_quitClicked     = false;
        m_restartClicked  = false;
    }

    public void ColorCrystal(int count, bool color = true)
    {
        if (count > m_energy.Count) return;

        m_energy[count - 1].GetComponent<Image>().sprite = color ? energyImgGreen : energyImgRed;
    }

    public void DisplayTime(float time, float timelimit)
    {
        m_timeText.text = (int)((timelimit - time) / 60) + ":" + ((int)(timelimit - time) % 60).ToString("00");
    }

    void LoadComponents()
    {
        m_scoreText = scoreTMP.GetComponent<TextMeshProUGUI>();
        m_timeText  = timeTMP.GetComponent<TextMeshProUGUI>();

        m_es                   = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        m_pauseMenuContinueBtn = GameObject.Find("Continue Button");
        m_pauseMenuQuitBtn     = GameObject.Find("Quit Button");
        m_pauseMenuRestartBtn  = GameObject.Find("Restart Button");
        m_pauseMenu            = GameObject.Find("PauseMenuCanvas");
    }

    public void PauseScreen(bool show)
    {
        m_pauseMenu.SetActive(show);

        if (show) m_es.SetSelectedGameObject(m_pauseMenuContinueBtn);
        else m_es.SetSelectedGameObject(null);
    }

    public override void Reset()
    {
        Score(0);

        ResetEnergy();
    }

    public void ResetEnergy()
    {
        for (int i = 0; i < m_energy.Count; i++) m_energy[i].GetComponent<Image>().sprite = energyImgRed;
    }

    public void Score(int score)
    {
        m_scoreText.text = score.ToString();
    }
}
