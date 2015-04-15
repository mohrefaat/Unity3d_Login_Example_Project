package com.ThugLeaf.GooglePlusForUnity;


import java.io.IOException;
import java.util.Random;

import android.accounts.AccountManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.content.IntentSender.SendIntentException;
import android.os.AsyncTask;
import android.os.Bundle;
import android.util.Log;
import android.widget.Toast;

import com.google.android.gms.auth.GoogleAuthException;
import com.google.android.gms.auth.GoogleAuthUtil;
import com.google.android.gms.auth.GooglePlayServicesAvailabilityException;
import com.google.android.gms.auth.UserRecoverableAuthException;
import com.google.android.gms.common.AccountPicker;
import com.google.android.gms.common.ConnectionResult;
import com.google.android.gms.common.GooglePlayServicesUtil;
import com.google.android.gms.common.Scopes;
import com.google.android.gms.common.api.CommonStatusCodes;
import com.google.android.gms.common.api.GoogleApiClient;
import com.google.android.gms.common.api.GoogleApiClient.ConnectionCallbacks;
import com.google.android.gms.common.api.ResultCallback;
import com.google.android.gms.plus.People;
import com.google.android.gms.plus.People.LoadPeopleResult;
import com.google.android.gms.plus.Plus;
import com.google.android.gms.plus.model.people.Person;
import com.google.android.gms.plus.model.people.Person.Image;
import com.google.android.gms.plus.model.people.PersonBuffer;
import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

/**
 * Created by Hamza on 1/16/2015.
 */


