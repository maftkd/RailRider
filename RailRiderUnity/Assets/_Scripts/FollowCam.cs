using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
	Transform _camTransform;
	Camera _mainCam;
	public Transform _railTracker;
	float _followDistance=3f;
	public Transform _lookTarget;
	int _camState=0;
	//0 = menu
	//1 = intro
	//2 = gameplay
	//
	float _followHeight = 2.4f;
	//float _followPitch = -13.23f;
	Quaternion _targetRot, _startRot;
	Vector3 _targetPos, _startPos;
	float _introTimer;

	// Start is called before the first frame update
	void Start()
	{
		_camTransform = transform.GetChild(0);
		_mainCam = _camTransform.GetComponent<Camera>();
		transform.position = _railTracker.position-_railTracker.forward*_followDistance;
	}

	// Update is called once per frame
	void Update()
	{
		transform.position = _railTracker.position-_railTracker.forward*_followDistance;
		_camTransform.LookAt(_lookTarget);
		switch(_camState){
			case 0:
				break;
			case 1:
				//here we want to swing the camTransform to its position behind ethan
				_introTimer+=Time.deltaTime;
				if(_introTimer<3f){
					//_camTransform.rotation = Quaternion.Slerp(_startRot,_targetRot,_introTimer/3f);
					_camTransform.localPosition = Vector3.Lerp(_startPos,_targetPos,_introTimer/3f);
				}
				else
				{
					_camState=2;
				}

				break;
			case 2:
				//follow rail tracker
				//transform.position = _railTracker.position-_railTracker.forward*_followDistance;

				//look at ethan
				//_camTransform.LookAt(_lookTarget);

				//check for upside down
				float railRoll = _railTracker.localEulerAngles.z;
				if(railRoll>90 && railRoll<270){
					Debug.Log("we upside down mah love");
				}
				break;
		}
	}

	public void SetCamToFollow(){
		_camState=1;
		//_startRot = _camTransform.rotation;
		//_camTransform.localEulerAngles = Vector3.right*_followPitch;
		//_camTransform.LookAt(_lookTarget);
		//_targetRot=_camTransform.rotation;
		//_camTransform.rotation=_startRot;

		_targetPos=Vector3.up*_followHeight;
		_startPos=_camTransform.localPosition;
	}

}
