using System.Collections;
using UnityEngine;

public class ContactInfo : ListItemInfo {
    public const string ID = "ID";
    private string id;

    public string Id {
        get {
            return id;
        }
        set {
            if (id != null && id.Equals(value)) {
                return;
            }
            id = value;
            OnPropertyChanged(ID);
        }
    }

    public const string NAME = "NAME";
    private string displayName;

    public string DisplayName {
        get {
            return displayName;
        }
        set {
            if (displayName != null && displayName.Equals(value)) {
                return;
            }
            displayName = value;
        }
    }

    public const string PROFILE_PICTURE_URL = "PROFILE_PICTURE_URL";
    private string profilePictureUrl;

    public string ProfilePictureUrl {
        get { return profilePictureUrl; }
        set {
            if (profilePictureUrl != null && profilePictureUrl.Equals(value)) {
                return;
            }
            profilePictureUrl = value;
            OnPropertyChanged(PROFILE_PICTURE_URL);
        }
    }

    public void OnProfilePictureDownloaded(Texture2D texture) {
        ProfilePicture = texture;
    }

    public const string PROFILE_PICTURE = "PROFILE_PICTURE";

    private Texture2D profilePicture = null;

    public Texture2D ProfilePicture {
        get { return profilePicture; }
        set {
            if (profilePicture == value) {
                return;
            }
            profilePicture = value;
            OnPropertyChanged(PROFILE_PICTURE);
        }
    }
}