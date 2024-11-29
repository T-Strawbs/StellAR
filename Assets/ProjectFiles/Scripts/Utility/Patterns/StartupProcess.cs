using System;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

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
    /// Register event listners for behaviour that needs to occur at the very start of the application
    /// </summary>
    public void onStartupProcess();
}
