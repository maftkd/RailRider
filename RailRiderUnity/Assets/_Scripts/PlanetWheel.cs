using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetWheel : MonoBehaviour
{
	Transform _sun;
	Transform _planet;
	float _planetRatio;
	float _sunRatio;
	float _rate;
	float _hitThresh = 0.90f;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
		_planet.RotateAround(transform.position,transform.forward,_rate*Time.deltaTime);
		_planet.Rotate(-Vector3.forward*_rate*_planetRatio*Time.deltaTime);
		_sun.Rotate(Vector3.forward*_rate*Time.deltaTime*(1+_planetRatio*_sunRatio));
    }

	public void Init(float r){
		_sun = transform.GetChild(0);
		_planet = transform.GetChild(1);
		_planetRatio=16f/4f;
		_sunRatio=4f/7f;
		//random offset
		float rand = Random.Range(0,16)*22.5f;
		_planet.RotateAround(transform.position,transform.forward,rand);
		_sun.Rotate(Vector3.forward*rand);
		//assign rate
		_rate=r;
	}

	public bool IsSafe(Transform t){
		Vector3 dir = _planet.localPosition.normalized;
		dir.x*=-1f;
		float dot =Vector3.Dot(dir,t.up); 
		//and player is in line, collect it
		//Debug.Log("safety check: "+dot);
		return dot<_hitThresh;
	}

	public void StopSpinning(){
		_rate=0;
	}
}
