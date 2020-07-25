using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
	public Transform _positionAnchor;
	public Transform _lookAt;
	public Vector3 _posOffset;
	Vector3 _posOffsetTarget;
	public float _moveLerpSpeed;
	public float _turnLerpSpeed;
	public float _yDiff;
	public bool _getPosOffsetAtStart;
	// Start is called before the first frame update
	void Start()
	{
		if (_getPosOffsetAtStart)
			_posOffset = transform.position - _positionAnchor.position;
		_posOffsetTarget=_posOffset;
	}

	// Update is called once per frame
	void Update()
	{
		Vector3 targetPos;
		_posOffset = Vector3.Lerp(_posOffset,_posOffsetTarget,1f*Time.deltaTime);
		targetPos = _positionAnchor.position + _positionAnchor.forward * _posOffset.z + _positionAnchor.right*_posOffset.x + Vector3.up * _yDiff;
		//handle cam look and target rotation
		transform.position = Vector3.Lerp(transform.position, targetPos, _moveLerpSpeed * Time.deltaTime);
		//transform.position = targetPos;
		Quaternion curRot = transform.rotation;
		transform.LookAt(_lookAt);
		Quaternion targetRot = transform.rotation;
		transform.rotation = Quaternion.Slerp(curRot,targetRot,_turnLerpSpeed*Time.deltaTime);
	}

	public void AdjustCamera(float momentum){
		if(momentum>0)
			_posOffsetTarget.x=Mathf.Abs(_posOffsetTarget.x);
		else
			_posOffsetTarget.x=-Mathf.Abs(_posOffsetTarget.x);
	}
}
