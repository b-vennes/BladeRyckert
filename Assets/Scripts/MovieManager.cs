using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

[Serializable]
public class MovieManager : MonoBehaviour {

	public VideoPlayer movie;

	public string nextSceneName;

	// Use this for initialization
	void Start () {

        movie = GameObject.Find("Movie" +"").GetComponent<VideoPlayer>();

		StartCoroutine(PlayMovie());
	}

    private void Update()
    {

    }

    IEnumerator PlayMovie()
    {
		// start movie
		movie.Play();

		// wait for movie to complete
		yield return new WaitForSeconds((float)movie.clip.length);
		
		// stop movie and go onto level 2
		movie.Stop();
        SceneManager.LoadScene(nextSceneName);
    }
}
