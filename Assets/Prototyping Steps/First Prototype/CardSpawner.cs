using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSpawner : MonoBehaviour
{
    public GameObject card;
    // public Queue card_deck = new Queue();
    public List<GameObject> deck = new List<GameObject>();
    public List<GameObject> field = new List<GameObject>();
    SpriteRenderer sr;
    // Start is called before the first frame update
    void Start()
    {
        int i;
        GameObject newcard;
        for (i = 0; i < 10; i++)
        {
            newcard = Instantiate(card, transform.position, Quaternion.identity, transform);
            newcard.GetComponent<SpriteRenderer>().color = new Color(i * 0.1f , 0, 0);
            deck.Add(newcard);
        }
        //sr.color = new Color(0, 0, 0);
        //Instantiate(card, new Vector3(0, 0, 0), Quaternion.identity);

    }

    // Update is called o   nce per frame
    void Update()
    {
    }
}
