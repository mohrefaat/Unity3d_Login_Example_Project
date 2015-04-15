using UnityEngine;
using System.Collections;

/// <summary>
/// Testing class used for configuring the playerprefs at start up (Awake), useful for testing specific combinations of login credentials
///	Expired Auth token for testing: C4B1CEE856CF3F54-0-0-98FA-8D21F384E701B7E-17F0B99202C37DBE.49D6ADAB313844AC
/// </summary>
public class TEST_PlayerPrefs : MonoBehaviour {
	public PlayFabLoginCalls.LoginPathways pathway;
	public string accountInfo = "{\"PlayFabId\":\"C4B1CEE856CF3F54\",\"Created\":\"2014-08-19T00:40:18Z\",\"Username\":\"sdktestuser1\",\"TitleInfo\":{\"DisplayName\":\"SDKTesting1\",\"Origination\":null,\"Created\":\"2015-01-25T02:41:17Z\",\"LastLogin\":\"2015-03-12T20:50:36Z\",\"FirstLogin\":\"2015-01-25T02:41:17Z\",\"isBanned\":null},\"PrivateInfo\":{\"Email\":\"sdktestuser1@playfabsandbox.com\"},\"FacebookInfo\":null,\"SteamInfo\":null,\"GameCenterInfo\":null}";
	public bool clearPrefsOnAwake = false;
	public bool loadTestPathway = false;
	public bool loadTestAccountInfo = false;

	void Awake()
	{
		if(this.enabled == true)
		{
			if(clearPrefsOnAwake == true)
			{
				PlayerPrefs.DeleteAll();
			}
			
			if(loadTestPathway == true)
			{
				PlayerPrefs.SetString("loginMethodUsed", pathway.ToString());
			}
			
			if(loadTestAccountInfo == true)
			{
				PlayerPrefs.SetString("accountInfo", accountInfo);
			}
		}
	}
	
	void Start()
	{
	}
	
	void Update()
	{
	}
}
