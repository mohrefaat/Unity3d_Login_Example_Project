PlayFab Unity package

Thanks for installing the PlayFab Unity package! You're just two steps away from completing your integration:

Step 1) Create a title on PlayFab

Sign up at https://developer.playfab.com and create a new game. (See https://playfab.com/getting-started for detailed instructions.) Once your game is set up, go to the Properties tab in the PlayFab Game Manager and copy the Title ID assigned to your game.

Step 2) Add the PlayFab Manager to your Unity project

In Unity, switch to your project's loading or startup scene. From the Project tab, drag the Assets/PlayFabSDK/Manager/PlayFabManager prefab into your scene's hierarchy window to add it to the scene. Next, click on the properties inspector for the new PlayFab Manager object and paste in your PlayFab Title ID.

That's it! PlayFab is now ready to start working for your project, and you're ready to start using any of the API calls in our library to enhance your game. See https://api.playfab.com/Documentation/Client to start learning about our client side APIs.