using System;
using UnityEngine;
[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Items")]
public class ItemSO : ScriptableObject
{
    // ---Item---
    public Rarity Rarity;
    public GameObject Prefab;

    // ---ItemData---
    public string Name;
    public float Value;
    public float Weight;
}
