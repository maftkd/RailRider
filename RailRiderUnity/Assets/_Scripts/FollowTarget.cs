﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
	public Transform _target;
	public Vector3 _posOffset;
	public Vector3 _altOffset;
	public float _moveLerpSpeed;
	public float _turnLerpSpeed;
	public float _yDiff;
	public bool _getPosOffsetAtStart;
	bool _followBehaviour=true;
	float _momentumThreshold = 0.001f;
	// Start is called before the first frame update
	void Start()
	{
		if (_getPosOffsetAtStart)
			_posOffset = transform.position - _target.position;
	}

	// Update is called once per frame
	void Update()
	{
		Vector3 targetPos;
		if (_followBehaviour)
		{
			targetPos = _target.position + _target.forward * _posOffset.z + _target.right*_posOffset.x + Vector3.up * _yDiff;
		}
		else
		{
			targetPos = _target.position + _target.forward * _altOffset.z + _target.right*_altOffset.x + Vector3.up * _yDiff;
		}
		//handle cam look and target rotation
		transform.position = Vector3.Lerp(transform.position, targetPos, _moveLerpSpeed * Time.deltaTime);
		Quaternion curRot = transform.rotation;
		transform.LookAt(_target);
		Quaternion targetRot = transform.rotation;
		transform.rotation = Quaternion.Slerp(curRot,targetRot,_turnLerpSpeed*Time.deltaTime);
	}

	public void AdjustCamera(float momentum){
		/*
		if(Mathf.Abs(momentum)<_momentumThreshold)
			_followBehaviour=false;
		else
			_followBehaviour=true;
			*/
	}
}
