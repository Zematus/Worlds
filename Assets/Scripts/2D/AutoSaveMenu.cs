using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoSaveMenu : MonoBehaviour
{
    public Dropdown Mode;
    public GameObject RealTimeObj;
    public Text RealTimeText;
    public GameObject GameTimeObj;
    public Text GameTimeText;
    // Start is called before the first frame update
    void Start()
    {
        RealTimeText.text = "every " + (GuiManagerScript.RealWorldAutoSaveInterval / 60f).ToString("0") + " minutes";
        GameTimeText.text = "every " + (GuiManagerScript.AutoSaveInterval / 365).ToString("0,0.#") + " years";
        if (GuiManagerScript.autoSaveMode == GuiManagerScript.AutoSaveMode.desactivate) {
            Mode.value = 0;
        }
        if (GuiManagerScript.autoSaveMode == GuiManagerScript.AutoSaveMode.OnRealWorldTime)
        {
            Mode.value = 1;
        }
        if (GuiManagerScript.autoSaveMode == GuiManagerScript.AutoSaveMode.OnGameTime)
        {
            Mode.value = 2;
        }
        if (GuiManagerScript.autoSaveMode == GuiManagerScript.AutoSaveMode.OnRealWorldOrGameTime)
        {
            Mode.value = 3;
        }
        if (GuiManagerScript.autoSaveMode == GuiManagerScript.AutoSaveMode.OnRealWorldAndGameTime)
        {
            Mode.value = 4;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ModeChange(int Index) {
        if (Index == 0) {
            GuiManagerScript.autoSaveMode = GuiManagerScript.AutoSaveMode.desactivate;
            RealTimeObj.SetActive(false);
            GameTimeObj.SetActive(false);
        }
        if (Index == 1)
        {
            GuiManagerScript.autoSaveMode = GuiManagerScript.AutoSaveMode.OnRealWorldTime;
            RealTimeObj.SetActive(true);
            GameTimeObj.SetActive(false);
        }
        if (Index == 2)
        {
            GuiManagerScript.autoSaveMode = GuiManagerScript.AutoSaveMode.OnGameTime;
            RealTimeObj.SetActive(false);
            GameTimeObj.SetActive(true);
        }
        if (Index == 3)
        {
            GuiManagerScript.autoSaveMode = GuiManagerScript.AutoSaveMode.OnRealWorldOrGameTime;
            RealTimeObj.SetActive(true);
            GameTimeObj.SetActive(true);
        }
        if (Index == 4)
        {
            GuiManagerScript.autoSaveMode = GuiManagerScript.AutoSaveMode.OnRealWorldAndGameTime;
            RealTimeObj.SetActive(true);
            GameTimeObj.SetActive(true);
        }
    }

    public void RealTimeChange(bool IsMinus) {
        float RealTimeInterval = GuiManagerScript.RealWorldAutoSaveInterval;
        // 1 min
        if (RealTimeInterval <= 60f) {
            if (IsMinus)
            {
                
            }
            else {
                GuiManagerScript.RealWorldAutoSaveInterval = 300f;
            }
        }
        //5 min
        else if (RealTimeInterval <= 300f) {
            if (IsMinus)
            {
                GuiManagerScript.RealWorldAutoSaveInterval = 60f;
            }
            else
            {
                GuiManagerScript.RealWorldAutoSaveInterval = 600f;
            }
        }
        //10 min
        else if (RealTimeInterval <= 600f) {
            if (IsMinus)
            {
                GuiManagerScript.RealWorldAutoSaveInterval = 300f;
            }
            else
            {
                GuiManagerScript.RealWorldAutoSaveInterval = 900f;
            }
        }
        //15 min
        else if (RealTimeInterval <= 900f) {
            if (IsMinus)
            {
                GuiManagerScript.RealWorldAutoSaveInterval = 600f;
            }
            else
            {
                GuiManagerScript.RealWorldAutoSaveInterval = 1800f;
            }
        }
        //30 min
        else if (RealTimeInterval <= 1800f) {
            if (IsMinus)
            {
                GuiManagerScript.RealWorldAutoSaveInterval = 900f;
            }
            else
            {
                GuiManagerScript.RealWorldAutoSaveInterval = 2700f;
            }
        }
        //45 min
        else if (RealTimeInterval <= 2700f) {
            if (IsMinus)
            {
                GuiManagerScript.RealWorldAutoSaveInterval = 1800f;
            }
            else
            {
                GuiManagerScript.RealWorldAutoSaveInterval = 3600f;
            }
        }
        //60 min - 1 hour
        else if (RealTimeInterval <= 3600f) {
            if (IsMinus)
            {
                GuiManagerScript.RealWorldAutoSaveInterval = 2700f;
            }
            else
            {
            }
        } 
        //more
        else {
            if (IsMinus)
            {
                GuiManagerScript.RealWorldAutoSaveInterval = 3600f;
            }
            else
            {
                
            }
        }

        Start();
    }

    public void GameTimeChange(bool IsMinus) {
        long GameTimeInterval = GuiManagerScript.AutoSaveInterval;
        //1 year
        if (GameTimeInterval <= 365)
        {
            if (IsMinus)
            {

            }
            else
            {
                GuiManagerScript.AutoSaveInterval = 3650;
            }
        }
        //10 year
        if (GameTimeInterval <= 3650)
        {
            if (IsMinus)
            {
                GuiManagerScript.AutoSaveInterval = 365;
            }
            else
            {
                GuiManagerScript.AutoSaveInterval = 36500;
            }
        }
        //100 year
        if (GameTimeInterval <= 36500)
        {
            if (IsMinus)
            {
                GuiManagerScript.AutoSaveInterval = 3650;
            }
            else
            {
                GuiManagerScript.AutoSaveInterval = 365000;
            }
        }
        //1 000 year
        if (GameTimeInterval <= 365000)
        {
            if (IsMinus)
            {
                GuiManagerScript.AutoSaveInterval = 36500;
            }
            else
            {
                GuiManagerScript.AutoSaveInterval = 3650000;
            }
        }
        //10 000 year
        if (GameTimeInterval <= 3650000)
        {
            if (IsMinus)
            {
                GuiManagerScript.AutoSaveInterval = 365000;
            }
            else
            {
                GuiManagerScript.AutoSaveInterval = 36500000;
            }
        }
        //100 000 year
        if (GameTimeInterval <= 36500000)
        {
            if (IsMinus)
            {
                GuiManagerScript.AutoSaveInterval = 3650000;
            }
            else
            {
                GuiManagerScript.AutoSaveInterval = 365000000;
            }
        }
        //1 000 000 year
        if (GameTimeInterval <= 365000000)
        {
            if (IsMinus)
            {
                GuiManagerScript.AutoSaveInterval = 36500000;
            }
            else
            {

            }
        }
        //more
        else {
            if (IsMinus)
            {
                GuiManagerScript.AutoSaveInterval = 365000000;
            }
        }

        Start();
    }
}
