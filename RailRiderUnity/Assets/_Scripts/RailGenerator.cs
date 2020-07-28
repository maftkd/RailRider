﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using SplineMesh;

public class RailGenerator : MonoBehaviour
{
	CubicBezierPath _path;
	List<Vector3> _knots = new List<Vector3>();
	LineRenderer _line;
	public Material _lineMat;
	float _nodeDist = 16f;//approximate segment length
	int _lineResolution = 10;//Number of points on line per segment
	float _moveSpeed=0.35f;//rate at which char moves along rail in segments/sec
	Transform _railTracker;
	float _balance;
	float _t;
	float _balanceSpeed = 100;//degrees per second of rotation at full balanceVelocity
	float _balanceSpeedIncrease = 20;
	int _balanceState = 0;//0=no input, 1=left input, 2=right input
	float _balanceVelocity = 0;//rate at which Character rotates
	float _balanceAcceleration = 8f;//rate at which touch input affects velocity
	Transform _helmet;
	Dictionary<float, Coin> _coins = new Dictionary<float, Coin>();
	public Transform _coin;
	float _coinHeight = 1.5f;
	float _crossThreshold = 0.001f;
	float _coinProbability = 0.15f;
	int _minCoinCluster=3;
	int _maxCoinCluster=12;
	int _tOffset=0;
	int _lookAheadTracks=8;
	public Transform _jumper;
	float _lastJump;
	float _jumpThreshold=1.1f;//spacing between jumps and other jumps
	float _minJumpThreshold=0.6f;
	float _jumpSpacing=0.5f;//spacing between coins and jumps
	float _minJumpSpacing=0.2f;
	float _lineResFrac;
	Transform _ethan;
	bool _jumping=false;
	float _jumpHeight = 1.85f;
	float _jumpDur = 0.65f;
	float _minJumpY = 1f;//min height to clear a coin
	public AnimationCurve _jumpCurve;
	float _spinSpeed=360f;
	float _inputDelayTimer=0;
	float _inputDelay=.05f;
	Dictionary<float, Jumper> _jumpers = new Dictionary<float, Jumper>();
	[ColorUsageAttribute(false,true)]
	public Color _coinHitColor;
	int _collectedCoins;
	float _coinHitThreshold = .96f;
	int _gameState=0;
	//0 = menu
	//1 = play
	//2 = collided with jumper
	//3 = gate check
	float _nextGate;//The time at which a new gate will be generated
	int _gatePos;//The position of the new gate
	Transform _gate;
	float _minGateSpace=5;
	float _maxGateSpace=7;
	float _gateGrowth = 1.2f;
	public UnityEvent _jumpHit;//essentially game over event
	CanvasGroup _gateMenu;
	Text _scoreText;
	float _maxSpeed=1f;
	float _speedIncreaseRate = 0.1f;//rate of speed increase 
	float _balanceSpeedMultiplier=250;
	public Text _tDebug,_ngDebug,_gpDebug;

	struct Coin {
		public Transform transform;
		public LineRenderer line;
		public MeshRenderer mesh;
		public bool collected;
		public Vector3 offset;
	}

	struct Jumper {
		public Transform transform;
		public MeshRenderer mesh;
	}
	
	// Start is called before the first frame update
	void Start()
	{
		//configure line renderer
		if(GetComponent<LineRenderer>()==null)
			_line = gameObject.AddComponent(typeof(LineRenderer)) as LineRenderer;
		else
			_line = GetComponent<LineRenderer>();
		_line.widthMultiplier=0.8f;
		_line.material=_lineMat;
		_lineResFrac=1/(float)_lineResolution;
		
		//configure railTracker
		_railTracker=transform.GetChild(0);

		GenerateStartingSection();
		
		ResetRail();

		GenerateCoins(0,_knots.Count-1);

		//set speed
		_balanceSpeed = _moveSpeed*_balanceSpeedMultiplier;

		//GenerateJumpers(0,_knots.Count-1);

		//This sets number of coins to 0
		AddCoin(0);

		//Get some references
		_helmet = GameObject.FindGameObjectWithTag("Helmet").transform;
		_ethan = _railTracker.GetChild(0); 
		_gate = GameObject.FindGameObjectWithTag("Gate").transform;
		_gateMenu = GameObject.Find("GateMenu").GetComponent<CanvasGroup>();
		_scoreText = _gateMenu.transform.Find("Score").GetComponent<Text>();
	}
	
