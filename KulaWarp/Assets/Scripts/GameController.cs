using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject player;
    public GameObject mainCamera;
    public GameObject exit;

    private CameraController m_cc;
    private PlayerController m_pc;
    private ExitController   m_ec;

    private int   m_score, m_totalScore, m_keyCount;
    private float m_curTime;

    public Vector3 startPosPlayer, startUp, startDir;
    public int targetCrystalCount = 1;
    public float timeLimit    = 120; // in Seconds

    private Stack<GameObject> m_deactivatedPickUps;

    void Start()
    {
        m_keyCount   = 0;
        m_score      = 0;
        m_totalScore = 0; // @TODO this has to be saved/loaded. 
        m_curTime    = 0;

        m_cc = mainCamera.GetComponent<CameraController>();
        m_pc = player.GetComponent<PlayerController>();
        m_ec = exit.GetComponent<ExitController>();

        Physics.gravity = -9.81f * startUp;

        m_deactivatedPickUps = new Stack<GameObject>();
    }

    void Update()
    {
        if (m_totalScore < 0) GameOver();
        if (m_curTime > timeLimit) RestartLevel();

        m_curTime += Time.deltaTime;
    }

    public void RestartLevel()
    {
        Physics.gravity = -9.81f * startUp;
        m_cc.ResetCamera();
        m_pc.ResetPlayer();

        m_curTime = 0;

        int c = m_deactivatedPickUps.Count;
        while (c-- > 0) m_deactivatedPickUps.Pop().SetActive(true);
    }

    public void GameOver()
    {

    }

    public void WinLevel()
    {
        player.SetActive(false);
        m_cc.isMoving = true;
        mainCamera.transform.position = new Vector3(1.5f, 6.5f, -6);
        mainCamera.transform.rotation = Quaternion.Euler(54, 0, 0);
    }

    public void Score(int points, GameObject o)
    {
        m_score += points;
        m_deactivatedPickUps.Push(o);
    }

    public void CollectCrystal(GameObject o)
    {
        m_keyCount++;
        m_deactivatedPickUps.Push(o);

        if (m_keyCount >= targetCrystalCount)
        {
            m_ec.Activate(true);
            exit.GetComponent<Renderer>().material.color = Color.green;
        }

        else
        {
            m_ec.Activate(false);
            exit.GetComponent<Renderer>().material.color = Color.red;
        }
    }

    public int getKeyCount()
    {
        return m_keyCount;
    }
}
