using UnityEngine;
using System.Collections;
using cakeslice;

namespace cakeslice
{
    public class Rotate : MonoBehaviour
    {
        public float _rotSpeed;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            transform.Rotate(Vector3.forward, Time.deltaTime * _rotSpeed);
        }
    }
}