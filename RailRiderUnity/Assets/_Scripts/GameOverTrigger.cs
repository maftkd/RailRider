using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameOverTrigger : MonoBehaviour
{
    public FadeHelper _outro;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        _outro.FadeIn(true);
        GameObject.Find("EventSystem").GetComponent<EventSystem>().SetSelectedGameObject(_outro.transform.Find("Button").gameObject);
    }
}
