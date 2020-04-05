using UnityEngine;


[CreateAssetMenu(fileName = "New Block", menuName = "Elements/Block")]
public class BlockSO : ElementSO
{
    public Material[] materials;

    public override string type { get { return "Block"; } }

    public override GameObject CreateElement()
    {
        GameObject newElement = ExtensionMethods.InstantiatePrefabAsPrefab(basePrefab);
        newElement.GetComponent<MeshRenderer>().materials = materials;

        return newElement;
    }
}
