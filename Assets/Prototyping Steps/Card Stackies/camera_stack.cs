using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera_stack : MonoBehaviour
{
    float cameraMoveScale = 10.0f;
    public Vector2 camSize;
    Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(new Vector3(-1, 0, 0) * Time.deltaTime * cameraMoveScale);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(new Vector3(1, 0, 0) * Time.deltaTime * cameraMoveScale);
        }
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(new Vector3(0, 1, 0) * Time.deltaTime * cameraMoveScale);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(new Vector3(0, -1, 0) * Time.deltaTime * cameraMoveScale);
        }
        cam.orthographicSize += Input.mouseScrollDelta[1] * -0.1f;
        camSize = Input.mouseScrollDelta;

    }
}
