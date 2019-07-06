using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitController : MonoBehaviour
{
    public  GameObject game;

    private GameController m_gc;

    private bool m_isActivated;

    void Start()
    {
        m_isActivated = false;

        LoadComponents();
    }

    void LoadComponents()
    {
        m_gc = game.GetComponent<GameController>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (m_isActivated) StartCoroutine(m_gc.WinLevel());
    }

    public void Activate(bool activated)
    {
        m_isActivated = activated;
        if (m_isActivated) gameObject.GetComponent<Renderer>().material.color = Color.green;
        else               gameObject.GetComponent<Renderer>().material.color = Color.red;
    }
}
