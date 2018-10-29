using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusedPolityListPanelScript : MonoBehaviour
{
    public FocusedPolityPanelScript FocusedPolityPanelPrefab;

    private List<FocusedPolityPanelScript> _activePanels = new List<FocusedPolityPanelScript>();

    private List<FocusedPolityPanelScript> _panelsToremove = new List<FocusedPolityPanelScript>();
    private List<Polity> _remainingPolities = new List<Polity>();

    // Use this for initialization
    void Start()
    {
        FocusedPolityPanelPrefab.SetVisible(false);
    }

    // Update is called once per frame
    void Update()
    {
        _panelsToremove.Clear();
        _remainingPolities.Clear();

        if (Manager.WorldIsReady)
        {
            _remainingPolities.AddRange(Manager.CurrentWorld.PolitiesUnderPlayerFocus);
        }

        foreach (FocusedPolityPanelScript panel in _activePanels)
        {
            if (!_remainingPolities.Contains(panel.Polity))
            {
                _panelsToremove.Add(panel);
            }
            else
            {
                _remainingPolities.Remove(panel.Polity);
            }
        }

        foreach (FocusedPolityPanelScript panel in _panelsToremove)
        {
            RemoveFocusedPolityPanel(panel);
        }

        foreach (Polity polity in _remainingPolities)
        {
            AddFocusedPolity(polity);
        }
    }

    public void AddFocusedPolity(Polity polity)
    {
        FocusedPolityPanelScript focusedPolityPanel = GameObject.Instantiate(FocusedPolityPanelPrefab) as FocusedPolityPanelScript;

        focusedPolityPanel.Set(polity);
        focusedPolityPanel.transform.SetParent(transform);

        focusedPolityPanel.SetVisible(true);

        _activePanels.Add(focusedPolityPanel);
    }

    public void RemoveFocusedPolityPanel(FocusedPolityPanelScript panel)
    {
        panel.SetVisible(false);
        GameObject.Destroy(panel);

        _activePanels.Remove(panel);
    }
}
