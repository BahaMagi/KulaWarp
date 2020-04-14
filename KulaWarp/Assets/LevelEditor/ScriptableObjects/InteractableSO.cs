using UnityEngine;
using UnityEngine.Events;


[CreateAssetMenu(fileName = "New Interactable", menuName = "Elements/Interactable")]
public class InteractableSO : ElementSO
{
    public Texture[]  textures;
    public Mesh       mesh;
    public AudioClip  interactionSound;
    public Collider   collider;
    public UnityEvent interactionCallBack;
}
