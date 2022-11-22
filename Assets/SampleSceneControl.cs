using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class SampleSceneControl : MonoBehaviour
{
    private void Start()
    {
        string imageFolder = Application.dataPath + "/Images";
        List<string> imageFileNames = Directory.GetFiles(imageFolder)
            .Where(fileName => fileName.EndsWith(".png"))
            .ToList();

        int index = 1;
        imageFileNames.ForEach(fileName =>
        {
            TestLoadTextureFromUri(fileName, index);
            index++;
        });
    }

    private void TestLoadTextureFromUri(string absolutePath, int index)
    {
        Debug.Log($"Absolute path: {absolutePath}");
        Debug.Log("File exists: " + File.Exists(absolutePath));
        if (!File.Exists(absolutePath))
        {
            return;
        }

        string uri = "file://" + absolutePath;
        StartCoroutine(LoadTextureFromFile(uri, index));
    }

    private IEnumerator LoadTextureFromFile(string uri, int index)
    {
        Debug.Log($"URI: {uri} has index {index}");

        using UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(uri);
        DownloadHandlerTexture downloadHandler = webRequest.downloadHandler as DownloadHandlerTexture;
        webRequest.SendWebRequest();

        while (!webRequest.isDone)
        {
            Debug.Log($"Waiting for the web request {index} to finish");
            yield return null;
        }

        if (webRequest.result
            is UnityWebRequest.Result.ConnectionError
            or UnityWebRequest.Result.ProtocolError
            or UnityWebRequest.Result.DataProcessingError)
        {
            Debug.LogError($"Web request {index} had error: {webRequest.error}");
            Debug.LogError($"Web request {index} download handler error: {downloadHandler.error}");
            yield break;
        }

        Texture2D texture = downloadHandler.texture;
        Debug.Log($"Web request {index} finished successfully! Width: {texture.width}, height: {texture.height}");

        // Destroy texture, this was just for testing.
        Destroy(texture);
    }
}