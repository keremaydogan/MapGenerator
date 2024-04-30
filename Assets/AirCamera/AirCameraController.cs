using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirCameraController : MonoBehaviour
{
    [Header("Input Settings")]
    public float mouseSensitivity = 1;
    public float movementSpeed = 10;
    public float runCoeff = 10;


    float rotX, rotY;
    float movX, movZ;
    float run;
    Vector3 movement;

    private void Update()
    {
        InputExecuter();
    }

    void InputExecuter()
    {
        rotX += Input.GetAxis("Mouse Y") * -mouseSensitivity;
        rotY += Input.GetAxis("Mouse X") * mouseSensitivity;

        transform.localEulerAngles = new Vector3(rotX, rotY);

        if(Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D)) {
            movX = 0f;
        }
        else if (Input.GetKey(KeyCode.D)) {
            movX = 1f;
        }
        else if (Input.GetKey(KeyCode.A)) {
            movX = -1f;
        }
        else {
            movX = 0f;
        }

        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.S)) {
            movZ = 0f;
        }
        else if (Input.GetKey(KeyCode.W)) {
            movZ = 1f;
        }
        else if (Input.GetKey(KeyCode.S)) {
            movZ = -1f;
        }
        else {
            movZ = 0f;
        }

        if (Input.GetKey(KeyCode.LeftShift)) {
            run = runCoeff;
        }
        else {
            run = 1f;
        }

        movement = run * movementSpeed * (movZ * transform.forward + movX * transform.right + Input.mouseScrollDelta.y * transform.up);

        transform.position += movement * Time.deltaTime;

    }

}
