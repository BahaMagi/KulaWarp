using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ElementHandle))]
public class ElementEditor : Editor
{
    private void OnSceneGUI()
    {
        Vector3 pos = (target as ElementHandle).transform.position;

        // Draw Central Cube button
        if (Handles.Button(pos, Quaternion.identity, 0.25f, 0.5f, Handles.CubeHandleCap))
            OnCubeHandleClick(Vector3.forward, target as ElementHandle);

        // Draw Arrows

        // Forward Arrow (0, 0, 1)
        
        Quaternion dir = Quaternion.identity; // Vector3.forward = (0, 0, l)

        Handles.color = Color.blue;

        if (Handles.Button(pos, dir, 1f, 1.0f, Handles.ArrowHandleCap))
            OnArrowHandleClick(Vector3.forward, target as ElementHandle);

        // Backward Arrow (0, 0, -1)
        Handles.color  = new Color(0, 1, 1, 1);
        dir            = dir * Quaternion.Euler(0, 180, 0); // Vector3.back = (0, 0, -l)

        if (Handles.Button(pos, dir, 1f, 1.0f, Handles.ArrowHandleCap))
            OnArrowHandleClick(Vector3.back, target as ElementHandle);

        // Right Arrow (1, 0, 0)
        Handles.color = Color.red;
        dir           = dir * Quaternion.Euler(0, -90, 0); // Vector3.right = (1, 0, 0)

        if (Handles.Button(pos, dir, 1f, 1.0f, Handles.ArrowHandleCap))
            OnArrowHandleClick(Vector3.right, target as ElementHandle);

        // Left Arrow (-1, 0, 0)
        Handles.color = new Color(1, 0.75f, 0, 1);
        dir           = dir * Quaternion.Euler(0, 180, 0); // Vector3.left = (-1, 0, 0)

        if (Handles.Button(pos, dir, 1f, 1.0f, Handles.ArrowHandleCap))
            OnArrowHandleClick(Vector3.left, target as ElementHandle);

        // Up Arrow (0, 1, 0)
        Handles.color = Color.green;
        dir           = dir * Quaternion.Euler(-90, 0, 0); // Vector3.up = (0, 1, 0)

        if (Handles.Button(pos, dir, 1f, 1.0f, Handles.ArrowHandleCap))
            OnArrowHandleClick(Vector3.up, target as ElementHandle);

        // Up Down (0, -1, 0)
        Handles.color = new Color(0, 0.6f, 0, 1);
        dir = dir * Quaternion.Euler(180, 0, 0); // Vector3.down = (0, -1, 0)

        if (Handles.Button(pos, dir, 1f, 1.0f, Handles.ArrowHandleCap))
            OnArrowHandleClick(Vector3.down, target as ElementHandle);
    }

    private void OnArrowHandleClick(Vector3 dir, ElementHandle e)
    {
        e.SpawnElement(e.transform.position + dir, e.GetComponent<ElementBuilder>().GetElementType());
    }

    private void OnCubeHandleClick(Vector3 dir, ElementHandle e)
    {
        Debug.Log("Cube");
    }
}
