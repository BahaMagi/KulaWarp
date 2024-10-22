﻿using UnityEngine;
using System.Collections.Generic;

/**
* Teleportation animation that simultaneously dissolves <dissolveObj> and makes <appearObj> appear in 
* a distance <warpDistance> from <dissolveObj> in the direction specified by the parameter 
* of the Play() method. 
* 
* This requires two shaders attached to the game objects that contain a <_cutoff> shader property
* that controlls the shader effect and goes from 0 to 1, 0 being the effect hasn't started (for both
* appear and dissolve!) and 1 being the effect has finished. 
* 
* For both effects a list of particle systems can be passed. For now, the whole list 
* <particleSystemsD> is played as soon as the effect reaches a percentage threshold <psThresD> 
* of the total effect time <effectTime>. The list <particleSystemsA> is played upon 
* starting the effects. 
*/
//[ExecuteInEditMode]
public class WarpAnimation : MonoBehaviour
{
    public GameObject dissolveObj;
    public GameObject appearObj;
    [Range(0.0f, 1.0f)]
    public float cutoffD    = 0.0f, cutoffA  = 0.0f;
    public float effectTime = 1.0f, psThresD = 0.9f;
    public AnimationCurve animCurveD, animCurveA;
    public List<ParticleSystem> particleSystemsD, particleSystemsA;
    public bool rootMotion = false;

    private Vector3     m_upD;
    private Renderer    m_rendererD,    m_rendererA;
    private List<int>   m_psIDsD,       m_psIDsA;
    private AudioSource m_audioSourceD, m_audioSourceA;
    private float       m_timer     = 0.0f;
    private bool        m_psDPlayed = false;
    private int         m_shaderProperty;
    private bool        m_playing   = false, m_playingD = false, m_playingA = false, m_reset = true;

    // Base Classes MonoBehaviour:
    void Awake()
    {
        // Load references to game objects and components
        LoadComponents();

        // Detach the appearing sphere from its parent to allow proper animation.
        appearObj.transform.parent = null;

        // Pool particle systems with 3 initiated instances. At least 2 are necesarry to 
        // be able to execute conescutive warps. 
        m_psIDsD = new List<int>(particleSystemsD.Capacity);
        foreach (ParticleSystem ps in particleSystemsD)
            m_psIDsD.Add(ParticleSystemsController.psc.AddPrefabtoPool(ps, 3));

        m_psIDsA = new List<int>(particleSystemsA.Capacity);
        foreach (ParticleSystem ps in particleSystemsA)
            m_psIDsA.Add(ParticleSystemsController.psc.AddPrefabtoPool(ps, 3));
    }

    void Update()
    {
        if (m_playing && m_timer <= effectTime)
        {
            m_timer += Time.deltaTime;

            if(m_playingD)  Dissolve();
            if(m_playingA)  Appear();
        }
        else if (m_playing && m_timer > effectTime)
        {
            if(m_reset) ResetAnim();
            else
            {
                m_timer = 0.0f;
                m_playing = false; m_psDPlayed = false;
                m_playingA = false; m_playingD = false;
            }
        }

        m_rendererD.material.SetFloat(m_shaderProperty, cutoffD);
        m_rendererA.material.SetFloat(m_shaderProperty, cutoffA);
    }

    // WarpAnimation:

    void LoadComponents()
    {
        // Get ID to the render property _cutoff which is used to animate the material
        m_shaderProperty = Shader.PropertyToID("_cutoff");

        // Get renderer components
        m_rendererD      = dissolveObj.GetComponent<Renderer>();
        m_rendererA      = appearObj.GetComponent<Renderer>();

        // Get AudioSource components
        m_audioSourceA = appearObj.GetComponent<AudioSource>();
        m_audioSourceD = dissolveObj.GetComponent<AudioSource>();

        // This is a workaround to make [ExecuteInEditMode] work with setting 
        // material properties. A temporary material copy is created and assiged. 
        Material tempMaterialD = new Material(m_rendererD.sharedMaterial);
        Material tempMaterialA = new Material(m_rendererA.sharedMaterial);
        m_rendererD.sharedMaterial = tempMaterialD;
        m_rendererA.sharedMaterial = tempMaterialA;
    }

    /**
     * Plays both the disappear and the appear animation at the same time.
     * If root motion is selected the dissolve object will be placed at the target 
     * position at the end of the animation.
     */
    public void Play(Vector3 target, Vector3 upD, Vector3 upA, bool reset = true)
    {
        m_playing = true; m_playingD = true; m_playingA = true; m_reset = reset;

        // Play sound effects:
        m_audioSourceA.Play(); m_audioSourceD.Play();

        // The up vectors are needed for the particle effects.
        m_upD = upD;

        // Start particle animations for appearing. Dissolve effects are 
        // started shortly before the animation is over. 
        foreach (int ID in m_psIDsA)
            ParticleSystemsController.psc.PlayPS(ID, target, upA);

        // Move the appearing object to the target location and adjust the 
        // orientation to match the dissolving object. 
        appearObj.transform.position = target;
        appearObj.transform.rotation = dissolveObj.transform.rotation;
    }

    /**
     * Plays only the disappear animation. Root motion has no effect on this. 
     */
    public void PlayD(Vector3 up, bool reset = false)
    {
        m_playing  = true; m_playingD = true; m_reset    = reset;

        // The up vectors are needed for the particle effects.
        m_upD = up;

        // Play sound effects:
        m_audioSourceD.Play();
    }

    /**
     * Plays only the appear animation at the @target position. 
     * Root motion has no effect on this. If the disappear object is supposed to end up
     * at the @target position, @ResetAnim() should be used when the animation has finished.
     */
    public void PlayA(Vector3 target, Vector3 up, bool reset = true)
    {
        m_playing = true; m_playingA = true; m_reset = reset;

        // Play sound effects:
        m_audioSourceA.Play();


        // Start particle animations for appearing
        foreach (int ID in m_psIDsA)
            ParticleSystemsController.psc.PlayPS(ID, target, up);

        // Move the appearing object to the target location and adjust the 
        // orientation to match the dissolving object. 
        appearObj.transform.position = target;
        appearObj.transform.rotation = dissolveObj.transform.rotation;
    }

    public void ResetAnim()
    {
        m_timer    = 0.0f;
        m_playing  = false; m_psDPlayed = false;
        m_playingA = false; m_playingD  = false;
        cutoffA    = 0.0f;  cutoffD     = 0.0f;

        if (rootMotion) dissolveObj.transform.parent.transform.position = appearObj.transform.position;

        m_rendererD.material.SetFloat(m_shaderProperty, cutoffD);
        m_rendererA.material.SetFloat(m_shaderProperty, cutoffA);
    }

    public bool isPlaying()
    { return m_playing; }

    void Dissolve()
    {
        if (m_timer >= (effectTime * psThresD) && !m_psDPlayed)
        {
            foreach (int ID in m_psIDsD)
                ParticleSystemsController.psc.PlayPS(ID, dissolveObj.transform.position, m_upD);

            m_psDPlayed = true;
        }

        cutoffD = animCurveD.Evaluate(Mathf.InverseLerp(0, effectTime, m_timer));
    }

    void Appear()
    {
        cutoffA = animCurveA.Evaluate(Mathf.InverseLerp(0, effectTime, m_timer));
    }
}
