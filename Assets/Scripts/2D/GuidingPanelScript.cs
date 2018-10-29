using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuidingPanelScript : MonoBehaviour
{
    public Text FactionText;

    public Button StopGuidingButton;

	public void SetVisible(bool value)
    {
        gameObject.SetActive(value);
    }

    public void SetState(Faction faction)
    {
        FactionText.text = faction.Name.Text + " " + faction.Type;
    }
}
