using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FadeHelper : MonoBehaviour
{
    [HideInInspector]
    public CanvasGroup _cg;
    public float _fadeDur;
    public bool _fadeInterupt=true; //implement this
    public bool _fadeOnAwake;
    public bool _pulseOnAwake;
    // Start is called before the first frame update
    void Start()
    {
        _cg = GetComponent<CanvasGroup>();
        if (_pulseOnAwake)
            StartPulse(false, _fadeDur);
    }

    public void FadeThenDestroy()
    {
        StartCoroutine(FadeAndDestroy());
    }

    public void FadeIn(bool isGui)
    {
        StopAllCoroutines();
        _fadeInterupt = false;
        if (isGui)
        {
            _cg.interactable = true;
            _cg.blocksRaycasts = true;
        }
        StartCoroutine(FadeInRoutine(_cg, _fadeDur));
    }

    public void FadeOut(bool isGui)
    {
        StopAllCoroutines();
        if (isGui)
        {
            _cg.interactable = false;
            _cg.blocksRaycasts = false;
        }
        StartCoroutine(FadeOut(_cg, _fadeDur));
    }

    public void StartPulse(bool isGui, float myFadeDur)
    {
        StopAllCoroutines();
        if (isGui)
        {
            _cg.interactable = false;
            _cg.blocksRaycasts = false;
        }
        StartCoroutine(Pulse(_cg, myFadeDur));
    }

    private IEnumerator FadeAndDestroy()
    {
        float timer = 0;
        while(timer < _fadeDur && !_fadeInterupt)
        {
            _cg.alpha = 1-timer / _fadeDur;
            timer += Time.deltaTime;
            yield return null;
        }
        Destroy(transform.gameObject);
    }

    public IEnumerator FadeInRoutine(CanvasGroup cg, float dur)
    {
        float timer = 0;
        while(timer < dur && !_fadeInterupt)
        {
            cg.alpha = timer / dur;
            timer += Time.deltaTime;
            yield return null;
        }
        cg.alpha = 1;
    }
    public IEnumerator FadeOut(CanvasGroup cg, float dur)
    {
        float timer = 0;
        while (timer < dur && !_fadeInterupt)
        {
            cg.alpha = 1-timer / dur;
            timer += Time.deltaTime;
            yield return null;
        }
        cg.alpha = 0;
    }

    public IEnumerator Pulse(CanvasGroup cg, float dur)
    {
        float timer = 0;
        while (_fadeInterupt)
        {
            cg.alpha = Mathf.PingPong(timer, dur);
            timer += Time.deltaTime;
            yield return null;
        }
    }
}
