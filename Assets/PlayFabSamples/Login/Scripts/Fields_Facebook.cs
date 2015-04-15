using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// View controller for the facebook login fields
/// </summary>
public class Fields_Facebook : MonoBehaviour {
	public Button Login;
	
	// Use this for initialization
	void Start () {
		Login.onClick.AddListener(() => LogIn());
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public void LogIn()
	{
		PlayFabLoginCalls.StartFacebookLogin();
	}
}
