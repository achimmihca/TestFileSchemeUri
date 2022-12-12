using System;
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
        // Test shared network folder.
        // In this example "AAS-LAPTOP" is the Windows computer name of the local machine.
        // Note that "localhost" and "127.0.0.1" did not work for some reason in my tests.
        var networkImagePaths = new List<string>()
        {
            @"//AAS-LAPTOP/Users/andre/Downloads/NetTest/cover.jpg",
            @"\\AAS-LAPTOP\Users\andre\Downloads\NetTest\cover.jpg",
        };
        networkImagePaths.ForEach(networkImagePath => TestLoadTextureFromAbsoluteFilePath(networkImagePath));

        var networkAudioPaths = new List<string>()
        {
            @"//AAS-LAPTOP/Users/andre/Downloads/NetTest/audio.mp3",
            @"\\AAS-LAPTOP\Users\andre\Downloads\NetTest\audio.mp3",
        };
        networkAudioPaths.ForEach(networkAudioPath => TestLoadAudioFromAbsoluteFilePath(networkAudioPath));

        // Test image files
        // An image file with a space character AND a plus character only works when wrapping it in a System.Uri object.
        string imageFolder = Application.dataPath + "/Images";
        List<string> imageFilePaths = Directory.GetFiles(imageFolder)
            .Where(fileName => fileName.EndsWith(".png"))
            .ToList();
        imageFilePaths.ForEach(imageFilePath => TestLoadTextureFromAbsoluteFilePath(imageFilePath));

        // Test audio files
        // An audio file with a space character AND a plus character only works when wrapping it in a System.Uri object.
        string audioFolder = Application.dataPath + "/Audio";
        List<string> audioFilePaths = Directory.GetFiles(audioFolder)
            .Where(fileName => fileName.EndsWith(".ogg"))
            .ToList();
        audioFilePaths.ForEach(audioFilePath => TestLoadAudioFromAbsoluteFilePath(audioFilePath));

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

    private void TestLoadTextureFromAbsoluteFilePath(string absolutePath)
    {
        Debug.Log($"Absolute path: {absolutePath}");
        Debug.Log("File exists: " + File.Exists(absolutePath));
        if (!File.Exists(absolutePath))
        {
            return;
        }

        string uri = AbsoluteFilePathToUri(absolutePath);
        StartCoroutine(LoadTextureFromUri(uri));
    }

    private IEnumerator LoadTextureFromUri(string uri)
    {
        using UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(new Uri(uri));
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

    private void TestLoadAudioFromAbsoluteFilePath(string absolutePath)
    {
        Debug.Log($"Absolute path: {absolutePath}");
        Debug.Log("File exists: " + File.Exists(absolutePath));
        if (!File.Exists(absolutePath))
        {
            return;
        }

        string uri = AbsoluteFilePathToUri(absolutePath);
        StartCoroutine(LoadAudioFromFile(uri));
    }

    private IEnumerator LoadAudioFromFile(string uri)
    {
        using UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(new Uri(uri), AudioType.UNKNOWN);
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

    private static string AbsoluteFilePathToUri(string absolutePath)
    {
        string uri;
        if (absolutePath.StartsWith(@"\\"))
        {
            // This is a network path.
            // MUST prefix it with the file:// scheme AND an additional slash for Unity API to work.
            // See https://forum.unity.com/threads/unitywebrequest-and-local-area-network.714353/
            return "file:///" + absolutePath;
        }

        if (absolutePath.StartsWith("//"))
        {
            // This also is a network path. But because forward slashes are used, MUST prefix it with the file:// scheme ONLY for Unity API to work.
            return "file://" + absolutePath;
        }

        // This is a local path. MUST NOT prefix it with the file:// scheme.
        // Otherwise some paths may not work, e.g., when it contains a space AND a plus character.
        // See https://forum.unity.com/threads/unitywebrequest-file-protocol-not-working-with-plus-character-in-path-how-to-escape-the-uri.1364499/#post-8655012
        return absolutePath;
    }
}