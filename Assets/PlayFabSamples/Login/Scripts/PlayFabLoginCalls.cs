using UnityEngine;
using UnityEngine.SocialPlatforms.GameCenter;
using System.Collections;
using System.Text.RegularExpressions;

using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Internal;
using PlayFab.Serialization.JsonFx;

/// <summary>
///  Contains static functions for all the PlayFab API and helper calls. If you are looking for PlayFab calls, they should all be in this class.
/// </summary>
public class PlayFabLoginCalls  {
	
#region vars	
	// convenient location for real-time login info for the session
	public enum LoginPathways {deviceId = 1, pf_username = 2, facebook = 3, gameCenter = 4, pf_email = 5, steam = 6, googlePlus = 7  } 
	public static LoginPathways LoginMethodUsed;
	public static UserAccountInfo LoggedInUserInfo;
	public bool hasTitleId = false;
	public static string android_id = string.Empty; // device ID to use with PlayFab login
	public static string ios_id = string.Empty; // device ID to use with PlayFab login
	
	// used to store our google token
	private static  string token;
	public static string Token {
		get {
			return token;
		}
		set {
			if (token != null && token.Equals(value)) {
				return;
			}
			token = value;
		}
	}
	
	/* Communication is diseminated across these 4 events */
	//called after a successful login 
	public delegate void SuccessfulLoginHandler(string path);
	public static event SuccessfulLoginHandler OnLoginSuccess;
	
	//called after a login error or when logging out
	public delegate void FailedLoginHandler(string message);
	public static event FailedLoginHandler OnLoginFail;
	
	//called when catching and rasing errors, the enum can be useful for tracking what API call threw the error. 
	public delegate void PlayFabErrorHandler(string message, PlayFabAPIMethods method);
	public static event PlayFabErrorHandler OnPlayFabError;
	
	// called after a successful API callback (useful for stopping the spinner)
	public delegate void CallbackSuccess(string message, PlayFabAPIMethods method);
	public static event CallbackSuccess OnPlayfabCallbackSuccess;
	/* ---------- */
	
	//convenient location to raise start spinner events
	public delegate void StartSpinner();
	public static event StartSpinner StartSpinnerRequest;
	
	// regex pattern for validating email syntax
	private const string emailPattern = @"^([0-9a-zA-Z]([\+\-_\.][0-9a-zA-Z]+)*)+@(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]*\.)+[a-zA-Z0-9]{2,17})$";
#endregion

#region Plugin Access & Helper Functions
	// Kicks off the Facebook login process
	public static void StartFacebookLogin()
	{
		if(FB.IsInitialized == true)
		{
			OnInitComplete();
		}
		else
		{
			FB.Init(OnInitComplete, OnHideUnity);
		}
	}
	
	// Kicks off the GameCenter login process
	public static void StartGameCenterLogin()
	{
		Debug.Log(Social.Active.ToString());
		Social.localUser.Authenticate(success => {
			if (success) {
				Debug.Log ("Authentication successful");
				LoginWithGamecenter(Social.localUser.id);
			}
			else
				Debug.Log ("Authentication failed");
		});
	}
	
