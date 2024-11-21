using System;
using Unity;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Interface for Derrived classes that need to be subscribed to the onPostStartProcess event of the
/// ApplicationManager
/// 
/// Dont forget to subscribe to the Application Managers events in the awake method:
/// 
/// private void Awake()
/// {
///     ApplicationManager.Instance.onStartupProcess.AddListener(onStartupProcess);
/// }
/// </summary>
public interface PostStartupListener
{
    /// <summary>
    /// Event listner for behaviour that needs to occur at the very end of the appication's startup phase 
    /// </summary>
    public void onPostStartup();
}