using UnityEngine;

//Simple script that makes sure the main camera will render an accessible depth texture, which will be used for water shading.
public class CameraDepthTextureMode : MonoBehaviour
{
    [SerializeField]
    DepthTextureMode depthTextureMode;

    private void OnValidate()
    {
        SetCameraDepthTextureMode();
    }

    private void Awake()
    {
        SetCameraDepthTextureMode();
    }

    private void SetCameraDepthTextureMode()
    {
        GetComponent<Camera>().depthTextureMode = depthTextureMode;
    }
}
