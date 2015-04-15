#PlayFab Login Example Readme (v1.51)
====
This repo exits to inform our developers and customers about the many PlayFab-supported login flows as well as the best practices around how to use them.

## Key Repository Components
----
### PlayFab Components 
  * PlayFab Unity SDK -- Our standard Unity package. Provides C# wrapper classes for accessing our web API from within Unity.
  * PlayFab Samples -- Contains all of the assets and scripts needed for running this sample. Start with "Assets/PlayFabSamples/Scenes/LoginFlow.unity".


### 3rd Party
  * Google_Play_Games Plugin (v6.1.7-000) -- The Google Play Games plugin for Unity allows you to access the Google Play Games API through Unity's social interface.
  * Facebook Unity Plugin (v6.2.1) -- The Facebook SDK for Unity provides a comprehensive collection of Facebook's social features, giving players of your Unity game the ability to share content with their friends and allowing you to create a personal, social gaming experience.
  * GooglePlusForUnity Plugin -- A custom Android plugin for obtaining the Google auth token contributed by Hamza Lazâar, an active developer in the PlayFab community.


## Installation & Configuration Instructions
----
This repo contains the raw Unity3d (v4.6.3) project. 

#### What to know before running the project:
1. This demo is intended to illustrate generic best practices around authenticating players with PlayFab's supported login flows. 
2. While not exclusively relevant for mobile, this demo makes heavy use of device IDs (which do not have an equivalent on PC / Mac / Web)
3. All authentication done through PlayFab revolves around the concept of the PlayFab account. 
  * These accounts can be:
  	* Partial (A.K.A. guest accounts) - These accounts do not have complete player information (username, password, email). These accounts are created when the CreateAccount parameter is true when calling any of our LogInWith____() APIs ([Facebook Example](https://api.playfab.com/Documentation/Client/method/LoginWithFacebook)).
    * Complete -  These are accounts have a username, password and email. 
  * Multiple authentication pathways can be linked to a PlayFab account. This is a powerful feature allowing a player to easily play and retain their player information across platforms. 
    * Example: When linked, a player can login using Facebook, Google+, email, or GameCenter and be assured that his or her player data persists across the various pathways. 
    * Documentation on linking accounts can be found [here](https://api.playfab.com/Documentation/Client/method/LinkFacebookAccount).

#### Getting up and running:
1. Clone the repo to your local machine and open the directory with Unity. LoginFlow is the primary scene in this example. 
2. Open the LoginFlow scene and build /run the project.  
3. This scene will take you through the following flow:
  1. Check for the saved login information & set the last used pathway to login automatically.
    * This prompts for passwords (pf account), we do NOT condone storing passwords.
  2. If no previous accounts were found and the platform is mobile automatically log in after a short countdown (this countdown is primarily for this demo, most games activate this instantly).
  3. Check to see if device ID is valid ( use login using createaccount = false).
  4. If countdown is not canceled & device id is new ( use login using createaccount = true ).
  5. Provide manual login with all available options for the platform (saves pathway for next time).
  6. Provide a registration field for new PlayFab accounts (automatically login after account creation);

## Copyright and Licensing Information
----
  Apache License
  Version 2.0, January 2004
  http://www.apache.org/licenses/

  License Details available in LICENSE.txt


## Known Bugs and Troubleshooting
----
This game has been tested on PC & Mac native / PC & Mac Web / Android (4.4+) & iOS 8.0+. Other Unity-compatible platforms should also function. Please let us know if you run into any bugs, especially ones that appear to be platform specific. 

  * No examples for Steam & GameCenter. This will be coming in a future version. 
  * Facebook -- To make this work you will need to connect this project to your Facebook game. We have included the Facebook plugin to make this easy; simply update your Facebook settings with their editor tools (Facebook > Edit Settings )
  *


## Special Thanks to Hamza Lazâar 
#####(Check out his Twitter @RealJohnTube) 
----
Hamza Lazâar has generously shared sample code for showing how to create a GooglePlay plugin that returns a google+ authentication token. This plugin provides the needed information for using LoginWithGoogleAccount on Android devices. 

His contributions include:
+ Assets/PlayFabSamples/Login/Scripts/HamzaScripts
+ Plugins/Android/GooglePlusForUnity 


## Contact Us
----
Do you have ideas on how we can make our products and services better? 

We love to hear from our developer community! 

Our Developer Success Team can assist with answering any questions as well as process any feedback you have about PlayFab.
mailto:devrel@playfab.com.


## Changelog
----
* 4/13/15 -- Initial Public Release (v1.52)

## Coming in Future Versions
----
-*- Coming soon -*-
* Steam Authentication Example
* Account Pathway linking and unlinking
* Integrated PlayFab Plugin examples
* Hamza Lazâar's 2.0 Google+ plugin
-*-


