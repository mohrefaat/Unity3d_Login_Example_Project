using System.Collections;
using UnityEngine;


public class GooglePlusWrapper : MonoBehaviour {
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
		}
	}
	
#if !UNITY_EDITOR && UNITY_ANDROID
	private string className = "com.ThugLeaf.GooglePlusForUnity.DemoActivity";
    private static AndroidJavaClass activityClass;
    private static AndroidJavaObject activityInstance;

    private void Init() {
        Debug.Log("Init start");
        AndroidJNI.AttachCurrentThread();
        activityClass = new AndroidJavaClass(className);
        activityInstance = activityClass.GetStatic<AndroidJavaObject>("mContext");
        Debug.Log("Init complete");
    }

    public void SetUnityObjectName(string objectName){
        activityClass.SetStatic<string>("UnityObjectName", objectName);
    }

    public void SetTokenCallbackName(string tokenCallback){
        activityClass.SetStatic<string>("TokenCallbackName", tokenCallback);
    }

    public void SetSignInCallbackName(string signInCallback){
        activityClass.SetStatic<string>("SignInCallbackName", signInCallback);
    }

    public void SetCirclesLoadedCallbackName(string circlesCallback){
        activityClass.SetStatic<string>("CirclesCallbackName", circlesCallback);
    }

    public void SetConnectionSuspendedCallbackName(string callback){
        activityClass.SetStatic<string>("ConnectionSuspendedCallbackName", callback);
    }

    private void Awake() {
        Init();
    }

    private void OnDestroy() {
        activityClass.Dispose();
        activityInstance.Dispose();
    }

    public void GetToken() {
        activityInstance.Call("GetToken");
    }

    public void SignIn() {
        activityInstance.Call("SignIn");
    }

    public void SignOut() {
        activityInstance.Call("SignOut");
    }

    public void LoadCircles() {
        activityInstance.Call("LoadCircles");
    }

    public string GetProfilePictureUrl() {
        return activityInstance.Call<string>("GetProfilePictureUrl");
    }

    public string GetDisplayName() {
        return activityInstance.Call<string>("GetDisplayName");
    }
    
	public void OnGoogleTokenReceived(string token) {
		Token = token;
		PlayFabLoginCalls.SignOnWithGoogle(token);
	}
	
#endif
}