using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Tubify : MonoBehaviour
{
    public MeshFilter _filter;
    public Transform _points;
    public int _numDivs;
    public float _radius;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        Mesh mesh;
        if (_filter.sharedMesh != null)
            mesh = _filter.sharedMesh;
        else
            mesh = new Mesh();
        Vector3[] verts;
        Vector3[] norms;        
        Vector2[] uvs;
        int[] tris;

        tris = new int[6 * _numDivs * (_points.childCount - 1)];

        verts = new Vector3[_points.childCount * _numDivs];
        norms = new Vector3[_points.childCount * _numDivs];
        uvs = new Vector2[_points.childCount * _numDivs];

        for(int ring = 0; ring < _points.childCount; ring++)
        {
            float ringFrac = (float)ring / _points.childCount;
            for (int div = 0; div < _numDivs; div++)
            {
                int index = ring * _numDivs + div;
                Transform curPoint = _points.GetChild(ring);
                float frac = ((float)div / _numDivs);
                float angle = Mathf.PI * 2f * frac;
                Vector3 offset = Vector3.zero;
                offset += curPoint.right * Mathf.Cos(angle) * _radius + curPoint.up * Mathf.Sin(angle) * _radius;
                verts[index] = curPoint.localPosition + offset;
                norms[index] = offset;
                uvs[index] = new Vector2(frac, ringFrac);

                //triangle
                if (ring < _points.childCount - 1)
                {
                    if(div < _numDivs - 1)
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

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;
        _filter.mesh = mesh;
    }
}
