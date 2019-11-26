using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public static AudioController ac;

    // Start is called before the first frame update
    void Awake()
    {
        if (ac == null)
        {
            DontDestroyOnLoad(gameObject); // This object is scene persistent
            ac = this;
        }
        else if (ac != this) Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
