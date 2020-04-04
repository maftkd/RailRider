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
    private float _prevYaw;
    public float _acceleration;
    [Tooltip("this controls how quckly delta yaw affects balance rate")]
    public float _centrifugal;
    [Tooltip("This controls how quickly you fall solely based on your angle to the rail")]
    public float _angAccel;

    //balance stuff
    public float _balanceAngle = 0;

    //input stuff
    public float _baseInputPower;
    public float _fallOffPowerMultiplier;
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
                    //control speed based off vertical differential
                    float yDelta = transform.position.y - _prevPos.y;                    
                    _speed += _acceleration * yDelta*yDelta *Mathf.Sign(yDelta);
                    _speed = Mathf.Min(_speed,_maxSpeed);

                    //control balance based on rail curvature (yaw)
                    float yawDelta = transform.rotation.y - _prevYaw;                    
                    _balanceAngle += yawDelta * _speed*_centrifugal;

                    //balance angle increases (along a curve, or exponentially)
                    float diff = (Mathf.PI * .5f - _balanceAngle)*-1f; //diff represents how off-balance the player is
                    _balanceAngle += diff * _angAccel * Time.deltaTime;

                    float inputInfluence = -1f * Input.GetAxis("Horizontal") * Time.deltaTime * (_baseInputPower*(1+Mathf.Abs(diff)*_fallOffPowerMultiplier));
                    Debug.Log("inputInfluence: " + inputInfluence);
                    _balanceAngle += inputInfluence;

                    if(Mathf.Abs(diff) > Mathf.PI * .5f)
                    {
                        Debug.Log("Fall off");
                        _paused = true;
                    }
                }
                _prevPos = transform.position;
                _prevYaw = transform.rotation.y;
            }
            else
            {
                _paused = true;
                Debug.Log("Level over!");
            }
        }
    }
}
