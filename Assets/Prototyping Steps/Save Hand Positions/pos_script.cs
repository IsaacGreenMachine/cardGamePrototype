using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pos_script : MonoBehaviour
{
    public GameObject deckObj;
    public pos_spawner cardSpawnScript;
    public List<GameObject> deck;
    public List<GameObject> field;
    public List<GameObject> hand;
    // Start is called before the first frame update
    void Start()
    {
        deckObj = transform.parent.gameObject;
        cardSpawnScript = deckObj.GetComponent<pos_spawner>();
        deck = cardSpawnScript.deck;
        field = cardSpawnScript.field;
        hand = cardSpawnScript.hand;
        // deck[deck.Count - 1].transform.position = new Vector3(transform.position.x, transform.position.y, -1);
    }

    // Update is called once per frame
    void Update()
    {

    }


    private void OnMouseDown()
    {
        int i;
        int pos;
        if (field.Contains(this.gameObject))
        {
            pos = field.IndexOf(this.gameObject);
            field.Remove(this.gameObject);
            Destroy(this.gameObject);
            field.Insert(pos, null);
        }
        else if (hand.Contains(this.gameObject) && field.FindAll(isBlank).Count < 5)
        {
            if (hand.Remove(this.gameObject))
            {
                for (i = 0; i < field.Count; i++)
                {
                    if (field[i] == null)
                    {
                        field[i] = this.gameObject;
                        break;
                    }

                }
                if (i == field.Count)
                {
                    field.Add(this.gameObject);
                }
                for (i = 0; i < field.FindAll(isBlank).Count; i++)
                    field[i].transform.position = new Vector3((i * 2) - 4, 2, 0);
            }
        }
        else if (hand.Count < 5)
        {
            if (deck.Remove(this.gameObject))
            {
                hand.Insert(0, deck[deck.Count - 1]);
                deck.RemoveAt(deck.Count - 1);
                for (i = 0; i < hand.Count; i++)
                    hand[i].transform.position = new Vector3((i * 2) - 4, -4, 0);
                if (deck.Count > 0)
                    deck[deck.Count - 1].transform.position = new Vector3(-6, -4, 0);
                if (deck.Count > 1)
                    deck[deck.Count - 2].transform.position = new Vector3(-6.5f, -4, 0);
            }
        }
        // Debug.Log(field[0] == null);

        // length of list - null game objects
        // Debug.Log(field.FindAll(isBlank).Count);

    }

    private static bool isBlank(GameObject card)
    {
        return (card is GameObject);
    }
}
