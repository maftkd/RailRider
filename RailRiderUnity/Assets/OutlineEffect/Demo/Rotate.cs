using UnityEngine;
using System.Collections;
using cakeslice;

namespace cakeslice
{
    public class Rotate : MonoBehaviour
    {
        public float _rotSpeed;
	public bool _hardSet=false;

        // Use this for initialization
        void Start()
        {
		if(!_hardSet){
			_rotSpeed = Random.Range(_rotSpeed*.5f,_rotSpeed);
			if(Random.value<0.5f)
				_rotSpeed*=-1f;
		}
        }

        // Update is called once per frame
        void Update()
        {
            transform.Rotate(Vector3.forward, Time.deltaTime * _rotSpeed);
        }
    }
}
