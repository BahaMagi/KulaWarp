﻿using UnityEngine;

public class FX_Appear : MonoBehaviour
{

    public float            spawnEffectTime = 2;
    public float            pause           = 1;
    public AnimationCurve   fadeIn;
    public ParticleSystem[] particleSystems;

    Renderer        m_renderer;
    float           m_timer = 0;
    int             m_shaderProperty;

	void Start ()
    {
        m_shaderProperty = Shader.PropertyToID("_cutoff");
        m_renderer       = GetComponent<Renderer>();
    }
	
	void Update ()
    {
        if (m_timer < spawnEffectTime + pause)
            m_timer += Time.deltaTime;
        else
        {
            foreach (ParticleSystem ps in particleSystems)
                ps.Play();

            m_timer = 0;
        }

        m_renderer.material.SetFloat(m_shaderProperty, fadeIn.Evaluate( Mathf.InverseLerp(0, spawnEffectTime, m_timer)));
    }
}