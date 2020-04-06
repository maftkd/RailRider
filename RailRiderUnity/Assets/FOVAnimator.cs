using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FOVAnimator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartFOVAnim(float targetFov, float duration)
    {
        StartCoroutine(AnimateFOV(targetFov, duration));
    }

    private IEnumerator AnimateFOV(float targetFov, float duration)
    {
        float timer = 0;
        Camera mainCam = Camera.main;
        float startFov = mainCam.fieldOfView;
        while (timer < duration)
        {
            Debug.Log("Animating fov");
            mainCam.fieldOfView = Mathf.Lerp(startFov, targetFov, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
    }
}
