using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class MaxGeometryMessagePanelScript : MonoBehaviour
{
    public Text GeometryText;
    public Text ForeGroundText;

    public virtual void ShowMessage(string message)
    {
        GeometryText.text = message;
        ForeGroundText.text = message;

        gameObject.SetActive(true);
    }
}
