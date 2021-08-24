using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera mainCamera;

    public float distance = 50f;

    //also known as mouse sensitivity 
    public float xSpeed = 250f;
    public float ySpeed = 150f;

    //speed for shift sprinting
    public float normalControlSpeed = 20f;
    public float fastControlSpeed = 40f;


    public float yMinLimit = 0f;
    public float yMaxLimit = 120f;

    public float controlSpeed;

    private Quaternion rotation;

    private float xAngle = 0f, yAngle = 0f;
    private float angleMultiplier = 0.02f;

    // Start is called before the first frame update
    void Start()
    {
        //make mouse cursor invisible 
        Cursor.visible = false;

        //mouse wont go off screen, need to press alt tab to exit
        Cursor.lockState = CursorLockMode.Locked;

        controlSpeed = normalControlSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        //moving side to side, GetAxis gets the X coordenate between 1 and -1
        xAngle += Input.GetAxis("Mouse X") * xSpeed * angleMultiplier;
        //up and down, this later will be needed to be between minimum and max values
        yAngle -= Input.GetAxis("Mouse Y") * ySpeed * angleMultiplier;

        //yAngle must be between min and max
        yAngle = ClampAngle(yAngle, yMinLimit, yMaxLimit);

        //angle to rotate by
        rotation = Quaternion.Euler(yAngle, xAngle, 0);
        transform.rotation = rotation;

        // speed up camera
        if (Input.GetKey(KeyCode.LeftShift))
        {
            controlSpeed = fastControlSpeed;
        }
        else
        {
            controlSpeed = normalControlSpeed;
        }

        // camera controls
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            transform.position += Camera.main.transform.forward * controlSpeed *  Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            transform.position -= Camera.main.transform.forward * controlSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            transform.position += Camera.main.transform.right * controlSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            transform.position -= Camera.main.transform.right * controlSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.Space))
        {
            transform.position -= Camera.main.transform.up * controlSpeed * Time.deltaTime;
        } 
        else if (Input.GetKey(KeyCode.Space))
        {
            transform.position += Camera.main.transform.up * controlSpeed * Time.deltaTime;
        }
    }

    float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f)
        {
            angle += 360f;
        }
        if (angle > 360)
        {
            angle -= 360f;
        }
        return Mathf.Clamp(angle, min, max);
    }
}
