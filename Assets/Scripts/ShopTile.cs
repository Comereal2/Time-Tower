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
    public Sprite itemIcon;

    private AudioClip[] itemPurchaseSFX;

    private TMP_Text itemTooltip;
    private Canvas shopCanvas;
    private Vector2 tooltipOffset = new Vector2(0, 1.5f);

    private void Awake()
    {
        itemPurchaseSFX = Resources.LoadAll<AudioClip>("SFX/Purchase");
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
        transform.GetChild(0).GetComponentInChildren<SpriteRenderer>().sprite = itemIcon;
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
            Camera.main.GetComponent<AudioSource>().PlayOneShot(itemPurchaseSFX[UnityEngine.Random.Range(0, itemPurchaseSFX.Length)]);
            player.ChangeScore(-cost);
            foreach(var modifier in modifiers)
            {
                player.ChangeVariable(modifier.modifiedVariable, modifier.modifierValue, modifier.modifierType);
            }
        }
    }
}