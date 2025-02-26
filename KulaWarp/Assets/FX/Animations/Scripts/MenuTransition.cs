﻿using UnityEngine;
using System;

public class MenuTransition : MonoBehaviour
{
    public MenuShrink ShrinkAnim;
    public MenuFlyIn  FlyInAnim;

    private MenuAnimation[] Anims;

    private void Awake()
    {
        // Initiate menu tansitions
        Anims = new MenuAnimation[2];

        Anims[0]   = ShrinkAnim;
        Anims[1]   = FlyInAnim;
    }

    void Update()
    {
        foreach(MenuAnimation a in Anims)
        { a.OnUpdate(); }
    }

    public bool isPlaying()
    {
        bool ret = false;
        foreach (MenuAnimation anim in Anims)
            ret = ret || anim.isPlaying();

        return ret;
    }

    public void Shrink(MenuCube cube, int sideBarPos)
    { ShrinkAnim.Play(cube, sideBarPos); }

    public void Unshrink(MenuCube cube)
    { ShrinkAnim.Reverse(cube); }

    public void FlyIn(MenuCube cube)
    { FlyInAnim.Play(cube); }

    public void FlyOut(MenuCube cube)
    { FlyInAnim.Reverse(cube); }

    [Serializable]
    public class MenuShrink : MenuAnimation
    {
        public AnimationCurve animCurvePos, animCurveScale;

        public Vector3 sideBarPos      = new Vector3(-1.25f, 0.5f, -0.25f);
        public Vector3 shrinkScale     = new Vector3( 0.5f, 0.5f,  0.5f);
        public Vector3 largeScale      = new Vector3( 1.0f,  1.0f,  1.0f);
        public float   spacing         = 0.0f;
        public float   sideBarRotation = 15.0f;

        private Vector3 m_startPos,   m_targetPos;
        private Vector3 m_startScale, m_targetScale;
        private int     m_sideBarIndex = 0;
        private float   m_tickAngle = 0.0f;
        private float   m_baseRot = 0.0f, m_targetRot = 0.0f;

        public override void Play(MenuCube cube)
        {
            m_cube        = cube;
            m_startPos    = cube.transform.localPosition;
            m_targetPos   = sideBarPos + m_sideBarIndex * Vector3.up * (shrinkScale.y + spacing); // + spot offset + margin
            m_startScale  = cube.transform.localScale;
            m_targetScale = shrinkScale;
            m_baseRot     = cube.transform.localRotation.eulerAngles.y;
            m_targetRot   = sideBarRotation;

            m_timer     = 0;
            m_isPlaying = true;
        }

        public void Play(MenuCube cube, int sideBarPos)
        {
            m_sideBarIndex = sideBarPos;
            Play(cube);
        }

        protected override void EvalCurves()
        {
            m_cube.transform.localPosition    = Vector3.Lerp(m_startPos,   m_targetPos,   animCurvePos.Evaluate  (Mathf.InverseLerp(0, duration/2, m_timer)));
            m_cube.transform.localScale       = Vector3.Lerp(m_startScale, m_targetScale, animCurveScale.Evaluate(Mathf.InverseLerp(0, duration/2, m_timer)));
            m_cube.transform.localEulerAngles = Vector3.Lerp(new Vector3(0, m_baseRot, 0), new Vector3(0, m_targetRot, 0), animCurveScale.Evaluate(Mathf.InverseLerp(0, duration/2, m_timer)));
        }

        protected override void EndAnim()
        {
            m_sideBarIndex = 0;

            m_cube.transform.localPosition = m_targetPos;
        }

        public void Reverse(MenuCube cube)
        {
            m_cube        = cube;
            m_startPos    = cube.transform.localPosition;
            m_targetPos   = Vector3.zero;
            m_startScale  = cube.transform.localScale;
            m_targetScale = largeScale;
            m_tickAngle   = (-1) * sideBarRotation / duration;
            m_baseRot     = cube.transform.localRotation.eulerAngles.y;
            m_targetRot   = 0.0f;

            m_timer     = 0;
            m_isPlaying = true;
        }
    }

    [Serializable]
    public class MenuFlyIn : MenuAnimation
    {
        public AnimationCurve animCurve;

        private Vector3 m_startPos, m_targetPos;
        
        public override void Play(MenuCube cube)
        {
            m_cube      = cube;
            m_startPos  = cube.transform.position;
            m_targetPos = cube.transform.parent.transform.position;
            m_timer     = 0;
            m_isPlaying = true;
        }

        public void Reverse(MenuCube cube)
        {
            m_cube      = cube;
            m_startPos  = cube.transform.position;
            m_targetPos = cube.transform.TransformPoint(Vector3.right * 10);
            m_timer     = 0;
            m_isPlaying = true;
        }

        protected override void EvalCurves()
        {
            m_cube.transform.position = Vector3.Lerp(m_startPos, m_targetPos, animCurve.Evaluate(Mathf.InverseLerp(0, duration, m_timer)));
        }

        protected override void EndAnim()
        {
            m_cube.transform.position = m_targetPos;
        }
    }
}

[Serializable]
public abstract class MenuAnimation
{
    public float     duration = 0.5f;

    protected bool   m_isPlaying = false;
    protected float  m_timer     = 0.0f;
    
    protected MenuCube m_cube;

    public bool isPlaying()
    { return m_isPlaying; }

    public    abstract void Play(MenuCube cube);
    protected abstract void EvalCurves();
    protected abstract void EndAnim();

    public void OnUpdate()
    {
        if (m_isPlaying)
        {
            m_timer += Time.deltaTime;

            EvalCurves();

            if (m_timer >= duration)
            {
                EndAnim();
                m_timer = 0.0f;
                m_isPlaying = false;
            }
        }
    }
}

