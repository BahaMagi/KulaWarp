using UnityEngine;


[CreateAssetMenu(fileName = "New PickUp", menuName = "Elements/PickUp")]
public class PickUpSO : ElementSO
{
    public override string type { get { return "PickUp"; } }

    public override GameObject CreateElement()
    {
        GameObject newElement = ExtensionMethods.InstantiatePrefabAsPrefab(basePrefab);

        return newElement;
    }

    public void SetUpVector(GameObject e, Vector3 up)
    {
        if (e.GetComponent<HoverIdleAnim>())
            e.GetComponent<HoverIdleAnim>().up = e.GetComponent<PickUpBase>().up;
    }
}
