using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class DialogPanelScript : ModalPanelScript
{
    public Text DialogText;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetDialogText(string text)
    {
        DialogText.text = text;
    }
}
