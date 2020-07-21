﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;

public class RailGenerator : MonoBehaviour
{
	CubicBezierPath _path;
	List<Vector3> _knots = new List<Vector3>();
	//temp code for debugging
	List<Vector3> _turnCenters = new List<Vector3>();
	LineRenderer _line;
	public Material _lineMat;
	float _nodeDist = 16f;//approximate segment length
	int _lineResolution = 10;//Number of points on line per segment
	float _moveSpeed=0.4f;//rate at which char moves along rail in segments/sec
	Transform _railTracker;
	float _balanceSpeed = 100;//degrees per second of rotation at full balanceVelocity
	int _balanceState = 0;//0=no input, 1=left input, 2=right input
	float _balanceVelocity = 0;//rate at which Character rotates
	float _balanceAcceleration = 4f;//rate at which touch input affects velocity
	float _gravityPower = 2f;//linear offset to balance
	float _gravityThreshold = 10f;//min angle for grav to kick in
	float _momentumPower = 50f;//strength of momentum along curvature
	
	// Start is called before the first frame update
	void Start()
	{
		//configure line renderer
		if(GetComponent<LineRenderer>()==null)
			_line = gameObject.AddComponent(typeof(LineRenderer)) as LineRenderer;
		else
			_line = GetComponent<LineRenderer>();
		_line.widthMultiplier=0.1f;
		_line.material=_lineMat;
		
		//configure railTracker
		_railTracker=transform.GetChild(0);

		//ok so lets try adding some nodes
		_knots.Add(Vector3.zero);
		_knots.Add(Vector3.forward*_nodeDist);

		//here we create a zig-zag with 10 cycles
		/*for(int i=0; i<10; i++){
			_knots.Add(_knots[_knots.Count-1]+new Vector3(.447f,0,.894f)*_nodeDist);
			_knots.Add(_knots[_knots.Count-1]+new Vector3(-.447f,0,.894f)*_nodeDist);
		}*/
		AddStraight(2);
		AddCurve(_nodeDist*2,3,false);
		//AddCurve(_nodeDist*2,3,true);
		AddZigZag(Mathf.PI/8f,4,false);
		AddCurve(_nodeDist*1.5f,3,true);
		AddStraight(5);
		//instantiate our cubic bezier path
		_path = new CubicBezierPath(_knots.ToArray());	
		
		//create a line to render the spline
		_line.positionCount=_lineResolution*(_path.GetNumCurveSegments());
		for(int i=0; i<_line.positionCount; i++){
			float t = i/(float)_lineResolution;
			_line.SetPosition(i,_path.GetPoint(t));
		}
		//start test
		StartCoroutine(TestRide());
		
	}
	
	IEnumerator TestRide(){
		float t=0;
		float balance=0;
		while(t<_knots.Count-1){
			balance+=Input.GetAxis("Horizontal")*Time.deltaTime*_balanceSpeed;
			if(Input.GetMouseButton(0))
			{
				Vector2 curPos = Input.mousePosition;
				if(curPos.x>Screen.width*.6f)
					_balanceState=2;
				else if(curPos.x<Screen.width*.4f)
					_balanceState=1;
			}
			else{
				_balanceState=0;
			}
			switch(_balanceState){
				case 0:
					//fall to 0
					_balanceVelocity=Mathf.Lerp(_balanceVelocity,0,_balanceAcceleration*Time.deltaTime);
					break;
				case 1:
					//climb to 1
					_balanceVelocity=Mathf.Lerp(_balanceVelocity,1f,_balanceAcceleration*Time.deltaTime);
					break;
				case 2:
					//climb to -1
					_balanceVelocity = Mathf.Lerp(_balanceVelocity,-1f,_balanceAcceleration*Time.deltaTime);
					break;
			}
			balance-=_balanceVelocity*Time.deltaTime*_balanceSpeed;
			Vector3 prevForward = _railTracker.forward;
			_railTracker.position = _path.GetPoint(t);
			_railTracker.forward = _path.GetTangent(t);
			Vector3 localEuler = _railTracker.localEulerAngles;
			//lets try some "physics here"
			float grav = Mathf.Abs(balance);
			grav = grav<_gravityThreshold ? 0 : Mathf.InverseLerp(0,90,grav)*Mathf.Sign(balance)*_gravityPower;
			float momentum = Vector3.Cross(prevForward, _railTracker.forward).y*_momentumPower;
			balance+=grav;
			balance-=momentum;
			localEuler.z = -balance;
			_railTracker.localEulerAngles=localEuler;
			t+=Time.deltaTime*_moveSpeed;
			yield return null;
		}
	}

	void AddStraight(int segments){
		Vector3 tan = _knots[_knots.Count-1]-_knots[_knots.Count-2];
		tan.Normalize();
		for(int i=0; i<segments; i++){
			_knots.Add(_knots[_knots.Count-1]+tan*_nodeDist);
		}
	}


	void AddZigZag(float angle, int zigzags, bool leftFirst){
		Vector3 tan = _knots[_knots.Count-1]-_knots[_knots.Count-2];
		float ang = Mathf.Atan2(tan.z,tan.x);
		int start= leftFirst ? 0 : 1;
		for(int i=start; i<zigzags; i++){
			float curAngle = i%2==0 ? ang+angle : ang-angle;
			_knots.Add(_knots[_knots.Count-1]+new Vector3(_nodeDist*Mathf.Cos(curAngle),0,_nodeDist*Mathf.Sin(curAngle)));
		}
	}


	void AddCurve(float radius, int sectors, bool toRight){
		//get the current rail head
		//get the center of the circle position
		//get circumference c=pi*2*r
		//get sectorfraction = circumference/_nodeDist	
		//get sectorFractionAngle = sectorFraction*2*PI
		float c = Mathf.PI*2*radius;
		float secFrac = _nodeDist/c;
		float secAngle = secFrac*Mathf.PI*2;
		secAngle = toRight ? secAngle*-1f : secAngle;
		//temp code - hardcoding
		Vector3 turnCenter = _knots[_knots.Count-1];
		Vector3 tan = turnCenter-_knots[_knots.Count-2];
		tan.Normalize();
		Vector3 right = Vector3.Cross(Vector3.up,tan);
		right = toRight? right : right*-1f;
		float angleOffset = Mathf.Atan2(-right.z,-right.x);
		turnCenter+=right*radius;
		_turnCenters.Add(turnCenter);
		for(int i=1; i<=sectors; i++){
			Vector3 pos;
			float ang = angleOffset+secAngle*i;
			pos = new Vector3(turnCenter.x+Mathf.Cos(ang)*radius,0,turnCenter.z+Mathf.Sin(ang)*radius);
			_knots.Add(pos);
		}
	}

	// Update is called once per frame
	void Update()
	{

	}

	void OnDrawGizmos(){
		if(_line!=null)
		{
			Gizmos.color = Color.green;
			for(int i=0; i<_line.positionCount; i++){
				Gizmos.DrawSphere(_line.GetPosition(i),.1f);
			}
		}
/*		if(_knots!=null)
		{
			Gizmos.color = Color.red;
			for(int i=0; i<_knots.Count; i++){
				Gizmos.DrawSphere(_knots[i],.5f);
			}
		}*/
		if(_turnCenters!=null){
			Gizmos.color = Color.blue;
			for(int i=0; i<_turnCenters.Count; i++){
				Gizmos.DrawSphere(_turnCenters[i],1f);
			}
		}
	}
}