public class DemoActivity extends UnityPlayerActivity 
		implements ConnectionCallbacks, 
					GoogleApiClient.OnConnectionFailedListener,
					ResultCallback<People.LoadPeopleResult> {
	public static Context mContext;
	private static final String TAG = DemoActivity.class.getSimpleName();

	private static final int REQUEST_CODE_RECOVER_PLAY_SERVICES = 1001;
	private static final int REQUEST_CODE_PICK_ACCOUNT = 1002;
	private static final int REQUEST_CODE_AUTH_CONSENT = 1003;
	private static final int REQUEST_CODE_SIGN_IN = 1004;
	
	public static String UnityObjectName = "GooglePlusGO";
	public static String TokenCallbackName = "OnGoogleTokenReceived";
	public static String CirclesCallbackName = "OnCirclesLoaded";
	public static String SignInCallbackName = "OnSignInSuccess";
	public static String ConnectionSuspendedCallbackName = "OnConnectionSuspended";
	
	private String accountName;
 
	/* A flag indicating that a PendingIntent is in progress and prevents
	   * us from starting further intents.
	   */
	  private boolean mIntentInProgress;
	
	/* Track whether the sign-in button has been clicked so that we know to resolve
	 * all issues preventing sign-in without waiting.
	 */
	private boolean mSignInClicked;
	
	private ConnectionResult lastConnectionResult;
	
	private String token;

    private GoogleApiClient mGoogleApiClient;
	
	private GoogleApiClient buildGoogleApiClient() {
        // When we build the GoogleApiClient we specify where connected and
        // connection failed callbacks should be returned, which Google APIs our
        // app uses and which OAuth 2.0 scopes our app requests.
        return new GoogleApiClient.Builder(this)
                .addConnectionCallbacks(this)
                .addOnConnectionFailedListener(this)
                .addApi(Plus.API, Plus.PlusOptions.builder().build())
                .addScope(Plus.SCOPE_PLUS_LOGIN)
                .build();
    }
	
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		mContext = this;
		mGoogleApiClient = buildGoogleApiClient();
		super.onCreate(savedInstanceState);
		Log.i(TAG, "onCreate");
	}

	@Override
	protected void onStart(){
		super.onStart();
		Log.i(TAG, "onStart");
	}
	
	@Override
	protected void onStop(){
		super.onStop();
		if (mGoogleApiClient.isConnected()) {
		      mGoogleApiClient.disconnect();
	    }
	}
	
	@Override
	protected void onActivityResult(int requestCode, int resultCode, Intent data) {
		switch (requestCode) {
		case REQUEST_CODE_SIGN_IN:
			mIntentInProgress = false;
			if (resultCode == RESULT_CANCELED){
				showErrorToast("This application requires a Google account.");
			}
			else {
				Log.i(TAG, "REQUEST_CODE_SIGN_IN : " + resultCode);
				if (!mGoogleApiClient.isConnecting()) {
		    	      mGoogleApiClient.connect();
			    }
			}
			return;
		case REQUEST_CODE_AUTH_CONSENT:
			if (resultCode == RESULT_OK) {
				checkAccount();
			} else if (resultCode == RESULT_CANCELED) {
				showErrorToast("This application requires your consent before signing you in.");
			} else {
				Log.i(TAG, "REQUEST_CODE_AUTH_CONSENT : " + resultCode);
			}
			return;
		case REQUEST_CODE_RECOVER_PLAY_SERVICES:
			if (resultCode == RESULT_OK) {
				checkAccount();
			} else if (resultCode == RESULT_CANCELED) {
				showErrorToast("Google Play Services must be installed.");
			} else {
				Log.i(TAG, "REQUEST_CODE_RECOVER_PLAY_SERVICES : " + resultCode);
			}
			return;
		case (REQUEST_CODE_PICK_ACCOUNT):
			if (resultCode == RESULT_OK) {
				accountName = data
						.getStringExtra(AccountManager.KEY_ACCOUNT_NAME);
				checkAccount();
			} else if (resultCode == RESULT_CANCELED) {
				showErrorToast("This application requires a Google account.");
			} else {
				Log.i(TAG, "REQUEST_CODE_PICK_ACCOUNT : " + resultCode);
			}
			return;
		}
		super.onActivityResult(requestCode, resultCode, data);
	}

	private String getToken(String accountName) {
		String scopes = "oauth2:" + Scopes.PLUS_LOGIN;
		String code = null;
		try {
			code = GoogleAuthUtil.getToken(this, // Context context
					accountName, // String accountName
					scopes // String scope
					);

		} catch (GooglePlayServicesAvailabilityException playEx) {
			showErrorDialog(playEx.getConnectionStatusCode());
			Log.e(TAG,
					"GooglePlayServicesAvailabilityException : "
							+ playEx.getMessage(), playEx);
		} catch (IOException transientEx) {
			// network or server error, the call is expected to succeed if you
			// try again later.
			// Don't attempt to call again immediately - the request is likely
			// to
			// fail, you'll hit quotas or back-off.
			Log.e(TAG,
					"transient error encountered : " + transientEx.getMessage(),
					transientEx);
			doExponentialBackoff();
			// return;
		} catch (UserRecoverableAuthException e) {
			// Requesting an authorization code will always throw
			// UserRecoverableAuthException on the first call to
			// GoogleAuthUtil.getToken
			// because the user must consent to offline access to their data.
			// After
			// consent is granted control is returned to your activity in
			// onActivityResult
			// and the second call to GoogleAuthUtil.getToken will succeed.

			startActivityForResult(e.getIntent(), REQUEST_CODE_AUTH_CONSENT);
			// return;
		} catch (GoogleAuthException authEx) {
			// Failure. The call is not expected to ever succeed so it should
			// not be
			// retried.
			Log.e(TAG,
					"Unrecoverable authentication exception : "
							+ authEx.getMessage(), authEx);

			// return;
		} catch (Exception e) {
			Log.e(TAG, "Unhandled exception : " + e.getMessage(), e);
		}
		return code;
	}

	private void doExponentialBackoff() {
		Backoff backoff = new Backoff();
		// Something is stressed out; the auth servers are by definition
		// high-traffic and you can't count on
		// 100% success. But it would be bad to retry instantly, so back off
		if (backoff.shouldRetry()) {
			backoff.backoff();
		}
	}

	private void showErrorDialog(int code) {
		GooglePlayServicesUtil.getErrorDialog(code, this,
				REQUEST_CODE_RECOVER_PLAY_SERVICES).show();
	}

	private void showErrorToast(String message) {
		Toast.makeText(this, message, Toast.LENGTH_SHORT).show();
		finish();
	}

	private boolean checkPlayServices() {
		int status = GooglePlayServicesUtil.isGooglePlayServicesAvailable(this);
		if (status != ConnectionResult.SUCCESS) {
			if (GooglePlayServicesUtil.isUserRecoverableError(status)) {
				showErrorDialog(status);
			} else {
				Toast.makeText(this, "This device is not supported.",
						Toast.LENGTH_LONG).show();
				finish();
			}
			return false;
		}
		return true;
	}

	private void getTokenAsync(String accountName) {
		AsyncTask<String, Void, String> task = new AsyncTask<String, Void, String>() {
			@Override
			protected String doInBackground(String... account) {
				return getToken(account[0]);
			}

			@Override
			protected void onPostExecute(String code) {
				token = code;
				// Log.i(TAG, "Access token retrieved:" + code);
				UnityPlayer.UnitySendMessage(UnityObjectName,
						TokenCallbackName, code);
			}

		};
		task.execute(accountName);
	}

	private void checkAccount() {
		if (accountName == null) {
			GetToken();
		} else {
			getTokenAsync(accountName);
		}
	}

	public void GetToken() {
		if (checkPlayServices()) {
			// Then we're good to go!
			showAccountPicker();
		}
	}

	public void InvalidateToken(){
		if (token == null){
			return;
		}
		try {
			GoogleAuthUtil.clearToken(this, token);
		} catch (GoogleAuthException | IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		token = null;
	}
	
	public void SignIn(){
		Log.i(TAG, "GooglePlusSignIn");
	 	mSignInClicked = true;
	 	if (!mGoogleApiClient.isConnecting()) {
    	      mGoogleApiClient.connect();
	    }
    }
	
	public void SignOut(){
		mSignInClicked = false;
		if (mGoogleApiClient.isConnected()){
			// We clear the default account on sign out so that Google Play
	        // services will not return an onConnected callback without user
	        // interaction.
	        Plus.AccountApi.clearDefaultAccount(mGoogleApiClient);
	        InvalidateToken();
	        mGoogleApiClient.disconnect();
	        mGoogleApiClient.connect();	
		}
        
	}

	public void RevokeAccess(){
		// Prior to disconnecting, run clearDefaultAccount().
		/*Plus.AccountApi.clearDefaultAccount(mGoogleApiClient);
		Plus.AccountApi.revokeAccessAndDisconnect(mGoogleApiClient)
		    .setResultCallback(new ResultCallback<Status>() {

		  onResult(Status status) {
		    // mGoogleApiClient is now disconnected and access has been revoked.
		    // Trigger app logic to comply with the developer policies
		  }

		});*/
	}
	
	private void showAccountPicker() {
		Intent pickAccountIntent = AccountPicker.newChooseAccountIntent(null,
				null, new String[] { GoogleAuthUtil.GOOGLE_ACCOUNT_TYPE },
				true, null, null, null, null);
		startActivityForResult(pickAccountIntent, REQUEST_CODE_PICK_ACCOUNT);
	}

    public String GetProfilePictureUrl(){
    	Person person = Plus.PeopleApi.getCurrentPerson(mGoogleApiClient);
    	if (person != null && person.hasImage()) {
    		Image image = person.getImage();
    		if (image.hasUrl()){
    			return person.getImage().getUrl();
    		}
    	}
		return null;
    }
    
    public String GetDisplayName(){
    	Person person = Plus.PeopleApi.getCurrentPerson(mGoogleApiClient);
    	if (person != null && person.hasDisplayName()) {
    		return person.getDisplayName();
    	}else {
    		return null;
    	}
    }

    public void LoadCircles(){
    	Plus.PeopleApi.loadVisible(mGoogleApiClient, null)
        .setResultCallback(this);
    }
    
    @Override
    public void onResult(LoadPeopleResult peopleData) {
      if (peopleData.getStatus().getStatusCode() == CommonStatusCodes.SUCCESS) {
        String circles = "";
        String contact;
        PersonBuffer personBuffer = peopleData.getPersonBuffer();
        try {
            int count = personBuffer.getCount();
            Log.i(TAG, "TOTAL CONTACTS = " + count);
            for (int i = 0; i < count; i++) {
            	contact = personBuffer.get(i).getId()+"|"+personBuffer.get(i).getDisplayName()+"|";
            	if (personBuffer.get(i).getImage().hasUrl()){
            		contact+=personBuffer.get(i).getImage().getUrl();
            	}
            	circles += contact + '\n';   
            }
        } finally {
            personBuffer.release();
        }
        UnityPlayer.UnitySendMessage(UnityObjectName, CirclesCallbackName, circles);
      } else {
        Log.e(TAG, "Error requesting visible circles: " + peopleData.getStatus());
      }
    }
    
	@Override
	public void onConnectionFailed(ConnectionResult result) {
		int errorCode = result.getErrorCode();
    	// Refer to the javadoc for ConnectionResult to see what error codes might
        // be returned in onConnectionFailed.
        Log.i(TAG, "onConnectionFailed: ConnectionResult.getErrorCode() = "
                + errorCode);
        
        switch (errorCode){
	        case ConnectionResult.API_UNAVAILABLE:
	        case ConnectionResult.INTERNAL_ERROR:
	        case ConnectionResult.CANCELED:
	        case ConnectionResult.DEVELOPER_ERROR:
	        case ConnectionResult.INTERRUPTED:
	        case ConnectionResult.SERVICE_MISSING:
	        case ConnectionResult.SERVICE_DISABLED:
	        case ConnectionResult.SERVICE_INVALID:
	        case ConnectionResult.SERVICE_VERSION_UPDATE_REQUIRED:
	        case ConnectionResult.NETWORK_ERROR:
	        case ConnectionResult.LICENSE_CHECK_FAILED:
	        case ConnectionResult.INVALID_ACCOUNT:
	        	if (GooglePlayServicesUtil.isUserRecoverableError(errorCode)) {
	        		//showErrorDialog();
	        	}
	        	break;
	        case ConnectionResult.SIGN_IN_REQUIRED:
	        case ConnectionResult.RESOLUTION_REQUIRED:
        		lastConnectionResult = result;
        		if (mSignInClicked) startResolution();
	        	break;
        }
		
	}

	private void startResolution() {
		if (lastConnectionResult!=null /*&& lastConnectionResult.hasResolution()*/ && !mIntentInProgress){
			Log.i(TAG, "RESOLVING");
			PendingIntent mSignInIntent = lastConnectionResult.getResolution();
			try {
				mIntentInProgress = true;
				startIntentSenderForResult(mSignInIntent.getIntentSender(),
	        		REQUEST_CODE_SIGN_IN, null, 0, 0, 0);
			} catch (SendIntentException e) {
		        Log.i(TAG, "Sign in intent could not be sent: "
		            + e.getMessage());
		        mIntentInProgress = false;
		        mGoogleApiClient.connect();
			}
		}else {
			mIntentInProgress = false;
			mGoogleApiClient.connect();
		}
	}
	
	@Override
	public void onConnected(Bundle connectionHint) {
		//getTokenAsync(Plus.AccountApi.getAccountName(mGoogleApiClient));
		UnityPlayer.UnitySendMessage(UnityObjectName,
				SignInCallbackName, Plus.PeopleApi.getCurrentPerson(mGoogleApiClient).getId());
	}
	
	@Override
	public void onConnectionSuspended(int cause) {
		Log.i(TAG, "onConnectionSuspended, cause : " + cause);
    	switch (cause){
	    	case ConnectionCallbacks.CAUSE_NETWORK_LOST:
	    		break;
	    	case ConnectionCallbacks.CAUSE_SERVICE_DISCONNECTED:
	    		break;
    	}
    	mGoogleApiClient.connect();
    	UnityPlayer.UnitySendMessage(UnityObjectName,
				SignInCallbackName, ""+cause);
	}
	
	 static class Backoff {

	        private static final long INITIAL_WAIT = 1000 + new Random().nextInt(1000);
	        private static final long MAX_BACKOFF = 1800 * 1000;

	        private long mWaitInterval = INITIAL_WAIT;
	        private boolean mBackingOff = true;

	        public boolean shouldRetry() {
	            return mBackingOff;
	        }

	        private void noRetry() {
	            mBackingOff = false;
	        }

	        public void backoff() {
	            if (mWaitInterval > MAX_BACKOFF) {
	                noRetry();
	            } else if (mWaitInterval > 0) {
	                try {
	                    Thread.sleep(mWaitInterval);
	                } catch (InterruptedException e) {
	                    // life's a bitch, then you die
	                }
	            }

	            mWaitInterval = (mWaitInterval == 0) ? INITIAL_WAIT : mWaitInterval * 2;
	        }
	    }
}

