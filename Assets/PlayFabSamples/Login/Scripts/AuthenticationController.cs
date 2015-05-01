using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using PlayFab.Serialization.JsonFx;

/// <summary>
/// Authentication controller, The main source of logic for this demo. 
/// </summary>
public class AuthenticationController : MonoBehaviour {
	// PLAYFAB ID -- ENTER YOUR OWN TITLE ID HERE OR USE OUR'S FOR TESTING (ID=9D68)
	public string PlayFabTitleId = string.Empty;
	public string publisherId;
	
	// references to other scene components
	public BottomMenuController bottomMenu;
	public DebugInfoController debugInfo;
	public AutoLoginController autoLogin;
	public RegisterController registerLogin;
	public ManualController manualLogin;

	// login states for controlling the visible views
	public enum LoginStates { Manual = 0, Register = 1, Auto = 2, LoggedIn = 3 }
	public LoginStates activeState = LoginStates.Manual;
	private LoginStates previousFrameState;
	
	// delegate and event for updating views based on LoginStates enum
	public delegate void OnLoginStateChange(LoginStates state);
	public static event OnLoginStateChange LoginStateChange;
	
	void OnEnable()
	{
		PlayFabLoginCalls.OnLoginFail += HandleOnLoginFail;
		PlayFabLoginCalls.OnLoginSuccess += HandleOnLoginSuccess;
		PlayFabLoginCalls.OnPlayfabCallbackSuccess += HandleCallbackSuccess;
	}
	
	void OnDisable()
	{
		PlayFabLoginCalls.OnLoginFail -= HandleOnLoginFail;
		PlayFabLoginCalls.OnLoginSuccess -= HandleOnLoginSuccess;
		PlayFabLoginCalls.OnPlayfabCallbackSuccess -= HandleCallbackSuccess;
	}
	
	
	// Use this for initialization
	void Start () 
	{
		if(!string.IsNullOrEmpty(this.PlayFabTitleId) && string.IsNullOrEmpty(PlayFab.PlayFabSettings.TitleId))
		{
			PlayFab.PlayFabSettings.TitleId = this.PlayFabTitleId;
			this.activeState = LoginStates.Auto;
		}
		else if(string.IsNullOrEmpty(PlayFab.PlayFabSettings.TitleId))
		{
			Debug.Log ("PlayFab Title Id Required. Please enter your Id on the Authentication Controller");
		}
		
		if(!string.IsNullOrEmpty(this.publisherId))
		{
			PlayFabLoginCalls.publisher_id = this.publisherId;
		}
		else
		{
			Debug.Log ("PlayFab Publiser ID not provided, using global account space");
		}
		
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(this.activeState != this.previousFrameState)
		{
			ChangeState(this.activeState);
		}
	}

	public void ChangeState(LoginStates state)
	{
		Debug.Log(string.Format("Changing State to: {0}", state.ToString()));
		switch(state)
		{
			case LoginStates.Manual:
				manualLogin.EnableUI();
				autoLogin.DisableUI();
				registerLogin.DisableUI();
			break;
			
			case LoginStates.Register:
				manualLogin.DisableUI();
				autoLogin.DisableUI();
				registerLogin.EnableUI();
			break;
			
			case LoginStates.Auto:
				manualLogin.DisableUI();
				autoLogin.EnableUI();
				registerLogin.DisableUI();
			break;
			
			case LoginStates.LoggedIn:
				manualLogin.DisableUI();
				autoLogin.DisableUI();
				registerLogin.DisableUI();
				bottomMenu.DisableUI();
				this.GetComponent<Image>().enabled = false;
			break;
		}
		
		this.previousFrameState = this.activeState;
		
		if(LoginStateChange != null)
			LoginStateChange(state);
		
	}
	
	/// <summary>
	/// Callback function that we are directing our Android native plugin to return the token to. Currently this is a bit messy, but we needed to guarantee a gameObject would be active to catch the callback.  
	/// </summary>
	/// <param name="token">Token from the native plugin</param>
	public void OnGoogleTokenReceived(string token) {
		Debug.Log("Token Recieved from the native plugin");
		PlayFabLoginCalls.Token = token;
		Debug.Log(string.Format("g+ Token: {0}", token));
		PlayFabLoginCalls.RequestSpinner();
		PlayFabLoginCalls.SignOnWithGoogle(token);
	}
	
	
	void HandleCallbackSuccess(string message, PlayFabAPIMethods method)
	{
		if(method == PlayFabAPIMethods.GetAccountInfo)
		{
			SaveUserAccountInfo();
			SaveLoginPathway();
		}
	}
	
	void HandleOnLoginSuccess(string message)
	{
		SaveLoginPathway();
		PlayFabLoginCalls.RequestSpinner();
		PlayFabLoginCalls.GetAccountInfo();	
		this.activeState = LoginStates.LoggedIn;
	}
	
	void HandleOnLoginFail(string message)
	{
		if(message == "Logout")
		{
			this.GetComponent<Image>().enabled = true;
			bottomMenu.EnableUI();
			SaveUserAccountInfo();
			SaveLoginPathway();
		}
		this.activeState = LoginStates.Manual;
	}
	
	void SaveUserAccountInfo()
	{
		Debug.Log ("Saving Account Info...");
		string serialized = JsonWriter.Serialize(PlayFabLoginCalls.LoggedInUserInfo);
		PlayerPrefs.SetString("accountInfo", serialized);
	}
	
	void SaveLoginPathway()
	{
		Debug.Log ("Saving Login Pathway Info...");
		PlayerPrefs.SetString("loginMethodUsed", PlayFabLoginCalls.LoginMethodUsed.ToString());
	}
	
}
