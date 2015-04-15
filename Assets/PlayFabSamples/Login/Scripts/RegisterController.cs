using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// View controller for the registration fields
/// </summary>
public class RegisterController : MonoBehaviour {
	public InputField user;
	public InputField email;
	public InputField pass1;
	public InputField pass2;
	public Button register;
	
	void OnEnable()
	{
		PlayFabLoginCalls.OnPlayfabCallbackSuccess += HandleCallbackSuccess;
	}
	
	void OnDisable()
	{
		PlayFabLoginCalls.OnPlayfabCallbackSuccess -= HandleCallbackSuccess;
	}
	
	
	// Use this for initialization
	void Start () {
		register.onClick.AddListener(() => RegisterNewAccount());
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void RegisterNewAccount()
	{
		PlayFabLoginCalls.RequestSpinner();
		PlayFabLoginCalls.RegisterNewPlayfabAccount(user.text, pass1.text, pass2.text, email.text);
	}
	
	void HandleCallbackSuccess(string message, PlayFabAPIMethods method)
	{
		if(method == PlayFabAPIMethods.RegisterPlayFabUser)
		{
			Debug.Log("Account Created, logging in with new account.");
			PlayFabLoginCalls.RequestSpinner();
			PlayFabLoginCalls.LoginWithUsername(user.text, pass1.text);
		}
	}
	
	public void EnableUI()
	{
		this.gameObject.SetActive(true);
	}
	
	public void DisableUI()
	{
		this.gameObject.SetActive(false);
	}
}
