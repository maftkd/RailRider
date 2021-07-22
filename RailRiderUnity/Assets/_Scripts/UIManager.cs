using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
	CanvasGroup _menu;
	public AudioMixer _mixer;
	bool _musicMuted=false;
	bool _sfxMuted=false;
	public Texture _musicMutedTex;
	public Texture _unmutedTex;
	public RawImage _musicButton;
	public RawImage _littleEx;
	RailGenerator _railGen;
	public CanvasGroup _pauseMenu;
	[HideInInspector]
	public int _coins;
	[HideInInspector]
	public int _gears;
	public Text _coinText;
	public Text _gearText;
	
	// Start is called before the first frame update
	void Start()
	{
		//get menu
		_menu = transform.GetChild(0).GetComponent<CanvasGroup>();
		if(_menu==null)
			_menu = GetComponent<CanvasGroup>();

		//get rail gen
		_railGen = GameObject.FindGameObjectWithTag("GameController").GetComponent<RailGenerator>();	
		
		if(_musicButton!=null){
			if(PlayerPrefs.HasKey("Music")){
				if(PlayerPrefs.GetInt("Music")==1){
					_musicButton.enabled=true;
					_mixer.SetFloat("MusicVolume", -80f);
					_musicMuted=true;
				}
			}
			if(PlayerPrefs.HasKey("Sfx")){
				if(PlayerPrefs.GetInt("Sfx")==1){
					//sfx texture = muted
					_littleEx.enabled=true;
					_mixer.SetFloat("SfxVolume",-80f);
					_sfxMuted=true;
				}
			}
		}
		//get coins
		if(_coinText!=null){
			if(PlayerPrefs.HasKey("Coin")){
				_coins = PlayerPrefs.GetInt("Coin");
			}
			if(PlayerPrefs.HasKey("Gear")){
				_gears = PlayerPrefs.GetInt("Gear");
			}
			_coinText.text=_coins.ToString("0");
			_gearText.text=_gears.ToString("0");
			_railGen.OnCoinCollected+=CoinCollected;
			_railGen.OnGearCollected+=GearCollected;
		}
	}

	public void CoinSpent(int c){
		_coins-=c;
		_coinText.text=_coins.ToString("0");
		PlayerPrefs.SetInt("Coin",_coins);
	}

	public void CoinCollected(){
		_coins++;
		_coinText.text=_coins.ToString("0");
		PlayerPrefs.SetInt("Coin",_coins);
	}
	public void GearCollected(){
		_gears++;
		_gearText.text=_gears.ToString("0");
		PlayerPrefs.SetInt("Gear",_gears);
	}

	// Update is called once per frame
	void Update()
	{

	}

	public void FadeOutMenu(){
		StartCoroutine(FadeOutRoutine());
	}

	IEnumerator FadeOutRoutine(){
		_menu.blocksRaycasts=false;
		_menu.interactable=false;
		float timer = 0;
		while (timer<1f){
			_menu.alpha = 1-timer*2;
			timer+=Time.deltaTime;
			yield return null;
		}
		_menu.alpha=0;
	}

	public void FadeInMenu(){
		StartCoroutine(FadeInRoutine());
	}
	IEnumerator FadeInRoutine(){
		float timer = 0;
		_menu.blocksRaycasts=true;
		_menu.interactable=true;
		while(timer < 1f){
			_menu.alpha = timer;
			timer+=Time.deltaTime;
			yield return null;
		}
		_menu.alpha=1;
	}

	public void ToggleMusic(){
		_musicMuted=!_musicMuted;
		_mixer.SetFloat("MusicVolume", _musicMuted ? -80f : 0f);
		if(_musicMuted)
			_musicButton.enabled=true;
		else
			_musicButton.enabled=false;
		if(PlayerPrefs.HasKey("Music")){
			PlayerPrefs.DeleteKey("Music");
		}
		PlayerPrefs.SetInt("Music",_musicMuted ? 1 : 0);
		PlayerPrefs.Save();
	}
	
	public void ToggleSfx(){
		_sfxMuted=!_sfxMuted;
		_mixer.SetFloat("SfxVolume", _sfxMuted ? -80f : 0f);
		if(_sfxMuted)
			_littleEx.enabled=true;
		else
			_littleEx.enabled=false;
		if(PlayerPrefs.HasKey("Sfx")){
			PlayerPrefs.DeleteKey("Sfx");
		}
		PlayerPrefs.SetInt("Sfx",_sfxMuted ? 1 : 0);
		PlayerPrefs.Save();
	}

	public void Reload(){
		SceneManager.LoadScene(0);
	}

	public void Pause(){
		if(_railGen._gameState==1){
			_railGen.Pause();		
			_pauseMenu.alpha=1;
			_pauseMenu.blocksRaycasts=true;
			_pauseMenu.interactable=true;
		}
	}

	public void Resume(){
		_railGen.Resume();
		_pauseMenu.alpha=0;
		_pauseMenu.blocksRaycasts=false;
		_pauseMenu.interactable=false;
	}
}
