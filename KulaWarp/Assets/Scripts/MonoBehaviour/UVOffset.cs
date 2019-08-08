using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVOffset : MonoBehaviour
{
    void Awake()
    {
        // Randomize the UV offset of the first texture of the object. 
        Renderer rend   = gameObject.GetComponent<Renderer>();
        Vector2  offset = new Vector2(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
        rend.material.SetTextureOffset("_MainTex", offset);
    }
}
