using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
	CanvasGroup _menu;
	public AudioMixer _mixer;
	bool _muted=false;
	
	// Start is called before the first frame update
	void Start()
	{

		_menu = GetComponent<CanvasGroup>();
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
			_menu.alpha = 1-timer;
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

	public void ToggleAudio(){
		_muted=!_muted;
		_mixer.SetFloat("Volume", _muted ? -80f : 0f);
	}

	public void Reload(){
		SceneManager.LoadScene(0);
	}
}
