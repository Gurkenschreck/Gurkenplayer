﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using Lidgren.Network;
using System.Threading;
using ICities;
using ColossalFramework.Plugins;

namespace CSLCoop
{
    public delegate void ServerEventHandler(object sender, EventArgs e);
    public delegate void ServerReceivedMessageEventHandler(object sender, ReceivedMessageEventArgs e);
    public delegate void ServerReceivedUnhandledMessageEventHandler(object sender, ReceivedUnhandledMessageEventArgs e);
    public delegate void ServerConnectionRequestEventHandler(object sender, ConnectionRequestEventArgs e);
    public class MPServer : IDisposable
    {
        //Event stuff
        #region Events and Eventmethods
        public event ServerEventHandler serverStarted;
        public event ServerEventHandler serverStopped;
        public event ServerReceivedMessageEventHandler clientConnected;
        public event ServerReceivedMessageEventHandler clientDisconnected;
        public event ServerEventHandler serverLeavingProcessingMessageThread;
        public event ServerReceivedMessageEventHandler allClientsDisconected;
        public event ServerConnectionRequestEventHandler clientConnectionRequestApproved;
        public event ServerConnectionRequestEventHandler clientConnectionRequestDenied;
        public event ServerReceivedUnhandledMessageEventHandler unhandledMessageReceived;

        /// <summary>
        /// Fires when the netServer is 100% started.
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnServerStarted(EventArgs e)
        {
            if (serverStarted != null)
                serverStarted(this, e);
        }
        /// <summary>
        /// Fires when the netServer is 100% stopped.
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnServerStopped(EventArgs e)
        {
            if (serverStopped != null)
                serverStopped(this, e);
        }
        /// <summary>
        /// Fired when a new client connected.
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnClientConnected(ReceivedMessageEventArgs e)
        {
            if (clientConnected != null)
                clientConnected(this, e);
        }
        /// <summary>
        /// Fired when a client disconnected.
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnClientDisconnected(ReceivedMessageEventArgs e)
        {
            if (clientDisconnected != null)
                clientDisconnected(this, e);
        }
        /// <summary>
        /// Fires when the ProcessMessage thread is about to end.
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnServerLeavingProcessingMessageThread(EventArgs e)
        {
            if (serverLeavingProcessingMessageThread != null)
                serverLeavingProcessingMessageThread(this, e);
        }
        /// <summary>
        /// Fires whenn all clients disconnected from the server.
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnAllClientsDisconnected(ReceivedMessageEventArgs e)
        {
            if (allClientsDisconected != null)
                allClientsDisconected(this, e);
        }
        /// <summary>
        /// Fires when a connection request of a client has been approved.
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnClientConnectionRequestApproved(ConnectionRequestEventArgs e)
        {
            if (clientConnectionRequestApproved != null)
                clientConnectionRequestApproved(this, e);
        }
        /// <summary>
        /// Fires when a connection request of a client has been denied.
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnClientConenctionRequestDenied(ConnectionRequestEventArgs e)
        {
            if (clientConnectionRequestDenied != null)
                clientConnectionRequestDenied(this, e);
        }
        /// <summary>
        /// Fires when a message arrived which type is not handled by the application.
        /// </summary>
        /// <param name="e">Information about the unhandled message.</param>
        public virtual void OnReceivedUnhandledMessage(ReceivedUnhandledMessageEventArgs e)
        {
            if (unhandledMessageReceived != null)
                unhandledMessageReceived(this, e);
        }
        #endregion

        //Fields
        #region Fields (and objects <3)
        NetPeerConfiguration config; //Is used to create the netServer
        NetServer netServer;
        string appIdentifier = "CSLCoop";
        string serverPassword = "Password";
        int serverPort = 4230;
        int serverMaximumPlayerAmount = 1; //1 extra player (coop)
        bool isServerStarted = false;
        string username = "Host";
        bool disposed = false;
        MPSharedCondition stopProcessMessageThread;
        //Message handle thread
        ParameterizedThreadStart pts; //Initializes in the constructor
        Thread messageProcessingThread;
        #endregion

