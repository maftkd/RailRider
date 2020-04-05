using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public Transform _target;
    public Vector3 _posOffset;
    public float _moveLerpSpeed;
    public float _turnLerpSpeed;
    public float _yDiff;
    public bool _dontLerp;
    public bool _getPosOffsetAtStart;
    public bool _standardOffset;
    // Start is called before the first frame update
    void Start()
    {
        if (_getPosOffsetAtStart)
            _posOffset = transform.position - _target.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_standardOffset)
        {
            Vector3 targetPos = _target.position + _target.forward * _posOffset.z + Vector3.up * _yDiff;
            transform.position = Vector3.Lerp(transform.position, targetPos, _moveLerpSpeed * Time.deltaTime);
            Vector3 oldEuler = transform.eulerAngles;
            transform.rotation = Quaternion.Slerp(transform.rotation, _target.rotation, _turnLerpSpeed * Time.deltaTime);
            //transform.eulerAngles = new Vector3(oldEuler.x, transform.eulerAngles.y, oldEuler.z);

        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, _target.position+_posOffset, _moveLerpSpeed * Time.deltaTime);
            transform.LookAt(_target);
        }
        
    }
}
