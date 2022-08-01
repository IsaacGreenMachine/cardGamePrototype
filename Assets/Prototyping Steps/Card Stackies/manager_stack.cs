using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class manager_stack : MonoBehaviour
{
    // world positon of mouse
    public Vector3 worldPos;
    // which object is being held by mouse
    public GameObject heldObj;
    // main camera
    public Camera cam;
    // list of cards being hovered over
    public Collider2D[] hoverCards;
    // farthest forward card, updated regularly
    public int currentRenderLayer;
    // whether card was recently dropped or not
    public int MouseRelease = 0;

    // Start is called before the first frame update
    void Start()
    {
        heldObj = null;
        cam = Camera.main;
        currentRenderLayer = 8;
    }

    // Update is called once per frame
    void Update()
    {
        // if left mouse is not down
        if (!Input.GetMouseButton(0))
        {
            // if a card was just dropped
            if (MouseRelease == 1)
            {
                GameObject closest = GetClosestCard(heldObj);
                // if closest card after dropping the card was within range
                if (closest != null && Vector3.Distance(heldObj.transform.position, closest.transform.position) < 0.5)
                {
                    CreateStack(heldObj, closest);
                }
                // events after card was dropped are now done
                MouseRelease = 0;
            }
            // card was dropped, no more held object
            heldObj = null;
        }

        // if the left mouse is down and card is currently held
        else if (heldObj != null)
        {
            // move card to mouse
            heldObj.transform.position = new Vector3(worldPos.x, worldPos.y, 0);
        }

        // if left mouse button down and no card is held
        else
        {
            if (hoverCards.Length > 0)
            {
                heldObj = GetFrontCard(hoverCards);

                if (heldObj.GetComponent<card_stack>().stack.Count > 1)
                    RemoveFromStack(heldObj);

                // front card's order in rendering is now the farthest forward
                heldObj.GetComponent<SpriteRenderer>().sortingOrder = currentRenderLayer;
                // next card picked up will be +1 farther forward
                currentRenderLayer++;
                // checks if card was recently dropped
                MouseRelease = 1;
            }
        }

        // ************ can be moved to inside 'if left mouse button down and no card is held' *****************
            // keep track of mouse position in world
            worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
            // keep track of what cards are being hovered over by mouse
            hoverCards = Physics2D.OverlapCircleAll(new Vector2(worldPos.x, worldPos.y), 0);
    }

    private List<GameObject> CreateStack(GameObject cardA, GameObject cardB)
    {
        int aLen = cardA.GetComponent<card_stack>().stack.Count;
        int bLen = cardB.GetComponent<card_stack>().stack.Count;
        Debug.Log(string.Format("creating stack between {0} ({1}) and {2} ({3})", cardA.name, aLen, cardB.name, bLen));
        // creates a stack from two cards, adds a card to a stack, or combines two stacks
        // card A and b are cards. Could be top of stack or child
        // card A is the card or stack being added to Card or Stack B
        // returns head of new stack

        // if card A is alone
        if (cardA.GetComponent<card_stack>().stack.Count < 2)
        {
            // if card B is alone
            if (cardB.GetComponent<card_stack>().stack.Count < 2)
            {
                List<GameObject> newStack = new List<GameObject>();
                newStack.Add(cardB);
                newStack.Add(cardA);
                cardB.GetComponent<card_stack>().stack = newStack;
                cardA.GetComponent<card_stack>().stack = newStack;
            }
            // if card B is already in a stack
            else
            {
                cardB.GetComponent<card_stack>().stack.Add(cardA);
                cardA.GetComponent<card_stack>().stack = cardB.GetComponent<card_stack>().stack;
            }
        }
        // if card A is stack
        else
        {
            // if card B is alone
            if (cardB.GetComponent<card_stack>().stack.Count < 2)
            {
                cardA.GetComponent<card_stack>().stack.Insert(0, cardB);
                cardB.GetComponent<card_stack>().stack = cardA.GetComponent<card_stack>().stack;
            }
                
            // if card B is already in a stack
            else
            {
                // make 'stack' attribute for all cards in A equal B stack
                foreach (GameObject card in cardA.GetComponent<card_stack>().stack)
                {
                    card.GetComponent<card_stack>().stack = cardB.GetComponent<card_stack>().stack;
                }
                // combine two stacks
                cardB.GetComponent<card_stack>().stack.AddRange(cardA.GetComponent<card_stack>().stack);
            }
        }
        return ArrangeCards(cardB.GetComponent<card_stack>().stack);
    }

    private List<GameObject> ArrangeCards(List<GameObject> stack)
    {
        // physically arrange the cards in a stack List structure
        float i = 0.0f;
        foreach (GameObject card in stack)
        {
            card.transform.position = new Vector3(stack[0].transform.position.x, stack[0].transform.position.y - (i * 0.5f), 0);
            i += 1;
            card.GetComponent<SpriteRenderer>().sortingOrder = currentRenderLayer;
            currentRenderLayer++;
        }
        return stack;
    }

    private List<GameObject> RemoveFromStack(GameObject card)
    {
        // removes given card from its stack and returns the new stack
        card.GetComponent<card_stack>().stack.Remove(card);
        // arrange old stack
        ArrangeCards(card.GetComponent<card_stack>().stack);
        // create new list for this card's stack
        card.GetComponent<card_stack>().stack = new List<GameObject>();
        card.GetComponent<card_stack>().stack.Add(card);
        return card.GetComponent<card_stack>().stack;
    }

    private GameObject GetFrontCard(Collider2D[] hoverCards)
    {
        int frontCardLayer = -1;
        GameObject frontCard = null;
        SpriteRenderer sr = null;

        // gets card that is farthest forward that mouse is hovering over
        // for each card's collider that the mouse is over
        foreach (Collider2D x in hoverCards)
        {
            // get the sprite renderer for collider's game object
            sr = x.gameObject.GetComponent<SpriteRenderer>();
            // if card from array is further forward than current furthest card
            if (sr.sortingOrder > frontCardLayer)
            {
                // front card is new card
                frontCardLayer = sr.sortingOrder;
                frontCard = sr.gameObject;
            }
        }
        return frontCard;

    }

    private GameObject GetClosestCard(GameObject card)
    {
        // returns closest card to given card, or null if none are within 1 unit radius
        // create collider array of nearby cards
        Collider2D[] stackCheck = Physics2D.OverlapCircleAll(new Vector2(card.transform.position.x, card.transform.position.y), 1);
        // closest card
        GameObject closest = null;
        // for each card close enough
        foreach (Collider2D coll in stackCheck)
        {
            // don't do anything if comparing itself
            if (coll.gameObject == card)
                continue;
            // if closest is empty or card in array is closer than closest
            else if (closest == null || Vector3.Distance(card.transform.position, coll.gameObject.transform.position) < Vector3.Distance(card.transform.position, closest.transform.position))
                // set closest as card from array
                closest = coll.gameObject;
        }
            return closest;
    }
}
