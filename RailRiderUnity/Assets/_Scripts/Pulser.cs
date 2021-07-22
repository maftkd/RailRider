using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pulser : MonoBehaviour
{
	CanvasGroup _cg;
    // Start is called before the first frame update
    void Start()
    {
		_cg = GetComponent<CanvasGroup>();
    }

    // Update is called once per frame
    void Update()
    {
		_cg.alpha = Mathf.PingPong(Time.time,1f);
    }
}
