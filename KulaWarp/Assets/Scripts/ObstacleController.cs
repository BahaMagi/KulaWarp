using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    public    GameObject     gameController;
    protected GameController m_gc;

    protected void Start()
    {
        LoadComponents();
    }

    protected void LoadComponents()
    {
        m_gc = gameController.GetComponent<GameController>();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        PlayerController.pc.Die();
    }
}
