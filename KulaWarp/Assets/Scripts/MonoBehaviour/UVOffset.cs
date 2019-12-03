using UnityEngine;

/** 
 * Randomize the UV offset of one texture of the object. 
 */
public class UVOffset : MonoBehaviour
{
    public int material_index = 0;

    void Awake()
    {
        // Get renderer of the gameobject the script is attached to.
        Renderer rend   = gameObject.GetComponent<Renderer>();

        Vector2 offset = new Vector2(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
        rend.materials[material_index].SetTextureOffset("_MainTex", offset);
    }
}
