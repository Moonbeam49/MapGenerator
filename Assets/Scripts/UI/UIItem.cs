using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//Controls single settings item behavior.
public class UIItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string Name;
    [TextArea]
    public string Description;
    public UIController UIParent;

    bool mouseOver;
    bool checkStarted;
    Coroutine delayRoutine;

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseOver = true;
        if (checkStarted == false)
        {
            delayRoutine = StartCoroutine(waitForTooltip());
            checkStarted = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseOver = false;
        if (checkStarted == true)
        {
            StopCoroutine(delayRoutine);
            UIParent.HideTooltip();
            checkStarted = false;
        }
    }

    IEnumerator waitForTooltip()
    {
        yield return new WaitForSecondsRealtime(1f);
        if (mouseOver)
        {
            UIParent.DisplayTooltip(Name, Description, transform.position);
        }
    }
}
