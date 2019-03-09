using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class SelectionPanelScript : MonoBehaviour
{
    public Text Title;

    public Toggle PrototypeToggle;

    public LinkedListNode<Toggle> _toggledNode = null;

    public Dictionary<string, Toggle> Toggles = new Dictionary<string, Toggle>();

    public ToggleGroup ToggleGroup;

    private LinkedList<Toggle> _linkedToggles = new LinkedList<Toggle>();

    // Use this for initialization
    void Start()
    {
        PrototypeToggle.gameObject.SetActive(false);
    }

    void Update()
    {
        ReadKeyboardInput();
    }

    public void ReadKeyboardInput()
    {
        if (Input.GetKeyUp(KeyCode.Tab) && (_linkedToggles.Count > 0))
        {
            if (_toggledNode == null)
            {
                _toggledNode = _linkedToggles.First;
            }
            else
            {
                _toggledNode = _toggledNode.Next;

                if (_toggledNode == null)
                {
                    _toggledNode = _linkedToggles.First;
                }
            }

            _toggledNode.Value.isOn = true;
        }
    }

    public void SetVisible(bool value)
    {
        gameObject.SetActive(value);
    }

    public bool IsVisible()
    {
        return gameObject.activeInHierarchy;
    }

    public void AddOption(string id, string text, UnityAction<bool> valueChangedHandler)
    {
        Toggle toggle = null;

        if (Toggles.TryGetValue(id, out toggle))
        {
            return;
        }

        toggle = GameObject.Instantiate(PrototypeToggle) as Toggle;

        _linkedToggles.AddLast(toggle);

        LinkedListNode<Toggle> toggleNode = _linkedToggles.Last;

        Toggles.Add(id, toggle);

        UnityAction<bool> toggleHandler = state => 
        {
            if (state)
            {
                _toggledNode = toggleNode;
            }
            else if (_toggledNode == toggleNode)
            {
                _toggledNode = null;
            }
        };

        toggle.onValueChanged.AddListener(valueChangedHandler);
        toggle.onValueChanged.AddListener(toggleHandler);

        toggle.transform.SetParent(gameObject.transform);

        SelectionToggleScript toggleScript = toggle.gameObject.GetComponent<SelectionToggleScript>();
        toggleScript.Label.text = text;

        toggle.gameObject.SetActive(true);
    }

    public void SetStateOption(string id, bool state)
    {
        Toggle toggle = null;

        if (Toggles.TryGetValue(id, out toggle))
        {
            toggle.isOn = state;
        }
    }

    public void RemoveAllOptions()
    {
        foreach (Toggle toggle in Toggles.Values)
        {
            toggle.gameObject.SetActive(false);
            toggle.transform.SetParent(null);

            GameObject.Destroy(toggle);
        }

        Toggles.Clear();
        _linkedToggles.Clear();

        _toggledNode = null;
    }
}
