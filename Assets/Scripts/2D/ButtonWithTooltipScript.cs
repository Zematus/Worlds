using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonWithTooltipScript : MonoBehaviour {

	public Text ButtonText;
	public Text TooltipText;

	public GameObject TooltipPanel;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void PointerEnterHandler () {
	
		TooltipPanel.gameObject.SetActive (true);
	}

	public void PointerExitHandler () {

		TooltipPanel.gameObject.SetActive (false);
	}
}
