using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class card : MonoBehaviour
{
    Camera cam;
    SpriteRenderer sr;
    public GameObject manager;
    public GameObject heldObj;
    public manager managerScript;
    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        manager = GameObject.Find("manager");
        managerScript = manager.GetComponent<manager>();
        heldObj = managerScript.heldObj;
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButton(0) && (managerScript.heldObj == null || managerScript.heldObj == this.gameObject))
        {
            sr.sortingOrder = 1;
            managerScript.heldObj = this.gameObject;
        }

        else
        {
            sr.sortingOrder = 0;
        }
            
    }

    private void OnMouseExit()
    {
        sr.sortingOrder = 0;
    }

    private void OnMouseUp()
    {
        managerScript.heldObj = null;
    }
}
