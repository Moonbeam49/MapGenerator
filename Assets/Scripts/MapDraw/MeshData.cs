using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class used to store all the mesh data for a single chunk.
public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;
    public int width, height;
    public int startX, startY;
    public Vector3[] normals;

    public MeshData(int meshWidth, int meshHeight, int startingXPoint, int startingYPoint)
    {
        width = meshWidth; height = meshHeight;
        startX = startingXPoint; startY = startingYPoint;
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
        normals = new Vector3[vertices.Length];
    }
    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = normals;

        return mesh;
    }

}
