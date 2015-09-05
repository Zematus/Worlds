using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class StartGuiManagerScript : MonoBehaviour {
	
	public Button LoadButton;

	public LoadFileDialogPanelScript LoadFileDialogPanelScript;
	public DialogPanelScript MainMenuDialogPanelScript;
	public ProgressDialogPanelScript ProgressDialogPanelScript;
	public WorldCustomizationDialogPanelScript SetSeedDialogPanelScript;
	public TextInputDialogPanelScript MessageDialogPanelScript;
	public WorldCustomizationDialogPanelScript CustomizeWorldDialogPanelScript;
	
	private bool _preparingWorld = false;
	
	private PostPreparationOperation _postPreparationOp = null;

	// Use this for initialization
	void Start () {

		Manager.UpdateMainThreadReference ();

		LoadFileDialogPanelScript.SetVisible (false);
		ProgressDialogPanelScript.SetVisible (false);
		SetSeedDialogPanelScript.SetVisible (false);
		MessageDialogPanelScript.SetVisible (false);
		CustomizeWorldDialogPanelScript.SetVisible (false);
		MainMenuDialogPanelScript.SetVisible (true);
		
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
	
	public void LoadWorld () {
		
		MainMenuDialogPanelScript.SetVisible (false);
		
		LoadFileDialogPanelScript.SetVisible (true);
		
		LoadFileDialogPanelScript.SetLoadAction (LoadAction);
	}
	
	public void LoadAction () {
		
		LoadFileDialogPanelScript.SetVisible (false);
		
		ProgressDialogPanelScript.SetVisible (true);
		
		ProgressUpdate (0, "Loading World...");
		
		string path = LoadFileDialogPanelScript.GetPathToLoad ();
		
		Manager.LoadWorldAsync (path, ProgressUpdate);
		
		Manager.WorldName = Path.GetFileNameWithoutExtension (path);
		
		_preparingWorld = true;
	}
	
	public void CancelLoadAction () {
		
		LoadFileDialogPanelScript.SetVisible (false);
		
		MainMenuDialogPanelScript.SetVisible (true);
	}
	
	public void SetGenerationSeed () {
		
		MainMenuDialogPanelScript.SetVisible (false);
		
		int seed = Random.Range (0, int.MaxValue);
		
		SetSeedDialogPanelScript.SetSeedStr (seed.ToString());
		
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
			string seedStr = SetSeedDialogPanelScript.GetSeedString ();
			
			if (!int.TryParse (seedStr, out seed)) {
				
				MessageDialogPanelScript.SetVisible (true);
				return;
			}
			
			if (seed < 0) {
				
				MessageDialogPanelScript.SetVisible (true);
				return;
			}
		}
		
		ProgressDialogPanelScript.SetVisible (true);
		
		ProgressUpdate (0, "Generating World...");
		
		_preparingWorld = true;
		
		Manager.GenerateNewWorldAsync (seed, ProgressUpdate);
		
		_postPreparationOp = () => {
			
			Manager.WorldName = "world_" + Manager.CurrentWorld.Seed;
			
			_postPreparationOp = null;
		};
	}
	
	public void CustomizeGeneration () {
		
		SetSeedDialogPanelScript.SetVisible (false);
		
		int seed = 0;

		string seedStr = SetSeedDialogPanelScript.GetSeedString ();
		
		if (!int.TryParse (seedStr, out seed)) {
			
			MessageDialogPanelScript.SetVisible (true);
			return;
		}
		
		if (seed < 0) {
			
			MessageDialogPanelScript.SetVisible (true);
			return;
		}
		
		CustomizeWorldDialogPanelScript.SetVisible (true);
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
