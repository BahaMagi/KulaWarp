using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/**
 * This class provides the callback (handles) for the interactable gui objects
 * attached to elements in the level editor. Clicking one of these arrows
 * invokes the SpawnElement method which then creats a new "empty" editor
 * element, i.e. an element that consists of the various gizmos
 * but does not have any geometry yet. 
 */
public class ElementHandle : MonoBehaviour
{
    public GameObject elementPrefab;

    public void SpawnElement(Vector3 pos, ElementBuilder.ElementType type)
    {
        // Check if there already is an element at the target position
        if (Physics.Raycast(transform.position, pos - transform.position, out RaycastHit hit, 1.0f, 1 << 16))
        {
            // If there is an element, change selection to that element
            Selection.objects = new GameObject[] { hit.transform.gameObject };
            return;
        }

        GameObject newElement = ExtensionMethods.InstantiatePrefabAsPrefab(elementPrefab);

        // Set the position as by default the prefab transform is copied.
        newElement.gameObject.transform.position = pos;

        // Set the type of the element to match the element this was created from.
        newElement.GetComponent<ElementBuilder>().SetElementType(type);

        // Select the newly created object. But as multiple objects can be selected 
        // Selection.objects expects an array.
        Selection.objects = new GameObject[] { newElement };
    }
}
