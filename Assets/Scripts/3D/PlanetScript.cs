using UnityEngine;
using System.Collections;

public class PlanetScript : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * Time.deltaTime * 10);
    }

    public void RefreshTexture()
    {
        Texture2D texture = Manager.CurrentMapTexture;

        GetComponent<Renderer>().material.mainTexture = texture;
    }
}
