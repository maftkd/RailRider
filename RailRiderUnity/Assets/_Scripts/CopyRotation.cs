using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyRotation : MonoBehaviour
{

    public Transform _target;
    public float _slerpSpeed;
    public float _rotateSpeed;
    private Quaternion _prevRot;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Slerp(_prevRot, _target.rotation, _slerpSpeed * Time.deltaTime);
        _prevRot = transform.rotation;
        transform.Rotate(Vector3.forward * Time.timeSinceLevelLoad * _rotateSpeed);
    }
}
