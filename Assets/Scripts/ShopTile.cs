using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopTile : MonoBehaviour
{
    public Item item;
    public bool isOneTimeUse = false;

    private GameObject itemTooltipPrefab;
    private AudioClip[] itemPurchaseSFX;

    private TMP_Text itemTooltip;
    private Canvas shopCanvas;
    private Vector2 tooltipOffset = new Vector2(0, 1.5f);

    private void Awake()
    {
        itemPurchaseSFX = Resources.LoadAll<AudioClip>("SFX/Purchase");
        itemTooltipPrefab = Resources.Load<GameObject>("Prefabs/ItemTooltip");
        shopCanvas = GameObject.FindGameObjectWithTag("ShopCanvas").GetComponent<Canvas>();
    }

    private void Start()
    {
        string itemDescription = "";
        foreach(var modifier in item.modifiers)
        {
            itemDescription += modifier.modifiedVariableVisibleDescription + '\n';
        }
        itemTooltipPrefab = Instantiate(itemTooltipPrefab, shopCanvas.transform);
        itemTooltip = itemTooltipPrefab.GetComponentInChildren<TMP_Text>();
        itemTooltip.text = "<size=72><b>" + item.itemName + " - Cost: " + item.cost + "</b></size>" + '\n' + "<size=56>" + itemDescription + "</size>";
        if (item.itemIcon != null)
        {
            var childRenderer = transform.GetChild(0).GetComponentInChildren<SpriteRenderer>();
            childRenderer.sprite = item.itemIcon;
            childRenderer.transform.localScale = new Vector2(item.spriteXScale, item.spriteYScale);
        }
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
            if (player.score < item.cost) return;
            player.PlaySound(itemPurchaseSFX[UnityEngine.Random.Range(0, itemPurchaseSFX.Length)]);
            player.ChangeScore(-item.cost);
            foreach(var modifier in item.modifiers)
            {
                player.ChangeVariable(modifier);
            }
            if (isOneTimeUse)
            {
                Destroy(itemTooltipPrefab);
                Destroy(gameObject);
            }
        }
    }
}