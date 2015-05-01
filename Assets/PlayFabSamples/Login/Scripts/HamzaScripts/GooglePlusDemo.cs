using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GooglePlusWrapper))]
public class GooglePlusDemo : MonoBehaviour {

    [SerializeField]
    private Button getTokenButton;

    [SerializeField]
    private Button signInButton;

    [SerializeField]
    private Button signOutButton;

    [SerializeField]
    private Button revokeButton;

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
            tokenText.SetText("TOKEN : {0}", token);
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

#if !UNITY_EDITOR && UNITY_ANDROID

    private GooglePlusWrapper wrapper;

    private void Awake() {
        wrapper = GetComponent<GooglePlusWrapper>();
    }
#endif

    private void Start() {
        displayNameText.text = "NOT CONNECTED !";
        tokenText.text = "NO TOKEN YET";
        getTokenButton.onClick.AddListener(OnGetTokenButtonClicked);
        getTokenButton.interactable = true;
        signInButton.onClick.AddListener(OnSignInButtonClicked);
        signInButton.interactable = true;
        signOutButton.onClick.AddListener(OnSignOutButtonClicked);
        signOutButton.interactable = false;
        revokeButton.onClick.AddListener(OnRevokeButtonClicked);
        revokeButton.interactable = false;
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
        revokeButton.interactable = false;
        displayNameText.text = "NOT CONNECTED !";
        tokenText.text = "NO TOKEN YET";
        profilePictureImage.sprite = new Sprite();
        googleCirclesList.ClearList();
    }

    public void OnRevokeButtonClicked() {
#if !UNITY_EDITOR && UNITY_ANDROID
        wrapper.RevokeAccess();
#endif
    }

    public void OnCirclesLoaded(string circles) {
        string[] tmp = circles.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        googlePlusContacts = new ContactInfo[tmp.Length];
        for (int i = 0; i < tmp.Length; i++) {
            googlePlusContacts[i] = ParseGooglePlusContact(tmp[i]);
            googleCirclesList.AddItem(googlePlusContacts[i]);
        }
    }

    public void OnTokenReceived(string token) {
        Token = token;
        getTokenButton.interactable = false;
    }

    public void OnSignInSuccess(string googleId) {
        Utils.Log("SignInSuccess for id={0}", googleId);
#if !UNITY_EDITOR && UNITY_ANDROID
        displayNameText.SetText("Hello {0} !", wrapper.GetDisplayName());
        string profilePictureUrl = wrapper.GetProfilePictureUrl().Replace("50", "180");
        DownloadManager.Instance.DownloadTextureAsync(profilePictureUrl, OnProfilePictureDownloaded);
        wrapper.LoadCircles();
        signOutButton.interactable = true;
        signInButton.interactable = false;
        revokeButton.interactable = true;
#endif
    }

    public void OnConnectionSuspended(string reason) {
        Utils.Log("is it working ? {0}", reason);
    }

    public void OnAccessRevoked() {
        getTokenButton.interactable = true;
        signOutButton.interactable = false;
        signInButton.interactable = true;
        revokeButton.interactable = false;
        displayNameText.text = "NOT CONNECTED !";
        tokenText.text = "NO TOKEN YET";
        profilePictureImage.sprite = new Sprite();
        googleCirclesList.ClearList();
    }

    public void OnProfilePictureDownloaded(Texture2D texture) {
        profilePictureImage.SetTexture2D(texture,
            ContactInfo.PROFILE_PICTURE_DIMENSION,
            ContactInfo.PROFILE_PICTURE_DIMENSION);
    }

    private ContactInfo ParseGooglePlusContact(string contact) {
        string[] tmp = contact.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
        ContactInfo info = new ContactInfo() {
            Id = tmp[0],
            DisplayName = tmp[1],
        };
        if (tmp.Length == 3) {
            string url = tmp[2];
            if (!url.Contains("null")) {
                if (url.Contains("?sz=50")) {
                    info.ProfilePictureUrl = url.Replace("50", ContactInfo.PROFILE_PICTURE_DIMENSION.ToString());
                    //Utils.Log("50 {0}", name);
                }
                else if (url.Contains("?sz=128")) {
                    info.ProfilePictureUrl = url.Replace("128", ContactInfo.PROFILE_PICTURE_DIMENSION.ToString());
                    //Utils.Log("128 {0}", name);
                }
                else {
                    info.ProfilePictureUrl = string.Concat(url,
                        string.Format("?sz={0}", ContactInfo.PROFILE_PICTURE_DIMENSION));
                    //Utils.Log("none {0}", name);
                }
            }
            else {
                //Utils.Log("url == {0}", url);
            }
        }
        return info;
    }
}