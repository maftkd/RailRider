using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scaler : MonoBehaviour
{
	float _scaleRate=2f;
	float minScale=1f;
	float maxScale=150f;
	float scaleTimer=0;
	public AnimationCurve _curve;
    // Start is called before the first frame update
    void Start()
    {
		transform.localScale=Vector3.one*minScale;
    }

    // Update is called once per frame
    void Update()
    {
		scaleTimer+=Time.deltaTime;
		float scale=Mathf.LerpUnclamped(minScale,maxScale,_curve.Evaluate(scaleTimer));
		transform.localScale=Vector3.one*scale;
		if(scaleTimer>=1f)
			enabled=false;
    }
}
