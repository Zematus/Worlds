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

    public Text NoFilesText;
    public Text SelectButtonText;

    public Button SelectButton;
    public Button CancelButton;

    public ToggleGroup ToggleGroup;

    private List<Toggle> _fileToggles = new List<Toggle>();

    private string _basePath;
    private HashSet<string> _pathsToLoad = new HashSet<string>();
    private string _pathToLoad = null;

    private string[] _validExtensions;

    private bool _loadDirectory;
    private bool _selectMultiple;

    private HashSet<string> _prevSelectedItems = new HashSet<string>();
    
    // Update is called once per frame
    void Update()
    {
        ReadKeyboardInput();
    }

    private void ReadKeyboardInput()
    {
        Manager.HandleKeyUp(KeyCode.Escape, false, false, CancelButton.onClick.Invoke);
    }

    public string GetPathToLoad()
    {
        return _pathToLoad;
    }

    public ICollection<string> GetPathsToLoad()
    {
        return _pathsToLoad;
    }

    public void Hide()
    {
        SetVisible(false);
    }

    public void Initialize(
        string dialogText, 
        string selectButtonText, 
        UnityAction selectAction,
        UnityAction cancelAction,
        string basePath, 
        string[] validExtensions = null,
        bool loadDirectory = false,
        bool selectMultiple = false,
        ICollection<string> prevSelectedItems = null)
    {
        _pathsToLoad.Clear();

        SetDialogText(dialogText);

        SelectButtonText.text = selectButtonText;

        SelectButton.onClick.RemoveAllListeners();
        CancelButton.onClick.RemoveAllListeners();

        SelectButton.onClick.AddListener(Hide);
        SelectButton.onClick.AddListener(selectAction);
        CancelButton.onClick.AddListener(Hide);
        CancelButton.onClick.AddListener(cancelAction);

        SelectButton.interactable = false;

        _basePath = basePath;
        _validExtensions = validExtensions;

        _loadDirectory = loadDirectory;
        _selectMultiple = selectMultiple;

        _prevSelectedItems.Clear();

        if (prevSelectedItems != null)
        {
            foreach (string path in prevSelectedItems)
            {
                _prevSelectedItems.Add(Path.GetFileName(path));
            }
        }
    }

    private void LoadFileNames()
    {
        FileListPanel.SetActive(true);
        NoFilesText.gameObject.SetActive(false);

        _fileToggles.Add(TogglePrefab);

        string[] files = Directory.GetFiles(_basePath);

        int i = 0;

        foreach (string file in files)
        {
            string ext = Path.GetExtension(file).ToUpper();

            if (_validExtensions != null)
            {
                bool found = false;
                foreach (string validExt in _validExtensions)
                {
                    found |= ext == validExt.ToUpper();
                }

                if (!found) continue;
            }

            string name = Path.GetFileName(file);

            SetFileToggle(name, i, _prevSelectedItems.Contains(name));

            i++;
        }

        if (i == 0)
        {
            FileListPanel.SetActive(false);

            string extTypes = string.Join(",", _validExtensions);

            NoFilesText.text = "No files of type {" + extTypes + "} found...";
            NoFilesText.gameObject.SetActive(true);
        }
    }

    private void LoadDirectoryNames()
    {
        FileListPanel.SetActive(true);
        NoFilesText.gameObject.SetActive(false);

        _fileToggles.Add(TogglePrefab);

        string[] directories = Directory.GetDirectories(_basePath);

        int i = 0;

        foreach (string directory in directories)
        {
            string name = Path.GetFileName(directory);

            SetFileToggle(name, i, _prevSelectedItems.Contains(name));

            i++;
        }

        if (i == 0)
        {
            FileListPanel.SetActive(false);
            
            NoFilesText.text = "No directories found...";
            NoFilesText.gameObject.SetActive(true);
        }
    }

    private void SetFileToggle(string name, int index, bool alreadySelected)
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

        toggle.group = _selectMultiple? null : ToggleGroup;

        string path = _basePath + name;

        toggle.onValueChanged.AddListener(value =>
        {
            if (!_selectMultiple)
                _pathsToLoad.Clear();

            if (value)
            {
                _pathsToLoad.Add(path);
                _pathToLoad = path;
            }
            else
            {
                _pathsToLoad.Remove(path);
                _pathToLoad = null;
            }

            SelectButton.interactable = _pathsToLoad.Count > 0;
        });

        toggle.isOn = alreadySelected;
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
            toggle.onValueChanged.RemoveAllListeners();
            toggle.isOn = false;

            if (first)
            {
                first = false;
                continue;
            }

            Destroy(toggle.gameObject);
        }

        _fileToggles.Clear();
    }

    public override void SetVisible(bool state)
    {
        if (state && GuiManagerScript.IsModalPanelActive())
            return; // Can't have more than one menu panel active at a time

        base.SetVisible(state);

        if (state)
        {
            if (_loadDirectory)
                LoadDirectoryNames();
            else
                LoadFileNames();
        }
        else
        {
            ResetToggles();
        }
    }
}
