using UnityEngine;

/**
* Extends the base PickUpController to count energy instead of points when picked up.
*/
public class EnergyController : PickUpController
{
    public float scaleAmount = 0.2f, scaleSpeed = 4.0f;

    private Vector3 m_idleScale;

    new void Awake()
    {
        base.Awake();

        m_idleScale = transform.localScale;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        gameObject.SetActive(false);
        LevelController.lc.CollectEnergy();
    }

    protected override void AnimObject()
    {
        transform.localScale = m_idleScale + Vector3.one * (1 + Mathf.Sin(scaleSpeed * m_timeOffset)) * 0.5f * scaleAmount;
        base.AnimObject();
    }
}
