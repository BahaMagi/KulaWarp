using UnityEngine;

/**
* Extends the base PickUpController to count energy instead of points when picked up.
*/
public class EnergyController : PickUpController
{
    protected override void OnTriggerEnter(Collider other)
    {
        gameObject.SetActive(false);
        LevelController.lc.CollectEnergy();
    }
}
