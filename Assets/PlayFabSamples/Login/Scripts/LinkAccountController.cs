using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LinkAccountController : MonoBehaviour {
	public InputField steamField; 
	public InputField googleField; 
	public InputField facebookField; 
	public InputField gameCenterField; 
	
	public Button steamButton;
	public Button googleButton;
	public Button facebookButton;
	public Button gameCenterButton;
	
	void OnEnable()
	{
		StartCoroutine(InitializeLinks());
		
	}
	
	IEnumerator InitializeLinks()
	{
		if(PlayFabLoginCalls.LoggedInUserInfo != null)
		{
			if(PlayFabLoginCalls.LoggedInUserInfo.SteamInfo == null)
			{
				steamButton.GetComponent<Image>().color = Color.green;
				steamButton.transform.FindChild("Text").GetComponent<Text>().text = "Link account to Steam";
				steamButton.onClick.RemoveAllListeners();
				//steamButton.onClick.AddListener(() => PlayFabLoginCalls.LinkDeviceId());
			}
			else
			{
				steamField.text = PlayFabLoginCalls.LoggedInUserInfo.SteamInfo.SteamId;
				steamButton.GetComponent<Image>().color = Color.red;
				steamButton.transform.FindChild("Text").GetComponent<Text>().text = "Unlink Steam";
				steamButton.onClick.RemoveAllListeners();
				//steamButton.onClick.AddListener(() => PlayFabLoginCalls.LinkDeviceId());
			}
			
			
			if(PlayFabLoginCalls.LoggedInUserInfo.FacebookInfo == null)
			{
				facebookButton.GetComponent<Image>().color = Color.green;
				facebookButton.transform.FindChild("Text").GetComponent<Text>().text = "Link account to Facebook";
				facebookButton.onClick.RemoveAllListeners();
				//facebookButton.onClick.AddListener(() => PlayFabLoginCalls.LinkDeviceId());
			}
			else
			{
				facebookField.text = PlayFabLoginCalls.LoggedInUserInfo.FacebookInfo.FacebookId;
				facebookButton.GetComponent<Image>().color = Color.red;
				facebookButton.transform.FindChild("Text").GetComponent<Text>().text = "Unlink Facebook";
				facebookButton.onClick.RemoveAllListeners();
				//facebookButton.onClick.AddListener(() => PlayFabLoginCalls.LinkDeviceId());
			}
			
			
			if(PlayFabLoginCalls.LoggedInUserInfo.GameCenterInfo == null)
			{
				gameCenterButton.GetComponent<Image>().color = Color.green;
				gameCenterButton.transform.FindChild("Text").GetComponent<Text>().text = "Link account to GameCenter";
				gameCenterButton.onClick.RemoveAllListeners();
				//gameCenterButton.onClick.AddListener(() => PlayFabLoginCalls.LinkDeviceId());
			}
			else
			{
				gameCenterField.text = PlayFabLoginCalls.LoggedInUserInfo.GameCenterInfo.GameCenterId;
				gameCenterButton.GetComponent<Image>().color = Color.red;
				gameCenterButton.transform.FindChild("Text").GetComponent<Text>().text = "Unlink GameCenter";
				gameCenterButton.onClick.RemoveAllListeners();
				//gameCenterButton.onClick.AddListener(() => PlayFabLoginCalls.LinkDeviceId());
			}
			
			
			if(PlayFabLoginCalls.LoginMethodUsed != PlayFabLoginCalls.LoginPathways.googlePlus)
			{
				googleButton.GetComponent<Image>().color = Color.green;
				googleButton.transform.FindChild("Text").GetComponent<Text>().text = "Link account to Google+";
				googleButton.onClick.RemoveAllListeners();
				//googleButton.onClick.AddListener(() => PlayFabLoginCalls.LinkDeviceId());
			}
			else
			{
				googleField.text = PlayFabLoginCalls.LoggedInUserInfo.SteamInfo.SteamId;
				googleButton.GetComponent<Image>().color = Color.red;
				googleButton.transform.FindChild("Text").GetComponent<Text>().text = "Unlink Google+";
				googleButton.onClick.RemoveAllListeners();
				//googleButton.onClick.AddListener(() => PlayFabLoginCalls.LinkDeviceId());
			}
		}
		else
		{
			yield return null;
			StartCoroutine(InitializeLinks());
		}
		yield return null;
	}
	
	
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
