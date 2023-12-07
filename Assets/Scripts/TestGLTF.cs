using System;
using System.Collections;
using Siccity.GLTFUtility;
using UnityEngine;
using UnityEngine.Networking;

public class TestGltf : MonoBehaviour
{
    private const string Path = "https://models.readyplayer.me/6570ff98869b42cd90a10bb6.glb";

    private void Start()
    {
        StartCoroutine(DownloadAvatar(Path));
    }

    private static IEnumerator DownloadAvatar(string url)
    {
        using var webRequest = UnityWebRequest.Get(url);
        yield return webRequest.SendWebRequest();

        switch (webRequest.result)
        {
            case UnityWebRequest.Result.ConnectionError:
            case UnityWebRequest.Result.DataProcessingError:
                Debug.LogError("Error: " + webRequest.error);
                break;
            case UnityWebRequest.Result.ProtocolError:
                Debug.LogError("HTTP Error: " + webRequest.error);
                break;
            case UnityWebRequest.Result.Success:
                Debug.Log("nReceived: " + webRequest.downloadHandler.data.Length);
                var avatar = Importer.LoadFromBytes(webRequest.downloadHandler.data);
                break;
            case UnityWebRequest.Result.InProgress:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}