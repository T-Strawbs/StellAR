using System;

/// <summary>
/// This is an interface for Classes that need initialised in a set sequence before the system has started.  
/// Mostly used for marking the derrived class as StartupProcess so that we can keep tabs on whats what.
/// 
/// Dont forget to subscribe to the Application Managers events in the awake method:
/// 
/// private void Awake()
/// {
///     ApplicationManager.Instance.onStartupProcess.AddListener(onStartupProcess);
/// }
/// </summary>
public interface StartupProcess
{
    /// <summary>
    /// Register event listners for NetworkManager events
    /// </summary>
    public void onStartupProcess();
}