using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class SelectParent : MonoBehaviour
{
    private void Update()
    {
        if(Selection.activeGameObject == gameObject)
        {
            Selection.activeGameObject = transform.parent.gameObject;
        }
    }
}
