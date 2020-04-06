using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreKeeper : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("hs"))
            GetComponent<Text>().text = "High Score:\n" + string.Format("{0:n0}", PlayerPrefs.GetInt("hs")*100);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SaveHighScore(int score)
    {
        if (PlayerPrefs.HasKey("hs"))
        {
            if (PlayerPrefs.GetInt("hs") >= score)
                return;
            PlayerPrefs.DeleteKey("hs");
        }

        PlayerPrefs.SetInt("hs", score);
    }
}
