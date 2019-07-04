using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    #region
    public GameObject      game;
    private GameController m_gc;

    public GameObject crystalImgPrefab; // The prefab that holds the UI.Image component of the black and white crystal
    public Sprite     crystalImgColor; // The sprite (= image file) of the colored crystal. 
    public Sprite     crystalImgBnW; // The sprite (= image file) of the black and white crystal. 
    public GameObject scoreTMP;

    private TextMeshProUGUI m_text;

    private List<GameObject> m_crystals;
    #endregion

    // Start is called before the first frame update
    void Awake()
    {
        LoadComponents();

        m_crystals = new List<GameObject>();
        for (int i = 0; i < m_gc.targetCrystalCount; i++)
        {
            // Instantiate at position (0, 0, 0) and zero rotation related to its parents transform.
            m_crystals.Add(Instantiate(crystalImgPrefab, Vector3.zero, Quaternion.identity));
            m_crystals[i].transform.SetParent(transform.GetChild(0).transform, false);
        }
    }

    void LoadComponents()
    {
        m_gc   = game.GetComponent<GameController>();
        m_text = scoreTMP.GetComponent<TextMeshProUGUI>();
    }

    public void ColorCrystal() // @TODO make this work for spending crystals as well. 
    {
        m_crystals[m_gc.getCrystalCount() - 1].GetComponent<Image>().sprite = crystalImgColor;
    }

    public void Score(int score)
    {
        m_text.text = "Score: " + score.ToString("000000");
    }
}
