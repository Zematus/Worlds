using UnityEngine;
using System.Collections;

public class PlanetScript : MonoBehaviour {

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate(Vector3.up * Time.deltaTime * 10);
	}

	public void RefreshTexture () {
		
		Texture2D texture = Manager.CurrentSphereTexture;
		
		GetComponent<Renderer>().material.mainTexture = texture;
	}
}
