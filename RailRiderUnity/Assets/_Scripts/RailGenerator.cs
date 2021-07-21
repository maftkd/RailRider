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
	public Material _buildingMat;
	public Material _skyMat;
	float _nodeDist = 16f;//approximate segment length
	int _lineResolution = 10;//Number of points on line per segment
	float _moveSpeed=0.6f;//rate at which char moves along rail in segments/sec
	float _targetMoveSpeed;
	Transform _railTracker;

	//balance vars
	float _balance;
	float _t;//measurement of time progress throughout the run
	float _balanceSpeed = 100;//degrees per second of rotation at full balanceVelocity
	int _prevBalanceState = 0;
	int _balanceState = 0;//0=no input, 1=left input, 2=right input
	float _balanceVelocity = 0;//rate at which Character rotates
	float _inputVelocity;
	float _balanceAcceleration = 3f;//rate at which touch input affects velocity
	float _balanceDecceleration = 3f;//rate at which touch velocity slows
	float _minVel = 0.01f;//min velocity before balance clamped to 0
	float _maxVel = 2f;
	public RectTransform _balanceMarker;
	CanvasGroup _balanceCanvas;

	Transform _helmet;
	Dictionary<float, Item> _coins = new Dictionary<float, Item>();
	public Transform _coin;
	float _coinHeight = 1.5f;
	float _batteryHeight = 1.75f;
	public AnimationCurve _coinHeightCurve;
	int _tOffset=0;//subtract from _t to get time about current sections of rail
	int _lookAheadTracks=8;
	public Transform _jumper;
	public Transform _ducker;
	public Transform _rack;
	public Transform _planet;
	public Transform _battery;
	public Transform _supportRail;
	float _lineResFrac;
	Transform _ethan;
	Transform _ethanHandSlot;
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
	Dictionary<float, Item> _planets = new Dictionary<float, Item>();
	Dictionary<float, Item> _batteries = new Dictionary<float, Item>();
	Dictionary<float, Item> _supports = new Dictionary<float, Item>();

	//[ColorUsageAttribute(false,true)]
	public Color _yellow;
	public Color _grey;
	public Color _coinHitColor;
	public Color _cyan;
	public Color _clear;
	public Color _magenta;
	int _collectedCoins;
	float _coinHitThreshold = .95f;
	public int _gameState=0;
	bool _inputLock;
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
	public UnityEvent _tutorialComplete;
	Text _scoreText;
	Text _scoreTextShadow;
	Text _comboText;
	Image _comboFg;
	Image _comboBg;
	Text _trickText;
	RectTransform _trickTextRect;
	float _trickTextTimer;
	float _maxTrickTextTimer=3f;
	float _spinTrick;
	public AnimationCurve _popup;
	public AnimationCurve _dialogCurve;
	CanvasGroup _scoreCanvas;
	public CanvasGroup _authorCanvas;
	float _balanceSpeedMultiplier=280;
	public Text _tDebug,_ngDebug,_gpDebug;
	public GameObject[] _wsText;
	float _scoreChangeTimer;
	int _defaultScoreFont;
	int _boldScoreFont;
	public Text [] _hsText;
	bool _tutorial;
	float _tutCoinStart;
	float _tutDuckerStart;
	float _tutJumperStart;
	float _tutLanternStart;
	CanvasGroup _readyCanvas;
	public AudioSource _music;
	public GameObject _tutorialObjs;
	Camera _main;
	float _curvature;
	public float _curvePower;
	public float _gravityPower;
	Transform _explosion;


	//effects
	public Material _ethanMat;
	public Gradient _invincibilityGrad;
	public AudioSource _takeOff;
	public AudioSource _landing;
	public AudioSource _woosh;
	public AudioSource _smash;
	public AudioSource _gearHit;
	public AudioSource _powerUp;
	public AudioSource _powerDown;
	public TrailRenderer _lTrail,_rTrail;
	public Transform _coinFx;
	int _combo;
	int _comboMult=1;
	float _comboPitch=1f;
	float _minComboPitch=1f;
	float _maxComboPitch=1.5f;
	AudioSource _comboSfx;

	//scoring vars
	float _balanceMult;
	float _trickMult;
	float _speedMult;
	Animator _anim;
	public bool _zen;
	
	//building spawn vars
	public Transform _buildingPrefab;
	int _numBuildings=400;
	Transform[] _buildings;
	float _seed;

	//jump / duck vars
	float _jumpDuckSpace;
	float _uncrouchTimer;
	float _uncrouchTime=0.1f;//time it takes to confirm an uncrouch
	public ParticleSystem _sparks;
	public ParticleSystem _flames;
	ParticleSystem _grindEffects;

	//spawn object spacing vars
	int _clusterCount;
	int _clusterCounter;
	float _lastJumper;
	float _lastCluster;
	float _lastDucker;
	float _lastRack;
	float _lastPlanet;
	float _lastBattery;
	float _lastObstacle;

	//coins vars
	int _leftOverCoins;
	float _leftOverCork;
	float _leftOverRotation;

	//phase vars
	public Phase [] _course;
	public Color [] _accentPalette;
	public Color [] _bgPalette;
	int _phaseCounter;
	Phase _curPhase;
	Phase _prevPhase;
	float _phaseTimer;
	bool _phaseChange;
	int _phaseChangeIn;
	float _phaseChangeT;

	//powerups
	float _batteryDur=-1;
	float _batteryCapacity;
	Transform _batteryTemp;
	Material _batteryTempMat;
	public float _minBatterySpace;
	public float _batteryProbability;
	public float _batteryDuration;
	public float _maxBatteryRotation;
	GameObject _bubble;
	ParticleSystem.MainModule _rain;

	//stats
	float _minBalance=0.1f;
	float _maxBalance=0.8f;
	float _minTrick=270f;
	float _maxTrick=1080f;
	float _minSpeed=0.45f;
	float _maxSpeed=0.8f;
	int _bars=8;

	//physics objects for fall anim
	Transform _ethanPhysics;

	public Transform _shopParent;
	UIManager _menu;

	//dialog
	int _dialogIndex;
	public string[] _dialog;
	bool _ready;

	[System.Serializable]
	public class Phase {
		public string _label;
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
		public float _minRackSpeed;
		public float _maxRackSpeed;
		[Header("General")]
		public float _minObstacleSpace;
		public float _duration;
		public Color _accent;
		public Color _bg;

		public Phase(){}

		public Phase(Phase other){
			_minJumpSpace=other._minJumpSpace;
			_jumpProbability=other._jumpProbability;
			_minCoinSpace=other._minCoinSpace;
			_coinProbability=other._coinProbability;
			_coinClusterSize=other._coinClusterSize;
			_maxCoinRotation=other._maxCoinRotation;
			_maxCoinCork=other._maxCoinCork;
			_minDuckerSpace=other._minDuckerSpace;
			_duckerProbability=other._duckerProbability;
			_minRackSpace=other._minRackSpace;
			_rackProbability=other._rackProbability;
			_minRackSpeed=other._minRackSpeed;
			_maxRackSpeed=other._maxRackSpeed;
			_minObstacleSpace=other._minObstacleSpace;
			_duration=other._duration;
			_accent=other._accent;
			_bg=other._bg;
		}
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

	class Planet : Item{
		public GameObject go;
		public PlanetWheel planet;
	}

	class Support : Item{
		public LineRenderer line;
	}

	class Battery : Item{
		public float dur;
		public Vector3 up;
	}

	int _numBoards;
	int _curBoard;
	int _equippedBoard;
	Transform _boardParent;
	Transform _board;
	Transform _ethanParent;
	
	// Start is called before the first frame update
	void Start()
	{
		//#temp
		_curPhase=GenerateNextPhase();
		_curPhase._accent=_course[0]._accent;
		_curPhase._bg=_course[0]._bg;

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

		//set colors
		_lineMat.SetColor("_EdgeColor",_curPhase._accent);
		_buildingMat.SetColor("_Color",_curPhase._accent);
		_main = Camera.main;
		_main.backgroundColor=_curPhase._bg;
		RenderSettings.fogColor=_curPhase._bg;
		_rain = GameObject.Find("Rain").GetComponent<ParticleSystem>().main;
		_rain.startColor=_curPhase._accent;
		
		//configure railTracker
		_railTracker=transform.GetChild(0);
		_ethanParent=_railTracker.GetChild(0).GetChild(0);
		_anim = _ethanParent.GetChild(0).GetComponent<Animator>();
		_ethanPhysics=_anim.transform;

		//check version
		if(PlayerPrefs.HasKey("version")){
			int v = PlayerPrefs.GetInt("version");
			//#todo check version
		}
		else{
			//if no version, clear all - coins, score, board
			PlayerPrefs.DeleteAll();
			PlayerPrefs.SetInt("version", 1);
			PlayerPrefs.Save();
		}

		//generate track starter
		_tutorial=!PlayerPrefs.HasKey("tut");
		GenerateStartingSection();
		_inputLock=_tutorial;

		//This sets number of coins to 0
		_collectedCoins=0;

		//Get some references
		_balanceCanvas = _balanceMarker.parent.GetComponent<CanvasGroup>();
		_helmet = GameObject.FindGameObjectWithTag("Helmet").transform;
		_ethan = _railTracker.GetChild(0); 
		_bubble = _ethan.Find("Sphere").gameObject;
		_ethanHandSlot = GameObject.Find("HandSlot").transform;
		_ethanMat.SetColor("_OutlineColor",Color.black);
		_gate = GameObject.FindGameObjectWithTag("Gate").transform;
		_scoreText = GameObject.FindGameObjectWithTag("Score").GetComponent<Text>();
		Transform stp = _scoreText.transform.parent;
		_comboText = stp.Find("ComboText").GetComponent<Text>();
		_comboBg = stp.Find("ComboBar").GetChild(0).GetComponent<Image>();
		_comboFg = stp.Find("ComboBar").GetChild(1).GetComponent<Image>();

		//_comboSfx = _comboText.transform.GetComponent<AudioSource>();
		_trickText = GameObject.Find("TrickText").GetComponent<Text>();
		_trickTextRect = _trickText.transform.parent.GetComponent<RectTransform>();
		_trickText.text="";
		_readyCanvas = GameObject.Find("ReadyArrow").GetComponent<CanvasGroup>();
		_scoreCanvas = _scoreText.transform.parent.GetComponent<CanvasGroup>();
		_defaultScoreFont=_scoreText.fontSize;
		_boldScoreFont=Mathf.FloorToInt(_defaultScoreFont*1.4f);
		if(PlayerPrefs.HasKey("hs")){
			foreach(Text t in _hsText)
				t.text=PlayerPrefs.GetInt("hs").ToString("HIGH SCORE: #");
		}
		_explosion = GameObject.FindGameObjectWithTag("Explosion").transform;
		_ethanMat.SetColor("_EmissionColor",Color.black);

		_balanceCanvas.alpha=0;

		SetupShop();
	}

	void SetupShop(){
		//position boards in shop
		_menu = GameObject.Find("Wallet").transform.parent.GetComponent<UIManager>();
		_boardParent = _shopParent.Find("Boards");
		_numBoards = _boardParent.childCount;
		if(PlayerPrefs.HasKey("board"))
		{
			_curBoard=PlayerPrefs.GetInt("board");
			_equippedBoard=_curBoard;
		}
		EquipBoard();
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

		//custom board stats from player prefs
		if(_curBoard==4){
			if(PlayerPrefs.HasKey("balance"))
				b._balance=PlayerPrefs.GetFloat("balance");
			if(PlayerPrefs.HasKey("trick"))
				b._trick=PlayerPrefs.GetFloat("trick");
			if(PlayerPrefs.HasKey("speed"))
				b._speed=PlayerPrefs.GetFloat("speed");
		}

		//show name
		Text title = _shopParent.Find("Canvas").Find("Title").GetComponent<Text>();
		title.text=b._name;

		Transform stats = _shopParent.Find("Canvas").Find("StatsContainer");
		//show cost
		Text cost = stats.GetChild(0).GetChild(0).GetComponent<Text>();
		Button buy = cost.transform.parent.GetChild(1).GetComponent<Button>();
		buy.interactable=false;
		Slider equip = cost.transform.parent.GetChild(5).GetComponent<Slider>();
		equip.gameObject.SetActive(false);
		buy.gameObject.SetActive(true);
		cost.text="Cost: "+b._cost.ToString("0");
		bool owned=false;
		bool equipped=_curBoard==_equippedBoard;
		if(PlayerPrefs.HasKey(b._name)||_curBoard==0){
			owned=true;
			buy.gameObject.SetActive(false);
			cost.text=equipped?"Equipped" : "Equip";
			//active equp toggle
			equip.gameObject.SetActive(true);
			//set toggle if equiped
			equip.value=equipped? 1 : 0;
		}
		else
		{
			if(_menu._coins>=b._cost)
				buy.interactable=true;
		}

		for(int i=1; i<=3; i++){
			Transform stat = stats.GetChild(i);
			Text text = stat.GetChild(0).GetComponent<Text>();
			text.gameObject.SetActive(owned);
			Transform statBars=stat.Find("Stats");
			statBars.gameObject.SetActive(owned);
			if(owned){
				int bars=1;
				switch(i){
					case 1:
						//text.text="Balance: "+b._balance.ToString("0.0");
						bars+=Mathf.RoundToInt((_bars-1)*
								Mathf.InverseLerp(_minBalance,_maxBalance,b._balance));
						break;
					case 2:
						//text.text="Trick: "+b._trick.ToString("0.0");
						bars+=Mathf.RoundToInt((_bars-1)*
								Mathf.InverseLerp(_minTrick,_maxTrick,b._trick));
						break;
					case 3:
						//text.text="Speed: "+b._speed.ToString("0.0");
						bars+=Mathf.RoundToInt((_bars-1)*
								Mathf.InverseLerp(_minSpeed,_maxSpeed,b._speed));
						break;
				}

				for(int j=1; j<=statBars.childCount;j++)
					statBars.GetChild(j-1).GetComponent<RawImage>().color =
						bars>=j ? _yellow : _grey;

			}
			bool customize=owned&&_curBoard==4;
			stat.Find("Minus").gameObject.SetActive(customize);
			stat.Find("Plus").gameObject.SetActive(customize);
		}

		//set particle fx
		if(_curBoard==3)
			_grindEffects=_flames;
		else
			_grindEffects=_sparks;
	}

	[ContextMenu("Clear version")]
	public void ClearVersion(){
		PlayerPrefs.DeleteKey("version");
		PlayerPrefs.Save();
	}

	[ContextMenu("Clear boards")]
	public void DeleteBoards(){
		PlayerPrefs.DeleteKey("The Plank");
		PlayerPrefs.DeleteKey("The Gusto");
		PlayerPrefs.DeleteKey("Flame Princess");
		PlayerPrefs.DeleteKey("Custom");
		PlayerPrefs.Save();
	}

	[ContextMenu("Clear stats")]
	public void ClearStats(){
		PlayerPrefs.DeleteKey("balance");
		PlayerPrefs.DeleteKey("trick");
		PlayerPrefs.DeleteKey("speed");
		PlayerPrefs.Save();
	}

	public void BuyBoard(){
		Hoverboard h = _boardParent.GetChild(_curBoard).GetComponent<Hoverboard>();
		PlayerPrefs.SetInt(h._name,0);
		PlayerPrefs.Save();
		_menu.CoinSpent(h._cost);
		RotateBoard(0);
	}

	public void ChangeStat(string stat){
		string[] parts = stat.Split('%');
		string name = parts[0];
		int dir = int.Parse(parts[1]);
		Debug.Log("Changing stat on custom board - "+name+" in direction: "+dir);
		switch(name){
			default:
			case "balance":
				_balanceMult+=dir*((_maxBalance-_minBalance)/(_bars-1));
				if(_balanceMult>_maxBalance)
					_balanceMult=_maxBalance;
				else if(_balanceMult<_minBalance)
					_balanceMult=_minBalance;
				PlayerPrefs.SetFloat("balance",_balanceMult);
				break;
			case "trick":
				_trickMult+=dir*((_maxTrick-_minTrick)/(_bars-1));
				if(_trickMult>_maxTrick)
					_trickMult=_maxTrick;
				else if(_trickMult<_minTrick)
					_trickMult=_minTrick;
				PlayerPrefs.SetFloat("trick",_trickMult);
				break;
			case "speed":
				_speedMult+=dir*((_maxSpeed-_minSpeed)/(_bars-1));
				if(_speedMult>_maxSpeed)
					_speedMult=_maxSpeed;
				else if(_speedMult<_minSpeed)
					_speedMult=_minSpeed;
				PlayerPrefs.SetFloat("speed",_speedMult);
				break;
		}
		if(_equippedBoard==4)
			EquipBoard();
		PlayerPrefs.Save();
		RotateBoard(0);
	}

	public void ToggleEquipBoard(){
		if(_curBoard!=_equippedBoard)
			_equippedBoard=_curBoard;
		EquipBoard();
		RotateBoard(0);
	}

	void EquipBoard(){
		if(_ethanParent.GetComponentInChildren<Hoverboard>()!=null)
			Destroy(_ethanParent.GetComponentInChildren<Hoverboard>().gameObject);
		//add new
		_board = Instantiate(_boardParent.GetChild(_equippedBoard),_ethanParent);
		Hoverboard b = _board.GetComponent<Hoverboard>();
		b._rotate=false;
		_board.localPosition=Vector3.zero;
		_board.localEulerAngles=Vector3.zero;
		_board.localScale=Vector3.one*1f;
		PlayerPrefs.SetInt("board",_curBoard);
		//set stats
		_trickMult=b._trick;
		_balanceMult=b._balance;
		_speedMult=b._speed;
		if(_curBoard==4){
			if(PlayerPrefs.HasKey("balance"))
				_balanceMult=PlayerPrefs.GetFloat("balance");
			if(PlayerPrefs.HasKey("trick"))
				_trickMult=PlayerPrefs.GetFloat("trick");
			if(PlayerPrefs.HasKey("speed"))
				_speedMult=PlayerPrefs.GetFloat("speed");
		}
		_moveSpeed=_speedMult;
		_targetMoveSpeed=_moveSpeed;
		_balanceSpeed = _moveSpeed*_balanceSpeedMultiplier;
	}
	
	//function used in update loop to reset velocity towards 0
	void LimitVelocity(){
		if(_inputVelocity!=0){
			_inputVelocity=Mathf.Lerp(_inputVelocity,0,_balanceDecceleration*Time.deltaTime*_balanceMult);
			if(Mathf.Abs(_inputVelocity)<_minVel)
				_inputVelocity=0;
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
		_grindEffects.Stop();
		//determine trick
		int trick=-1;
		Jumper j = new Jumper();
		foreach(float f in _jumpers.Keys){
			j=(Jumper)_jumpers[f];
			if(f>_t-1 && f<_t+4){
				if(f-_t<.5f&&f-_t>0){
					trick=j.type;
					//j.transform.tag="Collected";
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
			_grindEffects.Play();
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
		int trickScore=0;
		if(trick>=0)
			trickScore=4+Mathf.RoundToInt(Mathf.Abs(_spinTrick/90f));
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
		if(!_tutorial)
			AddCurve(_nodeDist*Random.Range(3,5f),3,(Random.value < 0.5f));

		ResetRail();

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
		int numCoins=_leftOverCoins>0? _leftOverCoins : _curPhase._coinClusterSize;
		//reset cluster count unless left over coins from prev segment
		if(_leftOverCoins<=0){
			_clusterCount=numCoins;
		}
		//determine spacing
		float coinSpacing=0.1f;//.075f;
		//for loop
		//float maxR = Mathf.Lerp(45f,180f,_playTimer/_maxTime);
		float maxR=_curPhase._maxCoinRotation;;
		float cork = _leftOverCoins>0? _leftOverCork : _curPhase._maxCoinCork*Random.Range(-1f,1f);
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
		curCoin.name=final?"final-"+_clusterCount:"coin";
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
		foreach(float k in _planets.Keys){
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

	void GenerateJumper(float key){
		Vector3 railPos = _path.GetPoint(key-_tOffset);
		Vector3 forward = _path.GetTangent(key-_tOffset);
		//instance the jumper
		Transform jump = Instantiate(_jumper,railPos,Quaternion.identity, null);
		jump.forward=-forward;
		//create the struct
		Jumper j=new Jumper();
		j.type=Random.Range(0,3);
		j.transform = jump;
		float rand = Random.Range(90f,180f)*(Random.Range(0,2)*2f-1f);
		j.transform.GetComponent<Rotator>()._speed=rand;
		//j.transform.GetComponent<Rotator>()._speed=Random.Range(90f,180f);
		j.mesh=jump.GetComponent<MeshRenderer>();
		_jumpers.Add(key,j);
	}

	void GenerateDucker(float distance){
		//_jumpDuckSpace=Mathf.Lerp(_maxJumpDuckSpace,_minJumpDuckSpace,_playTimer/_maxTime);
		float key = distance;
		Vector3 railPos = _path.GetPoint(key-_tOffset);
		Vector3 forward = _path.GetTangent(key-_tOffset);
		//instance the jumper
		Transform duck = Instantiate(_ducker,railPos,Quaternion.identity, null);
		duck.forward=-forward;
		//create the struct
		Ducker d=new Ducker();
		d.transform = duck;
		float rand = Random.Range(90f,180f)*(Random.Range(0,2)*2f-1f);
		d.transform.GetComponent<Rotator>()._speed=rand;
		d.mesh=duck.GetComponent<MeshRenderer>();
		d.mesh.enabled=false;
		_duckers.Add(key,d);
		//Debug.Log("<color=green>Added ducker @ "+key+" - current t: "+_t+"</color>");
	}

	void ResetDucker(float t, Ducker j){
		Vector3 railPos = _path.GetPoint(t-_tOffset);
		j.transform.position=railPos;
	}

	void GenerateRack(float distance){
		float key = distance;
		Vector3 railPos = _path.GetPoint(key-_tOffset);
		Vector3 forward = _path.GetTangent(key-_tOffset);
		//instance the jumper
		Transform rack = Instantiate(_rack,railPos,Quaternion.identity, null);
		rack.forward=-forward;
		//create the struct
		Rack r = new Rack();
		r.transform = rack;
		r.rack = rack.GetComponent<RackWheel>();
		float rand = Random.Range(_curPhase._minRackSpeed,_curPhase._maxRackSpeed)*(Random.Range(0,2)*2f-1f);
		r.rack.Init(rand);
		r.go = rack.gameObject;
		_racks.Add(key,r);

	}

	void ResetPlanet(float t, Planet j){
		Vector3 railPos = _path.GetPoint(t-_tOffset);
		j.transform.position=railPos;
	}

	void GenerateBattery(float distance){
		float key = distance;
		Vector3 railPos = _path.GetPoint(key-_tOffset);
		Vector3 forward = _path.GetTangent(key-_tOffset);
		//instance the battery
		Transform battery = Instantiate(_battery,railPos,Quaternion.identity, null);
		battery.localScale=Vector3.one*3f;
		//enable rotation
		battery.GetComponent<Rotator>().enabled=true;
		//setup struct
		Battery r = new Battery();
		r.transform = battery;
		r.mesh=battery.GetComponent<MeshRenderer>();
		r.dur=_batteryDuration;
		//random rotation around rail
		//r.RotateAround(railPos,forward,r);
		battery.position+=Vector3.up*_batteryHeight;
		float rand = Random.value*_maxBatteryRotation*(Random.Range(0,2)*2f-1f);
		battery.RotateAround(railPos,forward,rand);
		r.up=(battery.position-railPos).normalized;
		battery.rotation=Quaternion.identity;
		_batteries.Add(key,r);
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
		if(val<.33f || _tutorial){
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
		
		//reset previous duckers
		foreach(float key in _duckers.Keys){
			ResetDucker(key,(Ducker)_duckers[key]);
		}
		
		//reset previous planets
		/*
		foreach(float key in _planets.Keys){
			ResetPlanet(key,(Planet)_planets[key]);
		}
		*/

		//leftover coins
		if(_leftOverCoins>0)
			GenerateCoinCluster(knotT-1);

		if(_phaseChangeIn>0)
		{
			_phaseChangeIn--;
			Debug.Log("Phase change in: "+_phaseChangeIn);
			if(_phaseChangeIn<=0){
				_prevPhase=new Phase(_curPhase);
				_curPhase = GenerateNextPhase();
				_phaseTimer=0;
				_phaseChange=false;
			}
			return;
		}
		//spawn stuff
		//don't spawn nothing till the first few sections
		if(knotT>5f && !_tutorial){

			//spawn every .1 length
			for(float i=1; i>=0; i-=0.1f){
				//jumpers
				if(knotT-i>_lastJumper+_curPhase._minJumpSpace && Random.value<_curPhase._jumpProbability)
				{
					if(knotT-i>_lastObstacle+_curPhase._minObstacleSpace){
						GenerateJumper(knotT-i);
						_lastJumper=knotT-i;
						_lastObstacle=knotT-i;
					}
				}
				//coins
				if(knotT-i>_lastCluster+_curPhase._minCoinSpace && Random.value<_curPhase._coinProbability)
				{
					if(knotT-i>_lastObstacle+_curPhase._minObstacleSpace){
						GenerateCoinCluster(knotT-i);
						_lastCluster=knotT-i;
						_lastObstacle=knotT-i;
					}
				}
				//duckers
				if(knotT-i>_lastDucker+_curPhase._minDuckerSpace && Random.value<_curPhase._duckerProbability)
				{
					if(knotT-i>_lastObstacle+_curPhase._minObstacleSpace){
						GenerateDucker(knotT-i);
						_lastDucker=knotT-i;
						_lastObstacle=knotT-i;
					}
				}
				//racks
				if(knotT-i>_lastRack+_curPhase._minRackSpace && Random.value<_curPhase._rackProbability)
				{
					if(knotT-i>_lastObstacle+_curPhase._minObstacleSpace){
						GenerateRack(knotT-i);
						_lastRack=knotT-i;
						_lastObstacle=knotT-i;
					}
				}
				//batteries
				if(knotT-i>_lastBattery+_minBatterySpace && Random.value<_batteryProbability)
				{
					if(knotT-i>_lastObstacle+_curPhase._minObstacleSpace){
						GenerateBattery(knotT-i);
						_lastBattery=knotT-i;
						_lastObstacle=knotT-i;
					}
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
		/*
		if(_zen)
			return;
			*/
		if(setValue==-1)
		{
			if(OnCoinCollected!=null)
				OnCoinCollected.Invoke();
			//_collectedCoins++;
			//_combo++;
			AddScore(1);
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
		_collectedCoins+=points*_comboMult;
		_scoreText.text=_collectedCoins.ToString();
		_scoreText.fontSize=_boldScoreFont;
		_scoreChangeTimer=.4f;
		UpdateCombo();
	}

	public void StartRiding(bool zen){
		_zen=zen;
		_gameState=1;
		_grindEffects.Play();
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

	public void CheckCoinCollisions(){
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
						}
						/*
						if(c.transform.name.Split('-')[0]=="final")
						{
							_comboPitch=_minComboPitch;
							if(_clusterCounter==int.Parse(c.transform.name.Split('-')[1]))
								AddScore(5);
							//c.transform.name="foo";
						}
						*/
					}
					//on coin miss
					else if(_t-f>.05f && c.transform.tag!="Collected"){
						/*
						if(_combo>0){
						}
						if(c.transform.name=="final")
						{
							c.transform.name="foo";
						}
						*/
						UpdateCombo(false);
						c.transform.tag="Collected";
						//_clusterCounter=0;
					}
				}
			}
		}
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
						if(_zen){
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
						else if(_tutorial && _dialogIndex<20){
							_gearHit.Play();
							//rewind
							_gameState=2;
							StartCoroutine(RewindR(2f));
							_dialogIndex=15;
							ShowNextDialog();
							LockInput(true);
						}
						else{
							j.transform.GetComponent<Rotator>()._speed=0;
							GameOver();
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
						if(_zen){
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
							if(_tutorial)
							{
								if(_dialogIndex<20){
									//rewind
									_gameState=2;
									_gearHit.Play();
									StartCoroutine(RewindR(2f));
									_dialogIndex=12;
									ShowNextDialog();
									LockInput(true);
								}
							}
							else{
								tmp.transform.GetComponent<Rotator>()._speed=0;
								GameOver();
							}
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
						if(_zen){
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

	public void CheckPlanetCollisions(){
		bool genPlanet=false;
		Planet tmp;
		foreach(float f in _planets.Keys){
			tmp=(Planet)_planets[f];
			if(f>_t-1 && f<_t+4){
				bool notCol = tmp.transform.tag!="Collected";
				if(notCol)
				{
					if(Mathf.Abs(f-_t)<.04f && (!tmp.planet.IsSafe(_railTracker) || _ethan.localPosition.y < _minJumpY)){
						if(_ethan.localPosition.y<_minJumpY)
							Debug.Log("Jump fail");
						else
							Debug.Log("Planet fail");
						if(_zen){
							//destroy the gear
							ParticleSystem [] p = tmp.transform.GetComponentsInChildren<ParticleSystem>();
							foreach(ParticleSystem ps in p)
								ps.Play();
							tmp.transform.tag="Collected";
							MeshRenderer[] meshes = tmp.transform.GetComponentsInChildren<MeshRenderer>();
							foreach(MeshRenderer mr in meshes)
							{
								if(mr.transform!=tmp.transform)
									mr.enabled=false;
							}
							_smash.Play();
							genPlanet=true;
						}
						else{
							tmp.planet.StopSpinning();
							GameOver();
						}
					}
					else if(_t-f>.04f){
						genPlanet=true;
						tmp.transform.tag="Collected";
					}
				}
			}
		}
		if(genPlanet)
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
					if(_zen){
						//don't worry about collisions when invinc or zen
					}
					else{
						GameOver();
					}
				}
			}
		}
	}

	public void CheckBatteryCollisions(){
		Battery j;
		foreach(float f in _batteries.Keys){
			j = (Battery)_batteries[f];
			if(Mathf.Abs(f-_t)<.04f && Vector3.Dot(_railTracker.up,j.up)>0.85f){
				bool notCol = j.transform.tag!="Collected";
				if(notCol)
				{
					j.transform.tag="Collected";
					j.transform.gameObject.SetActive(false);
					AcquireBattery();
				}
				//tutorial
				else{
					return;
				}
			}
		}
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

				if(!_inputLock)
				{
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
								_inputVelocity=Mathf.Lerp(_inputVelocity,_maxVel,_balanceAcceleration*Time.deltaTime*_balanceMult);
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
								_inputVelocity = Mathf.Lerp(_inputVelocity,-_maxVel,_balanceAcceleration*Time.deltaTime*_balanceMult);

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
				}
				SetPosition();

				//check collisions with items
				CheckCoinCollisions();
				CheckDuckerCollisions();
				CheckRackCollisions();
				CheckJumperCollisions();
				CheckPlanetCollisions();
				CheckSupportCollisions();
				CheckBatteryCollisions();


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
					ClearOldStuff(_planets,_tOffset+removed);
					ClearOldStuff(_supports,_tOffset+removed);
					ClearOldStuff(_batteries,_tOffset+removed);

					//update line renderer
					ResetRail();

					_tOffset+=removed;
					UpdateFloorPlane();
				}

				//advance timer
				_t+=Time.deltaTime*_moveSpeed;

				if(_tutorial){
					if(!_ready)
					{
						_readyCanvas.alpha=Mathf.PingPong(Time.time,1f);
						//ready on touch
						if(_prevBalanceState==0 && _balanceState>0&&_balanceState<3)
						{
							_ready=true;
							_readyCanvas.alpha=0;
						}
					}
					//welcome / intro
					if(_t>3.5f && _dialogIndex==0){
						ShowNextDialog();
					}
					if(_dialogIndex==1&&_ready){
						ShowNextDialog();
					}
					if(_dialogIndex==2&&_ready){
						ShowNextDialog();
					}
					if(_dialogIndex==3&&_ready){
						ShowNextDialog();
					}
					if(_dialogIndex==4&&_ready){
						ShowNextDialog();
					}
					//tap to rotate / collect coins
					if(_dialogIndex==5&&_ready){
						ShowNextDialog();
						StartCoroutine(PulseAndDestroyCanvasR(GameObject.Find("TapRight")));
					}
					if(_dialogIndex==6&&_ready){
						ShowNextDialog();
						StopAllCoroutines();
						if(GameObject.Find("TapRight")!=null)
							Destroy(GameObject.Find("TapRight"));
						StartCoroutine(PulseAndDestroyCanvasR(GameObject.Find("TapLeft")));
					}
					if(_dialogIndex==7&&_ready){
						StopAllCoroutines();
						if(GameObject.Find("TapLeft")!=null)
							Destroy(GameObject.Find("TapLeft"));
						ShowNextDialog();
						//spawn some coins
						_tutCoinStart=_t+3f;
						GenerateTutorialCoins(_tutCoinStart);
						_balanceCanvas.alpha=1;
						_dialogIndex=8;
					}
					if(_dialogIndex==8 && _ready&&_inputLock)
						LockInput(false);
					if(_t>_tutCoinStart+2.1f && _dialogIndex==8){
						LockInput(true);
						if(_clusterCounter==10){
							ShowNextDialog();
							_dialogIndex=11;
						}
						else if(_clusterCounter>=7){
							ShowNextDialog();
							ShowNextDialog();
							_dialogIndex=11;
						}
						else{
							ShowNextDialog();
							ShowNextDialog();
							ShowNextDialog();
							_dialogIndex=7;
							_gameState=2;
							ClearTutorialCoins();
							StartCoroutine(RewindR(2f));
						}
					}
					if(_dialogIndex==11&&_ready){
						ShowNextDialog();
					}
					if(_dialogIndex==12&&_ready){
						ShowNextDialog();
						_tutDuckerStart=_t+2f;
						GenerateTutorialDucker(_tutDuckerStart);
					}
					if(_dialogIndex==13 && _ready && _inputLock)
						LockInput(false);
					if(_t>_tutDuckerStart+0.5f && _dialogIndex==13){
						ShowNextDialog();//nice duck
						LockInput(true);
					}
					if(_dialogIndex==14&&_ready){
						ShowNextDialog();
					}
					if(_dialogIndex==15&&_ready){
						ShowNextDialog();
						_tutJumperStart=_t+2f;
						GenerateTutorialJumper(_tutJumperStart);
					}
					if(_dialogIndex==16&&_ready&&_inputLock)
						LockInput(false);
					if(_t>_tutJumperStart+0.5f && _dialogIndex==16){
						ShowNextDialog();//nice jump
						LockInput(true);
					}
					if(_dialogIndex==17&&_ready){
						ShowNextDialog();
					}
					if(_dialogIndex==18&&_ready){
						ShowNextDialog();
					}
					if(_dialogIndex==19&&_ready){
						ShowNextDialog();
					}
					if(_dialogIndex==20&&_ready){
						ShowNextDialog();
						_tutLanternStart=_t+1f;
						_maxBatteryRotation=0;
						GenerateTutorialLantern(_tutLanternStart);
					}
					if(_t>_tutLanternStart+0.5f && _dialogIndex==21){
						_dialogIndex=22;
						//reusing tutLantern start
						_tutLanternStart=_t+1f;
						GenerateTutorialStuff(_tutLanternStart);
						LockInput(false);
					}
					if(_t>_tutLanternStart+3f && _dialogIndex==22){
						ShowNextDialog();
						LockInput(true);
					}
					if(_dialogIndex==23 && _ready){
						ShowNextDialog();
					}
					if(_dialogIndex==24 && _ready){
						ShowNextDialog();
					}
					if(_dialogIndex==25 && _ready){
						//end tutorial
						CompleteTut();
						_tutorialComplete.Invoke();
					}
				}

				//animate power-up battery cell
				if(_batteryDur>=0){
					_batteryDur-=Time.deltaTime;
					//update battery fill meter
					_batteryTempMat.SetFloat("_Power",_batteryDur/_batteryCapacity);
					if(_batteryDur<0)
					{
						RemoveBattery();
					}
				}

				//phase change logic
				if(!_phaseChange && !_tutorial){
					_phaseTimer+=Time.deltaTime;
					if(_phaseTimer>=_curPhase._duration){
						Debug.Log("Time for another phase");
						_phaseChange=true;
						float curT = _t-_tOffset;
						int curTi = Mathf.FloorToInt(curT);
						_phaseChangeIn = 1;
						_phaseChangeT=_t+_knots.Count-curTi-1;
					}
				}
				_prevBalanceState=_balanceState;
				break;
			case 2://rewind
				break;
			case 3://gate check
				break;
		}

		//animate score change
		if(_scoreChangeTimer>0){
			_scoreChangeTimer-=Time.deltaTime;
			if(_scoreChangeTimer<=0)
				_scoreText.fontSize=_defaultScoreFont;
		}

		//animate trick text
		if(_trickTextTimer>0){
			Vector2 lPos = _trickTextRect.anchoredPosition;
			if(_tutorial&&_dialogIndex!=8&&_dialogIndex!=13)
			{
				lPos.y = Mathf.Lerp(-250f,0f,
						_dialogCurve.Evaluate(1-_trickTextTimer/_maxTrickTextTimer));
			}
			else
				lPos.y = Mathf.Lerp(-250f,0f,_popup.Evaluate(_trickTextTimer/_maxTrickTextTimer));
			_trickTextRect.anchoredPosition=lPos;
			_trickTextTimer-=Time.deltaTime;
			/*
			if(_trickTextTimer<=0)
				_trickText.text="";
				*/
		}
	}

	void SetPosition(){
		//calc new pos and forward for rail
		float railT = _t-_tOffset;
		_railTracker.position = _path.GetPoint(railT);
		_railTracker.forward = _path.GetTangent(railT);
		Vector3 localEuler = _railTracker.localEulerAngles;

		//calc rail curvature
		Vector3 nextForward = _path.GetTangent(railT+1f/(float)_lineResolution);
		_curvature = Vector3.Cross(_railTracker.forward,nextForward).y;
		//Debug.Log("curve: "+_curvature);
		
		//balance physics
		if(!_jumping && !_zen)
		{
			//affect of gravity
			_balanceVelocity=Mathf.Sign(_balance)*_balance*_balance*Time.deltaTime*_gravityPower;
			//affect of curvature
			_balanceVelocity+=Mathf.Sign(_curvature)*Mathf.Abs(_curvature)*Time.deltaTime*_curvePower;
		}
		//apply input "velocity"
		float vel = _inputVelocity*Time.deltaTime+_balanceVelocity*_balanceMult;
		//_balance-=vel*Time.deltaTime*_balanceSpeed;
		if(!_inputLock)
			_balance-=vel*_balanceSpeed;

		if(Mathf.Abs(_balance)>110f && !_zen){
			GameOver(true);
		}

		//set ethan rotation
		localEuler.z = -_balance;
		_railTracker.localEulerAngles=localEuler;
		//set balance marker rotation
		localEuler.y=0;
		_balanceMarker.localEulerAngles=localEuler;
	}

	IEnumerator FadeInScoreText(float delay=6f){
		float timer=0;
		while(timer<1f){
			timer+=Time.deltaTime;
			_authorCanvas.alpha=timer;
			yield return null;

		}
		yield return new WaitForSeconds(delay-1);
		if(_zen){
			_scoreText.text="";
			_comboText.text="";
		}
		timer=0;
		while(timer<1){
			timer+=Time.deltaTime;
			_scoreCanvas.alpha=timer;
			_authorCanvas.alpha=1-timer;
			_balanceCanvas.alpha=timer;
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

	IEnumerator RewindR(float time=1f){
		float timer=0;
		_music.pitch=2;
		while(timer<time){
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

	[ContextMenu("Complete tut")]
	public void CompleteTut(){
		PlayerPrefs.SetInt("tut",0);
		PlayerPrefs.Save();
	}

	public void Pause(){
		_gameState=0;
		_grindEffects.Stop();
		_anim.enabled=false;
	}

	public void Resume(){
		_gameState=1;
		_grindEffects.Play();
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
				pos.x=Mathf.Round(pos.x)+0.1f;
				pos.y=Mathf.Round(pos.y)+0.1f;
				pos.z=Mathf.Round(pos.z)+0.1f;
				t.position=pos;
				float rng = Mathf.PerlinNoise(pos.x+_seed,pos.z);
				float width = Mathf.Round(Mathf.Lerp(7f,10f,1-rng));
				float height = Mathf.Round(Mathf.Lerp(20f,50f*(j+1),rng));
				t.localScale=new Vector3(width,height,width);
				/*
				float fracX = (pos.x+width*0.5f)%1f;
				if(Mathf.Abs(0.5f-fracX)>0.3f)
					t.position+=Vector3.right*0.5f;
				float fracZ = (pos.z+width*0.5f)%1f;
				if(Mathf.Abs(0.5f-fracZ)>0.3f)
					t.position+=Vector3.forward*0.5f;
				float fracY = (pos.y+height*0.5f)%1f;
				if(Mathf.Abs(0.5f-fracY)>0.3f)
					t.position+=Vector3.up*0.5f;
					*/
			}
		}
	}
	
	IEnumerator NightTimeR(){
		Color accentPrev = _curPhase._accent;
		Color accentCur = _course[1]._accent;
		Color bgPrev = _curPhase._bg;
		Color bgCur = _course[1]._bg;

		Color tmp;
		Color tmp2;
		float timer=0;
		while(timer<1f){
			timer+=Time.deltaTime;
			tmp = Color.Lerp(accentPrev,accentCur,timer/1f);
			tmp2 = Color.Lerp(bgPrev,bgCur,timer/1f);
			_lineMat.SetColor("_EdgeColor",tmp);
			_buildingMat.SetColor("_Color",tmp);
			_main.backgroundColor=tmp2;
			RenderSettings.fogColor=tmp2;
			_rain.startColor=tmp;

			yield return null;
		}
	}

	IEnumerator DayTimeR(){
		Color accentPrev = _course[1]._accent;
		Color accentCur = _curPhase._accent;
		Color bgPrev = _course[1]._bg;
		Color bgCur = _curPhase._bg;

		Color tmp;
		Color tmp2;
		float timer=0;
		while(timer<1f){
			timer+=Time.deltaTime;
			tmp = Color.Lerp(accentPrev,accentCur,timer/1f);
			tmp2 = Color.Lerp(bgPrev,bgCur,timer/1f);
			_lineMat.SetColor("_EdgeColor",tmp);
			_buildingMat.SetColor("_Color",tmp);
			_main.backgroundColor=tmp2;
			RenderSettings.fogColor=tmp2;
			_rain.startColor=tmp;
			yield return null;
		}
	}

	void AcquireBattery(){
		_balanceVelocity=0;
		//add a battery to ethands hands
		if(_batteryDur<0){
			_batteryTemp = Instantiate(_battery,_ethanHandSlot);
			_batteryTemp.GetChild(0).gameObject.SetActive(false);
			_batteryTempMat=_batteryTemp.GetComponent<MeshRenderer>().materials[1];
			_music.pitch=1.1f;
			StopAllCoroutines();
			StartCoroutine(NightTimeR());
		}
		_powerUp.Play();
		StartCoroutine(BlinkRoutine());
		_batteryTempMat.SetFloat("_Power",1f);
		_batteryDur=_batteryDuration;
		_batteryCapacity=_batteryDur;
		//set battery material thingy
		_anim.SetBool("carry",true);
		_zen=true;
		_bubble.SetActive(true);
	}

	void RemoveBattery(){
		_anim.SetBool("carry",false);
		Destroy(_batteryTemp.gameObject);
		_zen=false;
		_ethanMat.SetColor("_OutlineColor",Color.black);
		_music.pitch=1f;
		_powerDown.Play();
		_bubble.SetActive(false);
		StartCoroutine(DayTimeR());
		//correct rotation

		int extraSpins=0;
		//take out any full rotations
		if(Mathf.Abs(_balance)>=360){
			extraSpins=(int)(_balance/360);
			_balance-=360f*extraSpins;
		}
		if(Mathf.Abs(_balance)>180)
		{
			_balance=-Mathf.Sign(_balance)*180+(Mathf.Abs(_balance)-180);
		}
	}

	IEnumerator BlinkRoutine(){
		for(int i=0; i<5; i++){
			_ethanMat.SetColor("_OutlineColor",Color.yellow);
			yield return new WaitForSeconds(0.1f);
			_ethanMat.SetColor("_OutlineColor",Color.black);
			yield return new WaitForSeconds(0.1f);
		}
		_ethanMat.SetColor("_OutlineColor",Color.yellow);
	}

	void ShowNextDialog(){
		_trickText.text=_dialog[_dialogIndex];
		_trickTextTimer=_maxTrickTextTimer;
		_dialogIndex++;
		_ready=false;
	}

	IEnumerator PulseAndDestroyCanvasR(GameObject go,float dur=3f, int numCycles=3){
		CanvasGroup cg = go.GetComponent<CanvasGroup>();
		float timer=0;
		float frac;
		while(timer<dur){
			timer+=Time.deltaTime;
			frac=timer/dur;
			frac*=numCycles;
			//sine cycle and abs
			float t = Mathf.Sin(frac*6.28f);
			cg.alpha=t;
			yield return null;
		}
		Destroy(go);
	}

	void ClearTutorialCoins(){
		//Destroy coins and remove from dict
		List<float> deleteKeys = new List<float>();
		foreach(float f in _coins.Keys){
			deleteKeys.Add(f);
		}
		
		//Destroy coins and remove from dict
		foreach(float f in deleteKeys){
			Transform t = _coins[f].transform;
			Destroy(t.gameObject);
			_coins.Remove(f);
		}

	}
	void GenerateTutorialCoins(float time){
		//regen coins
		_clusterCount=11;//made this one big so cluster counter isn't reset
							//in CheckCoinCollision
		for(int i=0; i<5; i++)
			GenerateCoin(time+0.1f*i,-50f-i*3f,false);
		for(int i=0; i<5; i++)
			GenerateCoin(time+1.5f+0.1f*i,50f+i*3f,false);
		_clusterCounter=0;
	}

	void GenerateTutorialDucker(float time){
		GenerateDucker(time);
	}

	void GenerateTutorialLantern(float time){
		GenerateBattery(time);
	}

	void GenerateTutorialJumper(float time){
		GenerateJumper(time);
	}

	void GenerateTutorialStuff(float time){
		GenerateJumper(time);
		GenerateDucker(time+0.5f);
		GenerateRack(time+1f);
	}

	Phase GenerateNextPhase(){
		Debug.Log("Phase counter: "+_phaseCounter);
		Phase p = new Phase();
		if(_curPhase!=null){
			p._bg=_curPhase._bg;
			p._accent=_curPhase._accent;
		}

		//first 5 alternate jump/duck and rack
		//every 5 are coin levels
		//after 5, three are mixed
		//first 5 should take maybe 2-3 mins
		if(_phaseCounter>0 && _phaseCounter%5==0){
			p._minCoinSpace=0;
			p._coinProbability=0.2f;
			p._coinClusterSize=10;
			p._maxCoinRotation=45f;
			p._maxCoinCork=5f;
			p._minObstacleSpace=2.5f;
			p._duration=15f;
		}
		else if(_phaseCounter<5){
			float frac = Mathf.InverseLerp(0,4f,_phaseCounter);
			//coins
			p._minCoinSpace=2f;
			p._coinProbability=0.1f;
			p._coinClusterSize=5;
			p._maxCoinRotation=Mathf.Lerp(45f,60f,frac);
			p._maxCoinCork=Mathf.Lerp(5f,7f,frac);
			if(_phaseCounter%2==0){
				//jumps and ducks
				p._minJumpSpace=0.5f;
				p._jumpProbability=Mathf.Lerp(0.05f,0.2f,frac);
				p._minDuckerSpace=0.5f;
				p._duckerProbability=Mathf.Lerp(0.05f,0.2f,frac);

				//general
				p._minObstacleSpace=Mathf.Lerp(1f,0.75f,frac);
				p._duration=Mathf.Lerp(10f,15f,frac);
			}
			else{
				//racks
				p._minRackSpace=1;
				p._rackProbability=Mathf.Lerp(0.05f,0.5f,frac);
				p._minRackSpeed=Mathf.Lerp(30f,90f,frac);
				p._maxRackSpeed=Mathf.Lerp(60f,160f,frac);
				
				//general
				p._minObstacleSpace=Mathf.Lerp(1f,0.75f,frac);
				//p._duration=Mathf.Lerp(5f,10f,frac);
				//rack levels are shorter cuz they tougher
				p._duration=5f;
			}
		}
		else{
			float frac = Mathf.InverseLerp(6,10,_phaseCounter);
			//gen mixed
			//coins
			p._minCoinSpace=2f;
			p._coinProbability=0.3f;
			p._coinClusterSize=5;
			p._maxCoinRotation=Mathf.LerpUnclamped(60f,90f,frac);
			p._maxCoinCork=Mathf.LerpUnclamped(7f,9f,frac);

			//jumps and ducks
			p._minJumpSpace=0.5f;
			p._jumpProbability=Mathf.LerpUnclamped(0.2f,0.5f,frac);
			p._minDuckerSpace=0.5f;
			p._duckerProbability=Mathf.LerpUnclamped(0.2f,0.5f,frac);

			//racks
			p._minRackSpace=1;
			p._rackProbability=0.5f;
			p._minRackSpeed=30f;
			p._maxRackSpeed=160f;
			
			//general
			p._duration=Mathf.LerpUnclamped(15f,30f,frac);
			p._minObstacleSpace=Mathf.LerpUnclamped(0.75f,0.5f,frac);
		}

		_phaseCounter++;
		return p;
	}

	void UpdateCombo(bool increase=true){
		if(increase)
			_combo++;
		else
			_combo=0;

		int comboLevel = _combo/25;
		//cap combo level at 3
		if(comboLevel>3)
			comboLevel=3;
		//get mult
		_comboMult = Mathf.RoundToInt(Mathf.Pow(2,comboLevel));
		_comboText.text=_comboMult+"x";
		switch(comboLevel){
			case 0:
				_comboBg.color = _clear;
				_comboFg.color = _cyan;
				_comboText.color=Color.white;
				break;
			case 1:
				_comboBg.color = _cyan;
				_comboFg.color = _magenta;
				_comboText.color=_cyan;
				break;
			case 2:
				_comboBg.color = _magenta;
				_comboFg.color = _yellow;
				_comboText.color=_magenta;
				break;
			case 3:
			default:
				_comboBg.color = _yellow;
				_comboText.color=_yellow;
				break;
		}
		_comboFg.fillAmount = (_combo%25 / 25f);

		Debug.Log("combo: "+_combo);
		Debug.Log("combo mult: "+_comboMult);
	}

	public void GameOver(bool fall=false){
		_balanceCanvas.alpha=0;
		_gameState=2;
		_grindEffects.Stop();
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

		//fall animation
		float force=fall? 100f :300f;
		_ethanPhysics.gameObject.AddComponent<Rigidbody>();
		_ethanPhysics.gameObject.AddComponent<SphereCollider>();
		_ethanPhysics.GetComponent<Rigidbody>().AddForce(Random.insideUnitSphere*force);
		Destroy(_ethanPhysics.gameObject,10f);

		_board.gameObject.AddComponent<Rigidbody>();
		_board.gameObject.AddComponent<BoxCollider>();
		_board.GetComponent<Rigidbody>().AddForce(Random.insideUnitSphere*force);
		Destroy(_board.gameObject,10f);
	}

	void LockInput(bool l){
		_inputLock=l;
		_balanceCanvas.alpha=l?0:1;
		_crouching=false;
		_jumping=false;
		_balance=0;
		_inputVelocity=0;
		_balanceVelocity=0;
	}

	public void ReplayTutorial(){
		ClearTut();
		_tutorialComplete.Invoke();
	}

	/*
	void OnDrawGizmos(){
	}
	*/
}
