using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyLocation : MonoBehaviour
{
	public Transform _target;
	Vector3 _offset;
	float y;
	// Start is called before the first frame update
	void Start()
	{
		y=transform.position.y;
		_offset = transform.position-_target.position;
	}

	// Update is called once per frame
	void Update()
	{
		Vector3 p = _target.position;
		p.y=y;
		transform.position = p;
	}
}
