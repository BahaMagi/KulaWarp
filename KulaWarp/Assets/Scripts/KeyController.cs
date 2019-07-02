using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyController : MonoBehaviour
{
    public GameObject      gameController;
    private GameController m_gc;

    public float rotAngle = 45.0f;


    void Start()
    {
        transform.Rotate(new Vector3(0, Random.Range(0, 180), 0));

        m_gc = gameController.GetComponent<GameController>();
    }

    void Update()
    {
        AnimObject();
    }

    void AnimObject()
    {
        transform.Rotate(new Vector3(0, rotAngle, 0) * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        gameObject.SetActive(false);
        m_gc.CollectKey(gameObject);
    }
}
