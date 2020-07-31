using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunRotator : MonoBehaviour
{
	public bool _rotating=true;
	public float _spinSpeed;
	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		if(_rotating)
			transform.Rotate(_spinSpeed*Time.deltaTime,0,0);
	}
}
