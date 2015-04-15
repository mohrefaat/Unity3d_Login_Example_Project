using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// View controller for the google plus fields.
/// </summary>
public class Fields_GooglePlus : MonoBehaviour {
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
		#if !UNITY_EDITOR && UNITY_ANDROID
		PlayFabLoginCalls.StartGooglePlusLogin();
		#endif
	}
}
