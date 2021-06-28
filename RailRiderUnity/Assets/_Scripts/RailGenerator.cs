using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.Events;

public class RailGenerator : MonoBehaviour
{
	CubicBezierPath _path;
	List<Vector3> _knots = new List<Vector3>();
	LineRenderer _line;
	public Material _lineMat;
	float _nodeDist = 16f;//approximate segment length
	int _lineResolution = 10;//Number of points on line per segment
	float _moveSpeed=0.6f;//rate at which char moves along rail in segments/sec
	float _targetMoveSpeed;
	float _maxSpeed=0.8f;//speed obtained through tunnel
	float _speedChangeLerp=.12f;
	Transform _railTracker;
	float _balance;
	float _t;//measurement of time progress throughout the run
	float _balanceSpeed = 100;//degrees per second of rotation at full balanceVelocity
	int _balanceState = 0;//0=no input, 1=left input, 2=right input
	float _balanceVelocity = 0;//rate at which Character rotates
	float _balanceAcceleration = 3f;//rate at which touch input affects velocity
	float _balanceDecceleration = 1f;//rate at which touch velocity slows
	float _minVel = 0.01f;//min velocity before balance clamped to 0
	float _maxVel = 2f;
	Transform _helmet;
	Dictionary<float, Coin> _coins = new Dictionary<float, Coin>();
	public Transform _coin;
	float _coinHeight = 1.5f;
	public AnimationCurve _coinHeightCurve;
	float _crossThreshold = 0.001f;
	float _coinProbability = 0.05f;
	int _minCoinSpacing = 15;
	int _maxCoinSpacing = 30;
	int _minCoinCluster=3;
	int _maxCoinCluster=12;
	int _tOffset=0;//subtract from _t to get time about current sections of rail
	int _lookAheadTracks=8;
	public Transform _jumper;
	float _lastJump;
	float _jumpProbability = 0.05f;
	float _maxJumpProbability = .25f;
	float _jumpThreshold=1.5f;//spacing between jumps and other jumps
	float _minJumpThreshold=0.4f;
	float _jumpSpacing=0.6f;//spacing between coins and jumps
	float _minJumpSpacing=0.1f;
	public Transform _dynaMesh;
	float _lineResFrac;
	Transform _ethan;
	bool _jumping=false;
	bool _crouching=false;
	float _jumpHeight = 2.5f;//1.85f;
	float _jumpDur = 0.7f;
	float _minJumpY = 1f;//min height to clear a coin
	public AnimationCurve _jumpCurve;
	float _inputDelayTimer=0;
	float _inputDelay=.05f;
	Dictionary<float, Jumper> _jumpers = new Dictionary<float, Jumper>();
	[ColorUsageAttribute(false,true)]
	public Color _coinHitColor;
	int _collectedCoins;
	float _coinHitThreshold = .95f;
	public int _gameState=0;
	//0 = menu
	//1 = play
	//2 = collided with jumper
	//3 = gate check
	float _nextGate;//The time at which a new gate will be generated
	int _gatePos;//The position of the new gate
	Transform _gate;
	float _minGateSpace=1;
	float _maxGateSpace=30;
	public UnityEvent _jumpHit;//essentially game over event
	Text _scoreText;
	Text _scoreTextShadow;
	Text _comboText;
	Text _trickText;
	RectTransform _trickTextRect;
	float _trickTextTimer;
	float _maxTrickTextTimer=3f;
	float _spinTrick;
	public AnimationCurve _popup;
	CanvasGroup _scoreCanvas;
	public CanvasGroup _authorCanvas;
	float _jumpIncreaseRate = 0.3f;//rate of speed increase 
	float _balanceSpeedMultiplier=280;
	public Text _tDebug,_ngDebug,_gpDebug;
	public GameObject[] _wsText;
	float _scoreChangeTimer;
	int _defaultScoreFont;
	int _boldScoreFont;
	public Text _hsText;
	bool _tutorial;
	public AudioSource _music;
	public GameObject _tutorialObjs;
	Camera _main;
	float _crossMultiplier=-75f;//-125f;//-250f;
	//#dothis
	float _curvature;
	float _corkAngle;
	float _corkSpacing;
	Transform _explosion;
	float _invincibleSpeed=.65f;
	float _invincibleWarning=.05f;
	CanvasGroup _invincDisplay;
	Image _invincMeter;
	public Material _ethanMat;
	public Gradient _invincibilityGrad;
	public AudioSource _takeOff;
	public AudioSource _landing;
	public AudioSource _woosh;
	public AudioSource _smash;
	public AudioSource _gearHit;
	public TrailRenderer _lTrail,_rTrail;
	public Transform _coinFx;
	int _combo;
	float _comboPitch=1f;
	float _minComboPitch=1f;
	float _maxComboPitch=1.5f;
	AudioSource _comboSfx;
	float _touchSens=0.5f;
	Slider _sensSlider;
	float _balanceMult;
	float _trickMult;
	float _speedMult;
	Animator _anim;
	bool _zen;
	int _clusterCount;
	int _clusterCounter;
	public Transform [] _buildingPrefabs;

	public Transform _shopParent;
	UIManager _menu;

	struct Coin {
		public Transform transform;
		public MeshRenderer mesh;
		public Vector3 offset;
	}

