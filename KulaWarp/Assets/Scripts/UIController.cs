using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public GameObject crystalImgBnWPrefab;
    public Sprite crystalImgColor;

    public GameObject game;
    private GameController m_gc;

    private List<GameObject> m_crystals;

    // Start is called before the first frame update
    void Awake()
    {
        m_crystals = new List<GameObject>();
        m_gc = game.GetComponent<GameController>();

        for (int i = 0; i < m_gc.targetCrystalCount; i++)
        {
            // Instantiate at position (0, 0, 0) and zero rotation.
            m_crystals.Add(Instantiate(crystalImgBnWPrefab, Vector3.zero, Quaternion.identity));
            m_crystals[i].transform.SetParent(transform.GetChild(0).transform, false);
        }

    }

    public void ColorCrystal()
    {
        int keycount = m_gc.getKeyCount();

        m_crystals[keycount - 1].GetComponent<Image>().sprite = crystalImgColor;
    }
}
