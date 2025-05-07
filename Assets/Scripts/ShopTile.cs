using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
    private PlayerController player;
    private bool isRandomizedItem = false;
    private bool displayShopItems = true;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        itemPurchaseSFX = Resources.LoadAll<AudioClip>("SFX/Purchase");
        itemTooltipPrefab = Resources.Load<GameObject>("Prefabs/ItemTooltip");
        shopCanvas = GameObject.FindGameObjectWithTag("ShopCanvas").GetComponent<Canvas>();
        if (item == null)
        {
            RandomizeItem();
            isRandomizedItem = true;
        }
        displayShopItems = PlayerPrefs.GetInt("ShopTags", 1) == 1;
    }

    private void Start()
    {
        itemTooltipPrefab = Instantiate(itemTooltipPrefab, shopCanvas != null ? shopCanvas.transform : gameObject.transform); /*Make sure tooltip exists, even if it wont display*/
        itemTooltip = itemTooltipPrefab.GetComponentInChildren<TMP_Text>();
        UpdateItemText();
        UpdateItemSprite();
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
            int price = (int)Mathf.Max(item.cost * player.costModifier, 1);
            // Dont realize the purchase if player doesnt have the money for it
            if (player.score < price) return;

            player.PlaySound(itemPurchaseSFX[UnityEngine.Random.Range(0, itemPurchaseSFX.Length)]);
            player.ChangeScore(-price);

            foreach(var modifier in item.modifiers)
            {
                player.ChangeVariable(modifier);
            }

            if (isOneTimeUse)
            {
                Destroy(itemTooltipPrefab);
                Destroy(gameObject);
            }

            if (isRandomizedItem)
            {
                RandomizeItem();
            }

            //Update item description just in case price changes
            UpdateItemText();
        }
    }

    /// <summary>
    /// Randomizes the item in the shop tile
    /// </summary>
    private void RandomizeItem()
    {
        var resources = Resources.LoadAll("Data/Items");
        item = (Item)resources[UnityEngine.Random.Range(0, resources.Length)];
        UpdateItemSprite();
    }

    /// <summary>
    /// Updates the text displayed in the itemTooltip
    /// </summary>
    private void UpdateItemText()
    {
        string itemDescription = "";
        foreach (var modifier in item.modifiers)
        {
            itemDescription += modifier.modifiedVariableVisibleDescription + '\n';
        }
        if (displayShopItems) itemTooltip.text = "<size=72><b>" + item.itemName + " - Cost: " + (Mathf.Max(item.cost * player.costModifier, 1)).ToString() + "</b></size>" + '\n' + "<size=56>" + itemDescription + "</size>";
        else itemTooltip.text = "<size=72><b>??? - Cost: ???</b></size>" + '\n' + "???";
    }

    /// <summary>
    /// Updates the sprite displayed on the shop tile itself
    /// </summary>
    private void UpdateItemSprite()
    {
        var childRenderer = transform.GetChild(0).GetComponentInChildren<SpriteRenderer>();
        if (item.itemIcon != null)
        {
            childRenderer.sprite = item.itemIcon;
            childRenderer.transform.localScale = new Vector2(item.spriteXScale, item.spriteYScale);
        }
        else
        {
            childRenderer.sprite = null;
        }
    }
}