	//function used in update loop to reset velocity towards 0
	void LimitVelocity(){
		if(_balanceVelocity!=0){
			_balanceVelocity=Mathf.Lerp(_balanceVelocity,0,_balanceAcceleration*Time.deltaTime);
			if(Mathf.Abs(_balanceVelocity)<0.01f)
				_balanceVelocity=0;
		}
	}

	//jump
	IEnumerator JumpRoutine(){
		_jumping=true;
		_balanceVelocity=0;
		_inputDelayTimer=0;
		float timer=0;
		Vector3 startPos = _ethan.localPosition;
		Vector3 endPos = startPos+Vector3.up*_jumpHeight;
		while(timer<_jumpDur){
			timer+=Time.deltaTime;
			_ethan.localPosition = Vector3.LerpUnclamped(startPos,endPos,_jumpCurve.Evaluate(timer/_jumpDur));			
			yield return null;
		}
		_ethan.localPosition=startPos;
		_jumping=false;
		_inputDelayTimer=-_inputDelay*2;
	}

	void GenerateStartingSection(){
		//we add the previous node just so it looks like there's a stretch
		//of rail from the start menu
		_knots.Add(Vector3.back*_nodeDist);
		_knots.Add(Vector3.zero);
		_t=1f;
		_knots.Add(Vector3.forward*_nodeDist);

		//Starts out with 3 straights total
		AddStraight(2);

		//Then a long curve
		AddCurve(_nodeDist*Random.Range(3,5f),3,(Random.value < 0.5f));

		//so we have 11 sections so far
		_nextGate = Random.Range(_minGateSpace,_maxGateSpace);//remember this is not the location of the gate but the tOffset at which the gate spawns
		_gatePos=1024;//something arbitrarily high at the start - will be reset in AddGate()
	}

	void GenerateCoins(int startKnot, int endKnot){
		//coin generation
		int clusterCounter=0;
		float prevCross=0;
		//more temp code for jumper
		for(int i=startKnot*_lineResolution; i<endKnot*_lineResolution; i++){
			//converts line space to coin space
			float t = i/(float)_lineResolution;
			float key = t+_tOffset;

			//calculates the local position and tengent at t along rail
			Vector3 railPos = _path.GetPoint(t);
			Vector3 curForward = _path.GetTangent(t);

			//validate rail index (don't add any past the end)
			if(i<_line.positionCount-1){

				//If there are still coins to be added in a cluster
				if(clusterCounter>0){
					//declare coin struct
					Coin c = new Coin();
					//determine the curvature
					Vector3 nextForward = _path.GetTangent(t+1f/(float)_lineResolution);
					float cross = Vector3.Cross(curForward,nextForward).y*.1f;

					//make sure the delta isn't nuts
					if(prevCross!=0 && Mathf.Abs(cross-prevCross)>0.1f)
					{
						clusterCounter=0;
					}
					else{
						//rail curvature is OK
						//calculate the coins offset direction
						Vector3 right = Vector3.Cross(Vector3.up,curForward);
						Vector3 offset = Vector3.LerpUnclamped(Vector3.up,right,cross);
						offset.Normalize();
						c.offset=offset;

						//instance the coin
						Transform curCoin = Instantiate(_coin,railPos+offset*_coinHeight,Quaternion.identity, null);
						curCoin.LookAt(railPos);
						curCoin.localScale=new Vector3(.5f,1f,2f);
						c.transform = curCoin;

						c.collected=false;

						//set the line data
						LineRenderer curLine = curCoin.GetComponent<LineRenderer>();
						curLine.SetPosition(0,railPos);
						//offset by .3 so the direction is preserved, but the magnitude will be controlled in the Ride CoRoutine
						curLine.SetPosition(1,railPos+offset*.3f);
						c.line=curLine;

						//get the coin mesh data
						c.mesh = curCoin.GetComponent<MeshRenderer>();

						//add coin to coin dict
						if(!_coins.ContainsKey(key)){
							_coins.Add(key,c);
						}
						clusterCounter--;
						prevCross=cross;
					}
				}
				//If we are not currently generating a cluster
				else{
					bool nearJump=false;
					foreach(float k in _jumpers.Keys){
						if(Mathf.Abs(k-key)<_jumpSpacing)
						{
							nearJump=true;
							break;
						}
					}
						
					//see if we randomly create a cluster
					if(!nearJump && Random.value<_coinProbability)
					{
						//Make sure we don't generate coins on straight sections because that is boring and the coins are harder to see
						Vector3 nextForward = _path.GetTangent(t+1f/(float)_lineResolution);
						float cross = Vector3.Cross(curForward,nextForward).y*.1f;
						if(Mathf.Abs(cross)>_crossThreshold)
						{
							//Determine cluster size
							clusterCounter=Random.Range(_minCoinCluster,_maxCoinCluster+1);
							prevCross=0;
						}
					}
				}
			}
		}
	}

