using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class MapEditorToolbarScript : MonoBehaviour
{
    public Toggle Toggle1;
    public Toggle Toggle2;
    public Toggle Toggle3;
    public Toggle Toggle4;
    public Toggle Toggle5;
    public Toggle Toggle6;
    public Toggle Toggle7;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        ReadKeyboardInput();
    }

    public void ReadKeyboardInput()
    {
        bool shiftPressed = false;

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            shiftPressed = true;
        }

        if (shiftPressed)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Toggle1.isOn = !Toggle1.isOn;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Toggle2.isOn = !Toggle2.isOn;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Toggle3.isOn = !Toggle3.isOn;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                Toggle4.isOn = !Toggle4.isOn;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                Toggle5.isOn = !Toggle5.isOn;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                Toggle6.isOn = !Toggle6.isOn;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                Toggle7.isOn = !Toggle7.isOn;
            }
        }
    }
}
