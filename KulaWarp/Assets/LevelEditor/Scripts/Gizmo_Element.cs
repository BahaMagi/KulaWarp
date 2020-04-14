using UnityEngine;

public class Gizmo_Element : MonoBehaviour
{
    private float size          = 0.25f;
    private Color color         = Color.white;

    private void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawCube(transform.position, new Vector3(size, size, size));
    }
}
