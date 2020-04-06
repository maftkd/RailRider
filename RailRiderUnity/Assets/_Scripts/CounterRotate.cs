using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CounterRotate : MonoBehaviour
{
    Quaternion _startRot;
    // Start is called before the first frame update
    void Start()
    {
        _startRot = transform.rotation;
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = _startRot;
    }
}
