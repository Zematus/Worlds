using UnityEngine;
using System.Collections;

public class PlanetScript : MonoBehaviour {

	// Use this for initialization
	void Start () {

		World world = Manager.GenerateNewWorld();

		Texture2D texture = GenerateTextureFromWorld(world);

		GetComponent<Renderer>().material.mainTexture = texture;
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate(Vector3.up * Time.deltaTime * 10);
	}

	private Texture2D GenerateTestSurfaceTexture () {

		int sizeX = 200;
		int sizeY = 100;
		
		Texture2D texture = new Texture2D(sizeX, sizeY, TextureFormat.ARGB32, false);

		Color[] colors = {Color.white, Color.black, Color.red, Color.green, Color.blue};

		int colorIndex = 0;

		for (int i = 0; i < sizeX; i++)
		{
			for (int j = 0; j < sizeY; j++)
			{
				texture.SetPixel(i, j, colors[colorIndex]);

				colorIndex++;
				colorIndex = (colorIndex >= colors.Length) ? 0 : colorIndex;
			}
			
			colorIndex++;
			colorIndex = (colorIndex >= colors.Length) ? 0 : colorIndex;
		}
		
		texture.Apply();

		return texture;
	}
	
	public static Texture2D GenerateTextureFromWorld (World world) {
		
		int sizeX = world.Width;
		int sizeY = world.Height;
		
		Texture2D texture = new Texture2D(sizeX, sizeY, TextureFormat.ARGB32, false);
		
		for (int i = 0; i < sizeX; i++)
		{
			for (int j = 0; j < sizeY; j++)
			{
				float altitude = world.terrain[i][j].Altitude;
				
				texture.SetPixel(i, j, GetAltitudeColorFromAltitude(altitude));
			}
		}
		
		texture.Apply();
		
		return texture;
	}

	private static Color GetGreyScaleFromNoise (float value) {

		return new Color(value,value,value);
	}
	
	private static Color GetAltitudeColorFromAltitude (float value) {

		if (value < 0.5f) return Color.blue;

		Color brown = new Color(0.58f, 0.29f, 0);

		return new Color(brown.r * value, brown.g * value, brown.b * value);
	}
}
