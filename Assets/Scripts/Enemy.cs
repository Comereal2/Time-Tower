using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : ScriptableObject
{
    public int health = 1;
    public float speed = 3f;
    public float spawnTime = 30f;
    public bool hasCoin = false;
}