using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;

public class AudioLoader : Singleton<AudioLoader>, StartupProcess
{
    // used when the server returns audio data to a client so the client knows which UI element to add the audio to
    private Dictionary<int, AudioPlayerUI> audioSources = new Dictionary<int, AudioPlayerUI>();

    public void Awake()
    {
        ApplicationManager.Instance.onStartupProcess.AddListener(onStartupProcess);
    }

    public void onStartupProcess()
    {
        NetworkManager.Singleton.OnServerStarted += () =>
        {
            if (NetworkManager.Singleton.IsServer)
                registerMessages();
        };
        //Register lambda event that executes when the a client connects to the server.
        NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientID) =>
        {
            if (NetworkManager.Singleton.IsClient && NetworkManager.Singleton.LocalClientId == clientID)
                registerMessages();
        };
    }

    public void registerMessages()
    {
        //custom messages for transferring audio data across the network
        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("audioFileServerRequest", audioFileServerRequest);
        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("audioFileClientMessage", audioFileClientMessage);
    }

    /// <summary>
    /// Called on the client device to initiate an audio load request to the server.
    /// </summary>
    /// <param name="audioFileName">File name of audio clip to load.</param>
    /// <param name="associatedAudioPlayer">The AudioPlayerUI element that the returned audio data should be sent to.</param>
    public void requestAudioFromServer(string audioFileName, AudioPlayerUI associatedAudioPlayer, NetworkInteractableLookupData lookupData)
    {
        NetworkAudioRequest audioRequest = new NetworkAudioRequest
        { 
            audioPlayerID = associatedAudioPlayer.GetInstanceID(),
            audioFilePath = audioFileName,
            lookupData = lookupData
        };
        
        audioSources[associatedAudioPlayer.GetInstanceID()] = associatedAudioPlayer;

        var writeSize = FastBufferWriter.GetWriteSize(audioRequest.audioFilePath) +
            FastBufferWriter.GetWriteSize(audioRequest.audioPlayerID) +
            FastBufferWriter.GetWriteSize(audioRequest.lookupData);

        //create a new writer that will pack the file name into a network message payload
        var writer = new FastBufferWriter(writeSize, Allocator.Temp);

        using (writer)
        {
            DebugConsole.Instance.LogDebug("Requesting audio data from server.");

            //pack the spawn request into a message payload
            writer.WriteValueSafe(audioRequest);

            DebugConsole.Instance.LogDebug(writer.ToString());

            //send request to the server
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage
                (
                    "audioFileServerRequest", NetworkManager.ServerClientId, writer, NetworkDelivery.Reliable
                );

            DebugConsole.Instance.LogDebug(writer.ToString());
        }

    }

    /// <summary>
    /// Called from client on the server/host device. Loads audio data from disk and gives it back to the caller.
    /// </summary>
    /// <param name="senderId">Client to send audio to.</param>
    /// <param name="messagePayload">Contains the filename of audio to load.</param>
    private void audioFileServerRequest(ulong senderId, FastBufferReader messagePayload)
    {
        DebugConsole.Instance.LogDebug($"client({senderId}) requested an audio annotation from server({NetworkManager.Singleton.LocalClientId})");
        NetworkAudioRequest audioRequest;

        //read the data from the payload and output it
        messagePayload.ReadValueSafe(out audioRequest);

        string audioFilePath = $"file://{audioRequest.audioFilePath}";

        StartCoroutine(loadAudioCoroutine(audioFilePath, audioRequest, senderId));

    }

    private IEnumerator loadAudioCoroutine(string audioFilePath, NetworkAudioRequest audioRequest, ulong senderId)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(audioFilePath, AudioType.UNKNOWN))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                //get the audio clip from the handler
                AudioClip loadedClip = DownloadHandlerAudioClip.GetContent(www);

                //convert AudioClip into float array, from Unity scripting docs for AudioClip.GetData
                int numSamples = loadedClip.samples * loadedClip.channels;
                float[] audioData = new float[numSamples];
                loadedClip.GetData(audioData, 0);

                // pack the audio data with other required information for transfer over network
                NetworkAudioResponse response = new NetworkAudioResponse(audioRequest.audioPlayerID, audioRequest.audioFilePath, audioData, loadedClip.channels);

                // get size of data to be sent over network
                var writeSize = FastBufferWriter.GetWriteSize(response.audioFilePath) +
                    FastBufferWriter.GetWriteSize(response.audioPlayerID) +
                    FastBufferWriter.GetWriteSize(response.audioData) +
                    FastBufferWriter.GetWriteSize(response.numChannels);

                // send data to client
                var writer = new FastBufferWriter(writeSize, Allocator.Temp);
                using (writer)
                {
                    DebugConsole.Instance.LogDebug("Sending audio data back to client.");
                    //write the audio request response to the writer
                    writer.WriteValueSafe(response);
                    //send request to the client
                    NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage
                        (
                            "audioFileClientMessage", senderId, writer, NetworkDelivery.ReliableFragmentedSequenced
                        );
                }
            }
            else
            {
                DebugConsole.Instance.LogError($"Failed to load audio clip using path {audioFilePath}\nerror: {www.error}");
            }

            //if the annotation was added to the current selection on the server make sure to attach the audio clip to the UI
            if (SelectionManager.Instance.currentSelection.transform == MessageBasedInstanceManager.Instance.lookupNetworkInteractable(audioRequest.lookupData.parentKey, audioRequest.lookupData.objectIndex).transform)
            {
                DataPanelManager.Instance.onNewSelection(SelectionManager.Instance.currentSelection.transform);
            }

        }
    }

    /// <summary>
    /// Called from the server on the client device to return requested audio data.
    /// </summary>
    /// <param name="senderId"></param>
    /// <param name="messagePayload"></param>
    private void audioFileClientMessage(ulong senderId, FastBufferReader messagePayload)
    {
        // get network data 
        NetworkAudioResponse networkResponse;
        messagePayload.ReadValueSafe(out networkResponse);

        // convert raw audio data into AudioClip
        float[] audioData = networkResponse.audioData;
        int numSamples = audioData.Length / networkResponse.numChannels;

        try
        {
            //convert network data to AudioClip
            AudioClip clip = AudioClip.Create("", numSamples, networkResponse.numChannels, GlobalConstants.SAMPLE_RATE, false);
            clip.SetData(audioData, 0);

            //put AudioClip into the UI element that requested the data
            AudioPlayerUI audioPlayer = audioSources[networkResponse.audioPlayerID];
            audioPlayer.setAudioSource(clip);
        }
        catch(System.Exception e)
        {
            DebugConsole.Instance.LogError($"Unable to create audio clip using data retrieved from server.\n{e.ToString()}");
        }
    }

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
}

/// <summary>
/// Sent from client to server to request audio data.
/// </summary>
public struct NetworkAudioRequest : INetworkSerializable
{
    public int audioPlayerID;
    public string audioFilePath;
    public NetworkInteractableLookupData lookupData;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref audioPlayerID);
        serializer.SerializeValue(ref audioFilePath);
        serializer.SerializeValue(ref lookupData);
    }
}

/// <summary>
/// Returned to client from server to provide requested audio data.
/// </summary>
public struct NetworkAudioResponse : INetworkSerializable
{
    public int audioPlayerID;
    public string audioFilePath;
    public float[] audioData;
    public int numChannels;

    public NetworkAudioResponse(int audioPlayerID, string audioFilePath, float[] audioData, int numChannels)
    {
        this.audioPlayerID = audioPlayerID;
        this.audioFilePath = audioFilePath;
        this.audioData = audioData;
        this.numChannels = numChannels;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref audioPlayerID);
        serializer.SerializeValue(ref audioFilePath);
        serializer.SerializeValue(ref audioData);
        serializer.SerializeValue(ref numChannels);
    }
}