	void GenerateJumpers(int startKnot, int endKnot){
		
		for(int i=startKnot*_lineResolution; i<endKnot*_lineResolution; i++){
			//converts line space to coin space
			float t = i/(float)_lineResolution;
			float key = t+_tOffset;

			//calculates the local position and tengent at t along rail
			Vector3 railPos = _path.GetPoint(t);
			Vector3 curForward = _path.GetTangent(t);

			//validate rail index (don't add any past the end)
			if(i<_line.positionCount-1){

				//if jump is far enough from another jump and rng hits, then spawn a jumper
				if(key-_lastJump>_jumpThreshold && Random.value<0.05){
					
					_lastJump=key;
					Transform jumper = Instantiate(_jumper,railPos,Quaternion.identity, null);
					jumper.forward=-curForward;

					//add to jumper dict
					Jumper j;
					j.transform = jumper;
					j.mesh = jumper.GetComponent<MeshRenderer>();
					j.mesh.enabled=false;
					_jumpers.Add(key, j);

					//clear out nearby coins
					List<float> removeList = new List<float>();
					foreach(float k in _coins.Keys){
						if(Mathf.Abs(k-key)<_jumpSpacing){
							Transform trans = _coins[k].transform;
							Destroy(trans.gameObject,Random.value*.2f);
							removeList.Add(k);
						}
					}
					foreach(float k in removeList){
						_coins.Remove(k);
					}
				}
			}
		}
	}

	int GenerateNewTrack(){
		float val = Random.value;
		int numTracks = 0;
		//before we do any of this probability stuff
		//is current tOffset past or equal to the gate threshold?
		//if so generate a gate
		if(_t>=_nextGate){
			AddGate();
			return -1;
		}
		if(val<.33f){
			numTracks = Random.Range(1,4);
			//add a straight
			AddStraight(numTracks);
		}
		else if(val<.67f){
			//add a curve
			float trackToRad = Random.Range(1,6f);
			float radius = _nodeDist*trackToRad;
			numTracks = Random.Range(1,Mathf.FloorToInt(3*trackToRad));
			numTracks = Mathf.Min(5,numTracks);
			AddCurve(radius,numTracks,(Random.value<0.5f));
		}
		else{
			//add a zig zag
			numTracks = Random.Range(2,8);
			AddZigZag(Mathf.PI/Random.Range(5f,12f),numTracks,(Random.value<0.5f));
		}
		return numTracks;
	}

