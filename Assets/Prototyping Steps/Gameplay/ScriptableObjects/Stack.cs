using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Stack : ScriptableObject
{
    public List<GameObject> cards;
    public (bool, CardCombo) combining;
    public GameObject progressBar;
    public float percentFull;
}
