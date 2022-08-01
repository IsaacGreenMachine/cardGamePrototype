using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hand_Spawner : MonoBehaviour
{
    public GameObject card;
    // public Queue card_deck = new Queue();
    public List<GameObject> deck = new List<GameObject>();
    public List<GameObject> field = new List<GameObject>();
    public List<GameObject> hand = new List<GameObject>();
    SpriteRenderer sr;
    // Start is called before the first frame update
    void Start()
    {
        int i;
        GameObject newcard;
        for (i = 0; i < 30; i++)
        {
            newcard = Instantiate(card, transform.position, Quaternion.identity, transform);
            newcard.GetComponent<SpriteRenderer>().color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
            deck.Add(newcard);
        }
        for (i = 0; i < 5; i++)
        {
            hand.Add(deck[deck.Count - 1]);
            deck.RemoveAt(deck.Count - 1);
        }
        shiftCards(hand, -4);
        for (i = 0; i < hand.Count; i++)
            hand[i].transform.position = new Vector3((i * 2) - 4, -4, 0);

        deck[deck.Count - 1].transform.position = new Vector3(transform.position.x + 1, -4, 0);
        deck[deck.Count - 2].transform.position = new Vector3(transform.position.x + 0.5f, -4, 0);
        




    }

    // Update is called o   nce per frame
    void Update()
    {

    }

    void shiftCards(List<GameObject> someList, int ypos)
    {
        
    }
}
