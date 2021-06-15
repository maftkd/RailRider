using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hoverboard : MonoBehaviour
{
	public int _cost;
	public string _name;
	public float _balance;
	public float _speed;
	public float _turning;
	[HideInInspector]
	public bool _rotate;
	float _maxSpinSpeed=900;
	float _minSpinSpeed=45;
	float _spinSpeed;
	bool _prevRotate;

	void Update(){
		if(_rotate)
		{
			_spinSpeed=Mathf.Lerp(_spinSpeed,_minSpinSpeed,Time.deltaTime*2);
			transform.Rotate(Vector3.up*Time.deltaTime*_spinSpeed);
			transform.Rotate(Vector3.right*Time.deltaTime*_spinSpeed);
		}

		if(_prevRotate!=_rotate)
		{
			_spinSpeed=_maxSpinSpeed;
		}
		_prevRotate=_rotate;
	}
}
