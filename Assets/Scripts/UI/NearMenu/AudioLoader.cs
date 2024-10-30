using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;

public class AudioLoader : NetworkSingleton<AudioLoader>
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
                DebugConsole.Instance.LogError($"Failed to load audio clip using path {audioFilePath}\nerror: {www.error}");
            }
        }
    }

    [Rpc(SendTo.Server)]
    public void loadAudioOnlineServerRpc(string audioPath, ulong clientId)
    {
        string audioFilePath = $"file://{audioPath}";

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(audioFilePath, AudioType.UNKNOWN))
        {
            www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                //get the audio clip from the handler
                AudioClip loadledClip = DownloadHandlerAudioClip.GetContent(www);

                //convert AudioClip into float array, from Unity scripting docs for AudioClip.GetData
                var numSamples = loadledClip.samples * loadledClip.channels;
                float[] samples = new float[numSamples];
                loadledClip.GetData(samples, 0);

                //NOW YOU CAN SEND THE CLIP BACK TO THE CLIENT THAT REQUESTED IT

            }
            else
            {
                DebugConsole.Instance.LogError($"Failed to load audio clip using path {audioFilePath}\nerror: {www.error}");
            }
        }
    }
}
