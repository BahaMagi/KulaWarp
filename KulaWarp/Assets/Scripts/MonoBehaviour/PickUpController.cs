﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpController : ObjectBase
{
    public float rotSpeed   = 45.0f; // Speed of the rotation in angle / sec 
    public float hoverSpeed = 2.0f, hoverAmount = 0.01f; // Up/down cylces / sec and distance in units from center of the pickup. 
    public int scoreValue   = 100; //Point value of this pickup.
    public Vector3 up       = Vector3.up; // The up vector for a pick up has to be specified as depending on the position this could otherwise be ambiguous. 

    protected float   m_time = 0.0f;
    protected Vector3 m_idlePos;

    protected void Awake()
    {
        // Initialize the animations randomly to have each pickup start the animation at a different point.
        m_idlePos = transform.position;
        m_time    = Random.Range(0, 6);
        transform.Rotate(new Vector3(0, Random.Range(0, 180), 0));

        LevelController.lc.Register(this);
    }

    protected void Update()
    {
        AnimObject();
    }

    protected void AnimObject()
    {
        transform.Rotate(Vector3.up, rotSpeed * Time.deltaTime);
        transform.position = m_idlePos + up * Mathf.Sin(hoverSpeed * m_time) * hoverAmount;

        m_time += Time.deltaTime;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        gameObject.SetActive(false);
        LevelController.lc.Score(scoreValue);
    }

    public override void Reset()
    {
        gameObject.SetActive(true);
    }
}