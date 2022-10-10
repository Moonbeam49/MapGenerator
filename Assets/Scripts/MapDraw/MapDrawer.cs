using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
//Class that handles general map drawing operations.
public class MapDrawer : MonoBehaviour
{
    public GameObject TerrainMeshPrefab;
    public GameObject MeshParent;
    public GameObject WaterMeshPrefab;

    public UIController ControllerUI;

    //Settings used to colorize generated heightMap.
    public HeightColors[] heightColors;
    public float[] BaseBlends;

    //Struct used to convert height to color, can be set up in Unity editor.
    [System.Serializable]
    public struct HeightColors
    {
        public Color color;
        public float maxHeight;
    }

    public void drawHeightMapAs2dTexture(float[,] heightMap)
    {
        //Clear out all of the existing meshes.
        List<Transform> toDestroy = new List<Transform>();
        foreach (Transform child in MeshParent.transform)
        {
            toDestroy.Add(child);
        }

        for (int i = 0; i < toDestroy.Count; i++)
        {
            DestroyImmediate(toDestroy[i].gameObject);
        }

        //Converting heightmap into 2d texture.
        int size = heightMap.GetLength(1);
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x =0; x < size; x++)
            {
                Color color = Color.white;
                float curHeight = heightMap[x, y];
                for (int i = 0; i < heightColors.Length; i++)
                {
                    if (curHeight < heightColors[i].maxHeight)
                    {
                        color = heightColors[i].color;
                        break;
                    }
                }
                pixels[y * size + x] = color;
            }
        }
        Texture2D texture = new Texture2D(size,size, TextureFormat.RGB24, false);
        texture.SetPixels(pixels);
        texture.filterMode = FilterMode.Point;
        texture.Apply();

        //Send the newly created texture to the minimap.
        Sprite mapSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        ControllerUI.UpdateMinimap(mapSprite);
    }

    //Sends all of the data required for colorization to the terrain shader.
    public void UpdateMeshShader(float minHeight, float maxHeight)
    {
        MeshRenderer terrainMeshRenderer = TerrainMeshPrefab.GetComponent<MeshRenderer>();
        terrainMeshRenderer.sharedMaterial.SetFloat("minHeight", minHeight);
        terrainMeshRenderer.sharedMaterial.SetFloat("maxHeight", maxHeight);
        Color[] baseColors = new Color[heightColors.Length];
        float[] baseHeights = new float[heightColors.Length];
        for (int i = 0; i < heightColors.Length; i++)
        {
            baseColors[i] = heightColors[i].color;
            if (i == 0) baseHeights[i] = 0; else if (i == 1) baseHeights[i] = 0.1f; else baseHeights[i] = heightColors[i-1].maxHeight;
        }
        terrainMeshRenderer.sharedMaterial.SetInt("baseColorsLength", baseColors.Length);
        terrainMeshRenderer.sharedMaterial.SetColorArray("baseColors", baseColors);
        terrainMeshRenderer.sharedMaterial.SetFloatArray("baseHeights", baseHeights);
        terrainMeshRenderer.sharedMaterial.SetFloatArray("baseBlends", BaseBlends);
    }

    //Creates a pre generated chunk mesh with corresponding water mesh.
    public void CreateMesh(MeshData[] meshes)
    {
        int offset = meshes[0].width + meshes[0].width / 2 - 1;
        for (int i = 0; i < meshes.Length;i++)
        {
            GameObject thisMesh = Instantiate(TerrainMeshPrefab, MeshParent.transform);
            thisMesh.SetActive(true);
            thisMesh.GetComponent<MeshFilter>().sharedMesh = meshes[i].CreateMesh();
            GameObject waterMesh = Instantiate(WaterMeshPrefab, thisMesh.transform);
            waterMesh.GetComponent<TesselatedPlane>().Setup(meshes[i].width, meshes[i].height);
            waterMesh.GetComponent<TesselatedPlane>().Generate();
            Vector3 waterPosition = new Vector3(meshes[i].startX + (meshes[i].width-1f)/2, Options.WaterMeshHeight, meshes[i].startY + (meshes[i].height-1f)/2);
            waterMesh.transform.position = waterPosition;
            waterMesh.SetActive(true);       
        }
    }
}
