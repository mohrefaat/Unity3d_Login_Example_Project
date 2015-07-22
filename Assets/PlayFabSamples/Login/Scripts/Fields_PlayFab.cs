using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// View controller for the login with PlayFab account fields  
/// </summary>
public class Fields_PlayFab : MonoBehaviour {
	public InputField User;
	public InputField Password;
	public Button Login;
	
	
	void OnEnable()
	{
		if(PlayFabLoginCalls.LoggedInUserInfo != null)
		{
			this.User.text = PlayFabLoginCalls.LoggedInUserInfo.Username;
		}
		this.Password.text = string.Empty;
	}
	
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
		if(User.text.Contains("@"))
		{
			PlayFabLoginCalls.LoginWithEmail(User.text, Password.text);
		}
		else
		{
			PlayFabLoginCalls.LoginWithUsername(User.text, Password.text);
		}
	}
}