	int RemoveOldTrack(float t){
		//remove old tracks
		int removeUntil = Mathf.FloorToInt(t-_tOffset);
		int removedTracks = 0;
		for(int i=removeUntil-3; i>=0; i--)
		{
			_knots.RemoveAt(i);
			removedTracks++;
		}
		return removedTracks;
	}


	void ClearOldCoins(int endClear){

		//Determine which coins need to be removed
		List<float> deleteKeys = new List<float>();
		foreach(float f in _coins.Keys){
			if(f<endClear)
				deleteKeys.Add(f);
			else
				break;
		}
		
		//Destroy coins and remove from dict
		foreach(float f in deleteKeys){
			Transform t = _coins[f].transform;
			Destroy(t.gameObject,Random.value);//stagger the recycling process over a second
			_coins.Remove(f);
		}
	}

	void ClearOldJumpers(int endClear){
		List<float> deleteKeys = new List<float>();
		foreach(float f in _jumpers.Keys){
			if(f<endClear)
				deleteKeys.Add(f);
			else
				break;
		}
		foreach(float f in deleteKeys){
			Transform t = _jumpers[f].transform;
			Destroy(t.gameObject, Random.value);
			_jumpers.Remove(f);
		}
	}

	void ResetRail(){
		//instantiate our cubic bezier path
		_path = new CubicBezierPath(_knots.ToArray());	
		
		//Rail (line renderer) setup
		_line.positionCount=_lineResolution*(_path.GetNumCurveSegments());
		for(int i=0; i<_line.positionCount; i++){
			//set the rail position at point t along rail
			float t = i/(float)_lineResolution;
			Vector3 railPos = _path.GetPoint(t);
			Vector3 curForward = _path.GetTangent(t);
			_line.SetPosition(i,railPos);
		}
	}

	void AddGate(){
		AddStraight(3);
		_gatePos = _knots.Count-3+_tOffset;
		_nextGate=_gatePos+Random.Range(_minGateSpace,_maxGateSpace);
		Vector3 railLine = _knots[_knots.Count-2]-_knots[_knots.Count-3];
		railLine.Normalize();
		_gate.position = _knots[_knots.Count-3]+railLine*_nodeDist*.5f;
		_gate.position+=Vector3.up*1.5f;
		_gate.up=railLine;
		Vector3 localEulers = _gate.localEulerAngles;
		localEulers.x=90f;
		_gate.localEulerAngles = localEulers;
		//_gate.localScale = new Vector3(5,5,_nodeDist);
		_gate.GetComponent<MeshRenderer>().enabled=true;
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

		//calculate the angle for each section
		float c = Mathf.PI*2*radius;
		float secFrac = _nodeDist/c;
		float secAngle = secFrac*Mathf.PI*2;
		secAngle = toRight ? secAngle*-1f : secAngle;

		//calculate the circle's center
		Vector3 turnCenter = _knots[_knots.Count-1];
		Vector3 tan = turnCenter-_knots[_knots.Count-2];
		tan.Normalize();
		Vector3 right = Vector3.Cross(Vector3.up,tan);
		right = toRight? right : right*-1f;
		float angleOffset = Mathf.Atan2(-right.z,-right.x);
		turnCenter+=right*radius;
		//_turnCenters.Add(turnCenter); //this is for debugging (remove before ship)

		//Add knots
		for(int i=1; i<=sectors; i++){
			Vector3 pos;
			float ang = angleOffset+secAngle*i;
			pos = new Vector3(turnCenter.x+Mathf.Cos(ang)*radius,0,turnCenter.z+Mathf.Sin(ang)*radius);
			_knots.Add(pos);
		}
	}

	void AddCoin(int setValue=-1){
		if(setValue==-1)
			_collectedCoins++;
		else
			_collectedCoins=setValue;
		/*
		float coinFrac = _collectedCoins/(float)_requiredCoins;
		if(coinFrac<1)
			_lineMat.SetColor("_EdgeColor",new Color(1f,coinFrac,1-coinFrac));
		else
			_lineMat.SetColor("_EdgeColor",Color.green);
			*/
	}

	public void StartRiding(){
		_gameState=1;
	}