        //Properties
        #region Props
        /// <summary>
        /// Returns the used netServer password.
        /// </summary>
        public string ServerPassword
        {
            get { return serverPassword; }
            set { serverPassword = value; }
        }

        /// <summary>
        /// Returns the used netServer port.
        /// </summary>
        public int ServerPort
        {
            get { return serverPort; }
            set { serverPort = value; }
        }

        /// <summary>
        /// Returns the max amount of connections.
        /// </summary>
        public int ServerMaximumPlayerAmount
        {
            get { return serverMaximumPlayerAmount; }
            set { serverMaximumPlayerAmount = value; }
        }

        /// <summary>
        /// Returns the current amount of connections.
        /// </summary>
        public int ConnectionsCount
        {
            get { return netServer.ConnectionsCount; }
        }

        /// <summary>
        /// Returns true if the netServer is running.
        /// </summary>
        public MPSharedCondition StopMessageProcessingThread
        {
            get { return stopProcessMessageThread; }
            set { stopProcessMessageThread = value; }
        }

        /// <summary>
        /// Indicates wether the netServer is started or not.
        /// </summary>
        public bool IsServerStarted
        {
            get { return isServerStarted; }
            set { isServerStarted = value; }
        }

        /// <summary>
        /// Returns the username.
        /// </summary>
        public string Username
        {
            get { return username; }
            set { username = value; }
        }

        /// <summary>
        /// Returns true if the netServer is initialized and a netClient is connected to it.
        /// </summary>
        public bool CanSendMessage
        {
            get
            {
                if (netServer != null)
                    if (netServer.ConnectionsCount > 0)
                        return true;

                return false;
            }
        }
        #endregion


        //Constructor
        /// <summary>
        /// Private Server constructor used in the singleton pattern. It initialized the config.
        /// </summary>
        public MPServer(MPSharedCondition condition)
        {
            stopProcessMessageThread = condition ?? new MPSharedCondition(false);
            ResetConfig();
            pts = new ParameterizedThreadStart(this.ProcessMessage);
            messageProcessingThread = new Thread(pts);
        }

        /// <summary>
        /// Destructor logic.
        /// </summary>
        ~MPServer()
        {
            Dispose();
        }

        //Methods
        /// <summary>
        /// Method to start the netServer with 3 optional parameters. Arguments which are left blank take
        /// the default value.
        /// </summary>
        /// <param name="port">Port of the netServer. Default: 4230</param>
        /// <param name="password">Password of the netServer. Default: none</param>
        /// <param name="maximumPlayerAmount">Amount of players. Default: 2</param>
        public void StartServer(int port = 4230, string password = "", int maximumPlayerAmount = 1)
        {
            Log.Message("Starting the server. IsServerStarted? " + IsServerStarted + " -> Instance.Stop(). StopMessageProcess...: " + StopMessageProcessingThread.Condition);
            Stop();

            if (port < 1000 || port > 65535)
                throw new MPException("I am not going to bind a port under 1000.");

            if (maximumPlayerAmount < 1)
                throw new MPException("You cannot play alone!");

            //Field configuration
            ServerPort = port;
            ServerPassword = password;
            ServerMaximumPlayerAmount = maximumPlayerAmount;

            //NetPeerConfiguration configuration
            ResetConfig();

            //Initializes the NetServer object with the config and start it
            netServer = new NetServer(config);
            netServer.Start();
            IsServerStarted = true;

            //Separate thread in which the received messages are handled
            messageProcessingThread = new Thread(pts);
            messageProcessingThread.Start(netServer);
            OnServerStarted(EventArgs.Empty);
            Log.Message("Server started successfuly.");
        }

