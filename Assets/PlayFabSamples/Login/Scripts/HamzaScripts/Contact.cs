﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Contact : ListItem {

    [SerializeField]
    private Image ProfilePictureImage;

    [SerializeField]
    private Text DisplayNameText;

    protected override void SetDefaultUI() {
        ContactInfo info = (ContactInfo)ItemInfo;
        DisplayNameText.text = info.DisplayName;
        if (info.ProfilePicture == null) {
            ProfilePictureImage.sprite = new Sprite(); // set default avatar instead
            DownloadManager.Instance.DownloadTextureAsync(info.ProfilePictureUrl, info.OnProfilePictureDownloaded);
        }
        else {
            ProfilePictureImage.SetTexture2D(info.ProfilePicture,
                ContactInfo.PROFILE_PICTURE_DIMENSION,
                ContactInfo.PROFILE_PICTURE_DIMENSION);
        }
    }

    protected override void UpdateUI(object sender, System.ComponentModel.PropertyChangedEventArgs args) {
        ContactInfo info = (ContactInfo)ItemInfo;
        switch (args.PropertyName) {
            case ContactInfo.PROFILE_PICTURE:
                ProfilePictureImage.SetTexture2D(info.ProfilePicture,
                    ContactInfo.PROFILE_PICTURE_DIMENSION,
                    ContactInfo.PROFILE_PICTURE_DIMENSION);
                break;
        }
    }
}