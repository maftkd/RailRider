using System.Collections;
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
	float _moveSpeed=0.35f;//rate at which char moves along rail in segments/sec
	Transform _railTracker;
	float _balanceSpeed = 100;//degrees per second of rotation at full balanceVelocity
	float _balanceSpeedIncrease = 20;
	int _balanceState = 0;//0=no input, 1=left input, 2=right input
	float _balanceVelocity = 0;//rate at which Character rotates
	float _balanceAcceleration = 4f;//rate at which touch input affects velocity
	float _gravityPower = 2f;//linear offset to balance
	float _gravityThreshold = 10f;//min angle for grav to kick in
	float _momentumPower = 50f;//strength of momentum along curvature
	Transform _helmet;
	Dictionary<float, Coin> _coins = new Dictionary<float, Coin>();
	public Transform _coin;
	public AnimationCurve _indicatorCurve;
	float _indicatorHeight = 2f;
	float _indicatorWidth = .9f;
	FollowTarget _followTarget;
	float _crossThreshold = 0.001f;
	float _coinProbability = 0.3f;
	int _minCoinCluster=3;
	int _maxCoinCluster=8;
	int _tOffset=0;
	int _lookAheadTracks=8;

	struct Coin {
		public Transform transform;
		public LineRenderer line;
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
		
		//configure railTracker
		_railTracker=transform.GetChild(0);

		GenerateStartingSection();
		
		ResetRail();

		GenerateCoins(0,_knots.Count-1);

		//Get some references
		_helmet = GameObject.FindGameObjectWithTag("helmet").transform;
		_followTarget = Camera.main.transform.GetComponent<FollowTarget>();

		//start test
		StartCoroutine(Ride());
	}
	
	IEnumerator Ride(){
		float t=0;
		float balance=0;
		//this might need to get changed for an infinite track
		//more of a state based riding condition
		while(t-_tOffset<_knots.Count-1){
			//get input
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
			if(Input.GetKeyUp(KeyCode.Space)){
				Debug.Log("space pressed");
				_moveSpeed += 0.1f;
				//_followTarget._moveLerpSpeed += 2;
				_balanceSpeed += _balanceSpeedIncrease;
			}
			//handle balance logic
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

			//set player's root position and orientation
			balance-=_balanceVelocity*Time.deltaTime*_balanceSpeed;
			Vector3 prevForward = _railTracker.forward;
			float railT = t-_tOffset;
			_railTracker.position = _path.GetPoint(railT);
			_railTracker.forward = _path.GetTangent(railT);
			Vector3 localEuler = _railTracker.localEulerAngles;

			//physics
			float grav = Mathf.Abs(balance);
			grav = grav<_gravityThreshold ? 0 : Mathf.InverseLerp(0,90,grav)*Mathf.Sign(balance)*_gravityPower;
			float momentum = Vector3.Cross(prevForward, _railTracker.forward).y*_momentumPower;
			_followTarget.AdjustCamera(momentum);
			balance+=grav;
			balance-=momentum;
			localEuler.z = -balance;
			_railTracker.localEulerAngles=localEuler;

			//Check for point acquisitions
			foreach(float f in _coins.Keys){
				if(f>t && f <t+2){
					Coin c = _coins[f];
					float sqrMag = (c.transform.position-_helmet.position).sqrMagnitude;
					//if it's not in the ballpark just skip
					if(sqrMag<400){
						//this is not really ideal, we don't want to be setting this every frame they are close-by... I think...
						c.mesh.enabled=true;
						if(sqrMag>4){

							//animate the indicator line
							c.line.enabled=true;
							LineRenderer r = c.line;
							Vector3 pos0 = r.GetPosition(0);
							Vector3 pos1 = r.GetPosition(1);
							Vector3 dif = (pos1-pos0).normalized;
							float normDist = 1-(sqrMag/400f);
							pos1 = pos0+dif*_indicatorCurve.Evaluate(normDist)*_indicatorHeight;
							r.SetPosition(1,pos1);
							r.widthMultiplier=(1-normDist)*_indicatorWidth;
							//animate the coin along with line
							c.transform.position=pos1;
						}
						else
							c.line.enabled=false;
					}

					//this hit detection logic is continuous and not really valid (frame rate dependent)
					//temp code - need something more discrete
					if(sqrMag<.3f)
						Debug.Log("hit a coin");
				}
			}

			//check for new track gen if only X tracks lie ahead
			if(t-_tOffset>_knots.Count-_lookAheadTracks){

				int numTracks = GenerateNewTrack();

				int removed = RemoveOldTrack(t);

				ClearOldCoins(_tOffset+removed);

				ResetRail();

				_tOffset+=removed;

				GenerateCoins(_knots.Count-(numTracks+1),_knots.Count-1);
			}

			//tick for next frame
			t+=Time.deltaTime*_moveSpeed;
			yield return null;
		}
	}

	void GenerateStartingSection(){
		_knots.Add(Vector3.zero);
		_knots.Add(Vector3.forward*_nodeDist);
		//Starts out with 3 straights total
		AddStraight(2);
		//Then a long curve
		AddCurve(_nodeDist*Random.Range(3,5f),6,(Random.value < 0.5f));
	}

	void GenerateCoins(int startKnot, int endKnot){
		//coin generation
		int clusterCounter=0;
		float prevCross=0;
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
					Coin c;
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

						//instance the coin
						Transform curCoin = Instantiate(_coin,railPos+offset,Quaternion.identity, null);
						c.transform = curCoin;

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
					//see if we randomly create a cluster
					if(Random.value<_coinProbability)
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

	int GenerateNewTrack(){
		float val = Random.value;
		int numTracks = 0;
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
			AddCurve(radius,numTracks,(Random.value<0.5f));
		}
		else{
			//add a zig zag
			numTracks = Random.Range(2,12);
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
			_coins.Remove(f);
			Destroy(t.gameObject,Random.value);//stagger the recycling process over a second
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
		_turnCenters.Add(turnCenter); //this is for debugging (remove before ship)

		//Add knots
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