	struct Jumper {
		public Transform transform;
		public MeshRenderer mesh;
		public int type;
	}

	int _numBoards;
	int _curBoard;
	Transform _boardParent;
	Transform _board;
	Transform _ethanParent;
	
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
		_line.receiveShadows=true;
		_line.shadowCastingMode=ShadowCastingMode.Off;
		_lineResFrac=1/(float)_lineResolution;
		
		//configure railTracker
		_railTracker=transform.GetChild(0);
		_ethanParent=_railTracker.GetChild(0).GetChild(0);
		_anim = _ethanParent.GetChild(0).GetComponent<Animator>();

		//sens slider

		//generate track starter
		if(PlayerPrefs.HasKey("tut"))
		{
			GenerateStartingSection();
			_tutorial=false;
		}
		else{
			//generate tutorial section
			GenerateTutorialSection();
			_tutorial=true;
		}


		//GenerateJumpers(0,_knots.Count-1);

		//This sets number of coins to 0
		_collectedCoins=0;

		//Get some references
		_helmet = GameObject.FindGameObjectWithTag("Helmet").transform;
		_ethan = _railTracker.GetChild(0); 
		_gate = GameObject.FindGameObjectWithTag("Gate").transform;
		_scoreText = GameObject.FindGameObjectWithTag("Score").GetComponent<Text>();
		_comboText = _scoreText.transform.GetChild(0).GetComponent<Text>();
		_comboSfx = _comboText.transform.GetComponent<AudioSource>();
		_trickText = GameObject.Find("TrickText").GetComponent<Text>();
		_trickTextRect = _trickText.GetComponent<RectTransform>();
		_trickText.text="";
		//_invincDisplay = _scoreText.transform.GetChild(1).GetComponent<CanvasGroup>();
		//_invincMeter = _invincDisplay.transform.GetChild(0).GetComponent<Image>();
		_scoreCanvas = _scoreText.transform.GetComponent<CanvasGroup>();
		_defaultScoreFont=_scoreText.fontSize;
		_boldScoreFont=Mathf.FloorToInt(_defaultScoreFont*1.4f);
		if(PlayerPrefs.HasKey("hs")){
			_hsText.text=PlayerPrefs.GetInt("hs").ToString("HIGH SCORE: #");
		}
		_main = Camera.main;
		_explosion = GameObject.FindGameObjectWithTag("Explosion").transform;
		_ethanMat.SetColor("_EmissionColor",Color.black);

