using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class StartGuiManagerScript : MonoBehaviour {
	
	public Button LoadButton;

	public LoadFileDialogPanelScript LoadFileDialogPanelScript;
	public DialogPanelScript MainMenuDialogPanelScript;

	// Use this for initialization
	void Start () {

		SetEnabledModalLoadDialog (false);
		SetEnabledModalMainMenuDialog (true);
		
		LoadButton.interactable = HasFilesToLoad ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	private bool HasFilesToLoad () {
		
		string dirPath = Manager.SavePath;
		
		string[] files = Directory.GetFiles (dirPath, "*.PLNT");
		
		return files.Length > 0;
	}
	
	private void SetEnabledModalLoadDialog (bool value) {
		
		LoadFileDialogPanelScript.SetVisible (value);
	}
	
	private void SetEnabledModalMainMenuDialog (bool value) {
		
		MainMenuDialogPanelScript.SetVisible (value);
	}
	
	public void LoadWorld () {
		
		SetEnabledModalMainMenuDialog (false);
		
		SetEnabledModalLoadDialog (true);
		
		LoadFileDialogPanelScript.SetLoadAction (LoadAction);
	}
	
	public void LoadAction () {
		
		string path = LoadFileDialogPanelScript.GetPathToLoad ();

		Application.LoadLevel ("WorldView");
	}
	
	public void GenerateWorld () {
		
		Application.LoadLevel ("WorldView");
	}
}
