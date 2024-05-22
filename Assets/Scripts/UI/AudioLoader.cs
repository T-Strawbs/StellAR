using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class AudioLoader : Singleton<AudioLoader>
{
    public void loadAudio(System.Action<AudioClip,string> onloadComplete, string audioPath)
    {
        StartCoroutine(loadAudioCoroutine(onloadComplete, audioPath));
    }
    private IEnumerator loadAudioCoroutine(System.Action<AudioClip,string> onloadComplete, string audioPath)
    {
        string audioFilePath = $"file://{audioPath}";

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(audioFilePath, AudioType.UNKNOWN))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                //get the audio clip from the handler
                AudioClip loadledClip = DownloadHandlerAudioClip.GetContent(www);
                //invoke the onLoadComplete delegate
                onloadComplete?.Invoke(loadledClip,audioPath);
            }
            else
            {
                Debug.LogError($"Failed to load audio clip using path {audioFilePath}\nerror: {www.error}");
            }
        }
    }
}
