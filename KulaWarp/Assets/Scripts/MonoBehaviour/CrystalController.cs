using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalController : PickUpController
{
    protected override void OnTriggerEnter(Collider other)
    {
        gameObject.SetActive(false);
        LevelController.lc.CollectCrystal();
    }
}
