using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : ObjectBase
{
    [HideInInspector] public static UIController uic;

    public GameObject crystalImgPrefab; // The prefab that holds the UI.Image component of the black and white crystal
    public Sprite     crystalImgColor; // The sprite (= image file) of the colored crystal. 
    public Sprite     crystalImgBnW; // The sprite (= image file) of the black and white crystal. 
    public GameObject scoreTMP, timeTMP;

    private TextMeshProUGUI  m_scoreText, m_timeText;
    private List<GameObject> m_crystals;

    void Awake()
    {
        if (uic == null) uic = this;
        else if (uic != this) Destroy(gameObject);

        LoadComponents();

        LevelController.lc.Register(this);

        // Instantiate Crystal sprites at the bottom left corner of the screen
        m_crystals = new List<GameObject>();
        for (int i = 0; i < LevelController.lc.targetCryCount; i++)
        {
            m_crystals.Add(Instantiate(crystalImgPrefab, Vector3.zero, Quaternion.identity));
            m_crystals[i].transform.SetParent(transform.GetChild(0).transform, false);
        }
    }

    void LoadComponents()
    {
        m_scoreText = scoreTMP.GetComponent<TextMeshProUGUI>();
        m_timeText  = timeTMP.GetComponent<TextMeshProUGUI>();
    }

    public void DisplayTime(float time, float timelimit)
    {
        m_timeText.text = (int)((timelimit - time) / 60) + ":" + (int)(timelimit - time) % 60;
    }

    public void ColorCrystal(int count, bool color = true)
    {
        if (count > m_crystals.Count) return;

        m_crystals[count - 1].GetComponent<Image>().sprite = color ? crystalImgColor : crystalImgBnW;
    }

    public void Score(int score)
    {
        m_scoreText.text = "Score: " + score.ToString("000000");
    }

    public override void Reset()
    {
        Score(0);

        for (int i = 0; i < LevelController.lc.targetCryCount; i++)
            ColorCrystal(i, false);
    }
}
