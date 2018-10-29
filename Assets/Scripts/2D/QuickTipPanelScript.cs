using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class QuickTipPanelScript : MonoBehaviour
{
    public CanvasGroup CanvasGroup;

    public Text Text;

    private float _fadeStart = 10f;
    private float _fadespeed = 0.25f;

    private float _timeSpanned = 0;

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
            gameObject.SetActive(false);
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

    public void SetVisible(bool value)
    {
        gameObject.SetActive(value);
    }

    public void OnClick()
    {
        SetVisible(false);
    }
}
