using UnityEngine;

public class InteractableBase : MonoBehaviour
{
    protected virtual void OnTriggerEnter(Collider other)
    {
        Debug.Log("Interacted with object " + this.name);
    }
}
