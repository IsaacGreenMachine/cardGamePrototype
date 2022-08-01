using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CardCombo : ScriptableObject
{
    public enum cardsList {Bat, Bunicorn, GhostDog, Squirrowl, Turkle};
    public List<cardsList> Ingredients;
    public List<string> stringIngredients;
    public cardsList Result;
    public GameObject GOResult;
    [Range(0, 999)]
    public int craftTime;
}