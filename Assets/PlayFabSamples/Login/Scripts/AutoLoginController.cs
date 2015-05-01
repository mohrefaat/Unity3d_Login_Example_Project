using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using PlayFab.Serialization.JsonFx;
using PlayFab.ClientModels;

/// <summary>
/// Auto login controller this controls the logic when the login state is set for logging in automatically. This pathway is always tried first.
/// </summary>
public class AutoLoginController : MonoBehaviour {
	/* UI Components */ 
	public Text status;
	public Text details;
	public Text counter;
	public Text instructions;
	public InputField password;
	public Button login;
	public Button rock_it;
	public Button createNew;
	public Transform passwordPrompt;
	public Transform countdown;
	public Transform newAccountPrompt;
	public AuthenticationController authController;
	/* ---- */ 
	
	// autologin timer variables
	public float autoLoginAfter = 6.0f; // in seconds
	private float timeToStart = 0;
	private float timeToFinish = 0;
	private bool isCounting;
	private bool createNewAccount = false;
	private UserAccountInfo accountInfo;
	private PlayFabLoginCalls.LoginPathways loginPathToUse;	
	
	void OnEnable()
	{
		PlayFabLoginCalls.OnPlayFabError += HandlePlayFabError;
		timeToStart = 0;
		timeToFinish = 0;
		autoLoginAfter = 6.0f; // in seconds
		StartCoroutine(ReadLoginDataRecord());
	}
	
	void OnDisable()
	{
		PlayFabLoginCalls.OnPlayFabError -= HandlePlayFabError;
		this.isCounting = false;
	}
	
	void HandlePlayFabError(string message, PlayFabAPIMethods method)
	{
		if(message.Contains("createAccount = true") && method == PlayFabAPIMethods.LoginWithDeviceId)
		{
			Debug.Log("Prompt to continue with Create new account.");
			this.passwordPrompt.gameObject.SetActive(false);
			this.countdown.gameObject.SetActive(false);
			this.newAccountPrompt.gameObject.SetActive(true);
			this.isCounting = false;	
		}
	}
	
	/// <summary>
	/// Parses Unity PlayerPrefs for saved login information. See the gitHub readme for more information
	/// </summary>
	/// <returns> used for coroutine yielding </returns>
	IEnumerator ReadLoginDataRecord()
	{
		this.status.text = "Finding previous logins...";

		if(PlayerPrefs.HasKey("loginMethodUsed"))
		{
			this.status.text = "Previous login found... ";
			string raw = PlayerPrefs.GetString("loginMethodUsed");
	
			PlayFabLoginCalls.LoginPathways method = (PlayFabLoginCalls.LoginPathways) Enum.Parse(typeof(PlayFabLoginCalls.LoginPathways), raw);
			Debug.Log(method.ToString());
			
			switch(method)
			{
				case PlayFabLoginCalls.LoginPathways.pf_username:
					if(PlayerPrefs.HasKey("accountInfo"))
					{
						this.accountInfo = JsonReader.Deserialize<UserAccountInfo>(PlayerPrefs.GetString("accountInfo"));
						this.createNewAccount = false;
						PrompForPassword();
						this.details.text = string.Format("PlayFab Username: {0} found...", this.accountInfo.Username);
						this.instructions.text = "Enter password to continue, or change accounts manually by clicking below.";
						this.loginPathToUse = PlayFabLoginCalls.LoginPathways.pf_username;
					}
					break;
					
				case PlayFabLoginCalls.LoginPathways.pf_email:
					if(PlayerPrefs.HasKey("accountInfo"))
					{
						this.accountInfo = JsonReader.Deserialize<UserAccountInfo>(PlayerPrefs.GetString("accountInfo"));
						this.createNewAccount = false;
						PrompForPassword();
						this.details.text = string.Format("Email: {0} found...", this.accountInfo.PrivateInfo.Email);
						this.instructions.text = "Enter password to continue, or change accounts manually by clicking below.";
						this.loginPathToUse = PlayFabLoginCalls.LoginPathways.pf_email;
					}
					break;
					
				case PlayFabLoginCalls.LoginPathways.deviceId:
						EnableCountdown();
						this.createNewAccount = false;
						this.details.text = "Device Id, click below to manually change accounts";
						this.loginPathToUse = PlayFabLoginCalls.LoginPathways.deviceId;
					break;
					
				case PlayFabLoginCalls.LoginPathways.facebook:
						EnableCountdown();
						this.createNewAccount = false;
						this.details.text = "Facebook, click below to manually change accounts";
						this.loginPathToUse = PlayFabLoginCalls.LoginPathways.facebook;
					break;
					
				case PlayFabLoginCalls.LoginPathways.gameCenter:
						EnableCountdown();
						this.createNewAccount = false;
						this.details.text = "GameCenter, click below to manually change accounts";
						this.loginPathToUse = PlayFabLoginCalls.LoginPathways.gameCenter;
					break;
					
				case PlayFabLoginCalls.LoginPathways.googlePlus:
						EnableCountdown();
						this.createNewAccount = false;
						this.details.text = "Google+, click below to manually change accounts";
						this.loginPathToUse = PlayFabLoginCalls.LoginPathways.googlePlus;
					break;
					
				case PlayFabLoginCalls.LoginPathways.steam:
						EnableCountdown();
						this.createNewAccount = false;
						this.details.text = "Steam, click below to manually change accounts";
						this.loginPathToUse = PlayFabLoginCalls.LoginPathways.steam;
					break;
				default:
					AutoNewAccount();
				break;
			}
		}
		else
		{
			if(PlayFabLoginCalls.CheckForSupportedMobilePlatform())
			{
				AutoNewAccount();
			}
			else
			{
				PlayFabLoginCalls.TestDeviceIdHasAccount();
				yield return new WaitForSeconds(.333f);
				authController.activeState = AuthenticationController.LoginStates.Manual;
			}	
		}
	}

