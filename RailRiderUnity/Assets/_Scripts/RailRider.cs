﻿using SplineMesh;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailRider : MonoBehaviour
{
    //game state stuff
    public enum RideStates { PAUSED, RIDING, FALLING, WINNING}
    public RideStates _state = RideStates.PAUSED;
    public FadeHelper _introFader;
    

    public Spline _rail;

    public bool _paused = true;
    public float _levelProgress = 0;
    public float _speed = .1f;
    public float _maxSpeed = 1f;
    public float _heightOffRail;
    public float _maxProgress;

    //physics stuff
    private Vector3 _prevPos;
    private Vector3 _prevForward;
    public float _acceleration;
    [Tooltip("this controls how quckly delta yaw affects balance rate")]
    public float _centrifugal;
    [Tooltip("This controls how quickly you fall solely based on your angle to the rail")]
    public float _angAccel;

    //balance stuff
    public float _balanceAngle = 0;
    public Animator _avatarAnim;

    //input stuff
    public float _baseInputPower;
    public float _fallOffPowerMultiplier;
    public float _inputOffset;
    public float _inputDecayRate;
    // Start is called before the first frame update
    void Start()
    {
        _maxProgress = (float)(_rail.nodes.Count-1);
    }

    // Update is called once per frame
    void Update()
    {
        switch (_state)
        {
            case RideStates.PAUSED:
                //temp input till menu is in
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                {
                    _state = RideStates.RIDING;
                    _introFader.FadeOut(true);
                }
                break;
            case RideStates.RIDING:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    _state = RideStates.PAUSED;
                    _introFader.FadeIn(true);
                    return;
                }
                _levelProgress += _speed * Time.deltaTime;
                if (_levelProgress <= _maxProgress)
                {
                    CurveSample _curSample = _rail.GetSample(_levelProgress);
                    Vector3 right = Vector3.Cross(_curSample.up, _curSample.tangent);
                    Vector3 offset = right * Mathf.Cos(_balanceAngle) + _curSample.up * Mathf.Sin(_balanceAngle);
                    transform.position = _curSample.location + offset * _heightOffRail;
                    transform.forward = _curSample.tangent;
                    //transform.up = _curSample.up;
                    //transform.right = Vector3.Cross( _curSample.up, _curSample.tangent);
                    transform.GetChild(0).localEulerAngles = Vector3.forward * (_balanceAngle * Mathf.Rad2Deg - 90f);
                    //transform.up = offset;
                    //transform.right = Vector3.Cross(_curSample.tangent, offset);

                    //not on the first frame
                    if (_prevPos != Vector3.zero)
                    {
                        //control speed based off vertical differential
                        float yDelta = transform.position.y - _prevPos.y;
                        _speed += _acceleration * yDelta * yDelta * Mathf.Sign(yDelta);
                        _speed = Mathf.Min(_speed, _maxSpeed);

                        //control balance based on rail curvature (yaw)
                        Quaternion deltaQuat = Quaternion.FromToRotation(_prevForward, transform.forward);
                        float yawDelta = deltaQuat.y;
                        _balanceAngle += yawDelta * Mathf.Pow(_speed,0.5f) * _centrifugal;

                        //balance angle increases (along a curve, or exponentially)
                        float diff = (Mathf.PI * .5f - _balanceAngle) * -1f; //diff represents how off-balance the player is
                        _balanceAngle += diff * diff * _angAccel * Time.deltaTime;

                        float balanceLevel = 1f-(diff / (Mathf.PI * .5f) + 1) * .5f;
                        _avatarAnim.SetFloat("balance", balanceLevel);

                        float horIn = Input.GetAxis("Horizontal");
                        if (Input.GetMouseButton(0))
                            horIn = Input.mousePosition.x < 320 ? -1f : 1f;


                        _inputOffset += -1f * horIn * Time.deltaTime * (_baseInputPower * (1 + (diff*diff) * _fallOffPowerMultiplier));
                        _balanceAngle += _inputOffset;
                        _inputOffset = Mathf.Lerp(_inputOffset, 0, Time.deltaTime * _inputDecayRate);

                        if (Mathf.Abs(diff) > Mathf.PI * .5f)
                        {
                            Debug.Log("Fall off");
                            _state = RideStates.FALLING;
                        }
                    }
                    _prevPos = transform.position;
                    _prevForward = transform.forward;
                }
                else
                {
                    _state = RideStates.WINNING;
                    Debug.Log("Level over!");
                }
                break;
            case RideStates.FALLING:
                GetComponent<SphereCollider>().enabled = true;
                Rigidbody bod = GetComponent<Rigidbody>();
                bod.isKinematic = false;
                bod.useGravity = true;
                bod.AddTorque(Random.onUnitSphere*15f);
                FollowTarget ft = Camera.main.transform.GetComponent<FollowTarget>();
                ft._standardOffset = true;
                enabled = false;
                break;
            case RideStates.WINNING:
                break;
        }
    }
}
