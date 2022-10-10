using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

//Controls single item behavior in files list.
public class FileItem : MonoBehaviour, IPointerClickHandler
{
    public FilePanel PanelParent;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Location;
    public TextMeshProUGUI Time;
    public Image MyPanel;
    public int Id;

    Color myColor;
    
    void Awake()
    {
        myColor = MyPanel.color;
    }

    public void ShowInfoForItem(int id, string name, string location, DateTime creationTime)
    {
        Id = id;
        Name.text = name;
        Location.text = location;
        Time.text = creationTime.ToString("HH:mm dd.MM");
    }

    public void Tint()
    {
        MyPanel.color = myColor * 0.9f;
        
    }

    public void Highlight()
    {
        MyPanel.color = myColor * 1.3f;
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        PanelParent.ItemSelected(Id);
    }
}