	// Update is called once per frame
	void Update()
	{
		switch(_gameState){
			case 0://menu
				break;
			case 1://gameplay
				//multi-touch input loop
				_balanceState=0;
				foreach(Touch touch in Input.touches){
					Vector2 touchPos = touch.position;
					if(touchPos.x < Screen.width*0.4f){
						_balanceState+=1;
					}
					else if(touchPos.x > Screen.width*0.6f){
						_balanceState+=2;
					}
				}	
	#if UNITY_EDITOR
				float keyInput = Input.GetAxis("Horizontal");
				if(keyInput<0)
					_balanceState=1;
				else if(keyInput>0)
					_balanceState=2;
				else
					_balanceState=0;
	#endif
				//reset input delay
				if(_balanceState==0)
					_inputDelayTimer=0;

				//temp code for testing speed increase
				if(Input.GetKeyUp(KeyCode.Space)){
					_moveSpeed += 0.1f;
					_balanceSpeed += _balanceSpeedIncrease;
				}
				//temp code for testing jump
				if(Input.GetKey(KeyCode.UpArrow)){
					if(!_jumping)
						StartCoroutine(JumpRoutine());
				}

				//handle balance logic
				switch(_balanceState){
					case 0:
						//fall to 0
						LimitVelocity();
						break;
					case 1:
						//climb to 1
						if(!_jumping)
						{
							_balanceVelocity=Mathf.Lerp(_balanceVelocity,1f,_balanceAcceleration*Time.deltaTime);
						}
						//temp code for air spins
						else
							_ethan.Rotate(0,-_spinSpeed*Time.deltaTime, 0);
						break;
					case 2:
						//climb to -1
						if(!_jumping)
							_balanceVelocity = Mathf.Lerp(_balanceVelocity,-1f,_balanceAcceleration*Time.deltaTime);

						//temp code for air spins
						else
							_ethan.Rotate(0,_spinSpeed*Time.deltaTime, 0);
						break;
					case 3:
						//jump
						if(!_jumping)
							StartCoroutine(JumpRoutine());
						//and fall to 0
						LimitVelocity();
						break;
				}

				//set player's root position and orientation
				_balance-=_balanceVelocity*Time.deltaTime*_balanceSpeed;
				Vector3 prevForward = _railTracker.forward;
				SetPosition();

				//Check for point acquisitions
				foreach(float f in _coins.Keys){
					if(f>_t-1 && f <_t+2){
						Coin c = _coins[f];

						//disable coins
						if(f<_t-.5f)
							c.mesh.enabled=false;

						//enable coins
						else
						{
							c.mesh.enabled=true;

							//coinDetection
							if(Mathf.Abs(f-_t)<.05f && c.transform.tag=="Untagged"){
								if(Vector3.Dot(c.offset,_railTracker.up)>_coinHitThreshold)
								{
									c.collected=true;
									c.mesh.material.SetColor("_Color",_coinHitColor);
									c.transform.tag="Collected";
									AddCoin();
								}
							}
						}
					}
				}

				//check for jumper collisions
				foreach(float f in _jumpers.Keys){
					if(f>_t-1 && f<_t+2){
						_jumpers[f].mesh.enabled=true;
						
						if(Mathf.Abs(f-_t)<.02f && _ethan.localPosition.y < _minJumpY){
							_gameState=2;
							_jumpHit.Invoke();
						}
					}
				}

				//check for gate
				if(_t>=_gatePos)
				{
					_gameState=3;
					StartCoroutine(GateRoutine());
				}

				//check for new track gen if only X tracks lie ahead
				if(_t-_tOffset>_knots.Count-_lookAheadTracks){

					int numTracks = GenerateNewTrack();

					int removed = RemoveOldTrack(_t);

					ClearOldCoins(_tOffset+removed);
					ClearOldJumpers(_tOffset+removed);

					ResetRail();

					_tOffset+=removed;

					if(numTracks!=-1){
						GenerateCoins(_knots.Count-(numTracks+1),_knots.Count-1);
						GenerateJumpers(_knots.Count-(numTracks+1),_knots.Count-1);
					}
				}
				_t+=Time.deltaTime*_moveSpeed;
				break;
			case 2://collide with jumper
				break;
			case 3://gate check
				break;
		}
		//_tDebug.text="T: "+_t.ToString("#.#");
		//_ngDebug.text="NG: "+_nextGate.ToString("#.#");
		//_gpDebug.text="GP: "+_gatePos.ToString("#.#");
	}

