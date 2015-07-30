using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class StartGuiManagerScript : MonoBehaviour {
	
	public Button LoadButton;

	public LoadFileDialogPanelScript LoadFileDialogPanelScript;
	public DialogPanelScript MainMenuDialogPanelScript;
	public ProgressDialogPanelScript ProgressDialogPanelScript;
	
	private bool _preparingWorld = false;
	
	private PostPreparationOperation _postPreparationOp = null;

	// Use this for initialization
	void Start () {

		SetEnabledModalLoadDialog (false);
		SetEnabledModalProgressDialog (false);
		SetEnabledModalMainMenuDialog (true);
		
		LoadButton.interactable = HasFilesToLoad ();
	}
	
	// Update is called once per frame
	void Update () {
		
		Manager.ExecuteTasks (100);
		
		if (!Manager.WorldReady) {
			return;
		}
		
		if (_preparingWorld) {
			
			if (_postPreparationOp != null) 
				_postPreparationOp ();

			_preparingWorld = false;
			
			Application.LoadLevel ("WorldView");
		}
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
	
	private void SetEnabledModalProgressDialog (bool value) {
		
		ProgressDialogPanelScript.SetVisible (value);
	}
	
	public void LoadWorld () {
		
		SetEnabledModalMainMenuDialog (false);
		
		SetEnabledModalLoadDialog (true);
		
		LoadFileDialogPanelScript.SetLoadAction (LoadAction);
	}
	
	public void LoadAction () {
		
		SetEnabledModalLoadDialog (false);
		
		SetEnabledModalProgressDialog (true);
		
		ProgressUpdate (0, "Loading World...");
		
		string path = LoadFileDialogPanelScript.GetPathToLoad ();
		
		Manager.LoadWorldAsync (path, ProgressUpdate);
		
		Manager.WorldName = Path.GetFileNameWithoutExtension (path);
		
		_preparingWorld = true;
	}
	
	public void GenerateWorld () {
		
		SetEnabledModalMainMenuDialog (false);
		
		SetEnabledModalProgressDialog (true);
		
		ProgressUpdate (0, "Generating World...");
		
		_preparingWorld = true;
		
		Manager.GenerateNewWorldAsync (ProgressUpdate);
		
		_postPreparationOp = () => {
			
			Manager.WorldName = "world_" + Manager.CurrentWorld.Seed;
			
			_postPreparationOp = null;
		};
	}
	
	public void ProgressUpdate (float value, string message = null) {
		
		Manager.EnqueueTask (() => {
			
			if (message != null) ProgressDialogPanelScript.SetDialogText (message);
			
			ProgressDialogPanelScript.SetProgress (value);
			
			return true;
		});
	}
}
