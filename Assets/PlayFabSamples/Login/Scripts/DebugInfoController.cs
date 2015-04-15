using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// View controller for the top bar (visible only after login). Displays errors, provides log-out and detailed account info.
/// </summary>
public class DebugInfoController : MonoBehaviour {
	public Text errorLog;
	public Text version;
	public string buildVersion;
	public Image spinner;
	public bool isSpinning = false;
	public Button logout;
	
	public Button info;
	public Transform infoPanel;
	
	void OnEnable()
	{
		PlayFabLoginCalls.OnLoginFail += HandleOnLoginFail;
		PlayFabLoginCalls.OnLoginSuccess += HandleOnLoginSuccess;
		PlayFabLoginCalls.OnPlayFabError += HandlePlayFabError;
		PlayFabLoginCalls.OnPlayfabCallbackSuccess += HandleCallbackSuccess;
		PlayFabLoginCalls.StartSpinnerRequest += HandleStartSpinnerRequest;
	}
	
	void OnDisable()
	{
		PlayFabLoginCalls.OnLoginFail -= HandleOnLoginFail;
		PlayFabLoginCalls.OnLoginSuccess -= HandleOnLoginSuccess;
		PlayFabLoginCalls.OnPlayFabError -= HandlePlayFabError;
		PlayFabLoginCalls.OnPlayfabCallbackSuccess -= HandleCallbackSuccess;
		PlayFabLoginCalls.StartSpinnerRequest -= HandleStartSpinnerRequest;
	}
	
	void HandleOnLoginFail(string message)
	{
		StopSpinner();
		this.errorLog.color = Color.yellow;
		this.errorLog.text = message;
		
		if(message == "Logout")
		{
			this.logout.gameObject.SetActive(false);
			this.info.gameObject.SetActive(false);
			this.infoPanel.gameObject.SetActive(false);
			this.errorLog.text = "You successfully logged out.";	
		}
		
	}
	
	void HandlePlayFabError(string message, PlayFabAPIMethods method)
	{
		StopSpinner();
		this.errorLog.color = Color.yellow;
		this.errorLog.text = message;
	}
	
	void HandleOnLoginSuccess(string message)
	{
		StopSpinner();
		this.errorLog.color = Color.green;
		this.errorLog.text = string.Format("Login Success -- Player ID:{0}", message);
		Debug.Log(this.errorLog.text);
		this.logout.gameObject.SetActive(true);
		this.info.gameObject.SetActive(true);
		ToggleInfoPane();
	}
	
	void HandleCallbackSuccess(string message, PlayFabAPIMethods method)
	{
		StopSpinner();
		this.errorLog.color = Color.white;
		this.errorLog.text = string.Empty;
	}
	
	
	// Use this for initialization
	void Start () {
		this.logout.onClick.AddListener(() => Logout());
		this.info.onClick.AddListener(() => ToggleInfoPane());
		
		this.errorLog.text = string.Empty;
		this.version.text = this.buildVersion;
	}
	
	void ToggleInfoPane()
	{
		bool val = !this.infoPanel.gameObject.activeSelf;
		this.infoPanel.gameObject.SetActive(val);
		if(val == true)
		{
			this.info.gameObject.GetComponent<Image>().color = Color.green;
		}
		else
		{
			this.info.gameObject.GetComponent<Image>().color = Color.white;
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		float rotationSpeed = 120f; // x degrees per sec
		if(isSpinning == true)
		{
			this.spinner.rectTransform.Rotate(0,0,(Time.deltaTime * rotationSpeed));
		}
	}
	
	void HandleStartSpinnerRequest()
	{
		if(isSpinning == false)
		{
			isSpinning = true;
			spinner.gameObject.SetActive(true);
		}
	}
	
	void Logout()
	{
		PlayFabLoginCalls.Logout();
	}
	
	void StopSpinner()
	{
		isSpinning = false;
		spinner.gameObject.SetActive(false);
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
