using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PackPrefab : ScriptableObject
{
    // cardPrefab, raity
    public List<GameObject> PossibleCards;
    public List<int> Rarities;
    public float rarityTotal;
    public int packSize;
    public GameObject packObjPrefab;
}
