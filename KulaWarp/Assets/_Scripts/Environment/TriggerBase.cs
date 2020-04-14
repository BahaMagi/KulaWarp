using UnityEngine;
using UnityEngine.Events;

/**
 * Can be used for game objects which are only supposed to act as triggers,
 * like pickups or obstacles. The On Trigger Enter event can be assigned to 
 * a function in the inspector. 
 */
public class TriggerBase : MonoBehaviour
{
    public UnityEvent onTriggerEnter;

    protected virtual void OnTriggerEnter(Collider other)
    {
        onTriggerEnter.Invoke();
    }
}

