using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using TMPro;
using System.Diagnostics;
//Handles all of the file system operations and controls corresponding UI element.
public class FilePanel : MonoBehaviour
{
    public MapGenerator GeneratorObject;
    public UIController UIObject;
    public GameObject ItemPrefab;
    public Transform ItemParent;
    public TMP_InputField SaveNameInputField;
    public TextMeshProUGUI Header;

    bool readingMaps;
    string mapLocation;
    string settingsLocation;
    int currentSelection;

    List<FileItem> currentItems = new List<FileItem>();

    //Checks the default map location, creates the directory if it doesn't exist.
    public void OpenMapFolder()
    {
        mapLocation = Application.dataPath + "/Maps";

        readingMaps = true;
        Header.text = "Maps";
        if (!Directory.Exists(mapLocation))
        {
            Directory.CreateDirectory(mapLocation);
        }
        ShowDirectory(mapLocation);
    }

    //Checks the default settings location, creates the directory if it doesn't exist.
    public void OpenSettingsFolder()
    {
        settingsLocation = Application.dataPath + "/Settings";

        readingMaps = false;
        Header.text = "Settings profiles";
        if (!Directory.Exists(settingsLocation))
        {
            Directory.CreateDirectory(settingsLocation);
        }
        ShowDirectory(settingsLocation);
    }

    //Displays all of the files with the correct extension on UI.
    void ShowDirectory(string path)
    {
        gameObject.SetActive(true);
        DirectoryInfo directoryInfo = new DirectoryInfo(path);
        FileInfo[] fileInfo;
        if (readingMaps)
        {
            fileInfo = directoryInfo.GetFiles("*.map");
        } else
        {
            fileInfo = directoryInfo.GetFiles("*.set");
        }
        //Removing previously created items.
        currentItems.Clear();
        foreach (Transform child in ItemParent)
        {
            Destroy(child.gameObject);
        }
        //Creating new items, and selecting the first one if it exists.
        for (int i = 0; i < fileInfo.Length; i++)
        {
            AddItemToList(fileInfo[i], i);
            if (i != 0)
            {
                currentItems[i].Tint();
            }
            else
            {
                currentItems[i].Highlight();
            }
        }
        if (fileInfo.Length != 0)
        {
            SaveNameInputField.text = currentItems[0].Name.text;
        }
    }

    //Adds a single item at the end of the list.
    void AddItemToList(FileInfo fileInfo, int ind)
    {
        GameObject curObj = Instantiate(ItemPrefab, ItemParent);
        currentItems.Add(curObj.GetComponent<FileItem>());
        currentItems[ind].ShowInfoForItem(ind, fileInfo.Name, fileInfo.DirectoryName, fileInfo.CreationTime);
        currentItems[ind].PanelParent = this;
    }

    //Called whenever an item is clicked.
    public void ItemSelected(int id)
    {
        currentSelection = id;
        for (int i = 0; i < currentItems.Count; i++)
        {
            if (i == id)
            {
                currentItems[i].Highlight();
                SaveNameInputField.text = currentItems[i].Name.text;
            }
            else
            {
                currentItems[i].Tint();
            }
        }
    }

    byte[] HeightMapToByteArray()
    {
        float[] tempMap = new float[Options.MapSize * Options.MapSize];
        int floatInd = 0;
        for (int y = 0; y < Options.MapSize; y++)
        {
            for (int x = 0; x < Options.MapSize; x++)
            {
                tempMap[floatInd] = GeneratorObject.CurrentHeightMap[x, y];
                floatInd++;
            }
        }
        byte[] data = new byte[tempMap.Length * 4];
        Buffer.BlockCopy(tempMap,0,data,0,data.Length);
        
        return data;
    }

    void ByteArrayToHeightMap(byte[] data)
    {
        float[] tempMap = new float[data.Length / 4];
        Buffer.BlockCopy(data,0,tempMap,0,data.Length);
        int sideSize = (int)Math.Sqrt(tempMap.Length);
        float[,] tempHeightMap = new float[sideSize, sideSize];
        int floatInd = 0;
        for (int y = 0; y < sideSize; y++)
        {
            for (int x = 0; x < sideSize; x++)
            {
                tempHeightMap[x, y] = tempMap[floatInd];
                floatInd++;
            }
        }
        GeneratorObject.LoadHeightMap(tempHeightMap);
    }

    public void Save()
    {
        string name = SaveNameInputField.text;
        if (name == "")
        {
            name = "Untitled";
        }

        
        if (readingMaps)
        {
            if (!name.Contains(".map"))
            {
                name += ".map";
            }
            byte[] data = HeightMapToByteArray();
            File.WriteAllBytes(mapLocation + "/" + name, data);
        }
        else
        {
            string data;
            if (!name.Contains(".set"))
            {
                name += ".set";
            }
            UIObject.ReadSettings();
            data = Options.ConvertOptionsToString();
            File.WriteAllText(settingsLocation + "/" + name, data);
        }
        gameObject.SetActive(false);
    }

    public void Load()
    {
        string name = currentItems[currentSelection].Name.text;
        if (readingMaps)
        {
            byte[] data = File.ReadAllBytes(mapLocation + "/" + name);
            ByteArrayToHeightMap(data);
        }
        else
        {
            string data;
            data = File.ReadAllText(settingsLocation + "/" + name);
            Options.GetOptionsFromString(data);
            UIObject.SetSettingsFromOptions();
        }
        gameObject.SetActive(false);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