	/// <summary>
	/// Google plus wrapper class was contributed by Hamza Lazâar Twitter: @RealJohnTube
	/// For the purposes of PlayFab authentication, we are only using GetToken(); however, there are many more integrated Google+ features available. 
	/// This class calls a native java plugin which then returns via callback [OnTokenReceivedCallback] to a scene.
	/// </summary>
	/// <param name="callBackObject">This is the name of the gameObject to call back to with the token. (Plugin default = "GooglePlusGO")</param>
	/// <param name="callBackMethod">This is the name of the method to call. (Plugin default = "OnGoogleTokenReceived")</param>
	public static void StartGooglePlusLogin(string callBackObject="Panel_Authentication", string callBackMethod="OnGoogleTokenReceived")
	{
		#if UNITY_ANDROID
		// internal settings for using GooglePlusActivity
		//int SIGN_IN_REASON = 1;
		int GET_TOKEN_REASON = 2;
		//int SIGN_OUT_REASON = 3;
		//int LOAD_CIRCLES_REASON = 4;
		//int REVOKE_ACCESS_REASON = 5;
		//int INVALIDATE_TOKEN_REASON = 6;
		
		string className = "com.ThugLeaf.GooglePlusForUnity.GooglePlusActivity"; //com.ThugLeaf.GooglePlusForUnity.DemoActivity
		AndroidJNI.AttachCurrentThread();
		using( AndroidJavaClass activityClass = new AndroidJavaClass(className))
		{
			//using(AndroidJavaObject activityInstance = activityClass.GetStatic<AndroidJavaObject>("mContext"))
			//{
				activityClass.SetStatic<string>("UnityObjectName", callBackObject); //UnityObjectName
				activityClass.SetStatic<string>("OnTokenReceivedCallback", callBackMethod); //TokenCallbackName
				//activityInstance.Call("GetToken");
				activityClass.CallStatic("Start", new object[] { GET_TOKEN_REASON });
			//}
		}
		#endif
	}
	
	
	/// <summary>
	/// Validates the email.
	/// </summary>
	/// <returns><c>true</c>, if email was validated, <c>false</c> otherwise.</returns>
	/// <param name="em">Email address</param>
	public static bool ValidateEmail(string em){
		return Regex.IsMatch(em, PlayFabLoginCalls.emailPattern);
	}
	
	/// <summary>
	/// Validates the password.
	/// </summary>
	/// <returns><c>true</c>, if password was validated, <c>false</c> otherwise.</returns>
	/// <param name="p1">P1, text from password field one</param>
	/// <param name="p2">P2, text from password field two</param>
	public static bool ValidatePassword(string p1, string p2){
		return ((p1 == p2) && p1.Length > 5); 
	}
	
	/// <summary>
	/// Raises an even for requesting a spinner animation
	/// </summary>
	public static void RequestSpinner()
	{
		if(StartSpinnerRequest != null)
			StartSpinnerRequest();
	}
	
	/// <summary>
	/// Check to see if our current platform is supported (iOS & Android)
	/// </summary>
	/// <returns><c>true</c>, for supported mobile platform, <c>false</c> otherwise.</returns>
	public static bool CheckForSupportedMobilePlatform()
	{
		if(Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer)
		{
			return false;
		}
		return true;
	}
	
	/// <summary>
	/// Determines if the current platform can use a specific service path.
	/// </summary>
	/// <returns><c>true</c> if service can be used on the current platform; otherwise, <c>false</c>.</returns>
	/// <param name="path">Path.</param>
	public static bool CanUseService(PlayFabLoginCalls.LoginPathways path)
	{
		switch(path)
		{
			case LoginPathways.deviceId:
				return CheckForSupportedMobilePlatform();

			case LoginPathways.facebook:
				return CheckForSupportedMobilePlatform();

			case LoginPathways.steam:
				if(Application.platform == RuntimePlatform.LinuxPlayer || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.WindowsPlayer)
				{
					// need additional checks here once the steamworks API is integrated
					return true;
				}
			break;
			
			case LoginPathways.gameCenter:
				if(Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.IPhonePlayer)
				{
					return true;
				}
			break;
			
			case LoginPathways.pf_email:
				return true;
			
			case LoginPathways.pf_username:
				return true;
		}
		return false;
	}
	