	// Use this for initialization
	void Start () 
	{
		login.onClick.AddListener(() => Login());
		rock_it.onClick.AddListener(() => Login());	
		createNew.onClick.AddListener(() => CreateNewAndLogin());
	}
	
	void CreateNewAndLogin()
	{
		this.newAccountPrompt.gameObject.SetActive(false);
		PlayFabLoginCalls.RequestSpinner();
		PlayFabLoginCalls.LoginWithDeviceId(this.createNewAccount);
	}

	// Update is called once per frame
	void Update () 
	{
		if(this.isCounting)
		{
			if(this.timeToStart == 0)
			{
				this.timeToStart = Time.time;
				this.timeToFinish = this.timeToStart + autoLoginAfter;
			}
			
			if(Time.time < this.timeToFinish)
			{
				string label = (this.timeToFinish - Time.time) >1  ? (this.timeToFinish - Time.time).ToString("F") : "Blast Off!";
				this.counter.text = label;
			}
			else
			{
				this.isCounting = false;
				Login();
			}
		}
	}
	
	void PrompForPassword()
	{
		this.isCounting = false;
		this.countdown.gameObject.SetActive(false);
		this.passwordPrompt.gameObject.SetActive(true);
	}
	
	void EnableCountdown()
	{
		this.passwordPrompt.gameObject.SetActive(false);
		this.countdown.gameObject.SetActive(true);
		this.isCounting = true;
	}
	
	void AutoNewAccount()
	{
		this.status.text = "No previous login found...";
		this.details.text = "Using device id (if available)...\nclick below to manually change accounts";
		this.createNewAccount = false;
		this.autoLoginAfter *= 1.5f;
		EnableCountdown();
		this.loginPathToUse = PlayFabLoginCalls.LoginPathways.deviceId;
	}
	
	void Login()
	{	
		Debug.Log("Login Path: " + this.loginPathToUse);
		switch(this.loginPathToUse)
		{
			case PlayFabLoginCalls.LoginPathways.pf_username:
				PlayFabLoginCalls.RequestSpinner();
				PlayFabLoginCalls.LoginWithUsername(this.accountInfo.Username, this.password.text);
				break;
			case PlayFabLoginCalls.LoginPathways.pf_email:
				PlayFabLoginCalls.RequestSpinner();
				PlayFabLoginCalls.LoginWithEmail(this.accountInfo.PrivateInfo.Email, this.password.text);
				break;
			case PlayFabLoginCalls.LoginPathways.deviceId:
				PlayFabLoginCalls.RequestSpinner();
				PlayFabLoginCalls.TestDeviceIdHasAccount();
				break;
			case PlayFabLoginCalls.LoginPathways.facebook:
				this.isCounting = false;
				PlayFabLoginCalls.RequestSpinner();	
				PlayFabLoginCalls.StartFacebookLogin();
				break;
			case PlayFabLoginCalls.LoginPathways.gameCenter:
				this.isCounting = false;
				PlayFabLoginCalls.RequestSpinner();
				PlayFabLoginCalls.StartGameCenterLogin();
				break;
			case PlayFabLoginCalls.LoginPathways.googlePlus:
				this.isCounting = false;
				PlayFabLoginCalls.RequestSpinner();
				PlayFabLoginCalls.StartGooglePlusLogin();
				break;
			case PlayFabLoginCalls.LoginPathways.steam:
				Debug.LogWarning("Steam Token Authentication not yet implemented."); 
				break;
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
