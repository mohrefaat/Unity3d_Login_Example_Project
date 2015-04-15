using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public static class ExtensionMethods {

    public static void SetText(this Text text, object t) {
        text.text = t.ToString();
    }

    public static void SetText(this Text text, string format, params object[] objects) {
        text.text = string.Format(format, objects);
    }

    public static void SetTexture2D(this Image image, Texture2D avatarTexture, float width, float height) {
        if (avatarTexture == null || width < 0 || height < 0) {
            return;
        }
        image.sprite =
            Sprite.Create(
                avatarTexture,
                new Rect(0, 0, width, height),
                Vector2.zero
            );
    }

    public static bool isValidUrl(this string source) {
        if (string.IsNullOrEmpty(source)) {
            return false;
        }
        Uri uriResult;
        return Uri.IsWellFormedUriString(source, UriKind.Absolute)
            && Uri.TryCreate(source, UriKind.Absolute, out uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}