	/// <summary>
	/// Gets the device identifier and updates the static variables
	/// </summary>
	/// <returns><c>true</c>, if device identifier was obtained, <c>false</c> otherwise.</returns>
	static bool GetDeviceId()
	{
		if(PlayFabLoginCalls.CheckForSupportedMobilePlatform())
		{
			#if UNITY_ANDROID
			//http://answers.unity3d.com/questions/430630/how-can-i-get-android-id-.html
			AndroidJavaClass clsUnity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject objActivity = clsUnity.GetStatic<AndroidJavaObject>("currentActivity");
			AndroidJavaObject objResolver = objActivity.Call<AndroidJavaObject>("getContentResolver");
			AndroidJavaClass clsSecure = new AndroidJavaClass("android.provider.Settings$Secure");
			android_id = clsSecure.CallStatic<string>("getString", objResolver, "android_id");
			#endif
			
			#if UNITY_IPHONE
			ios_id = iPhone.vendorIdentifier;
			#endif
			return true;
		}
		else
		{
			if(OnPlayFabError != null)
			{
				OnPlayFabError("Must be using android or ios platforms to use deveice id.", PlayFabAPIMethods.Generic);
			}
			return false;
		}
	}
	
	#endregion
	
	#region PlayFab API calls
	/// <summary>
		/// Login with Facebook token.
		/// </summary>
		/// <param name="token">Token obtained through the FB plugin. (works on mobile and FB canvas only)</param>
		private static void LoginWithFacebook(string token)
		{
			LoginMethodUsed = LoginPathways.facebook;
			LoginWithFacebookRequest request = new LoginWithFacebookRequest();
			request.AccessToken = token;
			request.TitleId = PlayFabSettings.TitleId;
			request.CreateAccount = false;
			
			PlayFabClientAPI.LoginWithFacebook(request, OnLoginResult, OnLoginError);
		}
		
		/// <summary>
		/// Login with GamCenter token(temporarally disabled, will be reenabled on the next release)
		/// </summary>
		/// <param name="token">Token obtained through Unity's Social API</param>
		private static void LoginWithGamecenter(string token)
		{
//			LoginMethodUsed = LoginPathways.gameCenter;
//			LoginWithGameCenterRequest request = new LoginWithGameCenterRequest();
//			request.PlayerId = token;
//			request.TitleId = PlayFabSettings.TitleId;
//			request.CreateAccount = false;

//			PlayFabClientAPI.LoginWithGameCenter(request, OnLoginResult, OnLoginError);
		}
		
