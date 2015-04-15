using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// View Controller for the Account Info tab
/// </summary>
public class Panel_AcountInfo : MonoBehaviour {
	public Transform partialAccount;
	public Transform completeAccount;
	//public Transform 
	
	public Button UpdateDisplayName;
	public Button CompleteAccount;
	public Button SendRecoveryEmail;
	public Button LinkDevice;
	
	public InputField username;
	public InputField password;
	public InputField email;
	public InputField displayName;
	
	public Text activeUser;
	public Text activeEmail;
	public Text InfoHeading;
	
	void OnEnable()
	{
		PlayFabLoginCalls.OnPlayfabCallbackSuccess += HandleCallbackSuccess;
	}
	
	void OnDisable()
	{
		PlayFabLoginCalls.OnPlayfabCallbackSuccess -= HandleCallbackSuccess;
	}	
	

	// Use this for initialization
	void Start ()
	{
		this.UpdateDisplayName.onClick.AddListener(() => PlayFabLoginCalls.UpdateDisplayName(this.displayName.text));
		this.CompleteAccount.onClick.AddListener(() => PlayFabLoginCalls.AddUserNameAndPassword(username.text, password.text, email.text));
		this.SendRecoveryEmail.onClick.AddListener(() => PlayFabLoginCalls.SendAccountRecoveryEmail(PlayFabLoginCalls.LoggedInUserInfo.PrivateInfo.Email));
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}
	
	void EvaluateAccountInfo()
	{
		if(PlayFabLoginCalls.LoggedInUserInfo != null)
		{
			this.InfoHeading.text = string.Format("PlayFab Account Info ({0}):", PlayFabLoginCalls.LoggedInUserInfo.PlayFabId);
			if(PlayFabLoginCalls.LoggedInUserInfo.Username == null && PlayFabLoginCalls.LoggedInUserInfo.PrivateInfo.Email == null )
			{
				partialAccount.gameObject.SetActive(true);
				completeAccount.gameObject.SetActive(false);
				
			}
			else
			{
				partialAccount.gameObject.SetActive(false);
				completeAccount.gameObject.SetActive(true);
				this.activeEmail.text = string.Format("Email: {0}", PlayFabLoginCalls.LoggedInUserInfo.PrivateInfo.Email);
				this.activeUser.text = string.Format("Username: {0}", PlayFabLoginCalls.LoggedInUserInfo.Username);
			}
			
			if(!string.IsNullOrEmpty(PlayFabLoginCalls.LoggedInUserInfo.TitleInfo.DisplayName))
			{
				this.displayName.text = PlayFabLoginCalls.LoggedInUserInfo.TitleInfo.DisplayName;
			}	
			
			if(string.IsNullOrEmpty(PlayFabLoginCalls.android_id) && string.IsNullOrEmpty(PlayFabLoginCalls.ios_id))
			{
				LinkDevice.GetComponent<Image>().color = Color.green;
				LinkDevice.transform.FindChild("Text").GetComponent<Text>().text = "Link this device to this account";
				LinkDevice.onClick.RemoveAllListeners();
				LinkDevice.onClick.AddListener(() => PlayFabLoginCalls.LinkDeviceId());
			}
			else
			{
				LinkDevice.GetComponent<Image>().color = Color.red;
				LinkDevice.transform.FindChild("Text").GetComponent<Text>().text = "Unlink this device to this account";
				LinkDevice.onClick.RemoveAllListeners();
				LinkDevice.onClick.AddListener(() => PlayFabLoginCalls.UnlinkDeviceId());
			}						
		}
		else
		{
			Debug.Log("Account was null");
		}
	}
	
	void HandleCallbackSuccess(string message, PlayFabAPIMethods method)
	{
		if(method == PlayFabAPIMethods.GetAccountInfo || method == PlayFabAPIMethods.LinkAndroidDeviceID || method == PlayFabAPIMethods.UnlinkAndroidDeviceID || method == PlayFabAPIMethods.LinkIOSDeviceID || method == PlayFabAPIMethods.UnlinkIOSDeviceID)
		{
			EvaluateAccountInfo();
		}
	}
	
}
