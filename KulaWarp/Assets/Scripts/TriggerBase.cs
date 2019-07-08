using UnityEngine;
using UnityEngine.Events;

/**
 * Can be used for game objects which are only supposed to act as triggers,
 * like pickups or obstacles. 
 * If only the OnTriggerEnter event is important the base class can be used 
 * directly. If more is needed it should be inherited. 
 */
public class TriggerBase : MonoBehaviour
{
    public UnityEvent onTriggerEnter;

    protected virtual void OnTriggerEnter(Collider other)
    {
        onTriggerEnter.Invoke();
    }
}