		/// <summary>
		/// Registers the new PlayFab account.
		/// </summary>
		public static void RegisterNewPlayfabAccount(string user, string pass1, string pass2, string email )
		{
			if(user.Length == 0 || pass1.Length == 0 || pass2.Length ==0 || email.Length == 0)
			{
				if(OnPlayFabError != null)
				{
					OnPlayFabError("All fields are required.", PlayFabAPIMethods.RegisterPlayFabUser);
				}
				return;
			}
			
			bool passwordCheck = ValidatePassword(pass1, pass2);
			bool emailCheck = ValidateEmail(email);
			
			if(!passwordCheck)
			{
				if(OnPlayFabError != null)
				{
					OnPlayFabError("Passwords must match and be longer than 5 characters.", PlayFabAPIMethods.RegisterPlayFabUser);
				}
				return;
				
			}
			else if(!emailCheck)
			{
				if(OnPlayFabError != null)
				{
					OnPlayFabError("Invalid Email format.", PlayFabAPIMethods.RegisterPlayFabUser);
				}
				return;
			}
			else
			{
				RegisterPlayFabUserRequest request = new RegisterPlayFabUserRequest();
				request.TitleId = PlayFabSettings.TitleId;
				request.Username = user;
				request.Email = email;
				request.Password = pass1;
				PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterResult, OnPlayFabCallbackError);
			}
		}
	
		/// <summary>
		/// Login with PlayFab username.
		/// </summary>
		/// <param name="user">Username to use</param>
		/// <param name="pass">Password to use</param>
		public static void LoginWithUsername(string user, string password)
		{			
			if(user.Length>0 && password.Length>0)
			{
				LoginMethodUsed = LoginPathways.pf_username;
				LoginWithPlayFabRequest request = new LoginWithPlayFabRequest();
				request.Username = user;
				request.Password = password;
				request.TitleId = PlayFabSettings.TitleId;
				PlayFabClientAPI.LoginWithPlayFab(request, OnLoginResult, OnLoginError);
			}
			else
			{
				if(OnPlayFabError != null)
				{
					OnLoginFail("User Name and Password cannot be blank.");
				}
			}
		}
		
		/// <summary>
		/// Login using the email associated with a PlayFab account.
		/// </summary>
		/// <param name="user">User.</param>
		/// <param name="password">Password.</param>
		public static void LoginWithEmail(string user, string password)
		{		
			if(user.Length>0 && password.Length>0 && ValidateEmail(user))
			{
				LoginMethodUsed = LoginPathways.pf_email;
				LoginWithEmailAddressRequest request = new LoginWithEmailAddressRequest();
				request.Email = user;
				request.Password = password;
				request.TitleId = PlayFabSettings.TitleId;
				
				PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginResult, OnLoginError);
				
			}
			else
			{
				if(OnPlayFabError != null)
				{
					OnLoginFail("Username or Password is invalid. Check credentails and try again");
				}
			}
			
		}
			
		/// <summary>
		/// Logins the with device identifier (iOS & Android only).
		/// </summary>
		public static void LoginWithDeviceId( bool createAcount)
		{
			if( GetDeviceId())
			{
				LoginMethodUsed = LoginPathways.deviceId;
				if(!string.IsNullOrEmpty(android_id))
				{
					Debug.Log("Using Android Device ID: " + android_id);
					LoginWithAndroidDeviceIDRequest request = new LoginWithAndroidDeviceIDRequest();
					request.AndroidDeviceId = android_id;
					request.TitleId = PlayFabSettings.TitleId;
					request.CreateAccount = createAcount;

					PlayFabClientAPI.LoginWithAndroidDeviceID(request, OnLoginResult, OnLoginError);
				}
				else if (!string.IsNullOrEmpty(ios_id))
				{
					Debug.Log("Using IOS Device ID: " + ios_id);
					LoginWithIOSDeviceIDRequest request = new LoginWithIOSDeviceIDRequest();
					request.DeviceId = ios_id;
					request.TitleId = PlayFabSettings.TitleId;
					request.CreateAccount = createAcount;
	
					PlayFabClientAPI.LoginWithIOSDeviceID(request, OnLoginResult, OnLoginError);
				}
			}
		}
		
		/// <summary>
		/// Sign on using a google token.
		/// </summary>
		/// <param name="token">Token obtained with a native Google-Play plugin</param>
		public static void SignOnWithGoogle(string token)
		{
			LoginMethodUsed = LoginPathways.googlePlus;
			LoginWithGoogleAccountRequest request = new LoginWithGoogleAccountRequest();
			request.AccessToken = token;
			request.CreateAccount = true;
			
			PlayFabClientAPI.LoginWithGoogleAccount(request, OnLoginResult, OnLoginError);
			
		}
		
		/// <summary>
		/// Calls the UpdateUserTitleDisplayName request API
		/// </summary>
		public static void UpdateDisplayName(string displayName)
		{
			if(displayName == PlayFabLoginCalls.LoggedInUserInfo.TitleInfo.DisplayName)
			{
				Debug.Log ("Name remains the same, no updates needed.");
			}
			else if(displayName.Length > 2 && displayName.Length < 21)
			{ 
				RequestSpinner();
				UpdateUserTitleDisplayNameRequest request = new UpdateUserTitleDisplayNameRequest();
				request.DisplayName = displayName;
				
				PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnUpdateDisplayNameSuccess, OnPlayFabCallbackError);
			}
			else
			{
				if(OnPlayFabError != null)
				{
					OnPlayFabError("Display name must be between 3 and 20 characters", PlayFabAPIMethods.UpdateDisplayName);
				}
			}
		}
		
		/// <summary>
		/// Adds the user name and password to a partial (guest) account
		/// </summary>
		/// <param name="user">User - username to use (must be unique)</param>
		/// <param name="pass">Pass - password to use for the account, (must be > 5 characters)</param>
		/// <param name="email">Email - email to use (must be unique)</param>
		public static void AddUserNameAndPassword(string user, string pass, string email)
		{
			PlayFabLoginCalls.RequestSpinner();
			AddUsernamePasswordRequest request = new AddUsernamePasswordRequest();
			request.Email = email;
			request.Password = pass;
			request.Username = user;
			
			PlayFabClientAPI.AddUsernamePassword(request, OnAddUserNameAndPasswordSuccess, OnPlayFabCallbackError);
		}
		
		/// <summary>
		/// Gets the account info for the logged in player
		/// </summary>
		public static void GetAccountInfo()
		{
			GetAccountInfoRequest request = new GetAccountInfoRequest();
			PlayFabClientAPI.GetAccountInfo(request, OnGetAccountInfoSuccess, OnPlayFabCallbackError);
		}
		
		/// <summary>
		/// Triggers the backend to send an account recovery email to the account provided
		/// </summary>
		/// <param name="email">Email to match</param>
		public static void SendAccountRecoveryEmail(string email)
		{
			PlayFabLoginCalls.RequestSpinner();
			SendAccountRecoveryEmailRequest request = new SendAccountRecoveryEmailRequest();
			request.Email = email;
			request.TitleId = PlayFabSettings.TitleId;
			
			PlayFabClientAPI.SendAccountRecoveryEmail(request, OnSendAccountRecoveryEmailSuccess, OnPlayFabCallbackError);
		}
		
		/// <summary>
		/// Links a mobile device to a PlayFab account via the unique device id (A device can only be linked to one account at a time)
		/// </summary>
		public static void LinkDeviceId()
		{
			if( GetDeviceId())
			{
				PlayFabLoginCalls.RequestSpinner();
				if(!string.IsNullOrEmpty(android_id))
				{
					Debug.Log("Linking Android");
					LinkAndroidDeviceIDRequest request = new LinkAndroidDeviceIDRequest();
					request.AndroidDeviceId = android_id;
					
					PlayFabClientAPI.LinkAndroidDeviceID(request, OnLinkAndroidDeviceIdSuccess, OnPlayFabCallbackError);
				}
				else if (!string.IsNullOrEmpty(ios_id))
				{
					Debug.Log("Linking iOS");
					LinkIOSDeviceIDRequest request = new LinkIOSDeviceIDRequest();
					request.DeviceId = ios_id;
					
					PlayFabClientAPI.LinkIOSDeviceID(request, OnLinkIosDeviceIdSuccess, OnPlayFabCallbackError);
				}
			}
		}
		
		/// <summary>
		/// Unlinks a mobile device from a PlayFab account
		/// </summary>
		public static void UnlinkDeviceId()
		{
			if( GetDeviceId())
			{
				PlayFabLoginCalls.RequestSpinner();
				if(!string.IsNullOrEmpty(android_id))
				{
					Debug.Log("Unlinking Android");
					UnlinkAndroidDeviceIDRequest request = new UnlinkAndroidDeviceIDRequest();
					PlayFabClientAPI.UnlinkAndroidDeviceID(request, OnUnlinkAndroidDeviceIdSuccess, OnPlayFabCallbackError);
				}
				else if (!string.IsNullOrEmpty(ios_id))
				{
					Debug.Log("Unlinking iOS");
					UnlinkIOSDeviceIDRequest request = new UnlinkIOSDeviceIDRequest();
					PlayFabClientAPI.UnlinkIOSDeviceID(request, OnUnlinkIosDeviceIdSuccess, OnPlayFabCallbackError);
				}
			}
		}
		
		/// <summary>
		/// Tests attempts to use device identifier as a valid login credentials. If the device has been used before this will succeed, otherwise fail.
		/// </summary>
		public static void TestDeviceIdHasAccount()
		{
			if( GetDeviceId())
			{
				LoginMethodUsed = LoginPathways.deviceId;
				if(!string.IsNullOrEmpty(android_id))
				{
					Debug.Log("Testing Android Device ID: " + android_id);
					LoginWithAndroidDeviceIDRequest request = new LoginWithAndroidDeviceIDRequest();
					request.AndroidDeviceId = android_id;
					request.TitleId = PlayFabSettings.TitleId;
					request.CreateAccount = false;
					
					PlayFabClientAPI.LoginWithAndroidDeviceID(request, OnLoginResult, OnTestDeviceIdHasAccountError);
				}
				else if (!string.IsNullOrEmpty(ios_id))
				{
					Debug.Log("Testing IOS Device ID: " + ios_id);
					LoginWithIOSDeviceIDRequest request = new LoginWithIOSDeviceIDRequest();
					request.DeviceId = ios_id;
					request.TitleId = PlayFabSettings.TitleId;
					request.CreateAccount = false;
					
					PlayFabClientAPI.LoginWithIOSDeviceID(request, OnLoginResult, OnTestDeviceIdHasAccountError);
				}
			}
		}
			
		/// <summary>
		/// Logs the user out.
		/// </summary>
		public static void Logout()
		{
			OnLoginFail("Logout");
			PlayFabLoginCalls.android_id = string.Empty;
			PlayFabLoginCalls.ios_id = string.Empty;
			if(FB.IsInitialized == true && FB.IsLoggedIn == true)
			{
				CallFBLogout();
			}
		}
	#endregion
	
	#region PlayFab callbacks
		/// <summary>
		/// Callback for LinkAndroidDeviceId Success
		/// </summary>
		/// <param name="result">Result from the API Call</param>
		static void OnLinkAndroidDeviceIdSuccess(LinkAndroidDeviceIDResult result)
		{
			Debug.Log("Linking Android Success!");
			if(OnPlayfabCallbackSuccess != null)
			{
				OnPlayfabCallbackSuccess("", PlayFabAPIMethods.LinkAndroidDeviceID);	
			}
		}
		
		/// <summary>
		/// Callback for LinkIosDeviceId Success
		/// </summary>
		/// <param name="Result"> Result - from the API Call</param>
		static void OnLinkIosDeviceIdSuccess(LinkIOSDeviceIDResult result)
		{
			Debug.Log("Linking iOS Success!");
			if(OnPlayfabCallbackSuccess != null)
			{
				OnPlayfabCallbackSuccess("", PlayFabAPIMethods.LinkIOSDeviceID);	
			}
		}

		/// <summary>
		/// Callback for UnlinkAndroidDeviceId Success
		/// </summary>
		/// <param name="Result"> Result - from the API Call</param>
		static void OnUnlinkAndroidDeviceIdSuccess(UnlinkAndroidDeviceIDResult result)
		{
			Debug.Log("Unlink Android Success!");
			PlayFabLoginCalls.android_id = string.Empty;
			PlayFabLoginCalls.ios_id = string.Empty;
			if(OnPlayfabCallbackSuccess != null)
			{
				OnPlayfabCallbackSuccess("", PlayFabAPIMethods.UnlinkAndroidDeviceID);	
			}
		}
		
		/// <summary>
		/// Callback for UnlinkIosDeviceId Success
		/// </summary>
		/// <param name="Result">  Result - from the API Call</param>
		static void OnUnlinkIosDeviceIdSuccess(UnlinkIOSDeviceIDResult result)
		{
			Debug.Log("Unlink iOS Success!");
			if(OnPlayfabCallbackSuccess != null)
			{
				OnPlayfabCallbackSuccess("", PlayFabAPIMethods.UnlinkIOSDeviceID);	
			}
		}
	
		/// <summary>
		/// Callback for AddUserNameAndPassword Success
		/// </summary>
		/// <param name="Result">  Result - from the API Call</param>
		static void OnAddUserNameAndPasswordSuccess(AddUsernamePasswordResult result)
		{
			if(OnPlayfabCallbackSuccess != null)
			{
				OnPlayfabCallbackSuccess("", PlayFabAPIMethods.AddUsernamePassword);	
			}
		}

		/// <summary>
		/// Callback for SendAccountRecoveryEmail Success
		/// </summary>
		/// <param name="Result">  Result - from the API Call</param>
		static void OnSendAccountRecoveryEmailSuccess(SendAccountRecoveryEmailResult result)
		{
			if(OnPlayfabCallbackSuccess != null)
			{
				OnPlayfabCallbackSuccess("", PlayFabAPIMethods.SendAccountRecoveryEmail);	
			}
		}
	
		/// <summary>
		/// Generic callback for errors, raises the OnPlayFabError event
		/// </summary>
		/// <param name= Error Details </param>
		static void OnPlayFabCallbackError(PlayFabError error)
		{
			string errorMessage = error.ErrorMessage;

			if(OnPlayFabError != null)
			{
				OnPlayFabError(errorMessage, PlayFabAPIMethods.Generic);
			}
		}
		
		/// <summary>
		/// Callback for GetAccountInfo Success
		/// </summary>
		/// <param name="Result"> Result - from the API Call</param>
		static void OnGetAccountInfoSuccess( GetAccountInfoResult result)
		{
			PlayFabLoginCalls.LoggedInUserInfo = result.AccountInfo;
			if(OnPlayfabCallbackSuccess != null)
			{
				OnPlayfabCallbackSuccess("", PlayFabAPIMethods.GetAccountInfo);	
			}
		}
		
		/// <summary>
		/// Callback for TestDeviceIdHasAccount Error
		/// </summary>
		/// <param name="error"></para> error - from the API Call</param>
		public static void OnTestDeviceIdHasAccountError(PlayFabError error)
		{
			PlayFabLoginCalls.android_id = string.Empty;
			PlayFabLoginCalls.ios_id = string.Empty;
			
			if(OnPlayFabError != null)
			{
				if(error.HttpCode == 1001)
				{
					OnPlayFabError("No account matches this device id.", PlayFabAPIMethods.LoginWithDeviceId);
				}
			}
		}	
	
		/// <summary>
		/// Called on a successful registration result
		/// </summary>
		/// <param name="result">Result object returned from the PlayFab server</param>
		private static void OnRegisterResult(RegisterPlayFabUserResult result){
			if(OnPlayfabCallbackSuccess != null)
			{
				OnPlayfabCallbackSuccess("Registration Successful!", PlayFabAPIMethods.RegisterPlayFabUser);
			}
		}
		
		/// <summary>
		/// Called on a successful login attempt
		/// </summary>
		/// <param name="result">Result object returned from PlayFab server</param>
		private static void OnLoginResult(LoginResult result)
		{
			
			if(OnLoginSuccess != null)
			{
				OnLoginSuccess(string.Format("{0}", result.SessionTicket ));
			}
		}
		
		/// <summary>
		/// Raises the login error event.
		/// </summary>
		/// <param name="error">Error.</param>
		private static void OnLoginError(PlayFabError error)
		{
			string errorMessage = string.Empty;
			if (error.Error == PlayFabErrorCode.InvalidParams && error.ErrorDetails.ContainsKey("Password"))
			{
				errorMessage = "Invalid Password";
			}
			else if (error.Error == PlayFabErrorCode.InvalidParams && error.ErrorDetails.ContainsKey("Username") || (error.Error == PlayFabErrorCode.InvalidUsername))
			{
				errorMessage = "Invalid Username";
			}
			else if (error.Error == PlayFabErrorCode.AccountNotFound)
			{
				errorMessage = "Account Not Found, you must have a linked PlayFab account. Start by registering a new account or using your device id";
			}
			else if (error.Error == PlayFabErrorCode.AccountBanned)
			{
				errorMessage = "Account Banned";
			}
			else if (error.Error == PlayFabErrorCode.InvalidUsernameOrPassword)
			{
				errorMessage = "Invalid Username or Password";
			}
			else
			{
				errorMessage = string.Format("Error {0}: {1}", error.HttpCode, error.ErrorMessage);
			}
			
			if(OnLoginFail != null)
			{
				OnLoginFail(errorMessage);				
			}
			
			// reset these IDs (a hack for properly detecting if a device is claimed or not, we will have an API call for this soon)
			PlayFabLoginCalls.android_id = string.Empty;
			PlayFabLoginCalls.ios_id =string.Empty;
		
			//clear the token if we had a fb login fail
			if(FB.IsLoggedIn)
			{
				CallFBLogout();
			}
		}
	
		/// <summary>
		/// Called after a successful name update request
		/// </summary>
		/// <param name="result">Result.</param>
		private static void OnUpdateDisplayNameSuccess(UpdateUserTitleDisplayNameResult result)
		{
			PlayFabLoginCalls.LoggedInUserInfo.TitleInfo.DisplayName = result.DisplayName;
			
			if(OnPlayfabCallbackSuccess != null)
			{
				OnPlayfabCallbackSuccess(result.DisplayName, PlayFabAPIMethods.UpdateDisplayName);	
			}				
		}
	#endregion
	
	/* FOLLOWING CODE FROM FACEBOOK SDK EXAMPLES*/
	#region fb_helperfunctions
	
	// callback after FB.Init();
	public static void OnInitComplete()
	{
		Debug.Log("FB.Init completed: Is user logged in? " + FB.IsLoggedIn);
		if(FB.IsLoggedIn == false)
		{
			CallFBLogin();
		}
		else
		{
			LoginWithFacebook(FB.AccessToken);
		}
	}
	
	// Handler for OnHideUnity Events
	public static void OnHideUnity(bool isGameShown)
	{
		Debug.Log("Is game showing? " + isGameShown);
	}
	
	/// <summary>
	/// Calls FB login.
	/// </summary>
	private static void CallFBLogin()
	{
		FB.Login("public_profile,email,user_friends", LoginCallback);
	}
	
