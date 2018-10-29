using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SelfEnablingButtonScript : MonoBehaviour
{
    public Button Button;

    public void Disable(bool value)
    {
        Button.interactable = !value;
    }
}
