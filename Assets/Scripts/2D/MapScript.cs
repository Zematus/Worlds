using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MapScript : MonoBehaviour {

	public RawImage Image;

	// Use this for initialization
	void Start () {
		
		Texture2D texture = Manager.CurrentTexture;

		Image.texture = texture;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
