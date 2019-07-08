using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class LevelController : MonoBehaviour
{
    [HideInInspector] public static LevelController lc;

    private int              m_points  = 0, m_curCrys = 0;
    private float            m_curTime = 0.0f;
    private Vector3          m_worldUp, m_worldDir;
    private List<ObjectBase> m_objList;
    private GameObject       m_exit;

    public float           gravity = 9.81f, timeLimit = 120.0f;
    public int             targetCryCount = 1;
    public Vector3         startPos, startUp, startDir;

    #region MonoBehaviour
    void Awake()
    {
        // Make this a public singelton
        if (lc == null) lc = this;
        else if (lc != this) Destroy(gameObject);

        m_worldUp = startUp; m_worldDir = startDir;

        m_exit = GameObject.Find("Exit");
        m_objList = new List<ObjectBase>();
    }

    void Update()
    {
        if (m_curTime > timeLimit) Restart();

        m_curTime += Time.deltaTime;

        // HUD related:
        UIController.uic.DisplayTime(m_curTime, timeLimit);
    }
    #endregion MonoBehaviour

    void ActivateExit()
    {
        if (m_curCrys >= targetCryCount) m_exit.GetComponent<Renderer>().material.color = Color.green;
        else m_exit.GetComponent<Renderer>().material.color = Color.red;
    }

    public void CollectCrystal()
    {
        m_curCrys++;

        UIController.uic.ColorCrystal(m_curCrys);

        ActivateExit();
    }

    public void Pause()
    { }

    public void Register(ObjectBase o)
    {
        m_objList.Add(o);
    }

    public void Restart()
    {
        m_curTime = 0; m_curCrys = 0; m_points  = 0;
        Physics.gravity = -gravity * startUp;

        m_worldDir = startDir; m_worldUp = startUp;

        // Reset all objects that have registerd with the level, 
        // e.g. player, camera, pickups, crystals, ... 
        foreach(ObjectBase o in m_objList) o.Reset();
    }

    public void Resume()
    { }

    public void Score(int points)
    {
        m_points += points;
        UIController.uic.Score(m_points);
    }
}
