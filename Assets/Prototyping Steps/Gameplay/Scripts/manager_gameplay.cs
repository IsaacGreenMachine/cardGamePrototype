using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class manager_gameplay : MonoBehaviour
{
    // world positon of mouse
    public Vector3 worldPos;
    // helps with grabbing certain part of card
    public float cardOffset;
    // Stack that is being held by mouse
    public Stack heldStack;
    // pack held by mouse
    public Pack heldPack;
    // main camera
    public Camera cam;
    // list of cards being hovered over
    public Collider2D[] hoverObjs;
    // farthest forward card, updated regularly
    public int currentRenderLayer;
    // whether card was recently dropped or not
    public int MouseRelease = 0;
    // database/dictionary {'cardName' : gameObj} of all cards (not in engine, but list of all made cards)
    public Dictionary<string, GameObject> cardList;
    // database list<CardCombo> of all possible card combos where List is ingredients and gameObj is output
    public List<CardCombo> comboList;
    // prefab to instantiate progress bars from
    public GameObject ProgressBarPrefab;
    // list of all stacks in game
    public List<Stack> allStacks;
    // list of all cards in game
    public List<GameObject> allCards;
    // list of all objects in game (currently packs and cards)
    public List<GameObject> allObjects;
    // list of all packs in game
    public List<Pack> allPacks;
    // list of all known packs
    public List<PackPrefab> packList;
    // keeps track of mouse click on packs
    public int packFlag;

    // used in card bumping
    private ContactFilter2D conFil = new();
    private List<Collider2D> collList = new();

    // Start is called before the first frame update
    void Start()
    {
        allCards = new();
        allPacks = new();
        allObjects = new();

        // instantiate heldStack stack
        heldStack = CreateStack();
        // instantiate camera object
        cam = Camera.main;

        packFlag = 0;

        // instantiate list of all known cards
        cardList = DefineCardList();
        // setup for renderLayer
        currentRenderLayer = (cardList.Count * 2) + 1;

        // spawn two of each available card
        // instantiate comboList
        comboList = DefineComboList();
        // instantiate packList
        packList = DefinePackList();

        foreach (PackPrefab packPrefab in packList)
        {
            SpawnPack(packPrefab, new Vector2(Random.Range(-5.5f, 5.5f), Random.Range(-2.5f, 2.5f)));
        }
        // keeps current render layer from exploding
        InvokeRepeating("NormalizeRenderLayers", 20f, 20f);
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
                // if a pack was just dropped
                if (heldPack != null)
                {
                    // re-enable its box collider
                    heldPack.packObj.GetComponent<Collider2D>().enabled = true;
                    heldPack = null;
                }
                // if card/stack was just dropped
                else if (heldStack.cards.Count > 0)
                {
                    CheckForEmptyStacks();
                    // get the closest card not in current stack to dropped card
                    GameObject closest = GetClosestCard(heldStack.cards[0]);
                    // if closest card after dropping the card was within range
                    if (closest != null && Vector3.Distance(heldStack.cards[0].transform.position, closest.transform.position) < 0.8)
                    {
                        StackCards(heldStack, closest.GetComponent<card_gameplay>().stack);
                    }
                    else
                    {
                        // creating new stack since StackCards is cleared on MouseUp
                        StackCards(heldStack, CreateStack());
                    }
                    heldStack = CreateStack();
                }
                // events after card was dropped are now done
                MouseRelease = 0;
                cardOffset = 0;
            }
        }

        // left mouse is down and cards are held
        else if (heldStack.cards.Count != 0)
        {
            // move card[0] to mouse (offset so card is centered horiz. but not vertically)
            heldStack.cards[0].transform.position = new Vector3(worldPos.x, worldPos.y + cardOffset, 0);
            // move all other cards to adjust
            ArrangeCards(heldStack, false);
        }

        // left mouse down and pack is currently held
        else if (heldPack != null)
        {
            heldPack.packObj.transform.position = new Vector3(worldPos.x, worldPos.y + cardOffset, 0);
        }

        // left mouse is down and no card/pack is held
        else
        {
            if (hoverObjs.Length > 0)
            {
                GameObject front = GetFrontObj(hoverObjs);
                if (front != null)
                {
                    // object is a card
                    if (GetObjType(front) == "card")
                    {
                        int cardPos = front.GetComponent<card_gameplay>().stack.cards.IndexOf(front);
                        Stack oldStack = front.GetComponent<card_gameplay>().stack;
                        // gets all cards from grabbed card to bottom of stack and adds them to "heldStack"
                        heldStack = StackCards(CreateStack(front.GetComponent<card_gameplay>().stack.cards.GetRange(cardPos, front.GetComponent<card_gameplay>().stack.cards.Count - cardPos)), heldStack);
                        // disable interactions with held stack
                        foreach (GameObject card in front.GetComponent<card_gameplay>().stack.cards)
                            card.GetComponent<BoxCollider2D>().enabled = false;
                        var combining = CardsCanCombine(oldStack);
                        if (combining.Item1)
                        {
                            BeginCombining(oldStack, combining.Item2);
                        }

                        // to remove single card from stack:
                        /*
                        if (heldStack.GetComponent<card_gameplay>().stack.Count > 1)
                            RemoveFromStack(heldStack);
                        */

                        // checks if card was recently dropped
                        MouseRelease = 1;
                        cardOffset = heldStack.cards[0].transform.position.y - worldPos.y;
                    }

                    else if (GetObjType(front) == "pack")
                    {
                        Pack pack = front.GetComponent<pack_gameplay>().pack;
                        cardOffset = pack.packObj.transform.position.y - worldPos.y;
                        heldPack = pack;
                        MouseRelease = 1;
                        UpdateRenderLayer(pack.packObj);
                        front.GetComponent<BoxCollider2D>().enabled = false;
                    }
                }
            }
        }

        // right mouse is up
        if (!Input.GetMouseButton(1))
        {
            packFlag = 0;
        }

        // right mouse is down
        else
        {
            if (hoverObjs.Length > 0)
            {
                GameObject front = GetFrontObj(hoverObjs);
                if (front != null && packFlag == 0 && GetObjType(front) == "pack" && !Input.GetMouseButton(0))
                {
                    Pack pack = front.GetComponent<pack_gameplay>().pack;
                    SpawnCard(pack.Contents[0], new Vector2(Random.Range(-5.5f, 5.5f), Random.Range(-2.5f, 2.5f)));
                    pack.Contents.RemoveAt(0);
                    packFlag = 1;
                    if (front.GetComponent<pack_gameplay>().pack.Contents.Count == 0)
                        DeletePack(pack);
                }
            }
        }

        // updating progress bar on combining stacks
        foreach (Stack stack in allStacks.ToList())
        {
            if (stack.combining.Item1)
            {
                if (stack != heldStack && CardsCanCombine(stack).Item1)
                {
                    UpdateCombine(stack);
                }
                else
                {
                    StopCombining(stack);
                }
            }
        }

        // card/stack/pack bumping

        /*foreach (GameObject obj in allObjects)
        {
            if (obj.GetComponent<Collider2D>().OverlapCollider(conFil, collList) > 0)
            {
                foreach (Collider2D coll in collList)
                {
                    if (heldStack != null && heldStack.cards.Contains(coll.gameObject))
                        continue;
                    if (heldPack != null && GetObjType(coll.gameObject) == "pack" && heldPack == coll.gameObject.GetComponent<pack_gameplay>().pack)
                        continue;
                    // current object is above collided object and they're not in the same stack. Time to bump this object.
                    if (obj.GetComponent<SpriteRenderer>().sortingOrder > coll.gameObject.GetComponent<SpriteRenderer>().sortingOrder && !InSameStack(obj, coll.gameObject))
                    {
                        // bump object 
                        obj.transform.position = Vector3.MoveTowards(obj.transform.position, coll.gameObject.transform.position, -0.003f);

                        // if object is card, move its stack
                        if (GetObjType(obj) == "card" && obj.GetComponent<card_gameplay>().stack.cards.Count > 1)
                        {
                            Stack movingstack = obj.GetComponent<card_gameplay>().stack;
                            movingstack.progressBar.transform.position = new Vector3(movingstack.cards[0].transform.position.x, movingstack.cards[0].transform.position.y + 1.5f);
                            ArrangeCards(movingstack, false);
                        }
                    }
                }
            }
            collList.Clear();
        }*/

        // size animation
        if (hoverObjs.Length > 0)
        {
            GameObject frontObj = GetFrontObj(hoverObjs);
            if (frontObj != null)
            {
                MakeBig(frontObj);
            }
        }

        // keep track of mouse position in world
        worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        worldPos = new Vector3(worldPos.x, worldPos.y, 0);
        // keep track of what cards are being hovered over by mouse
        hoverObjs = Physics2D.OverlapCircleAll(new Vector2(worldPos.x, worldPos.y), 0);
    }

    private Stack StackCards(Stack stackA, Stack stackB)
    {
        /*
         * stacks stackA onto stackB 
         *  stackA : held cards 
         *  stackB : stationary cards
         * 
         * 
         */

        // the "ToList" here creates a copy of stackA to enumerate through since cards are being removed from stack A
        StopCombining(stackA);
        StopCombining(stackB);
        // stackB.progressBar = stackA.progressBar;
        // stackA.progressBar = null;

        foreach (GameObject card in stackA.cards.ToList())
        {
            AddCardToStack(card, stackB);
            stackA.cards.Remove(card);
        }
        DeleteStack(stackA);
        foreach (GameObject card in stackB.cards)
        {
            card.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            card.GetComponent<Collider2D>().enabled = true;
        }
        var combineable = CardsCanCombine(stackB);
        // if stack matches a recipe from comboList
        if (combineable.Item1)
        {
            BeginCombining(stackB, combineable.Item2);
        }        
        return ArrangeCards(stackB, true); 
    }

    private Stack ArrangeCards(Stack stack, bool updateRL)
    {
        /* Arranges stack "stack" physically based on its list order.
         * 
         * 
         * 
         * 
         * 
         */


        // physically arrange the cards in a stack List structure
        float i = 0.0f;
        foreach (GameObject card in stack.cards)
        {
            card.transform.position = new Vector3(stack.cards[0].transform.position.x, stack.cards[0].transform.position.y - i, 0);
            i += 0.5f;
            if (updateRL)
            UpdateRenderLayer(card);
        }
        return stack;
    }

    private Stack RemoveFromStack(GameObject card)
    {
        /* used in removing single (non front) cards from stacks. not currently used.
         * 
         * 
         * 
         * 
         */

        // removes given card from its stack and returns the new stack
        card.GetComponent<card_gameplay>().stack.cards.Remove(card);
        // arrange old stack
        ArrangeCards(card.GetComponent<card_gameplay>().stack, true);
        // create new list for this card's stack
        card.GetComponent<card_gameplay>().stack = CreateStack();
        card.GetComponent<card_gameplay>().stack.cards.Add(card);
        return card.GetComponent<card_gameplay>().stack;
    }

    private GameObject GetFrontObj(Collider2D[] hoverObjs)
    {
        /* returns card that is farthest forward if many are under mouse pointer
         * 
         * 
         * 
         * 
         */


        int frontObjLayer = -1;
        GameObject frontObj = null;
        SpriteRenderer sr;

        // gets card that is farthest forward that mouse is hovering over
        // for each card's collider that the mouse is over
        foreach (Collider2D x in hoverObjs)
        {
            // get the sprite renderer for collider's game object
            sr = x.gameObject.GetComponent<SpriteRenderer>();
            // if card from array is further forward than current furthest card
            if (sr.sortingOrder > frontObjLayer)
            {
                // front card is new card
                frontObjLayer = sr.sortingOrder;
                frontObj = sr.gameObject;
            }
        }
        return frontObj;

    }

    private GameObject GetClosestCard(GameObject card)
    {
        /*
         * 
         * 
         * returns closest card (not in stack) to given card, or null if none are within 1 unit radius
         */

        // create collider array of cards within 1 unit radius
        Collider2D[] stackCheck = Physics2D.OverlapCircleAll(new Vector2(card.transform.position.x, card.transform.position.y), 1);

        // closest card
        GameObject closest = null;
        // for each card close enough
        foreach (Collider2D coll in stackCheck)
        {
            if (GetObjType(coll.gameObject) == "card")
            {
                // skip the card itself and any cards in stack
                if (coll.gameObject == card || card.GetComponent<card_gameplay>().stack.cards.Contains(coll.gameObject))
                    continue;
                // if closest is empty or card in array is closer than closest
                else if (closest == null || Vector3.Distance(card.transform.position, coll.gameObject.transform.position) < Vector3.Distance(card.transform.position, closest.transform.position))
                {
                    // set closest as card from array
                    closest = coll.gameObject;
                }
            }
        }
        return closest;
    }

    private Dictionary<string, GameObject> DefineCardList()
    {
        /* defines cardList. a Dictionary<string, GameObject> of card names and their prefabs.
         * imports prefabs from Resources/cards folder
         * 
         * 
         */

        Dictionary<string, GameObject> cardList = new();
        foreach (GameObject card in Resources.LoadAll("Cards"))
        {
            cardList.Add(card.GetComponent<card_gameplay>().cardName, card);
        }
        return cardList;
    }

    private Stack SpawnCard(GameObject cardPrefab, Vector2 position)
    {
        /* spawns a new card from a GameObject prefab (use cardList dict) at a vector2 positon
         * 
         */

        GameObject newCard = Instantiate(cardPrefab, new Vector3(position.x, position.y, 0), Quaternion.identity);
        UpdateRenderLayer(newCard);

        // create new stack for card

        newCard.GetComponent<card_gameplay>().stack = CreateStack();
        newCard.GetComponent<card_gameplay>().stack.cards.Add(newCard);
        newCard.GetComponent<card_gameplay>().cardName = cardPrefab.GetComponent<card_gameplay>().cardName;
        allCards.Add(newCard);
        allObjects.Add(newCard);
        return newCard.GetComponent<card_gameplay>().stack;
    }

    private List<string> GetStackCardNames(Stack stack)
    {
        /* takes a List<GameObject> and returns a List<string> of the names.
         * used in comparisons for recipes.
         * 
         * 
         * 
         * 
         * 
         */

        List<string> names = new();
         foreach(GameObject card in stack.cards)
        {
            names.Add(card.GetComponent<card_gameplay>().cardName);
        }
        return names;
    }

    private string EnumToString(CardCombo.cardsList comboEnum) {
        /* helper function to get string names from an enum.
         * used in creating "comboList"s "String Ingredients" list, which is used in comparisons in recipes.
         * 
         * 
         */

        return comboEnum.ToString();
    }

    private List<CardCombo> DefineComboList()
    {
        /* creates "comboList" a List<Scriptable Objects CardCombo> containing all card recipes
         * 
         * 
         * 
         * 
         * 
         */

        List<CardCombo> comboList = new();
        foreach (CardCombo recipe in Resources.LoadAll("Recipes"))
        {
            // add CardCombo ScriptableObject to comboLis
            comboList.Add(recipe);
            // turn CardCombo.Ingrediets (list<enum>) into CardCombo.StringIngrediets (list<string>)
            recipe.stringIngredients = recipe.Ingredients.ConvertAll(new System.Converter<CardCombo.cardsList, string>(EnumToString));
            // sorting stringIngredients for comparison made in CardsCanCombine()
            recipe.stringIngredients.Sort(); 
            recipe.GOResult = cardList[recipe.Result.ToString()];
        }
        return comboList;
    }

    private (bool, CardCombo) CardsCanCombine(Stack stack)
    {
        /* returns True/False if stack's cards found as valid recipe in comboList and
         * Gameobject CardPrefab if valid recipe found or null if no valid recipe 
         * 
         * won't combine cards whilst held. to revert this, remove if check for if == heldStack
         * 
         * 
         */
        if (stack == heldStack)
        {
            return (false, null);
        }

        List<string> stackCardNames = GetStackCardNames(stack);
        // sorting stack for comparison. Ingredients are already sorted (in defineComboList)
        stackCardNames.Sort();
        foreach (CardCombo combo in comboList)
        {
            // only works on exact stack matches to ingredient lists
            if (Enumerable.SequenceEqual(combo.stringIngredients, stackCardNames))
                return (true, combo);
            // for combo-ing cards in a stack where there are extra cards present,

            // returns list of items in list 2 not in list 1, and vice versa
            // var firstNotSecond = list1.Except(list2).ToList();
            // var secondNotFirst = list2.Except(list1).ToList();
        }
        return (false, null);
    }

    private Stack CombineCards(Stack stack, CardCombo newcard)
    {
        /* Combines cards in "stack" into new List<GameObject> card, which is created from "newcard" card prefab. 
         * 
         * 
         * might need to change GameObject newcard into List<GameObject> newCard
         */
        // arrange cards
        // create a new progressBar


        //GameObject progressBar = Instantiate(ProgressBarPrefab, new Vector3(stack[0].transform.position.x, stack[0].transform.position.y + 1.5f), Quaternion.identity);


        // render in front of other cards
        /*foreach (Transform progressBarComponent in progressBar.transform)
        {
            progressBarComponent.gameObject.GetComponent<SpriteRenderer>().sortingOrder = currentRenderLayer;
            currentRenderLayer++;
        }
        */
        // set the animation speed
        //progressBar.transform.GetChild(0).gameObject.GetComponent<Animator>().speed = 3 - (0.01f * newcard.craftTime);
        // play the animation
        //progressBar.transform.GetChild(0).gameObject.GetComponent<Animator>().Play("grow");
        // wait for progress bar to finish, animation length in seconds is (seconds of animation) / animation speed
        Stack spawnedCardstack = SpawnCard(newcard.GOResult, new Vector2(stack.cards[0].transform.position.x, stack.cards[0].transform.position.y));
        DeleteStack(stack);
        // destroy progress bar
        //Destroy(progressBar);
        return spawnedCardstack;
    }

    private Stack AddCardToStack(GameObject card, Stack stack)
    {
        /* adds card "card" to stack "stack", sets its stack attribute, and removes it from its previous stack.
         * 
         * 
         * 
         * 
         */

        card.GetComponent<card_gameplay>().stack.cards.Remove(card);
        card.GetComponent<card_gameplay>().stack = stack;
        stack.cards.Add(card);
        return stack;
    }

    private Stack DeleteCardFromStack(GameObject card)
    {
        /* removes a card from a stack and destroys it.
         * 
         * 
         * 
         * 
         */

        Stack stack = card.GetComponent<card_gameplay>().stack;
        // removes given card from its stack and returns the new stack
        stack.cards.Remove(card);
        // arrange old stack
        ArrangeCards(stack, true);
        Destroy(card);
        return stack;
    }

    private float ProgressBarSpeedMapping(float time)
    {
        /*
         * maps an arbitrary value to 0.01->3.00
         */

        // time is high, animator speed is low (3 - (0.01f * 299)) -> (3 - 2.99) -> 0.01
        // time is low, animator speed is high (3 - (0.01f * 10)) -> (3 - 0.1) -> 2.90
        return 3 - (0.01f * time);
    }

    private Stack CreateStack(List<GameObject> objList=null)
    {
        Stack stack = ScriptableObject.CreateInstance<Stack>();
        if (objList == null)
            stack.cards = new List<GameObject>();
        else
            stack.cards = new List<GameObject>(objList);
        stack.combining = (false, null);
        stack.progressBar = Instantiate(ProgressBarPrefab, transform.position, Quaternion.identity);
        TurnOffProgressBar(stack);
        stack.percentFull = 0;
        allStacks.Add(stack);
        return stack;
    }

    private void TurnOffProgressBar(Stack stack) {

        // turns off script running inside:
        // stack.progressBar.SetActive(false);

        foreach (Transform barPiece in stack.progressBar.transform)
        {
            barPiece.GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    private void TurnOnProgressBar(Stack stack)        
    {
        stack.progressBar.transform.position = new Vector3(stack.cards[0].transform.position.x, stack.cards[0].transform.position.y + 1.5f);
        foreach (Transform barPiece in stack.progressBar.transform)
        {
            UpdateRenderLayer(barPiece.gameObject);
            barPiece.GetComponent<SpriteRenderer>().enabled = true;
        }
    }

    private void DeleteStack(Stack stack)
    {
        /*
         * used to delete a stack (single card or stack)
         */

        foreach (GameObject card in stack.cards)
        {
            allCards.Remove(card);
            allObjects.Remove(card);
            Destroy(card);
        }
        stack.cards.Clear();
        stack.combining = (false, null);
        Destroy(stack.progressBar);
        stack.progressBar = null;
        allStacks.Remove(stack);
        stack = null;
    }

    private Stack BeginCombining(Stack stack, CardCombo combo)
    {
        ArrangeCards(stack, true);
        stack.combining = (true, combo);
        TurnOnProgressBar(stack);
        return stack;
    }

    private Stack StopCombining(Stack stack)
    {
        stack.combining = (false, null);
        stack.percentFull = 0;
        TurnOffProgressBar(stack);
        return stack;
    }

    private void CheckForEmptyStacks()
    {
        foreach (Stack stack in allStacks.ToList())
        {
            if (stack.cards.Count == 0 && stack != heldStack)
            {
                DeleteStack(stack);
            }
        }
    }

    private void UpdateCombine(Stack stack)
    {
        // change size of progress bar    
        stack.progressBar.transform.GetChild(0).localScale = new Vector3(stack.percentFull / 50, 0.4f);
        // shift center of progress bar since scale is from center of object
        stack.progressBar.transform.GetChild(0).localPosition = new Vector3((stack.percentFull / 100) - 1, 0);
        stack.percentFull += ((1000f - stack.combining.Item2.craftTime) / 10000f);

        if (stack.percentFull >= 100)
        {
            CombineCards(stack, stack.combining.Item2);
        }
    }

    private List<PackPrefab> DefinePackList()
    {
        List<PackPrefab> packList = new();
        foreach (PackPrefab pack in Resources.LoadAll("PackPrefabs"))
        {
            packList.Add(pack);
            int packCount = pack.PossibleCards.Count();
            int raritiesCount = pack.Rarities.Count();
            if (raritiesCount != packCount)
            {
                if (raritiesCount > packCount)
                    pack.Rarities.RemoveRange(raritiesCount - (raritiesCount - packCount), (raritiesCount - packCount));
                else if (packCount > raritiesCount)
                {
                    while (pack.PossibleCards.Count() > pack.Rarities.Count())
                        pack.Rarities.Add(0);
                }
            }
            pack.rarityTotal = pack.Rarities.Sum();
        }
        return packList;
    }

    private Pack SpawnPack(PackPrefab packPrefab, Vector2 position)
    {
        Pack pack = ScriptableObject.CreateInstance<Pack>();
        pack.Contents = new();
        // instantiating actual pack GameObject
        pack.packObj = Instantiate(packPrefab.packObjPrefab, position, Quaternion.identity);
        UpdateRenderLayer(pack.packObj);
        pack.packObj.GetComponent<pack_gameplay>().pack = pack;
        // creating a pool of possible cards to add to pack
        List<GameObject> cardPool = new();
        foreach (int i in Enumerable.Range(0, packPrefab.PossibleCards.Count()))
            cardPool.AddRange(Enumerable.Repeat(packPrefab.PossibleCards[i], packPrefab.Rarities[i]));

        // adding random cards from pool to pack
        int randomChoice;
        foreach (int _ in Enumerable.Range(0, packPrefab.packSize))
        {
            randomChoice = Random.Range(0, cardPool.Count() - 1);
            pack.Contents.Add(cardPool[randomChoice]);
            cardPool.RemoveAt(randomChoice);
        }
        allPacks.Add(pack);
        allObjects.Add(pack.packObj);
        return pack;
    }

    private List<System.Type> GetCompList(GameObject obj)
    {
            List<System.Type> compList = new();
            foreach (Component comp in obj.GetComponents(typeof(Component)))
            {
                compList.Add(comp.GetType());
            }
            return compList;
    }

    private void DeletePack(Pack pack)
    {
        allObjects.Remove(pack.packObj);
        allPacks.Remove(pack);
        Destroy(pack.packObj);
        foreach(GameObject go in pack.Contents)
        {
            Destroy(go);
        }
        pack.Contents.Clear();
        pack.Contents = null;
    }

    private bool InSameStack (GameObject objA, GameObject objB)
    {
        if (GetObjType(objA) == "card" && GetObjType(objB) == "card" && objA.GetComponent<card_gameplay>().stack == objB.GetComponent<card_gameplay>().stack)
            return true;
        else
            return false;
    }

    private string GetObjType(GameObject obj)
    {
        if (obj != null)
        {
            if (GetCompList(obj).Contains(typeof(pack_gameplay)))
            {
                return "pack";
            }
            if (GetCompList(obj).Contains(typeof(card_gameplay)))
            {
                return "card";
            }
        }
        return null;
    }

    private void MakeBig(GameObject obj)
    {
        if (obj != null)
        {
            obj.transform.localScale = new Vector3(1.2f, 1.2f, 1);
            StartCoroutine(MakeSmall(obj));
        }
    }

    private IEnumerator MakeSmall(GameObject obj)
    {
        yield return new WaitForSeconds(0.1f);
        if (obj != null && (hoverObjs.Length == 0 || GetFrontObj(hoverObjs) != obj))
        {
            obj.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private void UpdateRenderLayer(GameObject obj)
    {
        if (obj.GetComponent<SpriteRenderer>().sortingOrder != currentRenderLayer - 1)
        {
            obj.GetComponent<SpriteRenderer>().sortingOrder = currentRenderLayer;
            currentRenderLayer++;
        }
    }

    private void NormalizeRenderLayers()
    {
        allObjects = allObjects.OrderBy(i => i.GetComponent<SpriteRenderer>().sortingOrder).ToList();
        int i = 0;
        foreach (GameObject obj in allObjects)
        {
            obj.GetComponent<SpriteRenderer>().sortingOrder = i;
            i++;
        }
        currentRenderLayer = i;
    }
}

