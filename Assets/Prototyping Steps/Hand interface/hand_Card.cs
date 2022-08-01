using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hand_Card : MonoBehaviour
{
    public GameObject deckObj;
    public hand_Spawner cardSpawnScript;
    public List<GameObject> deck;
    public List<GameObject> field;
    public List<GameObject> hand;
    // Start is called before the first frame update
    void Start()
    {
        deckObj = transform.parent.gameObject;
        cardSpawnScript = deckObj.GetComponent<hand_Spawner>();
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
        if (field.Remove(this.gameObject))
        {
            Destroy(this.gameObject);
            for (i = 0; i < field.Count; i++)
                field[i].transform.position = new Vector3((i * 2) - 4, 2, 0);
        }
        else
        {
            if (field.Count < 5)
            {
                if (hand.Remove(this.gameObject))
                {
                    field.Add(this.gameObject);
                    // this.gameObject.transform.position = new Vector3(transform.position.x, transform.position.y, 0);
                    // if (deck.Count > 0)
                    // deck[deck.Count - 1].transform.position = new Vector3(transform.position.x, transform.position.y, -1);
                    for (i = 0; i < field.Count; i++)
                        field[i].transform.position = new Vector3((i * 2) - 4, 2, 0);

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

        }
    }
}
