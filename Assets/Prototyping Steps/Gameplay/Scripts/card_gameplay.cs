using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class card_gameplay : MonoBehaviour
{
    Camera cam;
    SpriteRenderer sr;
    public GameObject manager;
    public manager_gameplay managerScript;
    public Collider2D myCollider;
    public Stack stack;
    public string cardName;
    public string type;
    public FixedJoint2D joint;
    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        manager = GameObject.Find("manager2");
        managerScript = manager.GetComponent<manager_gameplay>();
        myCollider = this.GetComponent<Collider2D>();
        type = "card";
        joint = GetComponent<FixedJoint2D>();
        joint.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
