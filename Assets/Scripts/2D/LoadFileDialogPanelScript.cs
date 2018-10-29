using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class LoadFileDialogPanelScript : DialogPanelScript
{
    public Button WorldNameButtonPrefab;

    public Button CancelActionButton;

    public Transform ActionButtonPanelTransform;

    public UnityEvent LoadButtonClickEvent;

    private List<Button> _worldNameButtons = new List<Button>();

    private string _pathToLoad;

    public string GetPathToLoad()
    {
        return _pathToLoad;
    }

    private void LoadFileNames()
    {
        _worldNameButtons.Add(WorldNameButtonPrefab);

        string dirPath = Manager.SavePath;

        string[] files = Directory.GetFiles(dirPath, "*.PLNT");

        int i = 0;

        foreach (string file in files)
        {
            string name = Path.GetFileNameWithoutExtension(file);

            SetWorldNameButton(name, i);

            i++;
        }
    }

    private void SetWorldNameButton(string name, int index)
    {
        Button button;

        if (index < _worldNameButtons.Count)
        {
            button = _worldNameButtons[index];
            button.GetComponentInChildren<Text>().text = name;
        }
        else
        {
            button = AddWorldNameButton(name);
        }

        button.onClick.RemoveAllListeners();

        string path = Manager.SavePath + name + ".PLNT";

        button.onClick.AddListener(() =>
        {
            _pathToLoad = path;
            LoadButtonClickEvent.Invoke();
        });
    }

    private Button AddWorldNameButton(string name)
    {
        Button newButton = Instantiate(WorldNameButtonPrefab) as Button;

        newButton.transform.SetParent(transform, false);
        newButton.GetComponentInChildren<Text>().text = name;

        _worldNameButtons.Add(newButton);

        ActionButtonPanelTransform.SetAsLastSibling();

        return newButton;
    }

    private void RemoveWorldNameButtons()
    {
        bool first = true;

        foreach (Button button in _worldNameButtons)
        {
            if (first)
            {
                first = false;
                continue;
            }

            GameObject.Destroy(button.gameObject);
        }

        _worldNameButtons.Clear();
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
            RemoveWorldNameButtons();
        }
    }
}
