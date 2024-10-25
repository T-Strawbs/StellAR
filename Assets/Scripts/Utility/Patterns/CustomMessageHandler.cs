using System;

/// <summary>
/// This is an interface for NGO Custom message Handlers that we make and is mostly used as a way to mark
/// the derrived class as CustomMessageHandler so that we can keep tabs on whats what.
/// 
/// Dont forget to subscribe to the Application Managers events in the awake method:
/// 
///  ApplicationManager.Instance.onProcessCustomMessengers.AddListener(registerNetworkEventListeners);
/// 
/// </summary>
public interface CustomMessageHandler
{
    /// <summary>
    /// Register event listners for NetworkManager events
    /// </summary>
    public void registerNetworkEventListeners();
    /// <summary>
    /// Register the message signatures with the NetworkMangers CustomMessageManager
    /// </summary>
    public void registerMessages();
}
