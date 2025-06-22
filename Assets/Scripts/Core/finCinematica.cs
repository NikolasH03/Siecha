using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class finCinematica : MonoBehaviour
{

    [SerializeField] VideoPlayer videoPlayer;  
    [SerializeField] string sceneName;       

    void Start()
    {

        videoPlayer.isLooping = false;


        videoPlayer.loopPointReached += OnVideoEnd;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ChangeScene();
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        ChangeScene();
    }
    void ChangeScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
