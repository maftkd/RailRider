using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;

public class RailGenerator : MonoBehaviour
{
	Spline _spline;
	LineRenderer _line;
	public Material _lineMat;
	float _nodeDist = 2f;
	int _lineResolution = 10;
	CurveSample _curSplineSample;
	Transform _railTracker;
	//float _uniformCurveModifier=
	// Start is called before the first frame update
	void Start()
	{
		//configure line renderer
		if(GetComponent<LineRenderer>()==null)
			_line = gameObject.AddComponent(typeof(LineRenderer)) as LineRenderer;
		else
			_line = GetComponent<LineRenderer>();
		_line.widthMultiplier=0.1f;
		_line.material=_lineMat;
		//configure spline
		if(GetComponent<Spline>()==null)
			_spline = gameObject.AddComponent(typeof(Spline)) as Spline;
		else
			_spline = GetComponent<Spline>();
		//configure railTracker
		_railTracker=transform.GetChild(0);
		//ok so lets try adding some nodes
		//root node
		_spline.AddNode(new SplineNode(Vector3.zero, Vector3.forward));
		AddNode(Vector3.forward);
		AddNode(new Vector3(1,0,2));
		AddNode(new Vector3(-1,0,2));
		AddNode(new Vector3(-1,0,2));
		AddNode(new Vector3(1,0,2));
		AddNode(new Vector3(0,0,1));
		AdjustDirections();
		
		//create a line to render the spline
		_line.positionCount=_lineResolution*(_spline.nodes.Count-1);
		int counter=0;
		for(int i=0; i<_line.positionCount; i++){
			float t = i/(float)_lineResolution;
			_line.SetPosition(i,_spline.GetSample(t).location);
		}
		//start test
		StartCoroutine(TestRide());
		
	}

	IEnumerator TestRide(){
		float t=0;
		while(t<_spline.nodes.Count-1){
			_curSplineSample = _spline.GetSample(t);
			_railTracker.position=_curSplineSample.location;
			t+=Time.deltaTime;
			yield return null;
		}
	}

	void AddNode(Vector3 dir){
		Vector3 prevPos = _spline.nodes[_spline.nodes.Count-1].Position;
		prevPos+=dir.normalized*_nodeDist;
		_spline.AddNode(new SplineNode(prevPos,prevPos));
	}

	void AdjustDirections(){
		for(int i=0; i<_spline.nodes.Count; i++){
			if(i==0){
				_spline.nodes[i].Direction = (_spline.nodes[i+1].Position-_spline.nodes[i].Position).normalized;
			}
			else if(i==_spline.nodes.Count - 1){
				//last node
				_spline.nodes[i].Direction = (_spline.nodes[i].Position-_spline.nodes[i-1].Position).normalized;
			}
			else{
				//in betweeners
				_spline.nodes[i].Direction = Vector3.Lerp((_spline.nodes[i+1].Position-_spline.nodes[i].Position).normalized,(_spline.nodes[i].Position-_spline.nodes[i-1].Position).normalized,0.5f);
			}
			_spline.nodes[i].Direction=_spline.nodes[i].Direction*0.3f*_nodeDist+_spline.nodes[i].Position;
		}
	}

	// Update is called once per frame
	void Update()
	{

	}

	void OnDrawGizmos(){
		if(_curSplineSample!=null)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(_curSplineSample.location,.1f);
		}
	}
}
