using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TooltipHandlerScript : MonoBehaviour
{
    public GameObject Tooltip;

    private GameObject _relativeObject = null;
    private bool _mirroredPosition = false;

    private const float _timeToSpawn = 2;
    private float _timeSinceSet = 0;

    private bool _setToShow = false;

    // Update is called once per frame
    void Update()
    {
        if (Tooltip.activeInHierarchy)
            return;

        if (!_setToShow)
            return;

        _timeSinceSet += Time.deltaTime;

        if (_timeSinceSet >= _timeToSpawn)
        {
            SetPosition();
            Tooltip.SetActive(true);
        }
    }

    public void SetToShow(bool state)
    {
        _setToShow = state;

        Tooltip.SetActive(false);

        if (state)
            _timeSinceSet = 0;
    }

    public void SetText(string text)
    {
        Text tooltipText = Tooltip.GetComponentInChildren<Text>();
        tooltipText.text = text;
    }

    public void SetPositionRelativeTo(GameObject obj)
    {
        _relativeObject = obj;
        _mirroredPosition = false;
    }

    public void SetPositionRelativeTo_Mirror(GameObject obj)
    {
        _relativeObject = obj;
        _mirroredPosition = true;
    }

    private void SetPosition()
    {
        RectTransform rectTransform = _relativeObject.GetComponent<RectTransform>();
        RectTransform rectTransformTooltip = Tooltip.GetComponent<RectTransform>();

        Vector3 position = _relativeObject.transform.position;
        position.x += rectTransform.rect.center.x - (_mirroredPosition ? rectTransformTooltip.rect.width : 0);
        position.y += rectTransform.rect.center.y;

        Tooltip.transform.position = position;
    }
}
