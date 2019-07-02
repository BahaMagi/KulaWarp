using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpController : MonoBehaviour
{
    public  GameObject     gameController; 
    private GameController m_gc;

    public float rotAngle = 45.0f;
    public int value = 100;
    [HideInInspector] public Vector3 up;

    private float m_time = 0.0f;

    void Start()
    {
        up     = Vector3.up;
        m_time = Random.Range(0, 6);
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
        transform.position = transform.position + up * Mathf.Sin(2.0f*m_time) *0.001f;
        m_time = m_time + Time.deltaTime;
        m_time = m_time <= 2 * Mathf.PI ? m_time : 0;
    }

    void OnTriggerEnter(Collider other)
    {
        gameObject.SetActive(false);
        m_gc.Score(value, gameObject);
    }
}
