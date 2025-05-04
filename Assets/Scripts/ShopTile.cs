using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopTile : MonoBehaviour
{
    public GameObject itemTooltipPrefab;
    public int cost = 1;
    public string itemName = "PlaceholderName";
    public Modifier[] modifiers;

    private TMP_Text itemTooltip;
    private Canvas shopCanvas;
    private Vector2 tooltipOffset = new Vector2(0, 1.5f);

    private void Awake()
    {
        shopCanvas = GameObject.FindGameObjectWithTag("ShopCanvas").GetComponent<Canvas>();
    }

    private void Start()
    {
        string itemDescription = "";
        foreach(var modifier in modifiers)
        {
            itemDescription += modifier.modifiedVariableVisibleDescription + '\n';
        }
        itemTooltipPrefab = Instantiate(itemTooltipPrefab, shopCanvas.transform);
        itemTooltip = itemTooltipPrefab.GetComponentInChildren<TMP_Text>();
        itemTooltip.text = "<size=72><b>" + itemName + " - Cost: " + cost + "</b></size>" + '\n' + "<size=56>" + itemDescription + "</size>";
    }

    private void Update()
    {
        itemTooltipPrefab.transform.position = Camera.main.WorldToScreenPoint(transform.position + (Vector3)tooltipOffset);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player.score < cost) return;
            player.ChangeScore(-cost);
            foreach(var modifier in modifiers)
            {
                player.ChangeVariable(modifier.modifiedVariable, modifier.modifierValue);
            }
        }
    }
}
