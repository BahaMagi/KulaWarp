using UnityEngine;
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

    int       m_shaderProperty;
    Renderer  m_rendererD, m_rendererA;
    float     m_timer = 0.0f;
    bool      m_psDPlayed = false;
    List<int> m_psIDsD, m_psIDsA;
    bool      m_playing = false;

    // Base Classes MonoBehaviour:
    void Start()
    {
        m_shaderProperty = Shader.PropertyToID("_cutoff");
        m_rendererD      = dissolveObj.GetComponent<Renderer>();
        m_rendererA      = appearObj.GetComponent<Renderer>();

        // This is a workaround to make [ExecuteInEditMode] work with setting 
        // material properties. A temporary material copy is created and assiged. 
        Material tempMaterialD = new Material(m_rendererD.sharedMaterial);
        Material tempMaterialA = new Material(m_rendererA.sharedMaterial);
        m_rendererD.sharedMaterial = tempMaterialD;
        m_rendererA.sharedMaterial = tempMaterialA;

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

            Dissolve();
            Appear();
        }
        else if (m_playing && m_timer > effectTime)
            Reset();

        m_rendererD.material.SetFloat(m_shaderProperty, cutoffD);
        m_rendererA.material.SetFloat(m_shaderProperty, cutoffA);
    }

    // WarpAnimation:
    public void Play(Vector3 target, Vector3 upA)
    {
        m_playing = true;

        foreach (int ID in m_psIDsA)
            ParticleSystemsController.psc.PlayPS(ID, target, upA);

        appearObj.transform.position = target;
        appearObj.transform.rotation = dissolveObj.transform.rotation;
    }

    public void Reset()
    {
        m_timer   = 0.0f;
        m_playing   = false; m_psDPlayed = false;
        cutoffA   = 0.0f;  cutoffD     = 0.0f;

        m_rendererD.material.SetFloat(m_shaderProperty, cutoffD);
        m_rendererA.material.SetFloat(m_shaderProperty, cutoffA);

        if(rootMotion) dissolveObj.transform.position = appearObj.transform.position;
    }

    public bool isPlaying()
    { return m_playing; }

    void Dissolve()
    {
        if (m_timer >= (effectTime * psThresD) && !m_psDPlayed)
        {
            foreach (int ID in m_psIDsD)
                ParticleSystemsController.psc.PlayPS(ID, dissolveObj.transform.position, PlayerController.pc.world_up);

            m_psDPlayed = true;
        }

        cutoffD = animCurveD.Evaluate(Mathf.InverseLerp(0, effectTime, m_timer));
    }

    void Appear()
    {
        cutoffA = animCurveA.Evaluate(Mathf.InverseLerp(0, effectTime, m_timer));
    }

    
}
