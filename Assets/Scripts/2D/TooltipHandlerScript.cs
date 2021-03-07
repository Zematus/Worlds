using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TooltipHandlerScript : MonoBehaviour
{
    public GameObject Tooltip;

    private GameObject _relativeObject = null;

    private const float _timeToSpawn = 1f;
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
            Tooltip.SetActive(true);
            SetPosition();
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
    }

    private void SetPosition()
    {
        RectTransform rectTransform = _relativeObject.GetComponent<RectTransform>();

        Vector3 position = _relativeObject.transform.position;
        position.x += rectTransform.rect.center.x;
        position.y += rectTransform.rect.center.y;

        Tooltip.transform.position = position;
    }
}
