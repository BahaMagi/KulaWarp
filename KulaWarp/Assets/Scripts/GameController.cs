using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject player;
    public GameObject mainCamera;

    private CameraController m_cc;
    private PlayerController m_pc;

    private int   m_score, m_totalScore, m_keyCount;
    private float m_curTime;

    public Vector3 startPosPlayer, startUp, startDir;
    public int targetKeyCount = 1;
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

        Physics.gravity = -9.81f * startUp;

        m_deactivatedPickUps = new Stack<GameObject>();
    }

    void Update()
    {
        if (m_keyCount == targetKeyCount) WinLevel();
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

    }

    public void Score(int points, GameObject o)
    {
        m_score += points;
        m_deactivatedPickUps.Push(o);
    }

    public void CollectKey(GameObject o)
    {
        m_keyCount++;
        m_deactivatedPickUps.Push(o);
    }

    public int getKeyCount()
    {
        return m_keyCount;
    }
}
