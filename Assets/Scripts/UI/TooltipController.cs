using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
//Controls settings tooltip behavior.
public class TooltipController : MonoBehaviour
{
    public TextMeshProUGUI Header;
    public TextMeshProUGUI Body;
    
    public void DisplayTooltip(string header, string body, Vector3 position)
    {
        gameObject.SetActive(true);
        Header.text = header;
        Body.text = body;
        transform.position = position;
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }
}
