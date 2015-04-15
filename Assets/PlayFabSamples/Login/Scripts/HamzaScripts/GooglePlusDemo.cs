using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;


[RequireComponent(typeof(GooglePlusWrapper))]
public class GooglePlusDemo : MonoBehaviour {

    [SerializeField]
    private Button getTokenButton;

    [SerializeField]
    private Button signInButton;

    [SerializeField]
    private Button signOutButton;

    private string token;

    public string Token {
        get {
            return token;
        }
        private set {
            if (token != null && token.Equals(value)) {
                return;
            }
            token = value;
            //tokenText.SetText("TOKEN : {0}", token);
        }
    }

    [SerializeField]
    private Text tokenText;

    [SerializeField]
    private Text displayNameText;

    [SerializeField]
    private Image profilePictureImage;

    [SerializeField]
    private ListView googleCirclesList;

    private ContactInfo[] googlePlusContacts;

    private const string DefaultObjectName = "GooglePlusGO";

#if !UNITY_EDITOR && UNITY_ANDROID

    private GooglePlusWrapper wrapper;

    private void Awake() {
        wrapper = GetComponent<GooglePlusWrapper>();
        this.gameObject.name = DefaultObjectName;
    }
#endif

    private void Start() {
//        getTokenButton.onClick.AddListener(OnGetTokenButtonClicked);
//        getTokenButton.interactable = true;
//        signInButton.onClick.AddListener(OnSignInButtonClicked);
//        signInButton.interactable = true;
//        signOutButton.onClick.AddListener(OnSignOutButtonClicked);
//        signOutButton.interactable = false;
    }

    public void OnGetTokenButtonClicked() {
#if !UNITY_EDITOR && UNITY_ANDROID
    wrapper.GetToken();
#endif
    }

    public void OnSignInButtonClicked() {
#if !UNITY_EDITOR && UNITY_ANDROID
    wrapper.SignIn();
#endif
    }

    public void OnSignOutButtonClicked() {
#if !UNITY_EDITOR && UNITY_ANDROID
    wrapper.SignOut();
#endif
        getTokenButton.interactable = true;
        signOutButton.interactable = false;
        signInButton.interactable = true;
        displayNameText.text = "NOT CONNECTED !";
        tokenText.text = "NOT TOKEN YET";
        profilePictureImage.sprite = new Sprite();
        googleCirclesList.ClearList();
    }

    public void OnCirclesLoaded(string circles) {
        string[] tmp = circles.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        googlePlusContacts = new ContactInfo[tmp.Length];
        for (int i = 0; i < tmp.Length; i++) {
            googlePlusContacts[i] = ParseGooglePlusContact(tmp[i]);
            googleCirclesList.AddItem(googlePlusContacts[i]);
        }
    }

    public void OnGoogleTokenReceived(string token) {
        Token = token;
		SignOnWithGoogle(token);
    }



	public void SignOnWithGoogle(string token)
	{
		Debug.Log(string.Format ("Signing into PlayFab with: {0}", token));
		LoginWithGoogleAccountRequest request = new LoginWithGoogleAccountRequest();
		request.AccessToken = token;
		request.CreateAccount = true;
		PlayFabClientAPI.LoginWithGoogleAccount(request, OnSignOnWithGoogleSuccess, OnPlayFabError);
		
	}

	public void OnPlayFabError(PlayFabError error)
	{
		Debug.Log(error.ErrorMessage);
	}

	public void OnSignOnWithGoogleSuccess(LoginResult result)
	{
		Debug.Log(result.SessionTicket);
	}




    public void OnSignInSuccess(string googleId) {
#if !UNITY_EDITOR && UNITY_ANDROID
        displayNameText.SetText("Hello {0} !", wrapper.GetDisplayName());
        string profilePictureUrl = wrapper.GetProfilePictureUrl().Replace("50", "180");
        DownloadManager.Instance.DownloadTextureAsync(profilePictureUrl, OnProfilePictureDownloaded);
        wrapper.LoadCircles();
        signOutButton.interactable = true;
        signInButton.interactable = false;
#endif
    }

    public void OnConnectionSuspended(string reason) {
    }

    public void OnProfilePictureDownloaded(Texture2D texture) {
        profilePictureImage.SetTexture2D(texture, 180, 180);
    }

    private ContactInfo ParseGooglePlusContact(string contact) {
        string[] tmp = contact.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
        ContactInfo info = new ContactInfo() {
            Id = tmp[0],
            DisplayName = tmp[1],
        };
        if (tmp.Length == 3) {
            info.ProfilePictureUrl = tmp[2].Replace("50", "180");
        }
        return info;
    }
}