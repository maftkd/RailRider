using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scaler : MonoBehaviour
{
	float scaleTimer=0;
	public AnimationCurve _curve;
	Vector3 baseScale;
    // Start is called before the first frame update
    void Start()
    {
		baseScale = transform.localScale;
		transform.localScale=Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
		scaleTimer+=Time.deltaTime;
		float scale=Mathf.LerpUnclamped(0,1,_curve.Evaluate(scaleTimer));
		transform.localScale=baseScale*scale;
		if(scaleTimer>=1f)
			enabled=false;
    }
}
