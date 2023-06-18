
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereRotation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {   
        //hacer que la esfera suba y baje poco 
        transform.Translate(0, Mathf.Sin(Time.time) * 0.001f, 0, Space.World);
        //rotar esfera sobre su eje Y lentamente
        transform.Rotate(0, 0.5f, 0, Space.Self);
    }
}
