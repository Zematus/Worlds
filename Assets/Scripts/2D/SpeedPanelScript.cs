using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpeedPanelScript : MonoBehaviour
{
    public Text Message;

    public void SetSpeedMessage(Speed speed)
    {
        Message.text = speed;
    }
}
