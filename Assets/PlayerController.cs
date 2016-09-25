using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    Rigidbody player;
    GameObject mouseHelp;
    Camera mainCam;
    float moveX;
    float moveY;
    public float speed = 2f;
    public LayerMask floorMask;
    RaycastHit floorPos;
    Ray camRay;

    // Use this for initialization
    void Start () {
        player = this.GetComponent<Rigidbody>();
        mouseHelp = GameObject.FindGameObjectWithTag("GameController");
        mouseHelp.GetComponent<Renderer>().sortingOrder = 1000000;
        mainCam = FindObjectOfType<Camera>();
        mouseHelp.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        camRay = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(camRay, out floorPos, floorMask))
        {
            mouseHelp.SetActive(true);
            Vector3 mousePos = new Vector3((int)floorPos.point.x + 0.5f, 0.001f, (int)floorPos.point.z + 0.5f);
            mouseHelp.transform.position = mousePos;
        }
        else {
            mouseHelp.SetActive(false);
        }
        if (Input.GetMouseButtonDown(0))
        {
            Movement();
        }
    }

    void Movement() {
        if (Physics.Raycast(camRay, out floorPos, floorMask))
        {
            Vector3 newPos = new Vector3((int)floorPos.point.x + 0.5f, player.transform.position.y, (int) floorPos.point.z + 0.5f);
            player.transform.position = newPos;
        }
    }

}
