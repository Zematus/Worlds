using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public abstract class ModalPanelScript : MonoBehaviour {

	public CanvasGroup ModalPanelCanvasGroup;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public virtual void SetVisible (bool value) {
		
		ModalPanelCanvasGroup.gameObject.SetActive (value);
		ModalPanelCanvasGroup.blocksRaycasts = value;
	
		gameObject.SetActive (value);
	}
}