        /// <summary>
        /// Stops the running netServer gracefully.
        /// </summary>
        public void Stop()
        {
            // If netServer is not started, return
            if (!IsServerStarted) 
                return;

            try
            {
                Log.Message("Shutting down the server");
                netServer.Shutdown("Bye bye Server!");
            }
            catch (NetException ex)
            {
                throw new MPException("NetException (Lidgren) in Server.ProcessMessage. Message: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new MPException("Exception in Server.Stop()", ex);
            }
            finally
            {
                ResetConfig();
                StopMessageProcessingThread.Condition = true;
                IsServerStarted = false;
                OnServerStopped(new EventArgs());
                Log.Message("Server shut down");
            }
        }

        /// <summary>
        /// Method to create a new config.
        /// </summary>
        private void ResetConfig()
        {
            config = new NetPeerConfiguration(appIdentifier);
            config.Port = ServerPort;
            config.MaximumConnections = ServerMaximumPlayerAmount;
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            config.AutoFlushSendQueue = false;
        }

        /// <summary>
        /// Gets rid of the Server instance.
        /// </summary>
        public void Dispose()
        {
            try
            {
                Log.Message("Disposing server. Current MPRole: " + MPManager.Instance.MPRole);

                Stop();
                
                if (!disposed)
                {
                    GC.SuppressFinalize(this);
                    disposed = true;
                }

                Log.Message("Server disposed. Current MPRole: " + MPManager.Instance.MPRole);
            }
            catch (Exception ex)
            {
                throw new MPException("Exception in Server.Dispose()", ex);
            }
        }

        /// <summary>
        /// ProcessMessage runs in a separate thread and manages the incoming messages of the clients.
        /// </summary>
        /// <param name="obj">object obj represents a NetServer object.</param>
        private void ProcessMessage(object obj)
        {
            try
            {
                netServer = (NetServer)obj;
                NetIncomingMessage msg;
                Log.Message("Server thread started. ProcessMessage(). Current MPRole: " + MPManager.Instance.MPRole);

                StopMessageProcessingThread.Condition = false;
                MPManager.Instance.IsProcessMessageThreadRunning = true;
                while (!StopMessageProcessingThread.Condition)
                { // As long as the netServer is started
                    while ((msg = netServer.ReadMessage()) != null)
                    {
                        switch (msg.MessageType)
                        {
                            // Debuggen
                            #region Debug
                            case NetIncomingMessageType.VerboseDebugMessage:
                            case NetIncomingMessageType.DebugMessage:
                            case NetIncomingMessageType.WarningMessage:
                            case NetIncomingMessageType.ErrorMessage:
                                Log.Warning("DebugMessage: " + msg.ReadString());
                                break;
                            #endregion

                            // StatusChanged
                            #region NetIncomingMessageType.StatusChanged
                            case NetIncomingMessageType.StatusChanged:
                                NetConnectionStatus state = (NetConnectionStatus)msg.ReadByte();
                                if (state == NetConnectionStatus.Connected)
                                {
                                    OnClientConnected(new ReceivedMessageEventArgs(msg));
                                }
                                else if (state == NetConnectionStatus.Disconnected || state == NetConnectionStatus.Disconnecting)
                                {
                                    OnClientDisconnected(new ReceivedMessageEventArgs(msg));
                                    if (netServer.ConnectionsCount == 0)
                                    {
                                        OnAllClientsDisconnected(new ReceivedMessageEventArgs(msg));
                                    }
                                }
                                break;
                            #endregion

                            // If the message contains data
                            #region NetIncomingMessageType.Data
                            case NetIncomingMessageType.Data:
                                Log.Message("Server Incoming Message Data.");
                                int type = msg.ReadInt32();
                                Log.Message("Type: " + type + "; MPMessageType: " + (MPMessageType)type);
                                ProgressData((MPMessageType)type, msg); //Test
                                break;
                            #endregion

                            // Connectionapproval
                            #region NetIncomingMessageType.ConnectionApproval
                            case NetIncomingMessageType.ConnectionApproval:
                                // Connection logic. Is the user allowed to connect
                                // Receive information to process
                                string sentPassword = msg.ReadString();
                                string sentUsername = msg.ReadString();
                                int sentEnabledModCount = msg.ReadInt32();

                                if (netServer.ConnectionsCount <= ServerMaximumPlayerAmount)
                                {   // Check if server is full
                                    Log.Warning(String.Format("User ({0}) trying to connect. Sent password: {1}", sentUsername, sentPassword));
                                    if (ServerPassword == sentPassword)
                                    {   // Password is the same
                                        if (PluginManager.instance.enabledModCount == sentEnabledModCount)
                                        {   // The client and server have the same amount of mods enabled
                                            msg.SenderConnection.Approve();
                                            OnClientConnectionRequestApproved(new ConnectionRequestEventArgs(msg, "User accepted", sentUsername, sentPassword));
                                        }
                                        else
                                        {
                                            msg.SenderConnection.Deny();
                                            OnClientConenctionRequestDenied(new ConnectionRequestEventArgs(msg, String.Format("Denied: Different amount of mods activated. User: {0}; Server: {1}", sentEnabledModCount, PluginManager.instance.enabledModCount), sentUsername, sentPassword));
                                        }
                                    }
                                    else
                                    {
                                        msg.SenderConnection.Deny();
                                        OnClientConenctionRequestDenied(new ConnectionRequestEventArgs(msg, "Denied: Wrong password.", sentUsername, sentPassword));
                                    }
                                }
                                else
                                {
                                    msg.SenderConnection.Deny();
                                    OnClientConenctionRequestDenied(new ConnectionRequestEventArgs(msg, "Denied: Game is full/Cannot accept any more connections.", sentUsername, sentPassword));
                                }
                                break;
                            #endregion

                            default:
                                OnReceivedUnhandledMessage(new ReceivedUnhandledMessageEventArgs(msg, msg.MessageType.ToString()));
                                break;
                        }
                    }
                }
            }
            catch (NetException ex)
            {
                throw new MPException("NetException (Lidgren) in Server.ProcessMessage. Message: " + ex.ToString(), ex);
            }
            catch (Exception ex)
            {
                throw new MPException("Exception in Server.ProcessMessage(). Message: " + ex.ToString(), ex);
            }
            finally
            {
                IsServerStarted = false;
                StopMessageProcessingThread.Condition = false;
                OnServerLeavingProcessingMessageThread(new EventArgs());
            }
        }
        
        /// <summary>
        /// Message to progress the received information.
        /// </summary>
        /// <param name="type">Type of the message. Indicates what the message's contents are.</param>
        /// <param name="msg">Received message.</param>
        private void ProgressData(MPMessageType msgType, NetIncomingMessage msg)
        {
            Log.Message("Server received " + msgType);
            switch (msgType)
            {
                case MPMessageType.MoneyUpdate: // Receiving money
                    EcoExtBase.MPCashChangeAmount += msg.ReadInt64();
                    // EcoExtBase.MPInternalMoneyAmount -= EcoExtBase.MPCashChangeAmount;
                    break;
                case MPMessageType.DemandUpdate: // Receiving demand
                    DemandExtBase.MPCommercialDemand = msg.ReadInt32();
                    DemandExtBase.MPResidentalDemand = msg.ReadInt32();
                    DemandExtBase.MPWorkplaceDemand = msg.ReadInt32();
                    break;
                case MPMessageType.TileUpdate:
                    AreaExtBase.MPXCoordinate = msg.ReadInt32();
                    AreaExtBase.MPZCoordinate = msg.ReadInt32();
                    // INFO: The unlock process is activated once every 4 seconds simutaniously with the
                    // EcoExtBase.OnUpdateMoneyAmount(long internalMoneyAmount).
                    // Maybe I find a direct way to unlock a tile within AreaExtBase
                    break;
                case MPMessageType.SimulationUpdate: // Receiving simulation time Update
                    SimulationManager.instance.SelectedSimulationSpeed = msg.ReadInt32();
                    SimulationManager.instance.SimulationPaused = msg.ReadBoolean();
                    break;
                case MPMessageType.CitizenUpdate: // Receiving citizen information
                    CitizenManager.instance.m_citizenCount = msg.ReadInt32();
                    break;
                default:
                    OnReceivedUnhandledMessage(new ReceivedUnhandledMessageEventArgs(msg, msgType.ToString()));
                    break;
            }
        }

        /// <summary>
        /// Sends economy update to all.
        /// </summary>
        public void SendEconomyInformationUpdateToAll()
        {
            if (CanSendMessage)
            {
                NetOutgoingMessage msg = netServer.CreateMessage();
                msg.Write((int)MPMessageType.MoneyUpdate);
                msg.Write(EcoExtBase.MPInternalMoneyAmount);
                netServer.SendToAll(msg, NetDeliveryMethod.ReliableOrdered);
                netServer.FlushSendQueue();
            }
        }

        /// <summary>
        /// Sends demand update to all. (Commercial demand, residental demand, workplace demand)
        /// </summary>
        public void SendDemandInformationUpdateToAll()
        {
            if (CanSendMessage)
            {
                NetOutgoingMessage msg = netServer.CreateMessage();
                msg.Write((int)MPMessageType.DemandUpdate);
                msg.Write(DemandExtBase.MPCommercialDemand);
                msg.Write(DemandExtBase.MPResidentalDemand);
                msg.Write(DemandExtBase.MPWorkplaceDemand);
                netServer.SendToAll(msg, NetDeliveryMethod.ReliableOrdered);
                netServer.FlushSendQueue();
            }
        }

        /// <summary>
        /// Sends the information of the new unlocked tile to all.
        /// </summary>
        /// <param name="x">X coordinate of the new tile.</param>
        /// <param name="z">Z coordinate of the new tile.</param>
        public void SendAreaInformationUpdateToAll(int x, int z)
        {
            if (CanSendMessage)
            {
                NetOutgoingMessage msg = netServer.CreateMessage();
                msg.Write((int)MPMessageType.TileUpdate);
                msg.Write(x);
                msg.Write(z);
                netServer.SendToAll(msg, NetDeliveryMethod.ReliableOrdered);
                netServer.FlushSendQueue();
            }
        }

        /// <summary>
        /// Sends information about the current simulation to all clients.
        /// </summary>
        public void SendSimulationInformationUpdateToAll()
        {
            if (CanSendMessage)
            {
                NetOutgoingMessage msg = netServer.CreateMessage();
                msg.Write((int)MPMessageType.SimulationUpdate);
                msg.Write(SimulationManager.instance.SelectedSimulationSpeed);
                msg.Write(SimulationManager.instance.SimulationPaused);
                netServer.SendToAll(msg, NetDeliveryMethod.ReliableOrdered);
                netServer.FlushSendQueue();
            }
        }

        /// <summary>
        /// Sends information about the current citizen count to all clients.
        /// </summary>
        public void SendCitizenInformationUpdateToAll()
        {
            if (CanSendMessage)
            {
                NetOutgoingMessage msg = netServer.CreateMessage();
                msg.Write((int)MPMessageType.CitizenUpdate);
                msg.Write(CitizenManager.instance.m_citizenCount);
                netServer.SendToAll(msg, NetDeliveryMethod.ReliableOrdered);
                netServer.FlushSendQueue();
            }
        }

        public void SendBuildingAddedInformationUpdateToAll()
        {
            if (CanSendMessage)
            {
                NetOutgoingMessage msg = netServer.CreateMessage();
                msg.Write((int)MPMessageType.BuildingAddedUpdate);
                msg.Write(ConvertionHelper.ConvertToByteArray(SkylinesOverwatch.Data.Instance.BuildingsAdded));

            }
        }

        public void SendBuildingRemovalInformationUpdateToAll()
        {
            if (CanSendMessage)
            {
                NetOutgoingMessage msg = netServer.CreateMessage();
                msg.Write((int)MPMessageType.BuildingRemovalUpdate);
                //Go on. Need to convert Data.Instance.BuildingsRemoved to byte[]
            }
        }
    }
}