using UnityEngine;
using System.Collections;
//Converts height map into an array of meshes.
public static class MeshGenerator
{
    static Vector3[] fullVerts;
    static Vector3[] fullNormals;
    static int[] fullTris;
    //Generates a single mesh with corresponding data, then splits it into chunks.
    public static MeshData[] GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve)
    {
        GenerateFullMesh(heightMap, heightMultiplier, heightCurve);

        //Calculating the amount of chunks.
        int chunksPerSide = heightMap.GetLength(0) / Options.MeshChunkMaxSize;
        if (heightMap.GetLength(0) % Options.MeshChunkMaxSize != 0) chunksPerSide++;
        int totalChunks = chunksPerSide * chunksPerSide;
        
        //Filling the chunks with data.
        MeshData[] meshes =  new MeshData[totalChunks];
        for (int i = 0; i < totalChunks; i++)
        {
            //Calculating chunk size, while handling border chunks that might be smaller than the max chunk size. 
            int thisChunkStartX = Options.MeshChunkMaxSize * (i % chunksPerSide);
            if (thisChunkStartX < 0)
            {
                thisChunkStartX = 0;
            }

            int thisChunkStartY = Options.MeshChunkMaxSize * (i / chunksPerSide);
            if (thisChunkStartY < 0)
            {
                thisChunkStartY = 0;
            }

            int width = Options.MeshChunkMaxSize;
            if (width > heightMap.GetLength(0) - thisChunkStartX)
            {
                width = (heightMap.GetLength(0) % Options.MeshChunkMaxSize);
            }

            int height = Options.MeshChunkMaxSize;
            if (height > heightMap.GetLength(1) - thisChunkStartY)
            {
                height = (heightMap.GetLength(1) % Options.MeshChunkMaxSize);
            }

            if (thisChunkStartX != 0)
            {
                thisChunkStartX--;
                width++;
            }
            if (thisChunkStartY != 0)
            {
                thisChunkStartY--;
                height++;
            }

            //Writing mesh data for the current chunk from the global generated mesh.
            MeshData meshData = new MeshData(width, height,thisChunkStartX, thisChunkStartY);
            int vertexIndex = 0;
            int trisIndex = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int vertFromFull = (thisChunkStartY + y) * heightMap.GetLength(0) + (thisChunkStartX + x);
                    meshData.vertices[vertexIndex] = fullVerts[vertFromFull];
                    meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);
                    meshData.normals[vertexIndex] = fullNormals[vertFromFull];
                    if (x < width - 1 && y < height - 1)
                    {
                        meshData.triangles[trisIndex] = vertexIndex;
                        meshData.triangles[trisIndex + 1] = vertexIndex + width;
                        meshData.triangles[trisIndex + 2] = vertexIndex + width + 1;
                        meshData.triangles[trisIndex + 3] = vertexIndex + width + 1;
                        meshData.triangles[trisIndex + 4] = vertexIndex + 1;
                        meshData.triangles[trisIndex + 5] = vertexIndex;

                        trisIndex += 6;
                    }

                    vertexIndex++;
                }
            }

            meshes[i] = meshData;
        }


        return meshes;

    }

    //Generates full mesh from height map.
    static void GenerateFullMesh(float[,] hMap, float heightMult, AnimationCurve heightCurve)
    {
        int sideSize = hMap.GetLength(0);
        fullVerts = new Vector3[sideSize * sideSize];
        fullNormals = new Vector3[fullVerts.Length];
        fullTris = new int[(sideSize - 1) * (sideSize - 1) * 6];

        int vertIndex = 0; 
        int trisIndex = 0;
        for (int y = 0; y < sideSize; y++)
        {
            for (int x = 0; x < sideSize; x++)
            {
                fullVerts[vertIndex] = new Vector3(x, heightCurve.Evaluate(hMap[x, y]) * heightMult, y);
                if (x < sideSize - 1 && y < sideSize - 1)
                {
                    fullTris[trisIndex] = vertIndex;
                    fullTris[trisIndex + 1] = vertIndex + sideSize + 1;
                    fullTris[trisIndex + 2] = vertIndex + sideSize;
                    fullTris[trisIndex + 3] = vertIndex + sideSize + 1;
                    fullTris[trisIndex + 4] = vertIndex;
                    fullTris[trisIndex + 5] = vertIndex + 1;
                    trisIndex += 6;
                }

                vertIndex++;
            }
        }

        //Manual normals calculation is used to avoid seams between chunks.
        int triangleCount = fullTris.Length/3;
        for (int i = 0; i < triangleCount; i++)
        {
            int curTriStart = i * 3;
            int indA = fullTris[curTriStart];
            int indB = fullTris[curTriStart+1];
            int indC = fullTris[curTriStart+2];
            Vector3 thisTriNormal = GetSurfaceNormal(indA, indB, indC);
            thisTriNormal.y *= -1;
            fullNormals[indA] += thisTriNormal;
            fullNormals[indB] += thisTriNormal;
            fullNormals[indC] += thisTriNormal;
        }

        for (int i = 0; i < fullNormals.Length; i++)
        {
            fullNormals[i].Normalize();
        }

        Vector3 GetSurfaceNormal(int a,int b,int c)
        {
            Vector3 sideAB = fullVerts[b] - fullVerts[a];
            Vector3 sideAC = fullVerts[c] - fullVerts[a];
            return Vector3.Cross(sideAB, sideAC).normalized;
        }
    }
}


