using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
///  view controller for the manual login screen login pathway controller component (pathway selector)
/// </summary>
public class LoginPathwayController : MonoBehaviour {

	public Button usePlayfab;
	public Button useFacebook;
	public Button useGoogle;
	public Button useSteam;
	public Button useGamecenter;
	public Image arrow;
	public enum LoginPathways { playfab =  0, facebook = 1, google = 2, steam =3, gamecenter = 4 }
	
	public delegate void OnLoginPathwayChange(LoginPathways path, Button btn);
	public static event OnLoginPathwayChange LoginPathwayChange;	
	

	// Use this for initialization
	void Start () {
		usePlayfab.onClick.AddListener(() => ChangeLoginPathway(LoginPathways.playfab, usePlayfab));
		useFacebook.onClick.AddListener(() => ChangeLoginPathway(LoginPathways.facebook, useFacebook));
		useGoogle.onClick.AddListener(() => ChangeLoginPathway(LoginPathways.google, useGoogle));
		useSteam.onClick.AddListener(() => ChangeLoginPathway(LoginPathways.steam, useSteam ));
		useGamecenter.onClick.AddListener(() => ChangeLoginPathway(LoginPathways.gamecenter, useGamecenter));
		
		if(Application.platform == RuntimePlatform.Android)
		{
			this.useGamecenter.interactable = false;
			this.useSteam.interactable = false;
		}
		if(Application.platform == RuntimePlatform.IPhonePlayer)
		{
			this.useGoogle.interactable = false;
			this.useSteam.interactable = false;
		}
	}
	
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void ChangeLoginPathway(LoginPathways path, Button btn)
	{
		LoginPathwayChange(path, btn);
		
		Vector2 pos = btn.GetComponent<RectTransform>().anchoredPosition;
		arrow.GetComponent<RectTransform>().anchoredPosition = new Vector2(pos.x, arrow.GetComponent<RectTransform>().anchoredPosition.y );
		
	}
	
}
