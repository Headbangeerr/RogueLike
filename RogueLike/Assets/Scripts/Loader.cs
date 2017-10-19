using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour {

    public GameManager gameManager;

    private void Awake()
    {
        if(GameManager.Instance==null)
        {
            GameObject.Instantiate(gameManager);
        }
    }
    // Update is called once per frame
    void Update () {
		
	}
}
