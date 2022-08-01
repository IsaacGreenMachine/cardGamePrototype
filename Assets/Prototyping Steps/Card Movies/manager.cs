using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class manager : MonoBehaviour
{
    public Vector3 worldPos;
    public GameObject heldObj;
    public Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        heldObj = null;
        cam = Camera.main;

    }

    // Update is called once per frame
    void Update()
    {
        if (heldObj != null)
        {
            worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
            heldObj.transform.position = new Vector3(worldPos.x, worldPos.y, 0);
        }
    }
}
