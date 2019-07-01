using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private int   m_score, m_totalScore, m_keyCount;
    private float m_curTime;

    public Vector3 m_startPosPlayer, m_startUp, m_startDir;
    public int targetKeyCount = 1;
    public float timeLimit    = 120;

    void Start()
    {
        m_keyCount   = 0;
        m_score      = 0;
        m_totalScore = 0; // @TODO this has to be saved/loaded. 
    }

    void Update()
    {
        if (m_keyCount == targetKeyCount) WinLevel();
        if (m_totalScore < 0) GameOver();
        if (m_curTime > timeLimit) RestartLevel();
    }

    public void RestartLevel()
    {

    }

    public void GameOver()
    {

    }

    public void WinLevel()
    {

    }

    public void Score(int points)
    {
        m_score += points;
    }

    public void CollectKey()
    {
        m_keyCount++;
    }

    public int getKeyCount()
    {
        return m_keyCount;
    }
}
