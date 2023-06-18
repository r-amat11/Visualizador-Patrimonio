using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orientation;

    float xRotation;
    float yRotation;
    bool gircam;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = true;
        gircam = true;
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {   
            if(gircam) {
                
            float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
            float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

            yRotation += mouseX;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
            orientation.rotation = Quaternion.Euler(0, yRotation, 0);
            }  
        }
            
    }

    public void updateGirCam(bool value) {
        gircam = value;
    }
}
