using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class LevelGenerator
{
    public const int s_EnvLayer = 10;

    public static void mergeBlockGeometry()
    {
        Debug.Log("Merging geometry...");

        // Get a list of all blocks:
        List<GameObject> blockList = LevelGenerator.gameObjectsByLayer(LevelGenerator.s_EnvLayer);
        if (blockList.Capacity == 0)
        {
            Debug.Log("No blocks to merge.");
            return;
        }

        // Create coordinate lookup:
        Dictionary<Vector3, GameObject> blockDict = new Dictionary<Vector3, GameObject>();
        foreach(GameObject block in blockList)
            blockDict.Add(block.transform.transform.position, block);

        int ccCount = 0; 
        // Find connected components:
        while(blockList.Count > 0) 
        { 
            ccCount++;

            List<GameObject>  connectedComponent = new List<GameObject>();
            Queue<GameObject> blockQueue         = new Queue<GameObject>();
            blockQueue.Enqueue(blockList[0]);
            blockDict.Remove(blockList[0].transform.transform.position);

            while(blockQueue.Count > 0)
            {
                GameObject block = blockQueue.Dequeue();
                Vector3    coord = block.transform.position;
                connectedComponent.Add(block);
                blockList.Remove(block);

                addNeighbors(coord, blockDict, blockQueue);
            }

            // Create a new gameobj to hold he connected component.
            GameObject combinedObj = new GameObject("Environment");
            combinedObj.AddComponent<MeshFilter>();
            combinedObj.AddComponent<MeshRenderer>();
            Mesh finalMesh = new Mesh();
            Vector3 offset = Vector3.zero;
            if(connectedComponent.Count > 1) 
            {
                List<MeshFilter> meshFilters = new List<MeshFilter>();
                List<Vector3>    coords      = new List<Vector3>();

                // Translate the component to the origin first to make sure the transform
                // of the new compontn is centered.
                offset = connectedComponent.Select(b => b.transform.localToWorldMatrix * b.transform.position)
                                                                   .Aggregate((res, c) => res + c)
                                                                   / connectedComponent.Count;
                combinedObj.transform.position += offset;

                foreach(GameObject b in connectedComponent)
                {
                    coords.Add(b.transform.localToWorldMatrix * b.transform.position);
                    meshFilters.Add(b.GetComponent<MeshFilter>());
                }
                // Merge the meshes into one mesh.
                finalMesh = MeshCombiner.combineCubeMeshes(meshFilters.ToArray(), coords);
            }
            else 
            {
                // Translate the component to the origin first to make sure the transform
                // of the new compontn is centered.
                offset = connectedComponent[0].transform.localToWorldMatrix * connectedComponent[0].transform.position;
                combinedObj.transform.position += offset;

                finalMesh = connectedComponent[0].GetComponent<MeshFilter>().sharedMesh;
            }

            // Apply mesh, material and translation to the new gameObj.
            combinedObj.GetComponent<MeshFilter>().sharedMesh        = finalMesh;
            combinedObj.GetComponent<MeshRenderer>().sharedMaterials = connectedComponent[0].GetComponent<MeshRenderer>().sharedMaterials;

            // Add physics compontents.
            combinedObj.AddComponent<MeshCollider>();
            combinedObj.GetComponent<MeshCollider>().sharedMesh = finalMesh;
            combinedObj.layer = s_EnvLayer;

        } // while(blockList.Count > 0) 
        Debug.Log("Found " + ccCount + " connected components.");
    }

    private static void addNeighbors(Vector3 coord, Dictionary<Vector3, GameObject> blockDict, Queue<GameObject> blockQueue) 
    {
        Vector3Int right = Vector3Int.right;
        Vector3Int left  = Vector3Int.left;
        Vector3Int up    = Vector3Int.up;
        Vector3Int down  = Vector3Int.down;
        Vector3Int forw  = new Vector3Int(0, 0, 1);
        Vector3Int backw = new Vector3Int(0, 0, -1);

        if(blockDict.ContainsKey(coord + right)) 
        { 
            blockQueue.Enqueue(blockDict[coord + right]); 
            blockDict.Remove(coord + right); 
        }
        if(blockDict.ContainsKey(coord + left))
        { 
            blockQueue.Enqueue(blockDict[coord + left]); 
            blockDict.Remove(coord + left); 
        }
        if(blockDict.ContainsKey(coord + up))
        { 
            blockQueue.Enqueue(blockDict[coord + up]); 
            blockDict.Remove(coord + up); 
        }
        if(blockDict.ContainsKey(coord + down))
        { 
            blockQueue.Enqueue(blockDict[coord + down]); 
            blockDict.Remove(coord + down); 
        }
        if(blockDict.ContainsKey(coord + forw))
        { 
            blockQueue.Enqueue(blockDict[coord + forw]); 
            blockDict.Remove(coord + forw); 
        }
        if(blockDict.ContainsKey(coord + backw))
        { 
            blockQueue.Enqueue(blockDict[coord + backw]); 
            blockDict.Remove(coord + backw); 
        }
    }

    // TODO: move to utils 
    public static List<GameObject> gameObjectsByLayer(int layer)
    {
        GameObject[] allGameObjects = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
        List<GameObject> objOnLayer = new List<GameObject>();

        foreach (GameObject obj in allGameObjects)
        {
            if (obj.layer == layer)
                objOnLayer.Add(obj);
        }

        return objOnLayer;
    }
}
