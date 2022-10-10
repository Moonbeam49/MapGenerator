using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Creates and stores water meshes.
public class TesselatedPlane : MonoBehaviour
{
    public int Width;
    public int Height;

    public void Setup(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public void Generate()
    {
        float topLeftX = ((Width - 1) / -2f);
        float topLeftZ = ((Height - 1) / 2f);

        int vertexIndex = 0;
        int triangleIndex = 0;
        Vector3[] vertices = new Vector3[Width * Height];
        Vector2[] uvs = new Vector2[Width * Height];
        int[] triangles = new int[(Width - 1) * (Height - 1) * 6];
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {

                vertices[vertexIndex] = new Vector3(topLeftX + x, 0, topLeftZ - y);
                uvs[vertexIndex] = new Vector2(x/ (float)Width, y / (float)Height);
                if (x < Width - 1 && y < Height - 1)
                {
                    triangles[triangleIndex] = vertexIndex;
                    triangles[triangleIndex+1] = vertexIndex + Width +1;
                    triangles[triangleIndex+2] = vertexIndex + Width;
                    triangles[triangleIndex+3] = vertexIndex + Width + 1;
                    triangles[triangleIndex+4] = vertexIndex;
                    triangles[triangleIndex+5] = vertexIndex+1;

                    triangleIndex += 6;
                }

                vertexIndex++;
            }
        }

        Mesh newMesh = new Mesh();
        newMesh.vertices = vertices;
        newMesh.triangles = triangles;
        newMesh.uv = uvs;
        newMesh.RecalculateNormals();
        gameObject.GetComponent<MeshFilter>().sharedMesh = newMesh;
    }
}
