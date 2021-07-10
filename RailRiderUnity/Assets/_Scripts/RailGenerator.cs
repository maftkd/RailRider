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
	Dictionary<float, Item> _coins = new Dictionary<float, Item>();
	public Transform _coin;
	float _coinHeight = 1.5f;
	public AnimationCurve _coinHeightCurve;
	int _tOffset=0;//subtract from _t to get time about current sections of rail
	int _lookAheadTracks=8;
	float _spawnPoint=6f;
	public Transform _jumper;
	public Transform _ducker;
	public Transform _rack;
	public Transform _supportRail;
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
	Dictionary<float, Item> _jumpers = new Dictionary<float, Item>();
	Dictionary<float, Item> _duckers = new Dictionary<float, Item>();
	Dictionary<float, Item> _racks = new Dictionary<float, Item>();
	Dictionary<float, Item> _supports = new Dictionary<float, Item>();

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
	public Transform _buildingPrefab;
	int _numBuildings=400;
	Transform[] _buildings;
	float _seed;
	float _playTimer;
	float _maxTime=180f;//360f;
	float _jumpDuckSpace;
	float _minJumpDuckSpace=0.5f;
	float _maxJumpDuckSpace=1f;
	public Color _gold, _silver, _copper;
	float _uncrouchTimer;
	float _uncrouchTime=0.05f;//time it takes to confirm an uncrouch
	public ParticleSystem _sparks;
	float _lastJumper;
	float _lastCluster;
	float _lastDucker;
	float _lastRack;
	public Phase [] _course;
	int _curPhase;
	int _leftOverCoins;
	float _leftOverCork;
	float _leftOverRotation;

	public Transform _shopParent;
	UIManager _menu;

	[System.Serializable]
	public class Phase {
		[Header("Jumpers")]
		public float _minJumpSpace;
		public float _jumpProbability;
		[Header("Coins")]
		public float _minCoinSpace;
		public float _coinProbability;
		public int _coinClusterSize;
		public float _maxCoinRotation;
		public float _maxCoinCork;
		[Header("Duckers")]
		public float _minDuckerSpace;
		public float _duckerProbability;
		[Header("Racks")]
		public float _minRackSpace;
		public float _rackProbability;
		public float _maxRackSpeed;
	}

	public class Item {
		public Transform transform;
		public MeshRenderer mesh;
		public Item(){}
	}

	class Coin : Item{
		public Vector3 offset;
	}

	class Jumper : Item{
		public int type;
	}

	class Ducker : Item{
	}

	class Rack : Item{
		public GameObject go;
		public RackWheel rack;
	}

	class Support : Item{
		public LineRenderer line;
	}

	int _numBoards;
	int _curBoard;
	Transform _boardParent;
	Transform _board;
	Transform _ethanParent;
	
	// Start is called before the first frame update
	void Start()
	{
		//#temp
		_curPhase=1;
		//get random seed
		_seed = 1f/System.DateTime.Now.Millisecond;
		_seed*=1000f;

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
		//reset uncrouch timer
		_uncrouchTimer=0;
		float startY = _ethan.localEulerAngles.y;
		//wait for player to un-crouch
		yield return new WaitForSeconds(0.05f);
		_sparks.Stop();
		//determine trick
		int trick=-1;
		Jumper j = new Jumper();
		foreach(float f in _jumpers.Keys){
			j=(Jumper)_jumpers[f];
			if(f>_t-1 && f<_t+4){
				if(f-_t<.3f&&f-_t>0){
					trick=j.type;
					j.transform.tag="Collected";
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
		if(_gameState==1)
			_sparks.Play();
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
	}

	void Uncrouch(){
		//inc uncrouchTimer
		_uncrouchTimer+=Time.deltaTime;
		//if uncrouchTimer > uncrouchThresh
		//	do stuff
		//	reset uncrouch timer
		if(_uncrouchTimer>=_uncrouchTime)
		{
			_crouching=false;
			_anim.SetBool("crouch",false);
			_uncrouchTimer=0;
		}
	}

	void GenerateStartingSection(){
		//we add the previous node just so it looks like there's a stretch
		//of rail from the start menu
		AddKnot(Vector3.back*_nodeDist);
		AddKnot(Vector3.zero);
		_t=1f;
		AddKnot(Vector3.forward*_nodeDist);

		//Starts out with 3 straights total
		AddStraight(2);

		//Then a long curve
		AddCurve(_nodeDist*Random.Range(3,5f),3,(Random.value < 0.5f));

		ResetRail();

		//_nextGate = Random.Range(_minGateSpace,_maxGateSpace);//remember this is not the location of the gate but the tOffset at which the gate spawns
		//_gatePos=1024;//something arbitrarily high at the start - will be reset in AddGate()
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

		//Spawn a jumper
		//GenerateJumpers(11,12,1);


		_nextGate = Random.Range(_minGateSpace,_maxGateSpace);//remember this is not the location of the gate but the tOffset at which the gate spawns
		_gatePos=1024;//something arbitrarily high at the start - will be reset in AddGate()
		UpdateFloorPlane();
	}

	void GenerateCoinCluster(float startT){
		//reset audio
		_comboPitch=_minComboPitch;
		//determine a starting point - say current pos+ 2 knots
		Phase phase = _course[_curPhase];
		int numCoins=_leftOverCoins>0? _leftOverCoins : phase._coinClusterSize;
		_clusterCount=numCoins;
		_clusterCounter=0;
		//determine spacing
		float coinSpacing=0.1f;//.075f;
		//for loop
		//float maxR = Mathf.Lerp(45f,180f,_playTimer/_maxTime);
		float maxR=phase._maxCoinRotation;;
		float cork = _leftOverCoins>0? _leftOverCork : phase._maxCoinCork*Random.Range(-1f,1f);
		float rotation = _leftOverCoins>0?_leftOverRotation : Random.Range(-maxR,maxR);
		//	place coins
		_leftOverCoins=0;
		for(int i=0;i<numCoins;i++)
		{
			if(!GenerateCoin(startT+i*coinSpacing,rotation+cork*i,i==numCoins-1))
			{
				_leftOverCoins=numCoins-i;
				_leftOverCork=cork;
				_leftOverRotation=rotation+cork*i;
				return;
			}
		}
	}

	bool GenerateCoin(float t,float r,bool final){
		if(t-_tOffset>=_knots.Count-1)
		{
			return false;
		}
		Vector3 railPos = _path.GetPoint(t-_tOffset);
		Vector3 curForward = _path.GetTangent(t-_tOffset).normalized;
		Vector3 nextForward = _path.GetTangent(t+1f/(float)_lineResolution).normalized;
		//determine the curvature
		//float cross = Vector3.Cross(curForward,nextForward).y;
		//declare coin struct
		Coin c = new Coin();
		//instance the coin
		Transform curCoin = Instantiate(_coin,railPos+Vector3.up*_coinHeight,Quaternion.identity, null);
		curCoin.name=final?"final":"coin";
		//not sure what this does
		curCoin.LookAt(curCoin.position+curForward);
		//spin coin
		//curCoin.RotateAround(railPos,curForward,cross*_crossMultiplier);
		curCoin.RotateAround(railPos,curForward,r);
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
					_coinHeightCurve.Evaluate(Mathf.InverseLerp(0.25f,0,ab))*2f;
				break;
			}
		}
		return true;
	}

	void ResetCoin(float t, Coin c){
		Vector3 railPos = _path.GetPoint(t-_tOffset);
		c.transform.position=railPos+c.offset*_coinHeight;
		//check jumpers #temp
		foreach(float k in _jumpers.Keys){
			float ab = Mathf.Abs(k-t);
			if(ab<0.25f)
			{
				c.transform.position+=c.transform.up*
					_coinHeightCurve.Evaluate(Mathf.InverseLerp(0.25f,0,ab))*2f;
				break;
			}
		}
	}

	void ResetJumper(float t, Jumper j){
		Vector3 railPos = _path.GetPoint(t-_tOffset);
		j.transform.position=railPos;
	}

	/*
	void GenNextDucker(){
		int off = Mathf.CeilToInt(_t-_tOffset);
		//As _t goes up, the next jumper range should go down
		float minRange = Mathf.Lerp(2f,1f,_playTimer/_maxTime);
		float maxRange = _knots.Count-1-off;
		//float maxRange = Mathf.Lerp(_knots.Count-1-off,2f,_playTimer/_maxTime);
		//GenerateJumper(Random.Range(2f,_knots.Count-1-off));
		GenerateDucker(Random.Range(minRange,maxRange));
	}

	void GenNextRack(){
		int off = Mathf.CeilToInt(_t-_tOffset);
		float minRange = Mathf.Lerp(2f,1f,_playTimer/_maxTime);
		float maxRange = _knots.Count-1-off;
		GenerateRack(Random.Range(minRange,maxRange));
	}
	*/

	void GenerateJumper(float key){
		_jumpDuckSpace=Mathf.Lerp(_maxJumpDuckSpace,_minJumpDuckSpace,_playTimer/_maxTime);
		//float key = _t+distance;
		foreach(float k in _duckers.Keys){
			float ab = Mathf.Abs(k-key);
			if(ab<_jumpDuckSpace)
			{
				key=k+_jumpDuckSpace;
				break;
			}
		}
		Vector3 railPos = _path.GetPoint(key-_tOffset);
		Vector3 forward = _path.GetTangent(key-_tOffset);
		//instance the jumper
		Transform jump = Instantiate(_jumper,railPos,Quaternion.identity, null);
		jump.forward=-forward;
		//create the struct
		Jumper j=new Jumper();
		j.type=Random.Range(0,3);
		j.transform = jump;
		j.transform.GetComponent<Rotator>()._speed=Random.Range(90f,180f);
		j.mesh=jump.GetComponent<MeshRenderer>();
		switch(j.type){
			case 0:
			default:
				j.mesh.materials[1].SetColor("_Color",_gold);
				break;
			case 1:
				j.mesh.materials[1].SetColor("_Color",_silver);
				break;
			case 2:
				j.mesh.materials[1].SetColor("_Color",_copper);
				break;
		}
		//j.mesh.enabled=false;
		_jumpers.Add(key,j);
		foreach(float k in _coins.Keys){
			float ab = Mathf.Abs(k-key);
			if(ab<0.25f)
			{
				Coin c = (Coin)_coins[k];
				c.transform.position+=c.transform.up*
					_coinHeightCurve.Evaluate(Mathf.InverseLerp(0.25f,0,ab))*2f;
			}
		}
	}

	void GenerateDucker(float distance){
		//_jumpDuckSpace=Mathf.Lerp(_maxJumpDuckSpace,_minJumpDuckSpace,_playTimer/_maxTime);
		float key = distance;
		/*
		foreach(float k in _jumpers.Keys){
			float ab = Mathf.Abs(k-key);
			if(ab<_jumpDuckSpace)
			{
				key=k+_jumpDuckSpace;
				break;
			}
		}
		*/
		Vector3 railPos = _path.GetPoint(key-_tOffset);
		Vector3 forward = _path.GetTangent(key-_tOffset);
		//instance the jumper
		Transform duck = Instantiate(_ducker,railPos,Quaternion.identity, null);
		duck.forward=-forward;
		//create the struct
		Ducker d=new Ducker();
		d.transform = duck;
		d.transform.GetComponent<Rotator>()._speed=Random.Range(90f,180f);
		d.mesh=duck.GetComponent<MeshRenderer>();
		d.mesh.enabled=false;
		_duckers.Add(key,d);
		//Debug.Log("<color=green>Added ducker @ "+key+" - current t: "+_t+"</color>");
	}

	void GenerateRack(float distance){
		_jumpDuckSpace=Mathf.Lerp(_maxJumpDuckSpace,_minJumpDuckSpace,_playTimer/_maxTime);
		float key = distance;
		/*
		foreach(float k in _jumpers.Keys){
			float ab = Mathf.Abs(k-key);
			if(ab<_jumpDuckSpace)
			{
				key=k+_jumpDuckSpace;
				break;
			}
		}
		*/
		Vector3 railPos = _path.GetPoint(key-_tOffset);
		Vector3 forward = _path.GetTangent(key-_tOffset);
		//instance the jumper
		Transform rack = Instantiate(_rack,railPos,Quaternion.identity, null);
		rack.forward=-forward;
		//create the struct
		Rack r = new Rack();
		r.transform = rack;
		r.rack = rack.GetComponent<RackWheel>();
		Phase p = _course[_curPhase];
		r.rack.Init(Random.Range(-1f,1f)*p._maxRackSpeed);
		//r.transform.GetComponent<Rotator>()._speed=Random.Range(90f,180f);
		r.go = rack.gameObject;
		//r.go.SetActive(false);
		//r.mesh=rack.GetComponent<MeshRenderer>();
		//r.mesh.enabled=false;
		_racks.Add(key,r);
		//Debug.Log("<color=magenta>Added rack @ "+key+" - current t: "+_t+"</color>");
	}

	int GenerateNewTrack(){
		float val = Random.value;
		int numTracks = 0;
		//before we do any of this probability stuff
		//is current tOffset past or equal to the gate threshold?
		//if so generate a gate
		/*
		if(_t>=_nextGate){
			AddGate();
			return -1;
		}
		*/
		//add a straight
		if(val<.33f){
			//max of 4 per straight
			numTracks = Random.Range(1,5);
			AddStraight(numTracks);
		}
		//add a curve
		else if(val<.67f){
			float trackToRad = Random.Range(1,6f);
			float radius = _nodeDist*trackToRad;
			numTracks = Random.Range(1,Mathf.FloorToInt(3*trackToRad));
			//max of 5 tracks per curve
			numTracks = Mathf.Min(5,numTracks);
			AddCurve(radius,numTracks,(Random.value<0.5f));
		}
		//add a zig zag
		else{
			numTracks = Random.Range(2,8);
			AddZigZag(Mathf.PI/Random.Range(5f,12f),numTracks,(Random.value<0.5f));
		}
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
		_path = new CubicBezierPath(_knots.ToArray());	
		return removedTracks;
	}

	void ClearOldStuff(Dictionary<float,Item> d, int endClear,bool log=false){
		
		if(log)
			Debug.Log("clearing old stuff up to: "+endClear);
		//Determine which coins need to be removed
		List<float> deleteKeys = new List<float>();
		foreach(float f in d.Keys){
			if(f<endClear)
			{
				deleteKeys.Add(f);
				if(log)
					Debug.Log("Found a thing at: "+f);
			}
		}
		
		//Destroy coins and remove from dict
		foreach(float f in deleteKeys){
			Transform t = d[f].transform;
			Destroy(t.gameObject,Random.value);//stagger the recycling process over a second
			d.Remove(f);
		}
	}

	//resets the linerenderer
	void ResetRail(){
		
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
			//_knots.Add(_knots[_knots.Count-1]+tan*_nodeDist);
			AddKnot(_knots[_knots.Count-1]+tan*_nodeDist);
		}
	}

	void AddZigZag(float angle, int zigzags, bool leftFirst){
		Vector3 tan = _knots[_knots.Count-1]-_knots[_knots.Count-2];
		float ang = Mathf.Atan2(tan.z,tan.x);
		int start= leftFirst ? 0 : 1;
		for(int i=start; i<zigzags; i++){
			float curAngle = i%2==0 ? ang+angle : ang-angle;
			//_knots.Add(_knots[_knots.Count-1]+new Vector3(_nodeDist*Mathf.Cos(curAngle),0,_nodeDist*Mathf.Sin(curAngle)));
			AddKnot(_knots[_knots.Count-1]+new Vector3(_nodeDist*Mathf.Cos(curAngle),0,_nodeDist*Mathf.Sin(curAngle)));
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
			//_knots.Add(pos);
			AddKnot(pos);
		}
	}

	public void AddKnot(Vector3 pos){
		//add knot
		_knots.Add(pos);
		//update path
		if(_knots.Count<2)
			return;
		_path = new CubicBezierPath(_knots.ToArray());	
		//get current time
		float knotT = _tOffset+_knots.Count-1;
		//add support rail
		Transform sr = Instantiate(_supportRail);
		Support s = new Support();
		s.transform=sr;
		s.line=sr.GetComponent<LineRenderer>();
		s.line.SetPosition(0,pos+Vector3.down*100f);
		s.line.SetPosition(1,pos+Vector3.down*0.3f);
		_supports.Add(knotT,s);

		//reset previous coins
		foreach(float key in _coins.Keys){
			ResetCoin(key,(Coin)_coins[key]);
		}

		//reset previous jumpers
		foreach(float key in _jumpers.Keys){
			ResetJumper(key,(Jumper)_jumpers[key]);
		}

		//spawn test
		Phase p = _course[_curPhase];
		//don't spawn nothing till the first few sections
		if(knotT>3f){
			//spawn leftover coins
			if(_leftOverCoins>0)
				GenerateCoinCluster(knotT-1);

			//spawn every .1 length
			for(float i=1; i>=0; i-=0.1f){
				//jumpers
				if(knotT-i>_lastJumper+p._minJumpSpace && Random.value<p._jumpProbability)
				{
					GenerateJumper(knotT-i);
					_lastJumper=knotT-i;
				}
				//coins
				if(knotT-i>_lastCluster+p._minCoinSpace && Random.value<p._coinProbability)
				{
					GenerateCoinCluster(knotT-i);
					_lastCluster=knotT-i;
				}
				//duckers
				if(knotT-i>_lastDucker+p._minDuckerSpace && Random.value<p._duckerProbability)
				{
					GenerateDucker(knotT-i);
					_lastDucker=knotT-i;
				}
				//racks
				if(knotT-i>_lastRack+p._minRackSpace && Random.value<p._rackProbability)
				{
					GenerateRack(knotT-i);
					_lastRack=knotT-i;
				}
			}
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
			_comboPitch = Mathf.Lerp(_comboPitch,_maxComboPitch,.3f);
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
		_sparks.Play();
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

	public void CheckCoinCollisions(){
		bool genNext=false;
		bool gotCluster=false;
		Coin c;
		foreach(float f in _coins.Keys){
			if(f>_t-1 && f <_t+4){
				c = (Coin)_coins[f];

				//disable coins that are too far away
				if(f<_t-.5f)
					c.mesh.enabled=false;
				else
				{
					c.mesh.enabled=true;
					//if coin is close and not collected
					if(Mathf.Abs(f-_t)<.05f && c.transform.tag!="Collected"){
						//check if player is in line
						float dot =Vector3.Dot(c.offset,_railTracker.up); 
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
		}
		//gen coin clusters after the previous has been collected
		//if(genNext)
		//	GenerateCoinCluster();
		//assign points for full cluster gather
		if(gotCluster)
			AddScore(5);
	}

	public void CheckJumperCollisions(){
		bool gearExplode=false;
		Jumper j;
		foreach(float f in _jumpers.Keys){
			j = (Jumper)_jumpers[f];
			if(f>_t-1 && f<_t+4){
				bool notCol = j.transform.tag!="Collected";
				if(notCol)
				{
					//j.mesh.enabled=true;
					
					if(Mathf.Abs(f-_t)<.04f && _ethan.localPosition.y < _minJumpY){
						if(!_tutorial){
							if(_moveSpeed>_invincibleSpeed || _zen){
								gearExplode=true;
								Transform vfx = j.transform.GetChild(0);
								ParticleSystem p = vfx.GetComponent<ParticleSystem>();
								p.Play();
								j.transform.tag="Collected";
								j.mesh.enabled=false;
								_smash.Play();
								if(OnGearCollected!=null)
									OnGearCollected.Invoke();
							}
							else{
								j.transform.GetComponent<Rotator>()._speed=0;
								GameOver();
							}
						}
						//tutorial
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
			AddScore(2);
	}

	public void CheckDuckerCollisions(){
		bool genDuck=false;
		Ducker tmp;
		foreach(float f in _duckers.Keys){
			tmp = (Ducker)_duckers[f];
			if(f>_t-1 && f<_t+4){
				//only process jumpers that haven't been 'collected' or 'destroyed'
				bool notCol = tmp.transform.tag!="Collected";
				if(notCol)
				{
					tmp.mesh.enabled=true;
					if(Mathf.Abs(f-_t)<.04f && !_crouching){
						if(_moveSpeed>_invincibleSpeed || _zen){
							//destroy the gear
							Transform vfx = tmp.transform.GetChild(0);
							ParticleSystem p = vfx.GetComponent<ParticleSystem>();
							p.Play();
							tmp.transform.tag="Collected";
							tmp.mesh.enabled=false;
							_smash.Play();
							genDuck=true;
						}
						else{
							tmp.transform.GetComponent<Rotator>()._speed=0;
							GameOver();
						}
					}
					else if(_t-f>.04f){
						genDuck=true;
						tmp.transform.tag="Collected";
					}
				}
			}
		}
		if(genDuck)
			AddScore(1);
	}

	public void CheckRackCollisions(){
		bool genRack=false;
		Rack tmp;
		foreach(float f in _racks.Keys){
			tmp=(Rack)_racks[f];
			if(f>_t-1 && f<_t+4){
				bool notCol = tmp.transform.tag!="Collected";
				if(notCol)
				{
					if(Mathf.Abs(f-_t)<.04f && !tmp.rack.IsSafe(_railTracker)){
						if(_moveSpeed>_invincibleSpeed || _zen){
							//destroy the gear
							ParticleSystem p = tmp.transform.GetComponentInChildren<ParticleSystem>();
							p.Play();
							tmp.transform.tag="Collected";
							tmp.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().enabled=false;
							_smash.Play();
							genRack=true;
						}
						else{
							tmp.rack.StopSpinning();
							GameOver();
						}
					}
					else if(_t-f>.04f){
						genRack=true;
						tmp.transform.tag="Collected";
					}
				}
			}
		}
		if(genRack)
		{
			//GenNextRack();
			AddScore(1);
		}
	}

	public void CheckSupportCollisions(){
		Support j;
		foreach(float f in _supports.Keys){
			j = (Support)_supports[f];
			if(Mathf.Abs(f-_t)<.04f && Vector3.Dot(_railTracker.up,Vector3.down)>0.98f){
				if(!_tutorial){
					if(_moveSpeed>_invincibleSpeed || _zen){
						//don't worry about collisions when invinc or zen
					}
					else{
						GameOver();
					}
				}
				//tutorial
				else{
					_gameState=2;
					_gearHit.Play();
					StartCoroutine(Rewind());
					return;
				}
			}
		}
	}

	public void CheckBatteryCollisions(){

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
							Uncrouch();
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
							Uncrouch();
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

				//check collisions with items
				CheckCoinCollisions();
				CheckDuckerCollisions();
				CheckRackCollisions();
				CheckJumperCollisions();
				CheckSupportCollisions();
				//CheckBatteryCollisions


				//check for new track gen if only X tracks lie ahead
				if(_t-_tOffset>_knots.Count-_lookAheadTracks){
					//update track
					int numTracks = GenerateNewTrack();
					int removed = RemoveOldTrack(_t);

					//clear stuff
					ClearOldStuff(_coins,_tOffset+removed);
					ClearOldStuff(_jumpers,_tOffset+removed);
					ClearOldStuff(_duckers,_tOffset+removed);
					ClearOldStuff(_racks,_tOffset+removed);
					ClearOldStuff(_supports,_tOffset+removed);

					//update line renderer
					ResetRail();

					_tOffset+=removed;
					UpdateFloorPlane();
				}

				//physicsy stuff
				_moveSpeed = Mathf.Lerp(_moveSpeed,_targetMoveSpeed,_speedChangeLerp*Time.deltaTime);
				_balanceSpeed = _moveSpeed*_balanceSpeedMultiplier;
				_t+=Time.deltaTime*_moveSpeed;

				//tutorial cleanup
				if(_tutorial && _t > 13){
					_tutorialObjs.SetActive(false);
					_tutorial=false;
					PlayerPrefs.SetInt("tut",1);
					PlayerPrefs.Save();
					_collectedCoins=0;
					AddCoin(0);
					StartCoroutine(FadeInScoreText(0));
				}

				//invincinbility effect
				/*
				if(_moveSpeed>_invincibleSpeed){
					Color color;
					if(_moveSpeed>_invincibleSpeed+_invincibleWarning){
						color = _invincibilityGrad.Evaluate(Mathf.PingPong(_t,1f))*.5f;
					}
					else{
						color = Color.white*Mathf.PingPong(_t,0.2f)*5f;
					}
					//_ethanMat.SetColor("_EmissionColor",color);
					//_invincMeter.fillAmount=Mathf.InverseLerp(_invincibleSpeed,_maxSpeed,_moveSpeed);
				}
				else
				{
					//_ethanMat.SetColor("_EmissionColor",Color.black);
					//_invincDisplay.alpha=0;
				}
				*/
				_playTimer+=Time.deltaTime;
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
		//player prefs helpers
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
			_balance-=_curvature*_balanceMult*2;
		localEuler.z = -_balance;
		_railTracker.localEulerAngles=localEuler;
	}

	/*
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
		foreach(ParticleSystem ps in _speedParts){
			ps.Play();
		}
	}
	*/


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
		Destroy(_gate.gameObject);
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
		_sparks.Stop();
		_anim.enabled=false;
	}

	public void Resume(){
		_gameState=1;
		_sparks.Play();
		_anim.enabled=true;
	}

	float minFloor=40;
	float maxFloor=20f;
	float _floorNoiseMult=0.25f;
	float tmpFloorWidth=15f;
	int resolution=2;
	int bDepth=4;
	void UpdateFloorPlane(){
		Mesh m = new Mesh();
		//2 verts per knot
		Vector3[] points = new Vector3[_knots.Count*2*resolution];
		//2 triangles per knot gap
		int[] tris = new int[((_knots.Count*resolution)-1)*6];
		int triCount=0;
		for(int i=0; i<_knots.Count*resolution; i++){
			//get right vector
			float t = i/(float)resolution;
			Vector3 forward = _path.GetTangent(t).normalized;
			Vector3 pos = _path.GetPoint(t);
			Vector3 right = -Vector3.Cross(forward,Vector3.up);

			//get vertex positions
			float floorDist = Mathf.Lerp(minFloor,maxFloor,
					Mathf.PerlinNoise((_tOffset+t)*_floorNoiseMult+_seed,0));
			Vector3 basePoint=pos+Vector3.down*floorDist;
			points[i*2]=basePoint+right*tmpFloorWidth;
			points[i*2+1]=basePoint-right*tmpFloorWidth;

			//do the tris
			if(i<(_knots.Count*resolution)-1){
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
		UpdateBuildings();
	}

	void UpdateBuildings(){
		//instance buildings at start
		if(_buildings==null)
		{
			_buildings = new Transform[_numBuildings];
			for(int i=0; i<_numBuildings; i++){
				Transform t = Instantiate(_buildingPrefab);
				t.gameObject.SetActive(false);
				_buildings[i]=t;
			}
		}
		//clear buildings
		else{
			for(int i=0; i<_numBuildings; i++){
				_buildings[i].gameObject.SetActive(false);
			}

		}

		//place buildings
		Vector3 [] verts = transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh.vertices;
		for(int i=0; i<verts.Length; i++){
			Vector3 offset=Vector3.right;
			if(i%2==0)//on right side
				offset=(verts[i]-verts[i+1]);
			else
				offset=(verts[i]-verts[i-1]);
			offset*=0.5f;
			for(int j=0; j<bDepth; j++){
				Transform t = _buildings[i*bDepth+j];
				t.gameObject.SetActive(true);
				Vector3 pos=verts[i]+j*offset;
				t.position=pos;
				float rng = Mathf.PerlinNoise(pos.x+_seed,pos.z);
				float width = Mathf.Lerp(7f,10f,1-rng);
				float height = Mathf.Lerp(20f,50f*(j+1),rng);
				t.localScale=new Vector3(width,height,width);
			}
		}
	}

	public void GameOver(){
		_gameState=2;
		_sparks.Stop();
		_anim.enabled=false;
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

	/*
	void OnDrawGizmos(){
	}
	*/
}
