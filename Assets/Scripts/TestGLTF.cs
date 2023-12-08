using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[Obsolete("This is just a test script which requires the GltUtility package")]
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

                // TODO:
                // to use the code below, we need to import the GLTFUtility package
                // and remove the gltFast package, and also include the namespace
                /*
                 var avatar = Importer.LoadFromBytes(webRequest.downloadHandler.data);

                 using Siccity.GLTFUtility;
                 */
                break;
            case UnityWebRequest.Result.InProgress:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}