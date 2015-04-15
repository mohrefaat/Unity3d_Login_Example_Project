using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DownloadManager : Singleton<DownloadManager> {

    public void DownloadTextureAsync(string url, Action<Texture2D> callback) {
        StartCoroutine(DownloadProfilePicture(url, callback));
    }

    private WWW imageDownloader = null;

    private IEnumerator DownloadProfilePicture(string url, Action<Texture2D> callback) {
        if (url.isValidUrl()) {
            while (imageDownloader != null) {
                yield return null;
            }
            //Utils.Log("Getting photo from URL = {0}", url);
            imageDownloader = new WWW(url);//WWW.LoadFromCacheOrDownload(url, 1);//
            yield return imageDownloader;
            if (!string.IsNullOrEmpty(imageDownloader.error)) {
                Utils.Log("error : {0} retrieving URL = {1}", imageDownloader.error, url);
            }
            else {
                Texture2D temp = new Texture2D(imageDownloader.texture.width,
                    imageDownloader.texture.height, TextureFormat.DXT1, false); //TextureFormat must be DXT5 or DXT1 ??
                //textFb2 = imageDownloader.texture;
                imageDownloader.LoadImageIntoTexture(temp);
                callback(temp);
                //texture = imageDownloader.texture;
            }
            imageDownloader.Dispose();
            imageDownloader = null;
        }
        else {
            Utils.Log("invalid url {0}", url);
        }
    }
}