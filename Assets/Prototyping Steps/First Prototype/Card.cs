using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public GameObject deckObj;
    public CardSpawner cardSpawnScript;
    public List<GameObject> deck;
    public List<GameObject> field;
    // Start is called before the first frame update
    void Start()
    {
        deckObj = transform.parent.gameObject;
        cardSpawnScript = deckObj.GetComponent<CardSpawner>();
        deck = cardSpawnScript.deck;
        field = cardSpawnScript.field;
        deck[deck.Count - 1].transform.position = new Vector3(transform.position.x, transform.position.y, -1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnMouseDown()
    {
        int i;
        if (field.Remove(this.gameObject))
            Destroy(this.gameObject);
        else
        {
            if (field.Count < 5)
            {
                deck.Remove(this.gameObject);
                field.Add(this.gameObject);
                this.gameObject.transform.position = new Vector3(transform.position.x, transform.position.y, 0);
                if (deck.Count > 0)
                    deck[deck.Count - 1].transform.position = new Vector3(transform.position.x, transform.position.y, -1);
                for (i = 0; i < field.Count; i++)
                    field[i].transform.position = new Vector3((i * 2) - 4, 2, 0);
            }

        }
    }

}
