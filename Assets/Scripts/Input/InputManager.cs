using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

//Class that handles player input outside of UI interactions.
public class InputManager : MonoBehaviour
{
    //Number of movement speed settings.
    public Minimap MinimapController;
    public float MainSpeed = 100.0f;
    public float ShiftAdd = 250.0f;
    public float MaxShift = 1000.0f;
    public float CamSens = 0.25f;

    float totalRun = 1.0f;
    bool cameraLocked = false;

    //Lock the mouse to rotate the camera at start.
    private void Start()
    {
        cameraLocked = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    void Update()
    {
        //Controlling camera rotation.
        if (!cameraLocked)
        {
            Vector3 newRotation = new Vector3(transform.eulerAngles.x + Input.GetAxis("Mouse Y") * -1 * CamSens, transform.eulerAngles.y + Input.GetAxis("Mouse X") * CamSens, 0);
            transform.eulerAngles = newRotation;
        }

        //Collecting movement direction data, if the shift key is pressed - speed will be increased.
        Vector3 p = GetBaseInput();
        if (p.sqrMagnitude > 0)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                totalRun += Time.deltaTime;
                p = p * totalRun * ShiftAdd;
                p.x = Mathf.Clamp(p.x, -MaxShift, MaxShift);
                p.y = Mathf.Clamp(p.y, -MaxShift, MaxShift);
                p.z = Mathf.Clamp(p.z, -MaxShift, MaxShift);
            }
            else
            {
                totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 1000f);
                p = p * MainSpeed;
            }

            p = p * Time.deltaTime;
            Vector3 newPosition = transform.position;
            transform.Translate(p);
        }

        //Handling custom hotkeys.
        if (Input.GetKeyDown(KeyCode.L))
        {
            if (cameraLocked)
            {
                cameraLocked = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                cameraLocked = true;
                Cursor.lockState = CursorLockMode.Confined;
            }
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            MinimapController.SwitchMinimapState();
        }
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (MinimapController.UIObject.HelpWindow.activeSelf)
            {
                MinimapController.UIObject.CloseHelp();
            }
            else
            {
                MinimapController.UIObject.OpenHelp();
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            MinimapController.UIObject.CloseHelp();
        }
    }

    //Gets the requested by the input movement vector.
    private Vector3 GetBaseInput()
    { 
        Vector3 p_Velocity = new Vector3();
        if (!cameraLocked)
        {
            if (Input.GetKey(KeyCode.W))
            {
                p_Velocity += new Vector3(0, 0, 1);
            }
            if (Input.GetKey(KeyCode.S))
            {
                p_Velocity += new Vector3(0, 0, -1);
            }
            if (Input.GetKey(KeyCode.A))
            {
                p_Velocity += new Vector3(-1, 0, 0);
            }
            if (Input.GetKey(KeyCode.D))
            {
                p_Velocity += new Vector3(1, 0, 0);
            }
            if (Input.GetKey(KeyCode.Space))
            {
                p_Velocity += new Vector3(0, 1, 0);
            }
            if (Input.GetKey(KeyCode.C))
            {
                p_Velocity += new Vector3(0, -1, 0);
            }
        }
        return p_Velocity;
    }
}