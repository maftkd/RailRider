using SplineMesh;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailRider : MonoBehaviour
{
    public Spline _rail;

    public bool _paused = true;
    public float _levelProgress = 0;
    public float _speed = .1f;
    public float _maxSpeed = 1f;
    public float _heightOffRail;
    public float _maxProgress;

    //physics stuff
    private Vector3 _prevPos;
    public float _acceleration;

    //balance stuff
    public float _balanceAngle = 0;
    // Start is called before the first frame update
    void Start()
    {
        _maxProgress = (float)(_rail.nodes.Count-1);
    }

    // Update is called once per frame
    void Update()
    {
        if (!_paused)
        {
            _levelProgress += _speed * Time.deltaTime;
            if (_levelProgress <= _maxProgress)
            {
                CurveSample _curSample = _rail.GetSample(_levelProgress);
                Vector3 right = Vector3.Cross(_curSample.up, _curSample.tangent);
                Vector3 offset = right * Mathf.Cos(_balanceAngle) + Vector3.up * Mathf.Sin(_balanceAngle);
                transform.position = _curSample.location + offset * _heightOffRail;
                transform.forward = _curSample.tangent;
                Camera.main.transform.localEulerAngles = Vector3.forward * (_balanceAngle*Mathf.Rad2Deg - 90f);
                //transform.up = offset;
                //transform.right = Vector3.Cross(_curSample.tangent, offset);

                //not on the first frame
                if (_prevPos != Vector3.zero)
                {
                    float yDelta = transform.position.y - _prevPos.y;
                    Debug.Log("yDelta: " + yDelta);
                    _speed += _acceleration * yDelta*yDelta *Mathf.Sign(yDelta);
                    _speed = Mathf.Min(_speed,_maxSpeed);
                }
                _prevPos = transform.position;
            }
            else
            {
                _paused = true;
                Debug.Log("Level over!");
            }
        }
    }
}
