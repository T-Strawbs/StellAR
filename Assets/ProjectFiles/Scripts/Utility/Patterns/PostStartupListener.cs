using System;
using Unity;

/// <summary>
/// Interface for Derrived class that need to be subscribed to the onPostStartProcess event of the
/// ApplicationManager
/// 
/// Make sure to add the onPostStartup callback as a listener of the onPostStartProcess event
/// in the awake method.
/// 
/// </summary>
public interface PostStartupListener
{
    /// <summary>
    /// callback to be executed when the onPostStartProcess event is invoked.
    /// </summary>
    public void onPostStartup();
}