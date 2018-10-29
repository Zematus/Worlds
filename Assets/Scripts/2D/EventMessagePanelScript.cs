using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public delegate void EventMessageGotoDelegate();

public class EventMessagePanelScript : MonoBehaviour
{
    public CanvasGroup CanvasGroup;

    public Button GotoButton;

    public Text Text;

    private float _fadeStart = 10f;
    private float _fadespeed = 0.25f;

    private float _timeSpanned = 0;

    private EventMessageGotoDelegate _gotoDelegate = null;

    // Use this for initialization
    void Start()
    {
        GotoButton.gameObject.SetActive(_gotoDelegate != null);
    }

    // Update is called once per frame
    void Update()
    {
        if (CanvasGroup.alpha == 0)
            return;

        _timeSpanned += Time.deltaTime;

        if (_fadeStart > _timeSpanned)
            return;

        float alpha = Mathf.Lerp(1, 0, _fadespeed * (_timeSpanned - _fadeStart));

        CanvasGroup.alpha = alpha;

        if (CanvasGroup.alpha == 0)
        {
            gameObject.SetActive(false);

            Destroy(gameObject);
        }
    }

    public void SetText(string text)
    {
        Text.text = text;
    }

    public void Reset(float fadeStart)
    {
        _fadeStart = fadeStart;
        _timeSpanned = 0;

        CanvasGroup.alpha = 1;
    }

    public void SetGotoDelegate(EventMessageGotoDelegate gotoDelegate)
    {
        _gotoDelegate = gotoDelegate;

        GotoButton.gameObject.SetActive(gotoDelegate != null);
    }

    public void OnClick()
    {
        gameObject.SetActive(false);

        Destroy(gameObject);
    }

    public void OnGotoButtonClick()
    {
        if (_gotoDelegate != null)
        {
            _gotoDelegate();
        }

        OnClick();
    }
}
