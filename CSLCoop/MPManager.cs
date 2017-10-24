﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSLCoop
{
    /// <summary>
    /// Manages the MPClient and MPServer instances while providing methods to access them. It makes sure
    /// that there is only one MPClient or one MPServer instance. The MPManager can only exist one time in
    /// the program.
    /// </summary>
    public class MPManager
    {
        // Fields
        static MPManager instance;
        bool isProcessMessageThreadRunning = false;
        MPRoleType mpRole = MPRoleType.None;
        MPClient mpClient;
        MPServer mpServer;
        // The message processing thread can be stopped by setting StopProcessMessageThread in MPManager
        // or even in MPClient/Server to true. They share this condition.
        // StopProcessMessageThread is also used by other threads like in the EcoExtBase class.
        static MPSharedCondition stopProcessMessageThread = new MPSharedCondition(false);

        // Properties
        /// <summary>
        /// Indicates the current status of te ProcessMessageThread.
        /// </summary>
        public bool IsProcessMessageThreadRunning
        {
            get { return isProcessMessageThreadRunning; }
            set { isProcessMessageThreadRunning = value; }
        }
        /// <summary>
        /// The condition which stops the ProcessMessage threads of netServer and client.
        /// </summary>
        public static MPSharedCondition StopProcessMessageThread
        {
            get { return MPManager.stopProcessMessageThread; }
            set { MPManager.stopProcessMessageThread = value; }
        }
        /// <summary>
        /// The used MPClient of this instance.
        /// </summary>
        public MPClient MPClient
        {
            get { return mpClient; }
            private set { mpClient = value; }
        }
        /// <summary>
        /// The used MPServer of this instance.
        /// </summary>
        public MPServer MPServer
        {
            get { return mpServer; }
            private set { mpServer = value; }
        }
        /// <summary>
        /// Indicates if the MPServer object is null or not.
        /// </summary>
        public bool IsMPServerInitialized 
        {
            get
            {
                return (MPServer != null) ? true : false;
            }
        }
        /// <summary>
        /// Indicates if the MPClient object is null or not.
        /// </summary>
        public bool IsMPClientInitialized
        {
            get
            {
                return (MPClient != null) ? true : false;
            }
        }

        /// <summary>
        /// Indicates the current multiplayer role of the user.
        /// </summary>
        public MPRoleType MPRole
        {
            get { return mpRole; }
            private set
            {
                if (value == MPRoleType.None)
                {
                    mpRole = MPRoleType.None;
                }
                else if (value == MPRoleType.Resetting)
                {
                    // Use MPRoleType.Resetting or the provided methods to reset the instances.
                    if (mpRole == MPRoleType.Server)
                    {
                        ServerStop();
                        ServerUninitialize();
                    }
                    else if (mpRole == MPRoleType.Client)
                    {
                        ClientDisconnect();
                        ClientUninitialize();
                    }

                    // MPRole should be set to None in the xUninitialize() methods.
                    // And if not:
                    if (mpRole != MPRoleType.None)
                        mpRole = MPRoleType.None;
                }
                else if (value == MPRoleType.Server)
                {
                    if (mpRole == MPRoleType.Client)
                    {
                        ClientDisconnect();
                        ClientUninitialize();
                    }
                    Log.Message("MPRole Property: mpRole >" + mpRole + "< and setting value >" + value + "<");
                    mpRole = value;
                }
                else if (value == MPRoleType.Client)
                {
                    if (mpRole == MPRoleType.Server)
                    {
                        ServerStop();
                        ServerUninitialize();
                    }
                    Log.Message("MPRole Property: mpRole >" + mpRole + "< and setting value >" + value + "<");
                    mpRole = value;
                }
            }
        }
        //Singleton
        public static MPManager Instance
        {
            get
            {
                return instance ?? (instance = new MPManager());
            }
        }

        //Constructor
        private MPManager()
        {
            Log.Warning("MPManager was just initialized.");
        }

        //Methods
        /// <summary>
        /// Resets the MPManager instance.
        /// </summary>
        public void Reset()
        {
            Log.Message("Resetting MPManager instance.");

            Log.Message("Resetting MPClient");
            ClientUninitialize();
            Log.Message("Resetting MPServer");
            ServerUninitialize();


            instance = new MPManager();
            SetMPRole(MPRoleType.None);
            Log.Message("MPManager instance resetted.");
        }
        /// <summary>
        /// Sets the current MPRole.
        /// </summary>
        /// <param name="newMPRoleType">New MPRoleType for MPRole.</param>
        public void SetMPRole(MPRoleType newMPRoleType)
        {
            MPRole = newMPRoleType;
        }

        //SERVER//
        #region netServer stuff
        /// <summary>
        /// Initializes the MPServer if it is not already initialized and the MPClient is not initialized.
        /// </summary>
        public void ServerInitialize()
        {
            if (IsMPClientInitialized)
            {
                Log.Warning("Cannot initialize server. Client is already initialized.");
                return;
            }

            if (IsMPServerInitialized)
            {
                Log.Warning("Server is already initialized.");
                return;
            }

            Log.Message("Initialize server.");
            MPServer = new MPServer(StopProcessMessageThread);
            MPServer.serverLeavingProcessingMessageThread += MPServer_serverLeavingProcessingMessageThreadEvent;
            MPServer.unhandledMessageReceived += MPServer_unhandledMessageReceivedEvent;
            MPServer.clientConnected += MPServer_clientConnectedEvent;
            MPServer.clientDisconnected += MPServer_clientDisconnectedEvent;
            MPServer.allClientsDisconected += MPServer_allClientsDissconectedEvent;
            MPServer.clientConnectionRequestApproved += MPServer_clientConnectionRequestApprovedEvent;
            MPServer.clientConnectionRequestDenied += MPServer_clientConnectionRequestDeniedEvent;
            SetMPRole(MPRoleType.Server);
            Log.Message("Server initialized.");
        }

        /// <summary>
        /// Uninitializes the MPServer. Sets it to null.
        /// </summary>
        public void ServerUninitialize()
        {
            if (!IsMPServerInitialized)
            {
                Log.Warning("Server is not initialized. Cannot uninitialize.");
                return;
            }

            Log.Message("Uninitialize server.");
            ServerStop();
            MPServer = null;
            SetMPRole(MPRoleType.None);
            Log.Message("Server uninitialized.");
        }
        /// <summary>
        /// Create a new instance of MPServer.
        /// </summary>
        public void ServerReset()
        {
            if (IsMPClientInitialized)
            {
                Log.Warning("Server cannot reset. The client is initialized.");
                return;
            }

            if (!IsMPServerInitialized)
            {
                Log.Warning("MPServer is not initialized. There is nothing to reset.");
                return;
            }

            Log.Message("Resetting server.");
            ServerStop();
            ServerInitialize();
            Log.Message("Server resetted. New instance created.");
        }
        /// <summary>
        /// Starts the netServer.
        /// </summary>
        public void ServerStart()
        {
            if (IsMPClientInitialized)
            {
                Log.Warning("Cannot start the server. Client is initialized.");
                return;
            }

            if (!IsMPServerInitialized)
            {
                Log.Warning("Cannot start the server. The server is not initialized.");
                return;
            }

            Log.Message("Starting server.");
            MPServer.StartServer();
            SetMPRole(MPRoleType.Server);
            Log.Message("Server started.");
        }
        /// <summary>
        /// Starts the netServer.
        /// </summary>
        /// <param name="port">The port to listen on.</param>
        /// <param name="password">The password of the netServer.</param>
        /// <param name="maximumPlayerAmount">Maximum amount of players.</param>
        public void ServerStart(int port, string password, int maximumPlayerAmount)
        {
            if (IsMPClientInitialized)
            {
                Log.Warning("Cannot start the server. Client is initialized.");
                return;
            }

            if (!IsMPServerInitialized)
            {
                Log.Warning("Cannot start the server. The server is not initialized.");
                return;
            }

            Log.Message("Starting server.");
            MPServer.StartServer(port, password, maximumPlayerAmount);
            Log.Message("Server started.");
        }
        /// <summary>
        /// Stops the netServer.
        /// </summary>
        public void ServerStop()
        {
            if (IsMPClientInitialized)
            {
                Log.Warning("Cannot stop the server. Client is initilalized");
                return;
            }
            if (!IsMPServerInitialized)
            {
                Log.Warning("Cannot stop the server. Server is not initialized.");
                return;
            }

            Log.Message("Stopping server.");
            MPServer.Stop();
            Log.Message("Server stopped.");
        }

        // Server Events
        #region Server Events
        /// <summary>
        /// Fired when the ProcessMessage thread of the MPSever is left.
        /// </summary>
        /// <param name="sender">MPServer instance.</param>
        /// <param name="e"></param>
        void MPServer_serverLeavingProcessingMessageThreadEvent(object sender, EventArgs e)
        {
            Log.Message("Left Server message processing thread!");
            IsProcessMessageThreadRunning = false;
            ServerUninitialize();
        }
        /// <summary>
        /// Fires when a unhandled message arrives.
        /// </summary>
        /// <param name="sender">MPServer object.</param>
        /// <param name="e">Information about the unhandled object.</param>
        void MPServer_unhandledMessageReceivedEvent(object sender, ReceivedUnhandledMessageEventArgs e)
        {
            Log.Warning(String.Format("Server: Unhandled MessageType/SubType: {0}/{1} ", e.Message.MessageType, e.Type));
        }
        /// <summary>
        /// Fires when a new client connected.
        /// </summary>
        /// <param name="sender">Information about the sender object, the MPServer.</param>
        /// <param name="e">Information about the received message.</param>
        void MPServer_clientConnectedEvent(object sender, ReceivedMessageEventArgs e)
        {
            Log.Message("Client connected. Client IP: " + e.Message.SenderEndPoint);
            Log.Error("ConnectionsCount new connected:::" + MPServer.ConnectionsCount.ToString());
        }
        /// <summary>
        /// Fires when a client disconnected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MPServer_clientDisconnectedEvent(object sender, ReceivedMessageEventArgs e)
        {
            Log.Message("Client disconnected. Client IP: " + e.Message.SenderEndPoint);
            Log.Error("ConnectionsCount new disconnect:::" + MPServer.ConnectionsCount.ToString());
        }
        /// <summary>
        /// Fires when all clients disconnected from the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MPServer_allClientsDissconectedEvent(object sender, EventArgs e)
        {
            MPGlobalValues.IsConfigurationFinished = false;
        }
        /// <summary>
        /// Fires when a connection request of a client has been accepted.
        /// </summary>
        /// <param name="sender">Sender object, MPServer.</param>
        /// <param name="e">Containing information about the connection request.</param>
        void MPServer_clientConnectionRequestApprovedEvent(object sender, ConnectionRequestEventArgs e)
        {
            Log.Message(String.Format("New client connected. IP:{0} Username:{1}", e.Message.SenderEndPoint, e.Username));
        }
        /// <summary>
        /// Fires when a conenction request of a client has been denied because of various reasons.
        /// </summary>
        /// <param name="sender">Sender object, MPServer.</param>
        /// <param name="e">Containing information about the connection request.</param>
        void MPServer_clientConnectionRequestDeniedEvent(object sender, ConnectionRequestEventArgs e)
        {
            Log.Warning(String.Format("Client connection request denied. IP:{0} Username:{1} Password:{2} Note:{3}", e.Message.SenderEndPoint, e.Username, e.Password, e.Note));
        }
        #endregion
        #endregion

        //CLIENT//
        #region netClient stuff
        /// <summary>
        /// Initilializes the Client if it is not already initialized and the MPServer is not initialized.
        /// </summary>
        public void ClientInitialize()
        {
            if (IsMPServerInitialized)
            {
                Log.Warning("Cannot initialize the client. Server is initilalized");
                return;
            }

            if (IsMPClientInitialized)
            {
                Log.Warning("Client is already initialized.");
            }

            Log.Message("Initializing client.");
            MPClient = new MPClient(StopProcessMessageThread);
            MPClient.clientLeavingProcessMessageThread += MPClient_clientLeavingProcessMessageThread;
            MPClient.clientConnected += MPClient_clientConnectedEvent;
            MPClient.clientDisconnected += MPClient_clientDisconnectedEvent;
            mpClient.receivedUnhandledMessage += mpClient_receivedUnhandledMessage;
            SetMPRole(MPRoleType.Client);
            Log.Message("Client initialized.");
        }

        /// <summary>
        /// Uninitializes the MPClient. Sets it to null.
        /// </summary>
        public void ClientUninitialize()
        {
            if (!IsMPClientInitialized)
            {
                Log.Warning("There is nothing to uninitialize.");
                return;
            }

            Log.Message("Uninitialize client.");
            ClientDisconnect();
            MPClient = null;
            SetMPRole(MPRoleType.None);
            Log.Message("Client uninitialized.");
        }
        /// <summary>
        /// Creates a new instance of MPClient.
        /// </summary>
        public void ClientReset()
        {
            if (IsMPServerInitialized)
            {
                Log.Warning("Could not reset the client. Server is initialized.");
                return;
            }

            if (!IsMPClientInitialized)
            {
                Log.Warning("Client is not initialized. There is nothing to reset.");
                return;
            }

            Log.Message("Resetting client");
            ClientDisconnect();
            ClientInitialize();
            Log.Message("Client resetted.");
        }
        
        /// <summary>
        /// Connect to localhost on port 4230 with no password ("").
        /// </summary>
        public void ClientConnect()
        {
            if (IsMPServerInitialized)
            {
                Log.Warning("Cannot connect client to server. Server is initialized.");
                return;
            }

            if (!IsMPClientInitialized)
            {
                Log.Warning("Cannot connect client to server. CLient is not initialized.");
            }

            Log.Message("Connecting client.");
            MPClient.ConnectToServer();
            Log.Message("Client connected.");
        }
        /// <summary>
        /// Connects to a remote netServer.
        /// </summary>
        /// <param name="ip">The IP address of the netServer.</param>
        /// <param name="port">The port of the netServer.</param>
        /// <param name="password">The netServer password.</param>
        public void ClientConnect(string ip, int port, string password)
        {
            if (IsMPServerInitialized)
            {
                Log.Warning("Cannot connect client to server. Server is initialized.");
                return;
            }

            if (!IsMPClientInitialized)
            {
                Log.Warning("Cannot connect client to server. CLient is not initialized.");
            }

            Log.Message("Connecting client.");
            MPClient.ConnectToServer(ip, port, password);
            Log.Message("Client connected.");
        }
        /// <summary>
        /// Disconnects from the netServer.
        /// </summary>
        public void ClientDisconnect()
        {
            if (IsMPServerInitialized)
            {
                Log.Warning("Could not disconnect from server. Server is initialized.");
                return;
            }

            if (!IsMPClientInitialized)
            {
                Log.Warning("Could not disconnect from the server. Client is not initialized.");
                return;
            }
            Log.Message("Disconnecting from server.");
            MPClient.DisconnectFromServer();
            Log.Message("Disconnected from server.");
        }

        // Client Events
        #region Client Events
        /// <summary>
        /// Fires when the process message thread is about to leave.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MPClient_clientLeavingProcessMessageThread(object sender, EventArgs e)
        {
            Log.Message("Left Client message processing thread!");
            IsProcessMessageThreadRunning = false;
            ClientUninitialize();
        }
        /// <summary>
        /// Fires when the client connected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MPClient_clientConnectedEvent(object sender, ReceivedMessageEventArgs e)
        {
            Log.Message("You connected. Client IP: " + e.Message.SenderEndPoint);
        }
        /// <summary>
        /// Fires when the client disconnected from the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MPClient_clientDisconnectedEvent(object sender, ReceivedMessageEventArgs e)
        {
            Log.Message("You disconnected. Client IP: " + e.Message.SenderEndPoint);
        }
        /// <summary>
        /// Fires when the client received an unhandled message.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void mpClient_receivedUnhandledMessage(object sender, ReceivedUnhandledMessageEventArgs e)
        {
            Log.Warning(String.Format("Client: Unhandled MessageType/SubType: {0}/{1} ", e.Message.MessageType, e.Type));
        }
        #endregion
        #endregion
    }
}