//	private static void CallFBLoginForPublish()
//	{
//		// It is generally good behavior to split asking for read and publish
//		// permissions rather than ask for them all at once.
//		//
//		// In your own game, consider postponing this call until the moment
//		// you actually need it.
//		FB.Login("publish_actions", LoginCallback);
//	}

	// callback called after a successful FB login.
	public static void LoginCallback(FBResult result)
	{
		if (result.Error != null)
		{
			if(OnPlayFabError != null)
			{
				OnPlayFabError("Facebook Error: " + result.Error, PlayFabAPIMethods.Generic);
			}
		}
		else if (!FB.IsLoggedIn)
		{
			if(OnPlayFabError != null)
			{
				OnPlayFabError("Facebook Error: Login cancelled by Player", PlayFabAPIMethods.Generic);
			}
		}
		else
		{
			LoginWithFacebook(FB.AccessToken);
		}
	}
	
	private static void CallFBLogout()
	{
		FB.Logout();
	}
	
	#endregion	
}

// An enum that maps to the PlayFab calls for tracking messages passed around the app
#region API Enum

public enum PlayFabAPIMethods { 
	Generic, 
	RegisterPlayFabUser, 
	LoginWithPlayFab, 
	LoginWithDeviceId,
	GetAccountInfo, 
	UpdateDisplayName, 
	SendAccountRecoveryEmail,
	AddUsernamePassword, 
	LinkAndroidDeviceID, 
	LinkIOSDeviceID, 
	LinkFacebookId, 
	LinkGameCenterId, 
	UnlinkAndroidDeviceID, 
	UnlinkIOSDeviceID, 
	UnlinkFacebookId, 
	UnlinkGameCenterId 
} 
#endregion