	void SetPosition(){
		float railT = _t-_tOffset;
		_railTracker.position = _path.GetPoint(railT);
		_railTracker.forward = _path.GetTangent(railT);
		Vector3 localEuler = _railTracker.localEulerAngles;

		//physics
		//float grav = Mathf.Abs(_balance);
		//grav = grav<_gravityThreshold ? 0 : Mathf.InverseLerp(0,90,grav)*Mathf.Sign(balance)*_gravityPower;
		//float momentum = Vector3.Cross(prevForward, _railTracker.forward).y*_momentumPower;
		//balance+=grav;
		//balance-=momentum;
		localEuler.z = -_balance;
		_railTracker.localEulerAngles=localEuler;
	}

	IEnumerator GateRoutine(){
		//ease out speed from t = _gatePos to t = _gatePos+.5
		yield return null;
		while(_t < _gatePos+.5f){
			SetPosition();
			_t+=_moveSpeed*Time.deltaTime;
			yield return null;
		}
		//raise menu
		float timer = 0;
		while(timer < 0.5f && !Input.GetMouseButtonDown(0)){
			timer+=Time.deltaTime;
			_gateMenu.alpha=timer*2f;
			yield return null;
		}
		_gateMenu.alpha=1;
		//increase score
		/*
		int prevScore = -1;
		if(int.TryParse(_scoreText.text, out prevScore)){
			_scoreText.fontSize=250;
			while(prevScore<_collectedCoins){
				prevScore++;
				_scoreText.text = prevScore.ToString();
				yield return null;
			}
			_scoreText.fontSize=200;
		}
		*/
		//prompt continue
		while(!Input.GetMouseButtonDown(0)){
			yield return null;
		}
		//hide menu
		timer=0;
		while(timer < 0.5f){
			timer+=Time.deltaTime;
			_gateMenu.alpha=1-timer*2f;
			yield return null;
		}
		_gateMenu.alpha=0;
		
		//Actually try not increasing speed
		//maybe temporary speed boosts could be fun though...
		//increase speed
		//_moveSpeed = Mathf.Lerp(_moveSpeed,_maxSpeed,_speedIncreaseRate);
		_balanceSpeed = _moveSpeed*_balanceSpeedMultiplier;
		_jumpSpacing = Mathf.Lerp(_jumpSpacing,_minJumpSpacing,_speedIncreaseRate);
		_jumpThreshold = Mathf.Lerp(_jumpThreshold,_minJumpThreshold,_speedIncreaseRate);	

		_minGateSpace*=_gateGrowth;
		_maxGateSpace*=_gateGrowth;
		_gatePos=Mathf.FloorToInt(_nextGate)+50;//this will get reset when the next gate is generated
		StartRiding();
	}

	void OnDrawGizmos(){
		/*
		if(_line!=null)
		{
			Gizmos.color = Color.green;
			for(int i=0; i<_line.positionCount; i++){
				Gizmos.DrawSphere(_line.GetPosition(i),.1f);
			}
		}
		if(_knots!=null)
		{
			Gizmos.color = Color.red;
			for(int i=0; i<_knots.Count; i++){
				Gizmos.DrawSphere(_knots[i],.5f);
			}
		}
		if(_turnCenters!=null){
			Gizmos.color = Color.blue;
			for(int i=0; i<_turnCenters.Count; i++){
				Gizmos.DrawSphere(_turnCenters[i],1f);
			}
		}
		*/
	}
}
