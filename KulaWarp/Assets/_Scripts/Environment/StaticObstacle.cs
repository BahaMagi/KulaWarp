using UnityEngine;

public class StaticObstacle : InteractableBase
{
    protected override void OnTriggerEnter(Collider other)
    {
        PlayerController.pc.Die();
    }
}
