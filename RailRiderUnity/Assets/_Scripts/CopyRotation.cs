using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyRotation : MonoBehaviour
{

    public Transform _target;
    public float _slerpSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, _target.rotation, _slerpSpeed * Time.deltaTime);
    }
}
