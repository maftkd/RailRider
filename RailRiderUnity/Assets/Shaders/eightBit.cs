using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class eightBit : MonoBehaviour
{
    public Material _eightBitMat;
    //public RenderTexture _rt;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //Graphics.Blit
        Graphics.Blit(source, destination, _eightBitMat);
    }
}
