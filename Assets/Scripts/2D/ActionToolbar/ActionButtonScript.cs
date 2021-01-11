using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ActionButtonScript : MonoBehaviour
{
    public Text Text;

    private Action _action;

    public void SetAction(Action action)
    {
        Text.text = action.Name;

        _action = action;
    }

    public void Remove()
    {
        gameObject.SetActive(false);

        Destroy(gameObject);
    }
}
