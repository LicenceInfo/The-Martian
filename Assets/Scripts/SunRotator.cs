using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunRotator : MonoBehaviour
{
    public int dayLength;
    float rotationSpeed;

    // Start is called before the first frame update
    void Start()
    {
        rotationSpeed = 360f / (float)dayLength;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);
    }
}
