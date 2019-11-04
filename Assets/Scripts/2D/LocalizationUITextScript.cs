using UnityEngine;
using UnityEngine.UI;

public class LocalizationUITextScript : MonoBehaviour
{
    public string key;

    void Start()
    {
        GetComponent<Text>().text = LocalizationManagerScript.Instance.GetText(key);

        LocalizationManagerScript.AddInstance(this);
    }

    public void UpdateText()
    {
        GetComponent<Text>().text = LocalizationManagerScript.Instance.GetText(key);
    }
}
