using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ActionToolbarScript : MonoBehaviour
{
    public void SetVisible(bool state)
    {
        gameObject.SetActive(state);
    }
}
