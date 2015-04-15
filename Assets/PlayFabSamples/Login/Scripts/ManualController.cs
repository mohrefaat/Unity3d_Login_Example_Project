using UnityEngine;
using System.Collections;

/// <summary>
/// View Controller for the manual login screen
/// </summary>
public class ManualController : MonoBehaviour {
	public Transform playfab_fields;
	public Transform facebook_fields;
	public Transform google_fields;
	public Transform steam_fields;
	public Transform gamecenter_fields;
	
	public LoginPathwayController pathwayController;
	
	void OnEnable()
	{
		LoginPathwayController.LoginPathwayChange += HandleOnLoginPathwayChange;
	}
	
	void OnDisable()
	{
		LoginPathwayController.LoginPathwayChange -= HandleOnLoginPathwayChange;
	}
	
	// Use this for initialization
	void Start () 
	{

	}
	
	void HandleOnLoginPathwayChange(LoginPathwayController.LoginPathways path, UnityEngine.UI.Button btn)
	{
		TurnOffForms();
		switch(path)
		{
			case LoginPathwayController.LoginPathways.playfab:
				playfab_fields.gameObject.SetActive(true);
			break;
			
			case LoginPathwayController.LoginPathways.facebook:
				facebook_fields.gameObject.SetActive(true);
			break;
			
			case LoginPathwayController.LoginPathways.google:
				google_fields.gameObject.SetActive(true);
			break;
			
			case LoginPathwayController.LoginPathways.steam:
				steam_fields.gameObject.SetActive(true);
			break;
			
			case LoginPathwayController.LoginPathways.gamecenter:
				gamecenter_fields.gameObject.SetActive(true);
			break;
		}	
	}
	
	void TurnOffForms()
	{
		playfab_fields.gameObject.SetActive(false);
		facebook_fields.gameObject.SetActive(false);
		google_fields.gameObject.SetActive(false);
		steam_fields.gameObject.SetActive(false);
		gamecenter_fields.gameObject.SetActive(false);	
	}
	
	
	// Update is called once per frame
	void Update () {
	
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
