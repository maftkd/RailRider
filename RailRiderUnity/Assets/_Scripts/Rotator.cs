using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
	public float _speed;
	public int _rotCode;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if(_rotCode==0)
			transform.Rotate(Vector3.forward*_speed*Time.deltaTime);
		else
			transform.Rotate(Vector3.up*_speed*Time.deltaTime);
    }
}
