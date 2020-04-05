using UnityEngine;


/**
 * Used to creat lists for the editor. A list for each type to provide the level editor with 
 * a reference to each element of a certain type. 
 */
[CreateAssetMenu(fileName = "New Element List", menuName = "Elements/List")]
public class ElementListSO : ScriptableObject
{
    public ElementSO[] elementList;
}
