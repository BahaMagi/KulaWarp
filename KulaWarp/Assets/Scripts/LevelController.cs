using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class LevelController : MonoBehaviour
{
    [HideInInspector] public static LevelController lc;

    private int              m_points  = 0, m_curCrys = 0;
    private float            m_curTime = 0.0f;
    private List<ObjectBase> m_objList;

    public float           gravity = 9.81f, timeLimit = 120.0f, boxSize = 1.0f;
    public int             targetCryCount = 1;
    public Vector3         startPos, startUp, startDir;
    public GameObject exit;

    #region MonoBehaviour
    void Awake()
    {
        // Make this a public singelton
        if (lc == null) lc = this;
        else if (lc != this) Destroy(gameObject);

        exit = GameObject.Find("Exit");
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
        if (m_curCrys >= targetCryCount) exit.GetComponent<Renderer>().material.color = Color.green;
        else exit.GetComponent<Renderer>().material.color = Color.red;
    }

    public void CollectCrystal()
    {
        m_curCrys++;

        UIController.uic.ColorCrystal(m_curCrys);

        ActivateExit();
    }

    public int  GetPoints()
    {
        return m_points;
    }

    public void OnExitEnter()
    {// Invoked by the TriggerBase script attached to the exit. 
        if (m_curCrys >= targetCryCount) StartCoroutine(GameController.gc.Win());
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
