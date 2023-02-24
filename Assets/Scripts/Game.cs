using System.Collections;
using System.Collections.Generic;
using Unity.Scenes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class Game : MonoBehaviour
{
    public static Game instance = null;
    public int score = 0;
    public int highScore = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void Start()
    {
        
    }

    public void AddToScore(int value)
    {
        score += value;
        if (score > highScore)
        {
            highScore = score;
        }
    }

    public void ResetScore()
    {
        score = 0;
    }

    public void OnGUI()
    {
        GUILayout.Label("Highscore : " + highScore);
        GUILayout.Label("score : " + score);
    }
}
