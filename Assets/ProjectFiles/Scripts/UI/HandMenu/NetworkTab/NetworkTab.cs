using UnityEngine;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Abstract class for NetworkTab UI elements that control various actions for establishing and managing
/// a connection to a server.
/// </summary>
public abstract class NetworkTab : MonoBehaviour
{
    /// <summary>
    /// A reference to the NetworkOptionsTab for us to 
    /// change its options state
    /// </summary>
    protected NetworkOptionsTab networkTab;
    /// <summary>
    /// method for initialising this NetworkTab Object
    /// </summary>
    /// <param name="networkTab"></param>
    public abstract void initialise(NetworkOptionsTab networkTab);
}