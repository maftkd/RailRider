using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogHelper : MonoBehaviour
{
	Cubemap _skybox;
	// Start is called before the first frame update
	void Start()
	{
		Camera main = Camera.main;
		main.depthTextureMode= main.depthTextureMode | DepthTextureMode.Depth;
	}

	// Update is called once per frame
	void Update()
	{

	}
}
