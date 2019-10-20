using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalActivationScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Activate(bool state)
    {
        Manager.DisableShortcuts = state;
        gameObject.SetActive(state);
    }
}
