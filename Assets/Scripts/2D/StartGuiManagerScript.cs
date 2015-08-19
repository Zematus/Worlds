using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class StartGuiManagerScript : MonoBehaviour {
	
	public Button LoadButton;

	public LoadFileDialogPanelScript LoadFileDialogPanelScript;
	public DialogPanelScript MainMenuDialogPanelScript;
	public ProgressDialogPanelScript ProgressDialogPanelScript;
	public TextInputDialogPanelScript SetSeedDialogPanelScript;
	public TextInputDialogPanelScript MessageDialogPanelScript;
	
	private bool _preparingWorld = false;
	
	private PostPreparationOperation _postPreparationOp = null;

	// Use this for initialization
	void Start () {

		Manager.UpdateMainThreadReference ();

		SetEnabledModalLoadDialog (false);
		SetEnabledModalProgressDialog (false);
		SetSeedDialogPanelScript.SetVisible (false);
		MessageDialogPanelScript.SetVisible (false);
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
	
	public void CancelLoadAction () {
		
		SetEnabledModalLoadDialog (false);
		
		SetEnabledModalMainMenuDialog (true);
	}
	
	public void SetGenerationSeed () {
		
		MainMenuDialogPanelScript.SetVisible (false);
		
		int seed = Random.Range (0, int.MaxValue);
		
		SetSeedDialogPanelScript.SetName (seed.ToString());
		
		SetSeedDialogPanelScript.SetVisible (true);
		
	}
	
	public void CancelGenerateAction () {
		
		SetSeedDialogPanelScript.SetVisible (false);
	}
	
	public void CloseSeedErrorMessageAction () {
		
		MessageDialogPanelScript.SetVisible (false);
		
		SetGenerationSeed ();
	}
	
	public void GenerateWorld (bool getSeedInput = true) {
		
		SetSeedDialogPanelScript.SetVisible (false);
		
		int seed = Random.Range (0, int.MaxValue);
		
		if (getSeedInput) {
			string seedStr = SetSeedDialogPanelScript.GetName ();
			
			if (!int.TryParse (seedStr, out seed)) {
				
				MessageDialogPanelScript.SetVisible (true);
				return;
			}
			
			if (seed < 0) {
				
				MessageDialogPanelScript.SetVisible (true);
				return;
			}
		}
		
		SetEnabledModalProgressDialog (true);
		
		ProgressUpdate (0, "Generating World...");
		
		_preparingWorld = true;
		
		Manager.GenerateNewWorldAsync (0, ProgressUpdate);
		
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
	
	public void Exit () {
		
		Application.Quit();
	}
}