		SetupShop();
	}

	void SetupShop(){
		//position boards in shop
		_menu = GameObject.Find("Wallet").transform.parent.GetComponent<UIManager>();
		_boardParent = _shopParent.Find("Boards");
		_numBoards = _boardParent.childCount;
		if(PlayerPrefs.HasKey("board"))
			_curBoard=PlayerPrefs.GetInt("board");
		RotateBoard(0);
	}

	public void RotateBoard(int dir){
		_curBoard+=dir;
		if(_curBoard<0)
			_curBoard=0;
		else if(_curBoard>=_numBoards)
			_curBoard=_numBoards-1;
		//position boards
		for(int i=0; i<_numBoards; i++){
			Transform t = _boardParent.GetChild(i);
			if(i!=_curBoard){
				t.localPosition=Vector3.right*
					Mathf.Lerp(1,-1,i/(float)(_numBoards-1))*3f;
				t.localEulerAngles=new Vector3(90,0,66);
				t.GetComponent<Hoverboard>()._rotate=false;
			}
			else{
				t.localPosition=
					new Vector3(-1.2f,-.35f,5.1f);
				t.eulerAngles=Vector3.zero;
				t.GetComponent<Hoverboard>()._rotate=true;
			}
		}
		Hoverboard b = _boardParent.GetChild(_curBoard).GetComponent<Hoverboard>();
		//show name
		Text title = _shopParent.Find("Canvas").Find("Title").GetComponent<Text>();
		title.text=b._name;

		Transform stats = _shopParent.Find("Canvas").Find("StatsContainer");
		//show cost
		Text cost = stats.GetChild(0).GetChild(0).GetComponent<Text>();
		Button buy = cost.transform.parent.GetChild(1).GetComponent<Button>();
		buy.interactable=false;
		cost.text="Cost: "+b._cost.ToString("0");
		if(PlayerPrefs.HasKey(b._name)||_curBoard==0){
			cost.text="Owned";
			//equip board
			//remove old board
			if(_ethanParent.GetComponentInChildren<Hoverboard>()!=null)
				Destroy(_ethanParent.GetComponentInChildren<Hoverboard>().gameObject);
			//add new
			_board = Instantiate(_boardParent.GetChild(_curBoard),_ethanParent);
			_board.GetComponent<Hoverboard>()._rotate=false;
			_board.localPosition=Vector3.zero;
			_board.localEulerAngles=Vector3.zero;
			_board.localScale=Vector3.one*1f;
			PlayerPrefs.SetInt("board",_curBoard);
			_trickMult=b._trick;
			_balanceMult=b._balance;
			//set speed
			_speedMult=b._speed;
			_moveSpeed=_speedMult;
			_maxSpeed=_moveSpeed*1.5f;
			_invincibleSpeed=_moveSpeed+0.05f;
			_targetMoveSpeed=_moveSpeed;
			_balanceSpeed = _moveSpeed*_balanceSpeedMultiplier;
		}
		else
		{
			if(_menu._coins>=b._cost)
				buy.interactable=true;
		}

		//show stat
		Text stat = stats.GetChild(1).GetChild(0).GetComponent<Text>();
		stat.text="Balance: "+b._balance.ToString("0.0");

		stat = stats.GetChild(3).GetChild(0).GetComponent<Text>();
		stat.text="Speed: "+b._speed.ToString("0.0");

		stat = stats.GetChild(2).GetChild(0).GetComponent<Text>();
		stat.text="Trick: "+b._trick.ToString("0.0");
	}

	public void BuyBoard(){
		Hoverboard h = _boardParent.GetChild(_curBoard).GetComponent<Hoverboard>();
		PlayerPrefs.SetInt(h._name,0);
		PlayerPrefs.Save();
		_menu.CoinSpent(h._cost);
		RotateBoard(0);
	}
	
	//function used in update loop to reset velocity towards 0
	void LimitVelocity(){
		if(_balanceVelocity!=0){
			_balanceVelocity=Mathf.Lerp(_balanceVelocity,0,_balanceDecceleration*Time.deltaTime*_balanceMult);
			if(Mathf.Abs(_balanceVelocity)<_minVel)
				_balanceVelocity=0;
		}
	}

	//jump
	IEnumerator JumpRoutine(){
		_jumping=true;
		_crouching=false;
		_anim.SetBool("crouch",false);
		float startY = _ethan.localEulerAngles.y;
		//wait for player to un-crouch
		yield return new WaitForSeconds(0.05f);
		//determine trick
		//
		int trick=-1;
		foreach(float f in _jumpers.Keys){
			if(f>_t-1 && f<_t+4){
				if(f-_t<.3f&&f-_t>0){
					trick=_jumpers[f].type;
					break;
				}
			}
		}
		_takeOff.pitch = Random.Range(.8f,1.4f);
		_takeOff.Play();
		//_balanceVelocity=0;
		_inputDelayTimer=0;
		float timer=0;
		Vector3 startPos = _ethan.localPosition;
		Vector3 endPos = startPos+Vector3.up*_jumpHeight;
		float l;
		while(timer<_jumpDur){
			l=timer/_jumpDur;
			timer+=Time.deltaTime;
			_ethan.localPosition = Vector3.LerpUnclamped(startPos,endPos,_jumpCurve.Evaluate(l));			
			switch(trick){
				case -1://ollie
				default:
					_board.localEulerAngles=Vector3.right*Mathf.Lerp(-45f,0,l);
					break;
				case 0://shuvit
					_board.localEulerAngles=new Vector3(Mathf.Lerp(-45,0,l),Mathf.Lerp(0,360f,l),0);
					break;
				case 1://kickflip
					_board.localEulerAngles=new Vector3(Mathf.Lerp(-45,0,l),0,Mathf.Lerp(0,360f,l));
					break;
				case 2://impossible
					_board.localEulerAngles=new Vector3(Mathf.Lerp(0,-360f,l),Mathf.Lerp(45f,0,Mathf.Abs(l-.5f)*2),0);
					break;
			}
			yield return null;
		}
		_ethan.localPosition=startPos;
		_board.localEulerAngles=Vector3.zero;
		_jumping=false;
		_inputDelayTimer=-_inputDelay*2;

		//snap rotation
		_landing.pitch = Random.Range(1,1.4f);
		_landing.Play();
		Vector3 localE = _ethan.localEulerAngles;
		float yLocal = localE.y;
		yLocal = Mathf.Round(yLocal/90f)*90;
		int pos=Mathf.RoundToInt(Mathf.Abs(yLocal))/90;
		localE.y=yLocal;
		_ethan.localEulerAngles=localE;
		string tricky="";
		float dir=Mathf.Sign(_spinTrick);
		int trickScore=0;
		//get spins
		if(Mathf.Abs(_spinTrick)>90){
			tricky+=dir==1? "FS " : "BS ";
			tricky+=Mathf.Round(Mathf.Abs(_spinTrick)/90)*90;
		}
		//get trick
		switch(trick){
			case -1:
				tricky+=" Ollie";
				break;
			case 0:
				tricky+=" Shuv-it";
				trickScore+=(1+Mathf.RoundToInt(Mathf.Abs(_spinTrick)/90f))*2;
				break;
			case 1:
				tricky+=" Kickflip";
				trickScore+=(1+Mathf.RoundToInt(Mathf.Abs(_spinTrick)/90f))*2;
				break;
			case 2:
				tricky+=" Impossible";
				trickScore+=(1+Mathf.RoundToInt(Mathf.Abs(_spinTrick)/90f))*2;
				break;
		}
		//get grind
		tricky+=" to\n";
		switch(pos){
			case 0:
			case 2:
			case 4:
				tricky+="Fifty Fifty";
				break;
			case 1:
			case 3:
				tricky+="Boardslide";
				break;
		}
		_trickText.text=tricky;
		_spinTrick=0;
		_trickTextTimer=_maxTrickTextTimer;
		if(trickScore>0)
			AddScore(trickScore);
		if(trick>=0)
			GenNextJumper();
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

		ResetRail();

		//AddStraight(3);//was 2 - is 3 to test corkscrews

		//#temp
		//GenerateCoins(3,_knots.Count-1);
		GenerateCoinCluster();
		GenNextJumper();

		_nextGate = Random.Range(_minGateSpace,_maxGateSpace);//remember this is not the location of the gate but the tOffset at which the gate spawns
		_gatePos=1024;//something arbitrarily high at the start - will be reset in AddGate()
		UpdateFloorPlane();
	}

	void GenerateTutorialSection(){
		_knots.Add(Vector3.back*_nodeDist);
		_knots.Add(Vector3.zero);
		_t=1f;
		//_knots.Add(Vector3.forward*_nodeDist);
		AddStraight(2);

		//instance some world space text items
		_tutorialObjs.SetActive(true);

		//add a right curve
		AddCurve(_nodeDist*2f,3,true);
		AddStraight(2);
		//add a left curve
		AddCurve(_nodeDist*2f,3,false);
		//add some straights
		AddStraight(2);

		ResetRail();

		//spawn coins along curves
		GenerateCoins(4,5,1);
		GenerateCoins(8,9,1);
		//Spawn a jumper
		GenerateJumpers(11,12,1);


		_nextGate = Random.Range(_minGateSpace,_maxGateSpace);//remember this is not the location of the gate but the tOffset at which the gate spawns
		_gatePos=1024;//something arbitrarily high at the start - will be reset in AddGate()
		UpdateFloorPlane();
	}

	void GenerateCoinCluster(){
		//determine a starting point - say current pos+ 2 knots
		float startT=_t+Random.Range(2f,4f);
		//determine the number of coins - say 5
		int numCoins=Random.Range(5,10);
		_clusterCount=numCoins;
		_clusterCounter=0;
		//determine spacing
		float coinSpacing=.075f;
		//for loop
		//	place coins
		for(int i=0;i<numCoins;i++)
		{
			GenerateCoin(startT+i*coinSpacing,i==numCoins-1);
		}
	}

	void GenerateCoin(float t,bool final){
		Vector3 railPos = _path.GetPoint(t-_tOffset);
		Vector3 curForward = _path.GetTangent(t-_tOffset);
		Vector3 nextForward = _path.GetTangent(t+1f/(float)_lineResolution);
		//determine the curvature
		float cross = Vector3.Cross(curForward,nextForward).y*.1f;
		//declare coin struct
		Coin c = new Coin();
		//instance the coin
		Transform curCoin = Instantiate(_coin,railPos+Vector3.up*_coinHeight,Quaternion.identity, null);
		curCoin.name=final?"final":"coin";
		//not sure what this does
		curCoin.LookAt(curCoin.position+curForward);
		//spin coin
		curCoin.RotateAround(railPos,curForward,cross*_crossMultiplier);
		c.offset=curCoin.up;
		//set scale
		curCoin.localScale=new Vector3(1f,2f,.5f);
		//set transform
		c.transform = curCoin;
		//get the coin mesh data
		c.mesh = curCoin.GetComponent<MeshRenderer>();
		//add coin to coin dict
		if(!_coins.ContainsKey(t)){
			_coins.Add(t,c);
		}
		//check jumpers #temp
		foreach(float k in _jumpers.Keys){
			float ab = Mathf.Abs(k-t);
			if(ab<0.25f)
			{
				c.transform.position+=curCoin.up*
					_coinHeightCurve.Evaluate(Mathf.InverseLerp(0.25f,0,ab));
				break;
			}
		}
	}

	int clusterCounter=0;
	int coinSpacing=0;
	void GenerateCoins(int startKnot, int endKnot,float probOverride=-1){
		//coin generation
		float prevCross=0;
		float prob = probOverride==-1? _coinProbability : probOverride;
		bool cork=false;
		float corkInc=0;
		for(int i=startKnot*_lineResolution; i<endKnot*_lineResolution; i++){
			//converts line space to coin space
			float t = i/(float)_lineResolution;
			float key = t+_tOffset;
			coinSpacing--;

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
					if(Mathf.Abs(cross-prevCross)>0.1f && probOverride==-1)
					{
						clusterCounter=0;
						cork=false;//reset to use for corkSpacing
					}
					else{
						//rail curvature is OK
						//calculate the coins offset direction
						Vector3 right = Vector3.Cross(Vector3.up,curForward);

						Vector3 offset;
						if(!cork){
							offset = Vector3.LerpUnclamped(Vector3.up,right,cross);
						}
						else{
							offset = Vector3.LerpUnclamped(Vector3.up,right,_corkAngle);
						}

						//instance the coin
						Transform curCoin = Instantiate(_coin,railPos+Vector3.up*_coinHeight,Quaternion.identity, null);
						curCoin.LookAt(curCoin.position+curForward);
						//rotate coin around rail
						if(!cork)
							curCoin.RotateAround(railPos,curForward,cross*_crossMultiplier);
						else
						{
							curCoin.RotateAround(railPos,curForward, _corkAngle*_crossMultiplier);
							if(Random.value<.1f)
								corkInc*=-1f;
							_corkAngle+=corkInc;
						}
						c.offset=curCoin.up;

						curCoin.localScale=new Vector3(1f,2f,.5f);
						c.transform = curCoin;

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
					//if(!nearJump && Random.value<prob)
					if(!nearJump && coinSpacing<=0)
					{
						//Make sure we don't generate coins on straight sections because that is boring and the coins are harder to see
						Vector3 nextForward = _path.GetTangent(t+1f/(float)_lineResolution);
						float cross = Vector3.Cross(curForward,nextForward).y*.1f;
						prevCross=cross;//need this to ensure the cluster doesn't get insta cancelled
						if(Mathf.Abs(cross)>_crossThreshold)
						{
							if(_corkSpacing >1f || prob==1)
							{
								//Determine cluster size
								clusterCounter=Random.Range(_minCoinCluster,_maxCoinCluster+1);
								coinSpacing=Random.Range(_minCoinSpacing,_maxCoinSpacing);
								cork=false;
								_corkAngle=0;
							}
						}
						else{
							//gen corkscrew
							clusterCounter=Random.Range(_minCoinCluster,_maxCoinCluster+1);
							coinSpacing=Random.Range(_minCoinSpacing,_maxCoinSpacing);
							cork=true;	
							corkInc=Random.Range(-.1f,.1f);
							_corkSpacing=0;
						}
					}
				}
			}
			if(!cork){
				_corkSpacing+=1/(float)_lineResolution;
			}
		}
	}

	void GenNextJumper(){
		int off = Mathf.CeilToInt(_t-_tOffset);
		GenerateJumper(Random.Range(2f,_knots.Count-1-off));
	}

	void GenerateJumper(float distance){
		float key = _t+distance;
		Vector3 railPos = _path.GetPoint(key-_tOffset);
		Vector3 forward = _path.GetTangent(key-_tOffset);
		//instance the jumper
		Transform jump = Instantiate(_jumper,railPos,Quaternion.identity, null);
		jump.forward=-forward;
		//create the struct
		Jumper j;
		j.type=Random.Range(0,3);
		j.transform = jump;
		j.transform.GetComponent<Rotator>()._speed=Random.Range(90f,180f);
		j.mesh=jump.GetComponent<MeshRenderer>();
		switch(j.type){
			case 0:
			default:
				j.mesh.material.SetColor("_EmissionColor",Color.yellow);
				break;
			case 1:
				j.mesh.material.SetColor("_EmissionColor",Color.cyan);
				break;
			case 2:
				j.mesh.material.SetColor("_EmissionColor",Color.magenta);
				break;
		}
		j.mesh.enabled=false;
		_jumpers.Add(key,j);
		foreach(float k in _coins.Keys){
			float ab = Mathf.Abs(k-key);
			if(ab<0.25f)
			{
				Coin c = _coins[k];
				c.transform.position+=c.transform.up*
					_coinHeightCurve.Evaluate(Mathf.InverseLerp(0.25f,0,ab));
			}
		}
	}

	void GenerateJumpers(int startKnot, int endKnot, float probOverride=-1){
		
		float prob = probOverride == -1 ? _jumpProbability : probOverride;
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
				if(key-_lastJump>_jumpThreshold && Random.value<prob && Mathf.Abs(key-_gatePos)>2f){
					_lastJump=key;

					int type=Random.Range(0,3);
					for(int j=0; j<2; j++){

						float nextT = (i+j*0.5f)/(float)_lineResolution;
						Vector3 nextPos = _path.GetPoint(nextT);
						Vector3 nextForward = _path.GetTangent(nextT);
						Transform jumper2 = Instantiate(_jumper,nextPos,Quaternion.identity, null);
						jumper2.forward=-nextForward;
						Jumper j2;
						j2.type=type;
						j2.transform = jumper2;
						if(j==0)
							j2.transform.GetComponent<Rotator>()._speed=Random.Range(90f,180f);
						else
							j2.transform.GetComponent<Rotator>()._speed=Random.Range(-90f,-180f);
						j2.mesh=jumper2.GetComponent<MeshRenderer>();
						switch(j2.type){
							case 0:
							default:
								j2.mesh.material.SetColor("_EmissionColor",Color.yellow);
								break;
							case 1:
								j2.mesh.material.SetColor("_EmissionColor",Color.cyan);
								break;
							case 2:
								j2.mesh.material.SetColor("_EmissionColor",Color.magenta);
								break;
						}
						j2.mesh.enabled=false;
						_jumpers.Add(nextT+_tOffset,j2);
					}

					//clear out nearby coins
					List<float> removeList = new List<float>();
					foreach(float k in _coins.Keys){
						if(Mathf.Abs(k-key)<_jumpSpacing){
							removeList.Add(k);
						}
					}
					foreach(float k in removeList){
						Transform trans = _coins[k].transform;
						_coins.Remove(k);
						trans.name="destroyed";
						Destroy(trans.gameObject);
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
			numTracks = Random.Range(1,5);
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
		UpdateFloorPlane();
		//what about down slopes?
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
		AddStraight(3,false);
		_gatePos = _knots.Count-3+_tOffset;
		_nextGate=_gatePos+Random.Range(_minGateSpace,_maxGateSpace);
		Vector3 railLine = _knots[_knots.Count-2]-_knots[_knots.Count-3];
		railLine.Normalize();
		_gate.position = _knots[_knots.Count-3];//+railLine*_nodeDist*1.25f;
		//_gate.position+=Vector3.up;
		_gate.up=railLine;
		Vector3 localEulers = _gate.localEulerAngles;
		localEulers.x=90f;
		_gate.localEulerAngles = localEulers;
		_gate.localScale = new Vector3(400,40,400);
		_gate.GetComponent<MeshRenderer>().enabled=true;
		_gate.GetChild(0).GetComponent<ParticleSystem>().Play();
	}


	void AddStraight(int segments, bool allowCorks=true){
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


	//event when coin is collected
	public delegate void EventHandler();
	public event EventHandler OnCoinCollected;
	//event when gear is collected
	public event EventHandler OnGearCollected;

	void AddCoin(int setValue=-1){
		if(_zen)
			return;
		if(setValue==-1)
		{
			if(OnCoinCollected!=null)
				OnCoinCollected.Invoke();
			//_collectedCoins++;
			//_combo++;
			Transform coinSfx = Instantiate(_coinFx);
			AudioSource coinAudio = coinSfx.GetComponent<AudioSource>();
			_comboPitch = Mathf.Lerp(_comboPitch,_maxComboPitch,.1f);
			coinAudio.pitch=_comboPitch;
			coinAudio.PlayScheduled(AudioSettings.dspTime+Random.value*.2);
			Destroy(coinSfx.gameObject,.5f);
			//_comboText.text=_combo.ToString("+#");
		}
		else
		{
			//_collectedCoins=setValue;
			_comboText.text="";
			if(_combo>3)
				_comboSfx.Play();
			_combo=0;
			_comboPitch=_minComboPitch;
		}
		//_scoreText.text=_collectedCoins.ToString();
		//_scoreTextShadow.text=_collectedCoins.ToString();
		//_scoreText.fontSize=_boldScoreFont;
		//_scoreChangeTimer=.4f;
	}

	void AddScore(int points){
		if(_zen)
			return;
		_collectedCoins+=points;
		_scoreText.text=_collectedCoins.ToString();
		_scoreText.fontSize=_boldScoreFont;
		_scoreChangeTimer=.4f;
	}

	public void StartRiding(bool zen){
		_zen=zen;
		_gameState=1;
		_lTrail.emitting=true;
		_rTrail.emitting=true;
		if(!_tutorial)
		{
			StartCoroutine(FadeInScoreText());
		}
		Destroy(_shopParent.gameObject);
	}

	public void PlayTutorial(){
		_gameState=1;
		_tutorial=true;
		_coins.Clear();
		_jumpers.Clear();
		_knots.Clear();
		GenerateTutorialSection();
	}

	public void UpdateSens(){
		_touchSens=_sensSlider.value;
		PlayerPrefs.SetFloat("sens",_touchSens);
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
				//#whatsthisfor
				if(_balanceState==0)
					_inputDelayTimer=0;

				//temp code for testing jump
				if(Input.GetAxis("Vertical")>0){
					_balanceState=3;
					/*
					if(!_jumping)
						StartCoroutine(JumpRoutine());
						*/
				}

				//handle balance logic
				switch(_balanceState){
					case 0:
						//fall to 0
						LimitVelocity();
						//#todo - something about crouch timer
						if(_crouching && !_jumping){
							StartCoroutine(JumpRoutine());
						}
						break;
					case 1:
						if(_crouching && !_jumping){
							StartCoroutine(JumpRoutine());
						}
						//climb to 1
						if(!_jumping)
						{
							_balanceVelocity=Mathf.Lerp(_balanceVelocity,_maxVel,_balanceAcceleration*Time.deltaTime*_balanceMult);
						}
						//temp code for air spins
						else
						{
							_spinTrick-=_trickMult*Time.deltaTime;
							_ethan.Rotate(0,-_trickMult*Time.deltaTime, 0);
						}
						break;
					case 2:
						if(_crouching && !_jumping){
							StartCoroutine(JumpRoutine());
						}
						//climb to -1
						if(!_jumping)
							_balanceVelocity = Mathf.Lerp(_balanceVelocity,-_maxVel,_balanceAcceleration*Time.deltaTime*_balanceMult);

						//temp code for air spins
						else
						{
							_spinTrick+=_trickMult*Time.deltaTime;
							_ethan.Rotate(0,_trickMult*Time.deltaTime, 0);
						}
						break;
					case 3:
						if(!_jumping)
							LimitVelocity();
						//jump
						if(!_crouching && !_jumping)
						{
							_crouching=true;
							_anim.SetBool("crouch",true);
						}
						break;
				}

				//set player's root position and orientation
				_balance-=_balanceVelocity*Time.deltaTime*_balanceSpeed;
				Vector3 prevForward = _railTracker.forward;
				SetPosition();

				//Check for point acquisitions
				bool genNext=false;
				bool gotCluster=false;
				foreach(float f in _coins.Keys){
					if(f>_t-1 && f <_t+4){
						Coin c = _coins[f];

						//disable coins that are too far away
						if(f<_t-.5f)
							c.mesh.enabled=false;

						//enable coins
						else
						{
							c.mesh.enabled=true;

							//if coin is close and not collected
							if(Mathf.Abs(f-_t)<.05f && c.transform.tag!="Collected"){
								float dot =Vector3.Dot(c.offset,_railTracker.up); 
								//and player is in line, collect it
								if(dot>_coinHitThreshold)
								{
									c.mesh.material.SetColor("_Color",_coinHitColor);
									c.transform.tag="Collected";
									AddCoin();
									_clusterCounter++;
									if(_clusterCounter==_clusterCount)
										gotCluster=true;
								}
								if(c.transform.name=="final")
								{
									c.transform.name="foo";
									genNext=true;
								}
							}
							//on coin miss
							else if(_t-f>.05f && c.transform.tag!="Collected"){
								if(_combo>0){
									//_collectedCoins+=_combo;
									//AddCoin(_collectedCoins);
									c.transform.tag="Collected";
								}
								if(c.transform.name=="final")
								{
									c.transform.name="foo";
									genNext=true;
								}
							}
						}
					}
				}//gen coin clusters after the previous has been collected
				if(genNext)
					GenerateCoinCluster();
				if(gotCluster)
					AddScore(5);


				bool gearExplode=false;
				//check for jumper collisions
				foreach(float f in _jumpers.Keys){
					if(f>_t-1 && f<_t+4){
						//only process jumpers that haven't been 'collected' or 'destroyed'
						bool notCol = _jumpers[f].transform.tag!="Collected";
						if(notCol)
						{
							_jumpers[f].mesh.enabled=true;
							
							if(Mathf.Abs(f-_t)<.04f && _ethan.localPosition.y < _minJumpY){
								if(!_tutorial){
									if(_moveSpeed>_invincibleSpeed || _zen){
										//destroy the gear
										//play particle
										gearExplode=true;
										Transform vfx = _jumpers[f].transform.GetChild(0);
										ParticleSystem p = vfx.GetComponent<ParticleSystem>();
										p.GetComponent<ParticleSystemRenderer>().material.SetColor("_EmissionColor",_jumpers[f].mesh.material.GetColor("_EmissionColor"));
										p.Play();
										_jumpers[f].transform.tag="Collected";
										_jumpers[f].mesh.enabled=false;
										_smash.Play();
										if(OnGearCollected!=null)
											OnGearCollected.Invoke();
									}
									else{
										_gameState=2;
										//_collectedCoins+=_combo;
										//AddCoin(_collectedCoins);
										_jumpHit.Invoke();
										if(!PlayerPrefs.HasKey("hs"))
											PlayerPrefs.SetInt("hs",_collectedCoins);
										else{
											if(PlayerPrefs.GetInt("hs")<_collectedCoins){
												PlayerPrefs.DeleteKey("hs");
												PlayerPrefs.SetInt("hs",_collectedCoins);
											}
										}
										PlayerPrefs.Save();
										_explosion.position = _ethan.position;
										_explosion.GetComponent<ParticleSystem>().Play();
									}
								}
								else{
									_gameState=2;
									_gearHit.Play();
									StartCoroutine(Rewind());
									return;
								}
							}
						}
					}
				}
				if(gearExplode)
					GenNextJumper();

				//check for gate
				if(_t>=_gatePos)
				{
					//_gameState=3;
					//StartCoroutine(GateRoutine());
					GateEvent();
				}

				//check for new track gen if only X tracks lie ahead
				if(_t-_tOffset>_knots.Count-_lookAheadTracks){

					int numTracks = GenerateNewTrack();

					int removed = RemoveOldTrack(_t);

					ClearOldCoins(_tOffset+removed);
					ClearOldJumpers(_tOffset+removed);

					ResetRail();

					_tOffset+=removed;
					
					//set this to -1 for special cases where coins are generated along with the track
					/*
					if(numTracks!=-1)
						GenerateCoins(_knots.Count-(numTracks+1),_knots.Count-1);
						*/
					//#temp
					//GenerateJumpers(_knots.Count-(numTracks+1),_knots.Count-1);
				}
				_moveSpeed = Mathf.Lerp(_moveSpeed,_targetMoveSpeed,_speedChangeLerp*Time.deltaTime);
				_balanceSpeed = _moveSpeed*_balanceSpeedMultiplier;
				_t+=Time.deltaTime*_moveSpeed;
				if(_tutorial && _t > 13){
					_tutorialObjs.SetActive(false);
					_tutorial=false;
					PlayerPrefs.SetInt("tut",1);
					PlayerPrefs.Save();
					_collectedCoins=0;
					AddCoin(0);
					StartCoroutine(FadeInScoreText(0));
				}
				if(_moveSpeed>_invincibleSpeed){
					//invincinbility effect
					Color color;
					if(_moveSpeed>_invincibleSpeed+_invincibleWarning){
						color = _invincibilityGrad.Evaluate(Mathf.PingPong(_t,1f))*.5f;
					}
					else{
						color = Color.white*Mathf.PingPong(_t,0.2f)*5f;
					}
					_ethanMat.SetColor("_EmissionColor",color);
					//_invincMeter.fillAmount=Mathf.InverseLerp(_invincibleSpeed,_maxSpeed,_moveSpeed);
				}
				else
				{
					_ethanMat.SetColor("_EmissionColor",Color.black);
					//_invincDisplay.alpha=0;
				}
				break;
			case 2://collide with jumper
				break;
			case 3://gate check
				break;
		}
		if(_scoreChangeTimer>0){
			_scoreChangeTimer-=Time.deltaTime;
			if(_scoreChangeTimer<=0)
				_scoreText.fontSize=_defaultScoreFont;
		}
		//_tDebug.text="jumpProb: "+_jumpProbability.ToString("#.##");
		//_ngDebug.text="NG: "+_nextGate.ToString("#.#");
		//_gpDebug.text="GP: "+_gatePos.ToString("#.#");
#if UNITY_EDITOR
		if(Input.GetKeyDown(KeyCode.T))
			PlayerPrefs.DeleteKey("tut");
#endif
		if(_trickTextTimer>0){
			Vector2 lPos = _trickTextRect.anchoredPosition;
			lPos.y = Mathf.Lerp(-250f,0f,_popup.Evaluate(_trickTextTimer/_maxTrickTextTimer));
			_trickTextRect.anchoredPosition=lPos;
			_trickTextTimer-=Time.deltaTime;
			if(_trickTextTimer<=0)
				_trickText.text="";
		}
	}

	void SetPosition(){
		float railT = _t-_tOffset;
		_railTracker.position = _path.GetPoint(railT);
		_railTracker.forward = _path.GetTangent(railT);
		Vector3 localEuler = _railTracker.localEulerAngles;

		Vector3 nextForward = _path.GetTangent(railT+1f/(float)_lineResolution);
		_curvature = Vector3.Cross(_railTracker.forward,nextForward).y;
		if(!_jumping)
			_balance-=_curvature*_balanceMult;
		localEuler.z = -_balance;
		_railTracker.localEulerAngles=localEuler;
	}

	void GateEvent(){
		//tighten up jumps
		_jumpSpacing = Mathf.Lerp(_jumpSpacing,_minJumpSpacing,_jumpIncreaseRate);
		_jumpThreshold = Mathf.Lerp(_jumpThreshold,_minJumpThreshold,_jumpIncreaseRate);	
		_jumpProbability = Mathf.Lerp(_jumpProbability,_maxJumpProbability,_jumpIncreaseRate);

		//determine next gate position
		_gatePos=Mathf.FloorToInt(_nextGate)+50;//this will get reset when the next gate is generated

		//speed boost
		_moveSpeed=_maxSpeed;

		_woosh.Play();

		//_invincDisplay.alpha=1f;
		//_invincMeter.fillAmount=1f;

		//particles
		/*
		foreach(ParticleSystem ps in _speedParts){
			ps.Play();
		}
		*/
	}


	IEnumerator FadeInScoreText(float delay=4f){
		yield return new WaitForSeconds(delay);
		float timer=0;
		if(_zen){
			_scoreText.text="";
			_comboText.text="";
		}
		while(timer<1){
			timer+=Time.deltaTime;
			_scoreCanvas.alpha=timer;
			_authorCanvas.alpha=1-timer;
			yield return null;
		}
		_scoreCanvas.alpha=1f;
		_authorCanvas.alpha=0f;
		yield return new WaitForSeconds(3f);
		//destroy world space text
		for(int i=_wsText.Length-1; i>=0; i--){
			Destroy(_wsText[i]);
		}
	}

	IEnumerator Rewind(){
		float timer=0;
		_music.pitch=2;
		while(timer<1){
			timer+=Time.deltaTime;
			_t-=Time.deltaTime;
			SetPosition();
			yield return null;
		}
		_music.pitch=1;
		_gameState=1;
	}

	[ContextMenu("Clear tut")]
	public void ClearTut(){
		PlayerPrefs.DeleteKey("tut");
		PlayerPrefs.Save();
	}

	public void Pause(){
		_gameState=0;
	}

	public void Resume(){
		_gameState=1;
	}

	float tmpFloorOffset=10f;
	float tmpFloorWidth=2f;
	void UpdateFloorPlane(){
		Mesh m = new Mesh();
		//2 verts per knot
		Vector3[] points = new Vector3[_knots.Count*2];
		//2 triangles per knot gap
		int[] tris = new int[(_knots.Count-1)*6];
		int triCount=0;
		for(int i=0; i<_knots.Count; i++){
			//get right vector
			Vector3 forward = _path.GetTangent(i);
			Vector3 right = -Vector3.Cross(forward,Vector3.up);

			//get vertex positions
			Vector3 basePoint=_knots[i]+Vector3.down*tmpFloorOffset;
			points[i*2]=basePoint+right*tmpFloorWidth;
			points[i*2+1]=basePoint-right*tmpFloorWidth;

			//do the tris
			if(i<_knots.Count-1){
				tris[triCount]=i*2;
				tris[triCount+1]=i*2+1;
				tris[triCount+2]=(i+1)*2;
				tris[triCount+3]=(i+1)*2;
				tris[triCount+4]=i*2+1;
				tris[triCount+5]=(i+1)*2+1;
				triCount+=6;
			}
		}
		m.vertices=points;
		m.triangles=tris;
		transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh=m;
	}

	void OnDrawGizmos(){
		for(int i=0; i<_knots.Count; i++){
			Gizmos.color=Color.blue;
			Gizmos.DrawSphere(_knots[i],1f);
			//get right vector
			Vector3 forward = _path.GetTangent(i);
			Vector3 right = Vector3.Cross(Vector3.up,forward);
			Gizmos.color=Color.green;
			Gizmos.DrawSphere(_knots[i]+right,1f);
			Gizmos.color=Color.red;
			Gizmos.DrawSphere(_knots[i]-right,1f);
		}
	}
}
