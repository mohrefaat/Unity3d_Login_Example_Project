using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// View controller for the bottom stoplight menu
/// </summary>
public class BottomMenuController : MonoBehaviour {
	public AuthenticationController authController;
	public Button green;
	public Button yellow;
	public Button red;
	
	
	void OnEnable()
	{
		AuthenticationController.LoginStateChange += HandleOnLoginStateChange;
	}
	
	void OnDisable()
	{
		AuthenticationController.LoginStateChange -= HandleOnLoginStateChange;
	}
	
	// Use this for initialization
	void Start () {
		green.onClick.AddListener(() => handleButton(green));
		yellow.onClick.AddListener(() => handleButton(yellow));
		red.onClick.AddListener(() => handleButton(red));
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	
	void HandleOnLoginStateChange(AuthenticationController.LoginStates state)
	{
		switch(state)
		{
			case AuthenticationController.LoginStates.Auto:
				green.transform.FindChild("Text").GetComponent<Text>().text = "Enter Account Manually";
				yellow.transform.FindChild("Text").GetComponent<Text>().text = "Register";
				//red.transform.FindChild("Text").GetComponent<Text>().text = "Exit";
			break;
			
			case AuthenticationController.LoginStates.Manual:
				green.transform.FindChild("Text").GetComponent<Text>().text = "Use Device Id";
				yellow.transform.FindChild("Text").GetComponent<Text>().text = "Register";
				//red.transform.FindChild("Text").GetComponent<Text>().text = "Exit";
			break;
			
			case AuthenticationController.LoginStates.Register:
				green.transform.FindChild("Text").GetComponent<Text>().text = "Use Device Id";
				yellow.transform.FindChild("Text").GetComponent<Text>().text = "Enter Account Manually";
				//red.transform.FindChild("Text").GetComponent<Text>().text = "Exit";
			break;
		}
	}
	
	void handleButton(Button b) 
	{
		Text buttonText = b.transform.FindChild("Text").GetComponent<Text>() as Text;
		
		switch(buttonText.text)
		{
			case "Use Device Id":	
				PlayFabLoginCalls.RequestSpinner();
				PlayFabLoginCalls.LoginWithDeviceId(false);
				break;
				
			case "Register":
				authController.activeState = AuthenticationController.LoginStates.Register;
				break;
				
			case "Exit":
				//authController.activeState = AuthenticationController.LoginStates.Register;
				Application.Quit();
				break;
			
			case "Enter Account Manually":
				authController.activeState = AuthenticationController.LoginStates.Manual;
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
