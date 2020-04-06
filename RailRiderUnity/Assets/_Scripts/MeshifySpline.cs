using SplineMesh;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MeshifySpline : MonoBehaviour
{
    public Spline _spline;
    [Range(0,1)]
    public float _meshEnd;
    public int _pointsPerCurve;
    private int _numPoints;
    public int _numDivs;
    public float _radius;

    //mesh data
    private Vector3[] verts;
    private Vector3[] norms;
    private Vector2[] uvs;
    private int[] tris;

    //renderer
    public MeshFilter _meshFilter;

    //safety first
    private bool _threadLocked;

    private void Awake()
    {
        mesh = new Mesh();
        _meshFilter.sharedMesh = mesh;
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateMesh();
    }

    // Update is called once per frame
    void Update()
    {
        //temp code to always update mesh
        //in practice this only needs to be done on animate
#if UNITY_EDITOR
        UpdateMesh();
#endif
    }
    Mesh mesh;
    public void UpdateMesh()
    {

        mesh.Clear();

        _numPoints = Mathf.FloorToInt(_pointsPerCurve * _spline.curves.Count * _meshEnd);
        int numCurves = _numPoints % _pointsPerCurve == 0 ? _numPoints / _pointsPerCurve : _numPoints / _pointsPerCurve + 1;
        int pointCounter = 0;
        if (_numPoints < 2)
            return;
        int vertCount = _numPoints * _numDivs;
        verts = new Vector3[vertCount];
        norms = new Vector3[vertCount];
        uvs = new Vector2[vertCount];
        tris = new int[6 * _numDivs * (_numPoints - 1)];
        
        for(int i=0; i<numCurves; i++)
        {
            CubicBezierCurve mCurve = _spline.curves[i];
            float param = 0;
            for (int j=0; j<_pointsPerCurve; j++)
            {
                param = (float)j / (float)_pointsPerCurve;
                if (pointCounter < _numPoints)
                {
                    Vector3 pos = mCurve.GetLocation(param);
                    Vector3 forward = (mCurve.GetLocation(param + .01f)-pos).normalized;
                    Vector3 up = mCurve.GetUp(param).normalized;
                    Vector3 right = Vector3.Cross(forward, up);
                    DrawRing(pointCounter, (float)pointCounter / (float)_numPoints, pos, right, up);
                }
                else
                    break;

                pointCounter++;
            }
        }
        
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.normals = norms;
        mesh.uv = uvs;
    }

    private void DrawRing(int ring, float ringFrac, Vector3 position, Vector3 right, Vector3 up)
    {
        for (int div = 0; div < _numDivs; div++)
        {
            int index = ring * _numDivs + div;
            float frac = ((float)div / _numDivs);
            float angle = Mathf.PI * 2f * frac;
            Vector3 offset = Vector3.zero;
            offset += right * Mathf.Cos(angle) * _radius + up * Mathf.Sin(angle) * _radius;
            verts[index] = position + offset;
            norms[index] = offset;
            uvs[index] = new Vector2(frac, (float)ring);

            //triangle
            if (ring < _numPoints-1)
            {
                if (div < _numDivs - 1)
                {
                    //use next div to complete tris
                    tris[index * 6] = index;
                    tris[index * 6 + 2] = index + _numDivs;
                    tris[index * 6 + 1] = index + _numDivs + 1;
                    tris[index * 6 + 3] = index;
                    tris[index * 6 + 5] = index + _numDivs + 1;
                    tris[index * 6 + 4] = index + 1;
                }
                else
                {
                    //use first div to complete tris
                    tris[index * 6] = index;
                    tris[index * 6 + 2] = index + _numDivs;
                    tris[index * 6 + 1] = index + 1;
                    tris[index * 6 + 3] = index;
                    tris[index * 6 + 5] = index + 1;
                    tris[index * 6 + 4] = index + 1 - _numDivs;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {/*
        Gizmos.color = Color.red;
        foreach(SplineNode node in _spline.nodes)
        {
            Gizmos.DrawSphere(node.Position,.1f);
        }
        Gizmos.color = Color.green;
        foreach(CubicBezierCurve curve in _spline.curves)
        {
            float param = 0;
            for(int i=0; i<_pointsPerCurve; i++)
            {
                param = (float)i / (float)_pointsPerCurve;
                Gizmos.DrawSphere(curve.GetLocation(param), .05f);
            }
        }*/
    }
}
