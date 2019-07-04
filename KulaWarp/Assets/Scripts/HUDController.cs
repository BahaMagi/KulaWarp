using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    #region
    public GameObject      game;
    private GameController m_gc;

    public GameObject crystalImgPrefab; // The prefab that holds the UI.Image component of the black and white crystal
    public Sprite     crystalImgColor; // The sprite (= image file) of the colored crystal. 
    public Sprite     crystalImgBnW; // The sprite (= image file) of the black and white crystal. 

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
        m_gc = game.GetComponent<GameController>();
    }

    public void ColorCrystal() // @TODO make this work for spending keys as well. 
    {
        m_crystals[m_gc.getKeyCount() - 1].GetComponent<Image>().sprite = crystalImgColor;
    }
}
