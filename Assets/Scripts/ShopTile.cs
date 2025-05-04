using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopTile : MonoBehaviour
{
    public GameObject itemTooltipPrefab;
    public int cost = 1;
    public string itemName = "PlaceholderName";
    public string itemDescription = "PlaceholderDescription";
    public string modifierName = "timeLeft";
    public float modifierValue = 20f;

    private TMP_Text itemTooltip;
    private Canvas shopCanvas;
    private Vector2 tooltipOffset = new Vector2(0, 1.5f);

    private void Awake()
    {
        shopCanvas = GameObject.FindGameObjectWithTag("ShopCanvas").GetComponent<Canvas>();
    }

    private void Start()
    {
        itemTooltipPrefab = Instantiate(itemTooltipPrefab, shopCanvas.transform);
        itemTooltip = itemTooltipPrefab.GetComponentInChildren<TMP_Text>();
        itemTooltip.text = "<b><size=30>" + itemName + "</size></b> - Cost: " + cost + '\n' + "<size=14>" + itemDescription + "</size>";
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
            player.ChangeVariable(modifierName, modifierValue);
        }
    }
}
