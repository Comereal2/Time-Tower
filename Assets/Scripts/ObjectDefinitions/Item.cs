using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Object/Item")]

public class Item : ScriptableObject
{
    public int cost = 1;
    // This could be deleted in place of Item.name, but in my opinion the display name should not rely on the file name
    public string itemName = "PlaceholderName";
    public Modifier[] modifiers;
    public Sprite itemIcon;
    public float spriteXScale = 1f, spriteYScale = 1f;
}