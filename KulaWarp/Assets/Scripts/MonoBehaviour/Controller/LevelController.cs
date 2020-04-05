using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/**
* Handles all level related settings, e.g. energy collection progress and current score. 
* Upon start/re-start the level is reset by its LevelController. Start, pause and end of a 
* level is controlled by the GameController. 
* The inspector values of this script have to be adjusted for each level. 
*/
public class LevelController : MonoBehaviour
{
    [HideInInspector] public static LevelController lc;

    // Level progress
    private int              m_points  = 0, m_curEnergy = 0;
    private float            m_curTime = 0.0f;

    private List<ObjectBase> m_objList; // List of game objects that is used to reset the level

    // Physics and level settings
    public float           gravity = 9.81f, timeLimit = 120.0f, boxSize = 1.0f;
    public int             targetEnergyCount = 1;
    public Vector3         startPos, startUp, startDir;
    public GameObject      exit;

    // Camera Settings
    public AnimationClip cameraIntroAnimation;
    public Vector3       pauseCamPos    = new Vector3(1.5f, 6.5f, -6.0f);
    public Vector3       pauseCamLookAt = new Vector3(0.0f, 0.0f, 0.0f);

    // Base Classes MonoBehaviour:

    void Awake()
    {
        // Make this a public singelton
        if (lc == null) lc = this;
        else if (lc != this) Destroy(gameObject);

        m_objList = new List<ObjectBase>();

        Physics.gravity = -gravity * startUp;
    }

    void Update()
    {
        if (m_curTime > timeLimit) Restart(); // @TODO Show a TimeOutScreen before restarting the level

        m_curTime += Time.deltaTime;

        // HUD related:
        UIController.uic.DisplayTime(m_curTime, timeLimit);
    }

    // LevelController: 

    void ActivateExit()
    {
        if (m_curEnergy >= targetEnergyCount) exit.GetComponent<Renderer>().material.color = Color.green;
        else exit.GetComponent<Renderer>().material.color = Color.red;
    }

    public void CollectEnergy()
    {
        m_curEnergy++;

        UIController.uic.ColorEnergy(m_curEnergy);

        ActivateExit();
    }

    public int  GetPoints()
    {
        return m_points;
    }

    public void OnExitEnter()
    {// Invoked by the TriggerBase script attached to the exit. 
        if (m_curEnergy >= targetEnergyCount) GameController.gc.Win();
    }

    public void Register(ObjectBase o)
    {
        // Called by ObjectBase objects to register with the LevelController. The m_objList is used to reset all 
        // registered objects when the level is restarted. 
        m_objList.Add(o);
    }

    public void Restart()
    {
        m_curTime = 0; m_curEnergy = 0; m_points  = 0;
        Physics.gravity = -gravity * startUp;

        ActivateExit();

        // Reset all objects that have registerd with the level, 
        // e.g. player, camera, pickups, energy, ... 
        foreach (ObjectBase o in m_objList) o.Reset();

        GameController.gc.Resume();
    }

    /**
     * Accumulates the values of pickups according to their 
     * currency. 
     */ 
    public void Score(int value, Currency currency)
    {
        switch(currency)
        {
            case Currency.Points:
                m_points += value;
                // Display the new score
                UIController.uic.Score(m_points);
                break;
            case Currency.Energy:
                m_curEnergy++;
                // Adjust Energy HUD
                UIController.uic.ColorEnergy(m_curEnergy);
                ActivateExit();
                break;
            case Currency.Secret:
                // TODO: Add secrets 
                break;
            default:
                break;
        }
    }
}
