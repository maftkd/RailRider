using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

public class SplashManager : MonoBehaviour
{
	public GameObject ppObj;
	ColorGrading cg;
	FloatParameter pe;
	// Start is called before the first frame update
	void Start()
	{

		PostProcessVolume pp = ppObj.GetComponent<PostProcessVolume>();
		pp.profile.TryGetSettings(out cg);
		cg.postExposure.value=0f;
		StartCoroutine(SplashHelper());
	}

	// Update is called once per frame
	void Update()
	{

	}

	IEnumerator SplashHelper(){
		float timer = 0;
		while(!Input.GetMouseButtonUp(0) && timer < 3f)
		{
			timer+=Time.deltaTime;
			yield return null;
		}
		timer=0;
		while(timer<3f && !Input.GetMouseButtonUp(0)){
			timer+=Time.deltaTime;
			//pe.value=timer;
			cg.postExposure.value=timer*2;
			//animate post exposure of post procesing profile
			yield return null;
		}
		SceneManager.LoadScene(1);
	}
}
