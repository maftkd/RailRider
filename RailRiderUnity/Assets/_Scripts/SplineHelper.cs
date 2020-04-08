using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;

[ExecuteInEditMode]
public class SplineHelper : MonoBehaviour
{
    public List<GameObject> _nodes;
    private List<Vector3> _prevPos;
    private List<Quaternion> _prevRot;
    private Spline _spline;
    public float _uniformCurveModifier = 1;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        //reset pos and rot vectors at start and when nodes are added / removed
        if (_prevPos == null || _nodes.Count!=_prevPos.Count)
            _prevPos = new List<Vector3>();
        if (_prevRot == null || _nodes.Count!=_prevRot.Count)
            _prevRot = new List<Quaternion>();
        for (int i=0; i<_nodes.Count; i++)
        {
            //detect a node's position / rotation has changed
            if(i < _prevPos.Count && (_prevPos[i]!=_nodes[i].transform.position ||
                _prevRot[i] != _nodes[i].transform.rotation))
            { //only update on change
                UpdateNodeData();
                
                return;
            }
        }
        _prevPos.Clear();
        _prevRot.Clear();
        foreach (GameObject g in _nodes)
        {
            _prevPos.Add(g.transform.position);
            _prevRot.Add(g.transform.rotation);
        }
#endif

    }

    public void UpdateNodeData()
    {
        Debug.Log(_spline.nodes.Count);
        List<SplineNode> newNodes = new List<SplineNode>();
        for(int i=0; i<_nodes.Count; i++)
        {
            SplineNode node;
            //first node
            if(i==0)
            {
                Vector3 pos = _nodes[i].transform.position;
                Vector3 nextPos = _nodes[i + 1].transform.position;
                Vector3 dir = (nextPos - pos).normalized*_uniformCurveModifier;
                node = new SplineNode(pos, pos + dir);
            }
            else if (i == _nodes.Count - 1)
            {
                //last node
                Vector3 pos = _nodes[i].transform.position;
                Vector3 prevPos = _nodes[i - 1].transform.position;
                Vector3 dir = (pos - prevPos).normalized* _uniformCurveModifier;
                node = new SplineNode(pos, pos + dir);
            }
            else
            {
                //in between node
                Vector3 pos = _nodes[i].transform.position;
                Vector3 nextPos = _nodes[i + 1].transform.position;
                Vector3 prevPos = _nodes[i - 1].transform.position;
                Vector3 dir = Vector3.Lerp((nextPos - pos).normalized,(pos-prevPos).normalized,.5f)* _uniformCurveModifier;
                node = new SplineNode(pos, pos + dir);
            }
            node.Up = _nodes[i].transform.up;
            newNodes.Add(node);
        }
        _spline.nodes = newNodes;
        _spline.CurveChanged.Invoke();
        _spline.RefreshCurves();
    }

    private void OnValidate()
    {
        _spline = GetComponent<Spline>();
        if (_nodes.Count < 2)
        {
            Debug.LogError("Need at least 2 nodes in spline");
        }
        UpdateNodeData();
    }
}
