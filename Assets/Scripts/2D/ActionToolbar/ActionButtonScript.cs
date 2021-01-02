using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ActionButtonScript : MonoBehaviour
{
    public Text Text;

    public void SetText(string text)
    {
        Text.text = text;
    }

    public void Remove()
    {
        gameObject.SetActive(false);

        Destroy(gameObject);
    }
}
