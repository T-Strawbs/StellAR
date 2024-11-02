using System;
using System.Collections;
using Unity;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using System.Net;
using System.Security.Policy;
public class ConnectionManager : Singleton<ConnectionManager>,PostStartupListener
{
    // colours assigned to players when they join the server.
    public Color[] playerColours = { Color.cyan, Color.red, Color.green, Color.yellow, Color.magenta, Color.blue, Color.black, Color.white, Color.grey };

    private void Awake()
    {
        ApplicationManager.Instance.onPostStartProcess.AddListener(onPostStartup);   
    }

    private void Start()
    {
        setupNetworkManagerEvents();
    }

    public void onPostStartup()
    {
        //setupNetworkManagerEvents();
    }

    private void setupNetworkManagerEvents()
    {
        //Register lambda event that executes when the server starts 
        NetworkManager.Singleton.OnServerStarted += () =>
        {
            DebugConsole.Instance.LogDebug("Success!: Server Started");
            //if(IsServer)
            //PrefabManager.Instance.registerMessages();
        };
        //Register lambda event that executes when the a client connects to the server.
        NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientID) =>
        {
            DebugConsole.Instance.LogDebug($"SUCCESS!: Client: client({clientID}) has joined the server");
            //if(IsClient && NetworkManager.Singleton.LocalClientId == clientID)
            //PrefabManager.Instance.registerMessages();
        };
        //Register lambda event that executes when the a client disconnects from the server.
        NetworkManager.Singleton.OnClientDisconnectCallback += (ulong clientID) =>
        {
            DebugConsole.Instance.LogDebug($"SUCCESS!: Client: client({clientID}) has left the server");
            // Log more details about the disconnection
            if (NetworkManager.Singleton.DisconnectReason != null)
            {
                DebugConsole.Instance.LogDebug($"Disconnection Reason: {NetworkManager.Singleton.DisconnectReason}");
            }
            else
            {
                DebugConsole.Instance.LogDebug("No specific reason provided for disconnection.");
            }
        };
    }

    public bool startHost()
    {
        //check if we can successfuly config the transport
        if(!configureTransport(""))
        {
            DebugConsole.Instance.LogDebug($"Couldnt config the transport when hosting");
            return false;
        }
        try
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            //start the server 
            if (NetworkManager.Singleton.StartHost())
            {
                DebugConsole.Instance.LogDebug($"the ip that we hosted on is {transport.ConnectionData.Address} on port {transport.ConnectionData.Port}");
            }
            else
            {
                DebugConsole.Instance.LogDebug($"couldnt host on ip of {transport.ConnectionData.Address} on port {transport.ConnectionData.Port}");
            }
                
            //set the applications NetworkStatus to online
            ApplicationManager.Instance.setNetworkStatus(NetworkStatus.ONLINE);

            return true;
        }
        catch (Exception e) 
        {
            DebugConsole.Instance.LogDebug($"Couldnt start the server when hosting\n{e}");

            return false;
        }
        
    }

    public bool joinServer(string ipAddress)
    {
        DebugConsole.Instance.LogDebug($"the ip that we want to join on is: '{ipAddress}'");

        if (!configureTransport(ipAddress))
        {
            DebugConsole.Instance.LogDebug($"Couldnt config the transport when joining");
            return false;
        }
        try
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            //start the server 
            if (NetworkManager.Singleton.StartClient())
            {
                DebugConsole.Instance.LogDebug($"the ip that we joined on is {transport.ConnectionData.Address} on port {transport.ConnectionData.Port}");
            }
            else
            {
                DebugConsole.Instance.LogDebug($"couldnt join on ip of {transport.ConnectionData.Address} on port {transport.ConnectionData.Port}");
            }
            
            //set the applications NetworkStatus to online
            ApplicationManager.Instance.setNetworkStatus(NetworkStatus.ONLINE);

            return true;
        }
        catch (Exception e)
        {
            DebugConsole.Instance.LogDebug($"Couldnt start as client when joining\n{e}");
            return false;
        }
    }

    public void disconnect()
    {
        try
        {
            //disconnect client
            NetworkManager.Singleton.Shutdown();

            //set the applications NetworkStatus to online
            ApplicationManager.Instance.setNetworkStatus(NetworkStatus.OFFLINE);
        }
        catch (Exception e)
        {
            DebugConsole.Instance.LogDebug($"Couldnt disconnect from the server\n{e}");
        }
    }

    private bool configureTransport(string ipAddress)
    {
        //get the network transport
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        //use the default port 7777
        transport.ConnectionData.Port = 7777;

        if (string.IsNullOrEmpty(ipAddress))
        {
            //get all ip addresses of the local device
            var ipAddresses = Dns.GetHostAddresses("");
            //for each ip found
            foreach(var ip  in ipAddresses)
            {
                //find the IPV4 address (i.e. "InterNetwork" address)
                if (ip.AddressFamily.Equals(System.Net.Sockets.AddressFamily.InterNetwork))
                {
                    // set the ip of the transport
                    ipAddress = ip.ToString();
                    break;
                }
            }
            // check if an address was found
            if (!string.IsNullOrEmpty(ipAddress))
            {
                DebugConsole.Instance.LogDebug($"the ip addr we grabed from dns is '{ipAddress}'");
                transport.ConnectionData.Address = ipAddress;//"127.0.0.1";
                return true;
            }

            return false;
        }
        ipAddress = ipAddress.Trim();

        //set the transports address to the ip address
        transport.ConnectionData.Address = ipAddress;
        return true;
    }
}