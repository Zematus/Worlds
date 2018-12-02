using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class LoadFileDialogPanelScript : DialogPanelScript
{
    public GameObject FileListPanel;
    public Toggle TogglePrefab;

    public Button CancelActionButton;

    public UnityEvent LoadButtonClickEvent;

    private List<Toggle> _fileToggles = new List<Toggle>();

    private string _pathToLoad;

    private string[] _validExtensions;

    public string GetPathToLoad()
    {
        return _pathToLoad;
    }

    public void Initialize(string[] validExtensions)
    {
        _validExtensions = validExtensions;
    }

    private void LoadFileNames()
    {
        _fileToggles.Add(TogglePrefab);

        string dirPath = Manager.SavePath;

        string[] files = Directory.GetFiles(dirPath);

        int i = 0;

        foreach (string file in files)
        {
            string ext = Path.GetExtension(file).ToUpper();

            bool found = false;
            foreach (string validExt in _validExtensions)
            {
                found |= ext.Contains(validExt);
            }

            if (!found) continue;

            string name = Path.GetFileName(file);

            SetFileToggle(name, i);

            i++;
        }
    }

    private void SetFileToggle(string name, int index)
    {
        Toggle toggle;

        if (index < _fileToggles.Count)
        {
            toggle = _fileToggles[index];
            toggle.GetComponentInChildren<Text>().text = name;
        }
        else
        {
            toggle = AddFileToggle(name);
        }

        toggle.onValueChanged.RemoveAllListeners();

        string path = Manager.SavePath + name;

        toggle.onValueChanged.AddListener(value =>
        {
            if (value)
            {
                _pathToLoad = path;
                //LoadButtonClickEvent.Invoke();
            }
        });
    }

    private Toggle AddFileToggle(string name)
    {
        Toggle newToggle = Instantiate(TogglePrefab) as Toggle;

        newToggle.transform.SetParent(FileListPanel.transform, false);
        newToggle.GetComponentInChildren<Text>().text = name;

        _fileToggles.Add(newToggle);

        return newToggle;
    }

    private void ResetToggles()
    {
        bool first = true;

        foreach (Toggle toggle in _fileToggles)
        {
            if (first)
            {
                first = false;
                continue;
            }

            GameObject.Destroy(toggle.gameObject);
        }

        _fileToggles.Clear();
    }

    public override void SetVisible(bool value)
    {
        base.SetVisible(value);

        if (value)
        {
            LoadFileNames();
        }
        else
        {
            ResetToggles();
        }
    }
}
