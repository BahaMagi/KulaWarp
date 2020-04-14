using UnityEngine;

/**
 * Base SO for the different element types { Empty, Block, Pickup, Interactable, Player }.
 */
public class ElementSO : ScriptableObject
{ 
    public virtual string type { get { return "None"; } }
    public GameObject basePrefab;

    public virtual GameObject CreateElement()
    { return new GameObject(); }
}
