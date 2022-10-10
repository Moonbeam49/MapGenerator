using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

//Controls general UI operations and current settings.
public class UIController : MonoBehaviour
{
    public GameObject CameraObject;
    public MapGenerator Generator;
    public GameObject SettingsObject;
    public Minimap MinimapObject;
    public FilePanel FileController;
    public GameObject HelpWindow;
    public GameObject ShapeSettingsGroup;

    public TooltipController Tooltip;

    public TMP_InputField SeedInputField;
    public TMP_Dropdown SizeDropdown;
    public TMP_Dropdown AlgortithmDropdown;
    public Toggle CreateMesh;
    public TMP_InputField NoiseScaleInputField;
    public TMP_InputField NoiseOctavesInputField;
    public Slider NoisePersistanceSlider;
    public Slider NoiseLacunaritySlider;
    public TMP_InputField ShapeCountInputField;
    public Slider ShapeSizeSlider;
    public Slider DensityRadiusSlider;
    public Slider ContourRadiusSlider;
    public Slider ContourConsitencySlider;

    public Vector3 StartingCameraPos;
    public Quaternion StartingCameraRotation;


    Vector3 settingsPosition;


    bool settingsVisible = true;
    void Start()
    {
        CameraObject.transform.position = StartingCameraPos;
        CameraObject.transform.rotation = StartingCameraRotation;
        settingsPosition = SettingsObject.transform.position;
        GenerateNewMap();
        MinimapObject.Initialize();
    }

    public void OpenHelp()
    {
        HelpWindow.gameObject.SetActive(true);
    }

    public void CloseHelp()
    {
        HelpWindow.gameObject.SetActive(false);
    }

    public void OnAlgorithmChange()
    {
        if (AlgortithmDropdown.value == 0)
        {
            ShapeSettingsGroup.SetActive(true);
        } 
        else
        {
            ShapeSettingsGroup.SetActive(false);
        }
    }

    //Displays the current values from Options class.
    public void SetSettingsFromOptions()
    {
        SeedInputField.text = Options.Seed;
        switch (Options.MapSize)
        {
            case 256:
                SizeDropdown.value = 0;
                break;
            case 512:
                SizeDropdown.value = 1;
                break;
            case 1024:
                SizeDropdown.value = 2;
                break;
            case 2048:
                SizeDropdown.value = 3;
                break;
        }
        if (Options.IsShaped)
        {
            AlgortithmDropdown.value = 0;
        }
        else
        {
            AlgortithmDropdown.value = 1;
        }
        if (Options.MeshRequested)
        {
            CreateMesh.isOn = true;
        }
        else
        {
            CreateMesh.isOn = false;
        }
        NoiseScaleInputField.text = Options.NoiseScale + "";
        NoiseOctavesInputField.text = Options.NoiseOctaves + "";
        NoisePersistanceSlider.value = Options.NoisePersistance;
        NoiseLacunaritySlider.value = Options.NoiseLacunarity;
        ShapeCountInputField.text = Options.ShapeCount+"";
        ShapeSizeSlider.value = Options.ShapeSize;
        DensityRadiusSlider.value = Options.DensityRadius;
        ContourRadiusSlider.value = Options.PContourDist;
        ContourConsitencySlider.value = Options.FillThreshold;
    }

    public void UpdateMinimap(Sprite mapSprite)
    {
        MinimapObject.MinimapImage.sprite = mapSprite;
    }

    public void ResetCamera()
    {
        CameraObject.transform.position = StartingCameraPos;
        CameraObject.transform.rotation = StartingCameraRotation;
    }

    public void GenerateNewMap()
    {
        ReadSettings();
        Generator.GenerateMap();
        SetSettingsFromOptions();
        if (!CreateMesh.isOn)
        {
            MinimapObject.ForceFullSize();
        } else
        {
            if (MinimapObject.FullSizeForced)
            {
                MinimapObject.Minimize();
                MinimapObject.FullSizeForced = false;
            }
        }
    }

    public void OpenMapFiles()
    {
        FileController.OpenMapFolder();
    }

    public void OpenSettingsFiles()
    {
        FileController.OpenSettingsFolder();
    }

    //Reads settings from UI and writes them to the Option class.
    public void ReadSettings()
    {
        Options.Seed = SeedInputField.text;
        switch (SizeDropdown.value)
        {
            case 0:
                Options.MapSize = 256;
                break;
            case 1:
                Options.MapSize = 512;
                break;
            case 2:
                Options.MapSize = 1024;
                break;
            case 3:
                Options.MapSize = 2048;
                break;
        }
        if (AlgortithmDropdown.value == 0)
        {
            Options.IsShaped = true;
        }
        else
        {
            Options.IsShaped = false;
        }
        if (NoiseScaleInputField.text == "")
        {
            Options.NoiseScale = 0;
        } 
        else
        {
            Options.NoiseScale = float.Parse(NoiseScaleInputField.text);
        }
        if (NoiseOctavesInputField.text == "")
        {
            Options.NoiseOctaves = 1;
        } else
        {
            int octaves = int.Parse(NoiseOctavesInputField.text);
            if (octaves <= 0)
            {
                octaves = 1;
            }
            Options.NoiseOctaves = 1;
        }
        Options.NoisePersistance = NoisePersistanceSlider.value;
        Options.NoiseLacunarity = NoiseLacunaritySlider.value;
        if (ShapeCountInputField.text == "")
        {
            Options.ShapeCount = 1;
        }
        else
        {
            int count = int.Parse(ShapeCountInputField.text);
            if (count <= 0)
            {
                count = 1;
            }
            Options.ShapeCount = count;
        }
        Options.ShapeSize = ShapeSizeSlider.value;
        Options.DensityRadius = (int)DensityRadiusSlider.value;
        Options.PContourDist = (int)ContourRadiusSlider.value;
        Options.FillThreshold = ContourConsitencySlider.value;
        Options.MeshRequested = CreateMesh.isOn;
    }

    public void ToggleSettings()
    {
        if (settingsVisible)
        {
            settingsVisible = false;
            HideSettings();
        }
        else
        {
            settingsVisible = true;
            ShowSettings();
        }
    }

    public void CloseApp()
    {
        Application.Quit();
    }

    void ShowSettings()
    {
        SettingsObject.transform.position = settingsPosition;
        SettingsObject.transform.Find("Hide").Find("Image").Rotate(0, 180f, 0, Space.Self);
    }

    void HideSettings()
    {
        float newX = SettingsObject.transform.position.x + SettingsObject.GetComponent<RectTransform>().rect.width;
        SettingsObject.transform.position = new Vector3(newX, SettingsObject.transform.position.y);
        SettingsObject.transform.Find("Hide").Find("Image").Rotate(0, 180f, 0, Space.Self);
    }

    public void DisplayTooltip(string header, string body, Vector3 position)
    {
        Tooltip.DisplayTooltip(header, body, position);
    }

    public void HideTooltip()
    {
        Tooltip.HideTooltip();
    }
}
