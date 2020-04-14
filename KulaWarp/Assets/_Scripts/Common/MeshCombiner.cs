using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;


public class MeshCombiner
{
    private const float Epsilon = 0.001f;
    public static Mesh combineCubeMeshes(MeshFilter[] meshFilters, List<Vector3> coords)
    {
        Mesh finalMesh = new Mesh();
        CombineInstance[] combiners = new CombineInstance[meshFilters.Length];
        for(int i = 0; i < meshFilters.Length; i++)
        {
            combiners[i].subMeshIndex = 0;
            combiners[i].mesh         = meshFilters[i].sharedMesh;
            combiners[i].transform    = meshFilters[i].transform.localToWorldMatrix;
        }
        finalMesh.CombineMeshes(combiners);

        MeshCombiner.removeInsideFaces(finalMesh, coords);

        return finalMesh;
    }

    public static Mesh removeInsideFaces(Mesh mesh, List<Vector3> coords)
    {
        List<Vector3>    vertices  = new List<Vector3>(mesh.vertices);
        List<Vector3>    normals   = new List<Vector3>(mesh.normals);
        List<Vector2>    uvs       = new List<Vector2>(mesh.uv);
        List<int>        triagIdx  = new List<int>(mesh.triangles);
        Vector3Int[]     triangles = new Vector3Int[triagIdx.Count / 3];

        // Extract the vector triples from the triangle list
        for(int i = 0; i < triagIdx.Count; i++)
            triangles[i/3][i%3] = triagIdx[i];

        // Find vertices that belong to a 
        // triangles that belongs to a face that is inside the mesh
        // by checking the triangles mid point + normal and check if it lies inside a cube:
        List<int> vertToDelete = new List<int>();
        triangles.ToList().ForEach(t => {
            Vector3 midPoint = ((vertices[t.x] + vertices[t.y] + vertices[t.z]) / 3);
            if(coords.Contains((midPoint + 0.5f*normals[t.x]).Round(Vector3.one)))
            {
                vertToDelete.Add(t.x);
                vertToDelete.Add(t.y);
                vertToDelete.Add(t.z);
            }
        });

        // Remove these vertices from the mesh:
        vertToDelete = vertToDelete.Distinct().ToList();
        vertToDelete.ForEach(v => triagIdx.RemoveAll(w => w == v));
        foreach(int i in vertToDelete.OrderByDescending(v => v))
        {
            uvs.RemoveAt(i);
            normals.RemoveAt(i);
            vertices.RemoveAt(i);
        }

        // Update the index values of the remaining vertices:
        vertToDelete.OrderByDescending(v => v) 
                    .ToList()
                    .ForEach(v => triagIdx = triagIdx.Select(w => w = w > v ? w-1: w).ToList());

        Debug.Log("Reduced vertex count of " + coords.Count + " objects by " + (mesh.vertexCount - vertices.Count) + ".");

        // Offset the vertices to translate them to the object. 
        Vector3 offset = coords.Aggregate((res, c) => res + c) / coords.Count;
        vertices = vertices.Select(v => v -= offset).ToList();

        // Build the new mesh:
        mesh.SetTriangles(triagIdx, 0);
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetNormals(normals);

        return mesh;
    }
}
