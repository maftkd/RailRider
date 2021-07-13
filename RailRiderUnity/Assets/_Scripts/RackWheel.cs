using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RackWheel : MonoBehaviour
{
	float _rate;
	Transform _pinion;
	Transform _gear;
	float _ratio;
	float _hitThresh = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
		_pinion.Rotate(Vector3.forward*_rate*Time.deltaTime);
		_gear.Rotate(Vector3.back*_rate*_ratio*Time.deltaTime);
    }

	public void Init(float r){
		_pinion = transform.GetChild(0);
		_gear = _pinion.GetChild(0);
		_ratio=16f/7f;
		//random offset
		float rand = Random.Range(0,16)*22.5f;
		_pinion.localEulerAngles=Vector3.forward*rand;
		//assign rate
		_rate=r;
	}

	public bool IsSafe(Transform t){
		float dot =Vector3.Dot(_pinion.up,t.up); 
		//and player is in line, collect it
		//Debug.Log("safety check: "+dot);
		return dot<_hitThresh;
	}

	public void StopSpinning(){
		_rate=0;
	}
}
