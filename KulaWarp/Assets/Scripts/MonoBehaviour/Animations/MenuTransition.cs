using UnityEngine;
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

        public Vector3 sideBarPos  = new Vector3(-1.25f, 0.5f, -0.25f);
        public Vector3 shrinkScale = new Vector3( 0.75f, 0.5f,  0.5f);
        public Vector3 largeScale  = new Vector3( 1.5f,  1.0f,  1.0f);
        public float   spacing     = 0.0f;

        private Vector3 m_startPos,   m_targetPos;
        private Vector3 m_startScale, m_targetScale;
        private int     m_sideBarPos = 0;

        public override void Play(MenuCube cube)
        {
            m_cube        = cube;
            m_startPos    = cube.transform.position;
            m_targetPos   = sideBarPos + m_sideBarPos * Vector3.down * (shrinkScale.y + spacing); // + spot offset + margin
            m_startScale  = cube.transform.localScale;
            m_targetScale = shrinkScale;

            m_timer     = 0;
            m_isPlaying = true;
        }

        public void Play(MenuCube cube, int sideBarPos)
        {
            m_sideBarPos = sideBarPos;
            Play(cube);
        }

        protected override void EvalCurves()
        {
                m_cube.transform.position   = Vector3.Lerp(m_startPos,   m_targetPos,   animCurvePos.Evaluate  (Mathf.InverseLerp(0, duration/2, m_timer)));
                m_cube.transform.localScale = Vector3.Lerp(m_startScale, m_targetScale, animCurveScale.Evaluate(Mathf.InverseLerp(0, duration/2, m_timer)));
        }

        protected override void EndAnim()
        {
            m_cube.transform.position = m_targetPos;
            m_sideBarPos = 0;
        }

        public void Reverse(MenuCube cube)
        {
            m_cube        = cube;
            m_startPos    = cube.transform.position;
            m_targetPos   = Vector3.zero;
            m_startScale  = cube.transform.localScale;
            m_targetScale = largeScale;

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
            m_targetPos = Vector3.zero;
            m_timer     = 0;
            m_isPlaying = true;
        }

        public void Reverse(MenuCube cube)
        {
            m_cube      = cube;
            m_startPos  = cube.transform.position;
            m_targetPos = cube.transform.position + Vector3.right * 10;
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

