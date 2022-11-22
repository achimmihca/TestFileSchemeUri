using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

public class SampleSceneControl : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    private void Start()
    {
        // Test image files
        // An image file with a space character AND a plus character does not work.
        string imageFolder = Application.dataPath + "/Images";
        List<string> imageFilePaths = Directory.GetFiles(imageFolder)
            .Where(fileName => fileName.EndsWith(".png"))
            .ToList();
        imageFilePaths.ForEach(imageFilePath => TestLoadTextureFromUri(imageFilePath));

        // Test audio files
        // An audio file with a space character AND a plus character does not work.
        string audioFolder = Application.dataPath + "/Audio";
        List<string> audioFilePaths = Directory.GetFiles(audioFolder)
            .Where(fileName => fileName.EndsWith(".ogg"))
            .ToList();
        audioFilePaths.ForEach(audioFilePath => TestLoadAudioFromUri(audioFilePath));

        // Test videos
        // All files work.
        string videoFolder = Application.dataPath + "/Video";
        List<string> videoFilePaths = Directory.GetFiles(videoFolder)
            .Where(fileName => fileName.EndsWith(".webm"))
            .ToList();
        videoPlayer.url = "file://" + videoFilePaths
            .FirstOrDefault(fileName => fileName.Contains("+")
                                        && fileName.Contains(" "));
    }

    private void TestLoadTextureFromUri(string absolutePath)
    {
        Debug.Log($"Absolute path: {absolutePath}");
        Debug.Log("File exists: " + File.Exists(absolutePath));
        if (!File.Exists(absolutePath))
        {
            return;
        }

        string uri = "file://" + absolutePath;
        StartCoroutine(LoadTextureFromFile(uri));
    }

    private IEnumerator LoadTextureFromFile(string uri)
    {
        using UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(uri);
        DownloadHandlerTexture downloadHandler = webRequest.downloadHandler as DownloadHandlerTexture;
        webRequest.SendWebRequest();

        while (!webRequest.isDone)
        {
            Debug.Log($"Waiting for the web request {uri} to finish");
            yield return null;
        }

        if (webRequest.result
            is UnityWebRequest.Result.ConnectionError
            or UnityWebRequest.Result.ProtocolError
            or UnityWebRequest.Result.DataProcessingError)
        {
            Debug.LogError($"Web request {uri} had error: {webRequest.error}");
            Debug.LogError($"Web request {uri} download handler error: {downloadHandler.error}");
            yield break;
        }

        Texture2D texture = downloadHandler.texture;
        Debug.Log($"Web request {uri} finished successfully! Image width: {texture.width}, height: {texture.height}");

        // Destroy object, this was just for testing.
        Destroy(texture);
    }

    private void TestLoadAudioFromUri(string absolutePath)
    {
        Debug.Log($"Absolute path: {absolutePath}");
        Debug.Log("File exists: " + File.Exists(absolutePath));
        if (!File.Exists(absolutePath))
        {
            return;
        }

        string uri = "file://" + absolutePath;
        StartCoroutine(LoadAudioFromFile(uri));
    }

    private IEnumerator LoadAudioFromFile(string uri)
    {
        using UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.OGGVORBIS);
        DownloadHandlerAudioClip downloadHandler = webRequest.downloadHandler as DownloadHandlerAudioClip;
        webRequest.SendWebRequest();

        while (!webRequest.isDone)
        {
            Debug.Log($"Waiting for the web request {uri} to finish");
            yield return null;
        }

        if (webRequest.result
            is UnityWebRequest.Result.ConnectionError
            or UnityWebRequest.Result.ProtocolError
            or UnityWebRequest.Result.DataProcessingError)
        {
            Debug.LogError($"Web request {uri} had error: {webRequest.error}");
            Debug.LogError($"Web request {uri} download handler error: {downloadHandler.error}");
            yield break;
        }

        AudioClip audioClip = downloadHandler.audioClip;
        Debug.Log($"Web request {uri} finished successfully! Audio length: {audioClip.length}");

        // Destroy object, this was just for testing.
        Destroy(audioClip);
    }
}