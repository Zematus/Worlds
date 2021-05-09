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
        RealTimeText.text = "Every " + (Manager.RealWorldAutoSaveInterval / 60f).ToString("0") + " minutes";
        GameTimeText.text = "Every " + (Manager.AutoSaveInterval / 365).ToString("0,0.#") + " years";

        if (Manager.AutoSaveMode == AutoSaveMode.Deactivate)
        {
            Mode.value = 0;
        }
        if (Manager.AutoSaveMode == AutoSaveMode.OnRealWorldTime)
        {
            Mode.value = 1;
        }
        if (Manager.AutoSaveMode == AutoSaveMode.OnGameTime)
        {
            Mode.value = 2;
        }
        if (Manager.AutoSaveMode == AutoSaveMode.OnRealWorldOrGameTime)
        {
            Mode.value = 3;
        }
        if (Manager.AutoSaveMode == AutoSaveMode.OnRealWorldAndGameTime)
        {
            Mode.value = 4;
        }

        ModeChange(Mode.value);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ModeChange(int Index)
    {
        if (Index == 0)
        {
            Manager.AutoSaveMode = AutoSaveMode.Deactivate;
            RealTimeObj.SetActive(false);
            GameTimeObj.SetActive(false);
        }
        if (Index == 1)
        {
            Manager.AutoSaveMode = AutoSaveMode.OnRealWorldTime;
            RealTimeObj.SetActive(true);
            GameTimeObj.SetActive(false);
        }
        if (Index == 2)
        {
            Manager.AutoSaveMode = AutoSaveMode.OnGameTime;
            RealTimeObj.SetActive(false);
            GameTimeObj.SetActive(true);
        }
        if (Index == 3)
        {
            Manager.AutoSaveMode = AutoSaveMode.OnRealWorldOrGameTime;
            RealTimeObj.SetActive(true);
            GameTimeObj.SetActive(true);
        }
        if (Index == 4)
        {
            Manager.AutoSaveMode = AutoSaveMode.OnRealWorldAndGameTime;
            RealTimeObj.SetActive(true);
            GameTimeObj.SetActive(true);
        }
    }

    public void RealTimeChange(bool IsMinus)
    {
        float RealTimeInterval = Manager.RealWorldAutoSaveInterval;
        // 1 min
        if (RealTimeInterval <= 60f)
        {
            if (IsMinus)
            {

            }
            else
            {
                Manager.RealWorldAutoSaveInterval = 300f;
            }
        }
        //5 min
        else if (RealTimeInterval <= 300f)
        {
            if (IsMinus)
            {
                Manager.RealWorldAutoSaveInterval = 60f;
            }
            else
            {
                Manager.RealWorldAutoSaveInterval = 600f;
            }
        }
        //10 min
        else if (RealTimeInterval <= 600f)
        {
            if (IsMinus)
            {
                Manager.RealWorldAutoSaveInterval = 300f;
            }
            else
            {
                Manager.RealWorldAutoSaveInterval = 900f;
            }
        }
        //15 min
        else if (RealTimeInterval <= 900f)
        {
            if (IsMinus)
            {
                Manager.RealWorldAutoSaveInterval = 600f;
            }
            else
            {
                Manager.RealWorldAutoSaveInterval = 1800f;
            }
        }
        //30 min
        else if (RealTimeInterval <= 1800f)
        {
            if (IsMinus)
            {
                Manager.RealWorldAutoSaveInterval = 900f;
            }
            else
            {
                Manager.RealWorldAutoSaveInterval = 2700f;
            }
        }
        //45 min
        else if (RealTimeInterval <= 2700f)
        {
            if (IsMinus)
            {
                Manager.RealWorldAutoSaveInterval = 1800f;
            }
            else
            {
                Manager.RealWorldAutoSaveInterval = 3600f;
            }
        }
        //60 min - 1 hour
        else if (RealTimeInterval <= 3600f)
        {
            if (IsMinus)
            {
                Manager.RealWorldAutoSaveInterval = 2700f;
            }
            else
            {
            }
        }
        //more
        else
        {
            if (IsMinus)
            {
                Manager.RealWorldAutoSaveInterval = 3600f;
            }
            else
            {

            }
        }

        Start();
    }

    public void GameTimeChange(bool IsMinus)
    {
        long GameTimeInterval = Manager.AutoSaveInterval;
        //1 year
        if (GameTimeInterval <= 365)
        {
            if (IsMinus)
            {

            }
            else
            {
                Manager.AutoSaveInterval = 3650;
            }
        }
        //10 year
        if (GameTimeInterval <= 3650)
        {
            if (IsMinus)
            {
                Manager.AutoSaveInterval = 365;
            }
            else
            {
                Manager.AutoSaveInterval = 36500;
            }
        }
        //100 year
        if (GameTimeInterval <= 36500)
        {
            if (IsMinus)
            {
                Manager.AutoSaveInterval = 3650;
            }
            else
            {
                Manager.AutoSaveInterval = 365000;
            }
        }
        //1 000 year
        if (GameTimeInterval <= 365000)
        {
            if (IsMinus)
            {
                Manager.AutoSaveInterval = 36500;
            }
            else
            {
                Manager.AutoSaveInterval = 3650000;
            }
        }
        //10 000 year
        if (GameTimeInterval <= 3650000)
        {
            if (IsMinus)
            {
                Manager.AutoSaveInterval = 365000;
            }
            else
            {
                Manager.AutoSaveInterval = 36500000;
            }
        }
        //100 000 year
        if (GameTimeInterval <= 36500000)
        {
            if (IsMinus)
            {
                Manager.AutoSaveInterval = 3650000;
            }
            else
            {
                Manager.AutoSaveInterval = 365000000;
            }
        }
        //1 000 000 year
        if (GameTimeInterval <= 365000000)
        {
            if (IsMinus)
            {
                Manager.AutoSaveInterval = 36500000;
            }
            else
            {

            }
        }
        //more
        else
        {
            if (IsMinus)
            {
                Manager.AutoSaveInterval = 365000000;
            }
        }

        Start();
    }
}
