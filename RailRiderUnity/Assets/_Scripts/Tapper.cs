using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tapper : MonoBehaviour
{
	float timer=0;
	RawImage _dot;
	RawImage _ring;
	Transform _ringTransform;
	public Color _dotFull;
	public Color _dotTrans;
	public Color _ringFull;
	public Color _ringTrans;
	CanvasGroup _cg;
	Transform _cam;
	// Start is called before the first frame update
	void Start()
	{
		_dot=GetComponent<RawImage>();
		_ringTransform = transform.GetChild(0);
		_ring = _ringTransform.GetComponent<RawImage>();
		_cg = transform.parent.GetComponent<CanvasGroup>();
		_cam = Camera.main.transform;
	}

	// Update is called once per frame
	void Update()
	{
		if(timer<1f)
			timer+=Time.deltaTime;
		else
			timer=0;

		_dot.color=Color.Lerp(_dotFull,_dotTrans,timer);
		_ringTransform.localScale = Vector3.one*Mathf.Lerp(.5f,1.5f,timer);
		_ring.color=Color.Lerp(_ringFull,_ringTrans,timer);

		float dist = Vector3.Distance(transform.position,_cam.position);
		_cg.alpha = Mathf.Lerp(1,0,(dist-20)/5);
	}
}
