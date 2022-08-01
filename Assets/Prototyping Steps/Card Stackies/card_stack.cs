using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class card_stack : MonoBehaviour
{
    Camera cam;
    SpriteRenderer sr;
    public GameObject manager;
    public manager_stack managerScript;
    public Collider2D[] hoverCards;
    public Collider2D myCollider;
    public List<GameObject> stack;
    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        manager = GameObject.Find("manager2");
        managerScript = manager.GetComponent<manager_stack>();
        myCollider = this.GetComponent<Collider2D>();
        // sr.sortingOrder = -1;
    }

    // Update is called once per frame
    void Update()
    {
 
    }
}
