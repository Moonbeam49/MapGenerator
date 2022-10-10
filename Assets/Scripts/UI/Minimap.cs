using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//Class used to control the minimap on UI.
public class Minimap : MonoBehaviour
{
    public UIController UIObject;
    public GameObject MinimapParent;
    public Image MinimapImage;
    public GameObject MinimapBlip;
    public bool FullSizeForced;


    Vector3 lastCameraPos;
    public int MinimapState = 1; // 0 - Hidden, 1 - Minimized, 2 - Full size
    int screenSize;
    float minimizedSize;
    Vector3 minimizedPosition;
    Vector3 fullSizePosition;


    //Collects basic parameters needed for state switching.
    public void Initialize()
    {
        lastCameraPos = UIObject.CameraObject.transform.position;
        MinimapBlip.transform.localPosition = MinimapPosFromCameraPos();
        float fullSizeX = (Screen.width - Screen.height) / 2;
        float fullSizeY = Screen.height / 2;
        screenSize = Screen.height;
        if (screenSize < Screen.width)
        {
            screenSize = Screen.height;
            fullSizeX = (Screen.height - Screen.width) / 2;
        }
        minimizedSize = MinimapParent.GetComponent<RectTransform>().rect.width;
        minimizedPosition = MinimapParent.transform.position;
        fullSizePosition = new Vector3(fullSizeY, fullSizeY, 0);
    }

    public void SwitchMinimapState()
    {
        if (!FullSizeForced)
        {
            MinimapState++;
            if (MinimapState > 2)
            {
                MinimapState = 0;
            }
            switch (MinimapState)
            {
                case 0:
                    gameObject.SetActive(false);
                    break;
                case 1:
                    gameObject.SetActive(true);
                    MinimapParent.GetComponent<RectTransform>().sizeDelta = new Vector2(minimizedSize, minimizedSize);
                    MinimapParent.transform.position = minimizedPosition;
                    MinimapBlip.transform.localPosition = MinimapPosFromCameraPos();
                    break;
                case 2:
                    gameObject.SetActive(true);
                    MinimapParent.GetComponent<RectTransform>().sizeDelta = new Vector2(screenSize, screenSize);
                    MinimapParent.transform.position = fullSizePosition;
                    MinimapBlip.transform.localPosition = MinimapPosFromCameraPos();
                    break;
            }
        }
    }

    //Called when a map is generated without a mesh.
    public void ForceFullSize()
    {
        MinimapState = 2;
        FullSizeForced = true;
        gameObject.SetActive(true);
        MinimapParent.GetComponent<RectTransform>().sizeDelta = new Vector2(screenSize, screenSize);
        MinimapParent.transform.position = fullSizePosition;
        MinimapBlip.SetActive(false);
    }

    public void Minimize()
    {
        MinimapState = 1;
        gameObject.SetActive(true);
        MinimapParent.GetComponent<RectTransform>().sizeDelta = new Vector2(minimizedSize, minimizedSize);
        MinimapParent.transform.position = minimizedPosition;
        MinimapBlip.transform.localPosition = MinimapPosFromCameraPos();
        MinimapBlip.SetActive(true);
    }

    void Update()
    {
        if (MinimapState !=0 && lastCameraPos != UIObject.CameraObject.transform.position)
        {
            MinimapBlip.transform.localPosition = MinimapPosFromCameraPos();
        }
    }

    //Calculates blip position on the minimap based on the current camera location.
    Vector3 MinimapPosFromCameraPos()
    {
        float cameraX = UIObject.CameraObject.transform.position.x;
        float cameraY = UIObject.CameraObject.transform.position.z;
        int mapSize = Options.MapSize;
        if (cameraX < 0)
        {
            cameraX = 0;
        }
        else if (cameraX > mapSize)
        {
            cameraX = mapSize;
        }
        if (cameraY < 0)
        {
            cameraY = 0;
        }
        else if (cameraY > mapSize)
        {
            cameraY = mapSize;
        }
        float xPosPercent = ((float)cameraX / mapSize);
        float yPosPercent = ((float)cameraY / mapSize);
        float minimapHalfSize = MinimapImage.gameObject.GetComponent<RectTransform>().rect.width / 2f;
        cameraX = Mathf.Lerp(-minimapHalfSize, minimapHalfSize, xPosPercent);
        cameraY = Mathf.Lerp(-minimapHalfSize, minimapHalfSize, yPosPercent);
        lastCameraPos = UIObject.CameraObject.transform.position;
        return new Vector3(cameraX, cameraY, 0);
    }
}
