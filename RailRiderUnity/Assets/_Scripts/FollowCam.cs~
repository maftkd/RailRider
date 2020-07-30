using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
	Transform _camLerpTarget;
	Transform _camTransform;
	Camera _cam;
	public Transform _railTracker;
	float _followDistance=3f;
	public Transform _lookTarget;
	int _camState=0;
	//0 = menu
	//1 = intro
	//2 = gameplay
	//
	float _followHeight = 2.65f;
	//float _followPitch = -13.23f;
	Quaternion _targetRot, _startRot;
	Vector3 _targetPos, _startPos;
	float _introTimer;
	float _lerpSpeed=4f;
	float _slerpSpeed=4f;

	// Start is called before the first frame update
	void Start()
	{
		_camLerpTarget = transform.GetChild(0);
		_camTransform = transform.GetChild(1);
		_cam =_camTransform.GetComponent<Camera>();
		transform.position = _railTracker.position-_railTracker.forward*_followDistance;
	}

	// Update is called once per frame
	void Update()
	{
		transform.position = _railTracker.position-_railTracker.forward*_followDistance;
		_camLerpTarget.LookAt(_lookTarget);
		transform.up = _railTracker.up;
		switch(_camState){
			case 0:
				break;
			case 1:
				//here we want to swing the camTransform to its position behind ethan
				_introTimer+=Time.deltaTime;
				if(_introTimer<3f){
					_camLerpTarget.localPosition = Vector3.Lerp(_startPos,_targetPos,_introTimer/3f);
				}
				else
				{
					_camState=2;
				}

				break;
			case 2:
				break;
		}
		_camTransform.localPosition = Vector3.Lerp(_camTransform.localPosition,_camLerpTarget.localPosition,_lerpSpeed*Time.deltaTime);
		_camTransform.rotation = Quaternion.Slerp(_camTransform.rotation,_camLerpTarget.rotation,_slerpSpeed*Time.deltaTime);
		Vector3 eulers = _camTransform.eulerAngles;
		eulers.z=0;
		_camTransform.eulerAngles=eulers;
	}

	public void SetCamToFollow(){
		_camState=1;
		_targetPos=Vector3.up*_followHeight;
		_startPos=_camLerpTarget.localPosition;
	}

}
