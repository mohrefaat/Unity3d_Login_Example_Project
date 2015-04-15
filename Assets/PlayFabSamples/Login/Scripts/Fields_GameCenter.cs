using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// View controller for the GameCenter fields
/// </summary>
public class Fields_GameCenter : MonoBehaviour {
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
		PlayFabLoginCalls.RequestSpinner();
		PlayFabLoginCalls.StartGameCenterLogin();
	}
	
}
