﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICities;
using UnityEngine;
using ColossalFramework;
using ColossalFramework.UI;

namespace CSLCoop
{
    class ConfigurationPanel : UIPanel //Testing
    {
        //Fields
        #region Fields
        MPManager mpManager = MPManager.Instance;
        //General area
        UILabel lbl_CSLCoop;
        UILabel lbl_Username;
        UITextField txt_Username;
        UILabel lbl_Password;
        UITextField txt_Password;
        //Client area
        UILabel lbl_Client;
        UILabel lbl_ClientIP;
        UITextField txt_ClientIP;
        UILabel lbl_ClientPort;
        UITextField txt_ClientPort;
        UIButton btn_ClientConnect;
        //Server area
        UILabel lbl_Server;
        UILabel lbl_ServerPlayers;
        UITextField txt_ServerPlayers;
        UILabel lbl_ServerPort;
        UITextField txt_ServerPort;
        UIButton btn_ServerStart;
        #endregion

        //Properties
        //Methods
        public override void Start()
        {
            UIDragHandle dh = (UIDragHandle)this.AddUIComponent(typeof(UIDragHandle)); //Activates the dragging of the window

            try
            {
                //Configures this window
                this.backgroundSprite = "GenericPanel";
                this.name = "MPConfigurationPanel";
                this.color = new Color32(51, 204, 51, 220);
                this.width = 385;
                this.height = 523;
                this.relativePosition = new Vector3(700, 270, 0);
                Log.Message(String.Format("panel height:{0} width:{1} position x-y-z:{2}-{3}-{4} relposition x-y-z:{5}-{6}-{7} transposition x-y-z:{8}-{9}-{10}", this.height, this.width, this.position.x, this.position.y, this.position.z, this.relativePosition.x, this.relativePosition.y, this.relativePosition.z, this.transformPosition.x, this.transformPosition.y, this.transformPosition.z));

                //Adds all elements to the panel
                try
                {
                    PopulateConfigurationPanel();
                }
                catch (Exception ex)
                {
                    Log.Error("Populate Error: " + ex.ToString());
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error creating Panel: " + ex.ToString());
            }
        }

        /// <summary>
        /// Update is calles every frame while the configuration panel is enabled.
        /// </summary>
        public override void Update()
        {
        }

        /// <summary>
        /// OnDisable event of UIPanel. Triggers when the object is disabled.
        /// </summary>
        public override void OnDisable()
        {
            MPGlobalValues.IsConfigurationFinished = true;
        }

        protected override void OnVisibilityChanged()
        {
            if (isVisible)
                MPGlobalValues.IsConfigurationFinished = false;
            else
                MPGlobalValues.IsConfigurationFinished = true;
        }

        /// <summary>
        /// Adds all the controls to the ConfigurationPanel.
        /// </summary>
        private void PopulateConfigurationPanel()
        {
            //CSLCoop Label
            lbl_CSLCoop = (UILabel)this.AddUIComponent(typeof(UILabel));
            lbl_CSLCoop.text = "CSL-Coop";
            lbl_CSLCoop.textScale = 1.6f;
            lbl_CSLCoop.position = new Vector3(110, -20, 0);

            //Username Label
            lbl_Username = (UILabel)this.AddUIComponent(typeof(UILabel));
            lbl_Username.text = "Username:";
            lbl_Username.textScale = 1.1f;
            lbl_Username.position = new Vector3(40, -55, 0);

            //Username TextField
            txt_Username = (UITextField)this.AddUIComponent(typeof(UITextField));
            txt_Username.width = 210;
            txt_Username.height = 22;
            txt_Username.text = "";
            txt_Username.maxLength = 48;
            txt_Username.isEnabled = true;
            txt_Username.builtinKeyNavigation = true;
            txt_Username.isInteractive = true;
            txt_Username.readOnly = false;
            txt_Username.selectionSprite = "EmptySprite";
            txt_Username.selectionBackgroundColor = new Color32(204, 255, 51, 255);
            txt_Username.normalBgSprite = "TextFieldPanel";
            txt_Username.textColor = new Color32(255, 255, 255, 255);
            txt_Username.disabledTextColor = new Color32(153, 153, 153, 220);
            txt_Username.textScale = 1.1f;
            txt_Username.opacity = 1;
            txt_Username.color = new Color32(191, 255, 191, 255);
            txt_Username.disabledColor = new Color32(115, 153, 115, 220);
            txt_Username.position = new Vector3(160, -55, 0);

            //Password Label
            lbl_Password = (UILabel)this.AddUIComponent(typeof(UILabel));
            lbl_Password.text = "Password:";
            lbl_Password.textScale = 1.1f;
            lbl_Password.position = new Vector3(40, -90, 0);

            //Password TextField
            txt_Password = (UITextField)this.AddUIComponent(typeof(UITextField));
            txt_Password.width = 210;
            txt_Password.height = 22;
            txt_Password.text = "";
            txt_Password.maxLength = 48;
            txt_Password.isEnabled = true;
            txt_Password.builtinKeyNavigation = true;
            txt_Password.isInteractive = true;
            txt_Password.readOnly = false;
            txt_Password.selectionSprite = "EmptySprite";
            txt_Password.selectionBackgroundColor = new Color32(204, 255, 51, 255);
            txt_Password.normalBgSprite = "TextFieldPanel";
            txt_Password.textColor = new Color32(255, 255, 255, 255);
            txt_Password.disabledTextColor = new Color32(153, 153, 153, 220);
            txt_Password.textScale = 1.1f;
            txt_Password.opacity = 1;
            txt_Password.color = new Color32(191, 255, 191, 255);
            txt_Password.disabledColor = new Color32(115, 153, 115, 220);
            txt_Password.position = new Vector3(160, -90, 0);

            //Client Label
            lbl_Client = (UILabel)this.AddUIComponent(typeof(UILabel));
            lbl_Client.text = "Client";
            lbl_Client.textScale = 1.3f;
            lbl_Client.position = new Vector3(145, -140, 0);

            //Client enter Server IP Label
            lbl_ClientIP = (UILabel)this.AddUIComponent(typeof(UILabel));
            lbl_ClientIP.text = "IP:";
            lbl_ClientIP.textScale = 1.1f;
            lbl_ClientIP.position = new Vector3(80, -175, 0);

            //Client enter Server IP TextField
            txt_ClientIP = (UITextField)this.AddUIComponent(typeof(UITextField));
            txt_ClientIP.width = 210;
            txt_ClientIP.height = 22;
            txt_ClientIP.text = "localhost";
            txt_ClientIP.maxLength = 48;
            txt_ClientIP.isEnabled = true;
            txt_ClientIP.builtinKeyNavigation = true;
            txt_ClientIP.isInteractive = true;
            txt_ClientIP.readOnly = false;
            txt_ClientIP.selectionSprite = "EmptySprite";
            txt_ClientIP.selectionBackgroundColor = new Color32(204, 255, 51, 255);
            txt_ClientIP.normalBgSprite = "TextFieldPanel";
            txt_ClientIP.textColor = new Color32(255, 255, 255, 255);
            txt_ClientIP.disabledTextColor = new Color32(153, 153, 153, 220);
            txt_ClientIP.textScale = 1.1f;
            txt_ClientIP.opacity = 1;
            txt_ClientIP.color = new Color32(191, 255, 191, 255);
            txt_ClientIP.disabledColor = new Color32(115, 153, 115, 220);
            txt_ClientIP.position = new Vector3(160, -175, 0);

            //Client port Label
            lbl_ClientPort = (UILabel)this.AddUIComponent(typeof(UILabel));
            lbl_ClientPort.text = "Port:";
            lbl_ClientPort.textScale = 1.1f;
            lbl_ClientPort.position = new Vector3(70, -210, 0);

            //Client port TextField
            txt_ClientPort = (UITextField)this.AddUIComponent(typeof(UITextField));
            txt_ClientPort.width = 210;
            txt_ClientPort.height = 22;
            txt_ClientPort.text = "4230";
            txt_ClientPort.maxLength = 48;
            txt_ClientPort.isEnabled = true;
            txt_ClientPort.builtinKeyNavigation = true;
            txt_ClientPort.isInteractive = true;
            txt_ClientPort.readOnly = false;
            txt_ClientPort.selectionSprite = "EmptySprite";
            txt_ClientPort.selectionBackgroundColor = new Color32(204, 255, 51, 255);
            txt_ClientPort.normalBgSprite = "TextFieldPanel";
            txt_ClientPort.textColor = new Color32(255, 255, 255, 255);
            txt_ClientPort.disabledTextColor = new Color32(153, 153, 153, 220);
            txt_ClientPort.textScale = 1.1f;
            txt_ClientPort.opacity = 1;
            txt_ClientPort.color = new Color32(191, 255, 191, 255);
            txt_ClientPort.disabledColor = new Color32(115, 153, 115, 220);
            txt_ClientPort.position = new Vector3(160, -210, 0);

            //Client connect Button
            btn_ClientConnect = (UIButton)this.AddUIComponent(typeof(UIButton));
            btn_ClientConnect.text = "Connect";
            btn_ClientConnect.width = 280;
            btn_ClientConnect.height = 22;
            btn_ClientConnect.position = new Vector3(40, -240, 0);
            btn_ClientConnect.normalBgSprite = "ButtonMenu";
            btn_ClientConnect.disabledBgSprite = "ButtonMenuDisabled";
            btn_ClientConnect.hoveredBgSprite = "ButtonMenuHovered";
            btn_ClientConnect.focusedBgSprite = "ButtonMenuFocused";
            btn_ClientConnect.pressedBgSprite = "ButtonMenuPressed";
            btn_ClientConnect.textColor = new Color32(255, 51, 153, 150);
            btn_ClientConnect.disabledTextColor = new Color32(7, 7, 7, 200);
            btn_ClientConnect.hoveredTextColor = new Color32(255, 255, 255, 255);
            btn_ClientConnect.pressedTextColor = new Color32(204, 0, 0, 255);
            btn_ClientConnect.playAudioEvents = true;
            btn_ClientConnect.eventClick += btn_ClientConnect_eventClick;

            //Server Label
            lbl_Server = (UILabel)this.AddUIComponent(typeof(UILabel));
            lbl_Server.text = "Server";
            lbl_Server.textScale = 1.3f;
            lbl_Server.position = new Vector3(145, -290, 0);

            //Server players Label
            lbl_ServerPlayers = (UILabel)this.AddUIComponent(typeof(UILabel));
            lbl_ServerPlayers.text = "Players:";
            lbl_ServerPlayers.textScale = 1.1f;
            lbl_ServerPlayers.position = new Vector3(60, -325, 0);

            //Server amount of connections/player allowed 2 readonly TextField
            txt_ServerPlayers = (UITextField)this.AddUIComponent(typeof(UITextField));
            txt_ServerPlayers.width = 210;
            txt_ServerPlayers.height = 22;
            txt_ServerPlayers.text = "2";
            txt_ServerPlayers.maxLength = 48;
            txt_ServerPlayers.isEnabled = true;
            txt_ServerPlayers.builtinKeyNavigation = true;
            txt_ServerPlayers.isInteractive = true;
            txt_ServerPlayers.readOnly = true;
            txt_ServerPlayers.selectionSprite = "EmptySprite";
            txt_ServerPlayers.selectionBackgroundColor = new Color32(204, 255, 51, 255);
            txt_ServerPlayers.normalBgSprite = "TextFieldPanel";
            txt_ServerPlayers.textColor = new Color32(255, 255, 255, 255);
            txt_ServerPlayers.disabledTextColor = new Color32(153, 153, 153, 220);
            txt_ServerPlayers.textScale = 1.1f;
            txt_ServerPlayers.opacity = 1;
            txt_ServerPlayers.color = new Color32(191, 255, 191, 255);
            txt_ServerPlayers.disabledColor = new Color32(115, 153, 115, 220);
            txt_ServerPlayers.position = new Vector3(160, -325, 0);

            //Server port Label
            lbl_ServerPort = (UILabel)this.AddUIComponent(typeof(UILabel));
            lbl_ServerPort.text = "Port:";
            lbl_ServerPort.textScale = 1.1f;
            lbl_ServerPort.position = new Vector3(70, -355, 0);

            //Server port TextField
            txt_ServerPort = (UITextField)this.AddUIComponent(typeof(UITextField));
            txt_ServerPort.width = 210;
            txt_ServerPort.height = 22;
            txt_ServerPort.text = "4230";
            txt_ServerPort.maxLength = 48;
            txt_ServerPort.isEnabled = true;
            txt_ServerPort.builtinKeyNavigation = true;
            txt_ServerPort.isInteractive = true;
            txt_ServerPort.readOnly = false;
            txt_ServerPort.selectionSprite = "EmptySprite";
            txt_ServerPort.selectionBackgroundColor = new Color32(204, 255, 51, 255);
            txt_ServerPort.normalBgSprite = "TextFieldPanel";
            txt_ServerPort.textColor = new Color32(255, 255, 255, 255);
            txt_ServerPort.disabledTextColor = new Color32(153, 153, 153, 220);
            txt_ServerPort.textScale = 1.1f;
            txt_ServerPort.opacity = 1;
            txt_ServerPort.color = new Color32(191, 255, 191, 255);
            txt_ServerPort.disabledColor = new Color32(115, 153, 115, 220);
            txt_ServerPort.position = new Vector3(160, -355, 0);

            //Server start lobby Button
            btn_ServerStart = (UIButton)this.AddUIComponent(typeof(UIButton));
            btn_ServerStart.text = "Start lobby";
            btn_ServerStart.width = 280;
            btn_ServerStart.height = 22;
            btn_ServerStart.position = new Vector3(40, -385, 0);
            btn_ServerStart.normalBgSprite = "ButtonMenu";
            btn_ServerStart.disabledBgSprite = "ButtonMenuDisabled";
            btn_ServerStart.hoveredBgSprite = "ButtonMenuHovered";
            btn_ServerStart.focusedBgSprite = "ButtonMenuFocused";
            btn_ServerStart.pressedBgSprite = "ButtonMenuPressed";
            btn_ServerStart.textColor = new Color32(255, 51, 153, 150);
            btn_ServerStart.disabledTextColor = new Color32(7, 7, 7, 200);
            btn_ServerStart.hoveredTextColor = new Color32(255, 255, 255, 255);
            btn_ServerStart.pressedTextColor = new Color32(204, 0, 0, 255);
            btn_ServerStart.playAudioEvents = true;
            btn_ServerStart.eventClick += btn_ServerStart_eventClick;

            UIButton btn_Reset = (UIButton)this.AddUIComponent(typeof(UIButton));
            btn_Reset.text = "Reset";
            btn_Reset.width = 280;
            btn_Reset.height = 22;
            btn_Reset.position = new Vector3(40, -410, 0);
            btn_Reset.normalBgSprite = "ButtonMenu";
            btn_Reset.disabledBgSprite = "ButtonMenuDisabled";
            btn_Reset.hoveredBgSprite = "ButtonMenuHovered";
            btn_Reset.focusedBgSprite = "ButtonMenuFocused";
            btn_Reset.pressedBgSprite = "ButtonMenuPressed";
            btn_Reset.textColor = new Color32(255, 51, 153, 150);
            btn_Reset.disabledTextColor = new Color32(7, 7, 7, 200);
            btn_Reset.hoveredTextColor = new Color32(255, 255, 255, 255);
            btn_Reset.pressedTextColor = new Color32(204, 0, 0, 255);
            btn_Reset.playAudioEvents = true;
            btn_Reset.eventClick += btn_Reset_eventClick;

            UIButton btn_Close = (UIButton)this.AddUIComponent(typeof(UIButton));
            btn_Close.text = "Close";
            btn_Close.width = this.width;
            btn_Close.height = 22;
            btn_Close.position = new Vector3(0, -501, 0);
            btn_Close.normalBgSprite = "ButtonMenu";
            btn_Close.disabledBgSprite = "ButtonMenuDisabled";
            btn_Close.hoveredBgSprite = "ButtonMenuHovered";
            btn_Close.focusedBgSprite = "ButtonMenuFocused";
            btn_Close.pressedBgSprite = "ButtonMenuPressed";
            btn_Close.textColor = new Color32(255, 51, 153, 150);
            btn_Close.disabledTextColor = new Color32(7, 7, 7, 200);
            btn_Close.hoveredTextColor = new Color32(255, 255, 255, 255);
            btn_Close.pressedTextColor = new Color32(204, 0, 0, 255);
            btn_Close.playAudioEvents = true;
            btn_Close.eventClick += btn_Close_eventClick;
        }

        /// <summary>
        /// Starts the netServer eventClick event.
        /// </summary>
        /// <param name="component">Triggered UIComponent.</param>
        /// <param name="eventParam"></param>
        void btn_ServerStart_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            mpManager.ServerInitialize();
            if (mpManager.IsMPServerInitialized)
            {
                mpManager.MPServer.clientConnected += MPServer_clientConnectedEvent;
                mpManager.MPServer.serverStopped += MPServer_serverStoppedEvent;
                try
                {
                    Log.Message(String.Format("Trying to start a lobby on port {0} with the username {1} and password {2}", txt_ClientPort.text, txt_Username.text, txt_Password.text));
                    // Try to start the netServer on click
                    mpManager.ServerStart(port: Convert.ToInt32(txt_ServerPort.text), password: txt_Password.text, maximumPlayerAmount: Convert.ToInt32(txt_ServerPlayers.text));

                    if (mpManager.MPServer.IsServerStarted)
                    {   // Check if the netServer is started correctly.
                        Log.Message("Server lobby started! Current MPRole is " + mpManager.MPRole);
                        btn_ClientConnect.Disable();
                        btn_ServerStart.Disable();
                    }
                    else
                    {
                        Log.Message("Server lobby could not start.");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Could not start the lobby. Exception: " + ex.ToString());
                }
            }
        }

        /// <summary>
        /// Connects to the netServer eventClick event.
        /// </summary>
        /// <param name="component">Triggered UIComponent.</param>
        /// <param name="eventParam">Mouseinformaion.</param>
        void btn_ClientConnect_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            mpManager.ClientInitialize();
            if (mpManager.IsMPClientInitialized)
            {
                mpManager.MPClient.clientConnected += MPClient_clientConnectedEvent;
                mpManager.MPClient.clientDisconnected += MPClient_clientDisconnectedEvent;
                try
                {
                    Log.Message(String.Format("Trying to connect to {0}:{1} with the username _{2}_ and password _{3}_", txt_ClientIP.text, txt_ClientPort.text, txt_Username.text, txt_Password.text));
                    // Tries to connect to the netServer
                    mpManager.ClientConnect(txt_ClientIP.text, Convert.ToInt32(txt_ClientPort.text), txt_Password.text);
                    if (mpManager.MPClient.IsClientConnected)
                    {   // Check if the netClient is connected correctly
                        btn_ClientConnect.Disable();
                        btn_ServerStart.Disable();
                    }
                    else
                    {
                        Log.Message("Could not connect to " + txt_ClientIP.text + " Current MPRole: " + mpManager.MPRole);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Could not connect to server. Exception: " + ex.ToString());
                }
            }
        }

        /// <summary>
        /// Resets everything eventClick event.
        /// </summary>
        /// <param name="component">Triggered UIComponent.</param>
        /// <param name="eventParam">Mouseinformaion.</param>
        void btn_Reset_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            try
            {
                mpManager.Reset();
                btn_ClientConnect.Enable();
                btn_ServerStart.Enable();
                Log.Message("Reset completed. Current MPRole: " + mpManager.MPRole);
            }
            catch(Exception ex)
            {
                Log.Error("Reset click Exception. " + ex.ToString());
            }
        }

        /// <summary>
        /// Fires when the Close button on the configuration panel is clicked.
        /// </summary>
        /// <param name="component">Triggered UIComponent.</param>
        /// <param name="eventParam">Mouseinformation.</param>
        void btn_Close_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            base.isVisible = false;
        }

        /// <summary>
        /// Fired when a client connected to the netServer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MPServer_clientConnectedEvent(object sender, EventArgs e)
        {
            MPGlobalValues.IsConfigurationFinished = true;
        }

        /// <summary>
        /// Fired when the netServer stopped.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MPServer_serverStoppedEvent(object sender, EventArgs e)
        {
            Log.Message("ServerStoppedEvent");
            try
            {
                if (btn_ClientConnect.enabled)
                    btn_ClientConnect.Enable();
                if (btn_ServerStart.enabled)
                    btn_ServerStart.Enable();
            }
            catch (Exception ex)
            {
                Log.Error("ServerStoppedEvent error: " + ex.ToString());
            }
        }
        /// <summary>
        /// Fired when the client is 100% connected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MPClient_clientConnectedEvent(object sender, EventArgs e)
        {
            MPManager.Instance.SetMPRole(MPRoleType.Client);
            MPGlobalValues.IsConfigurationFinished = true;
        }
        /// <summary>
        /// Fired when the client is 100% disconnected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MPClient_clientDisconnectedEvent(object sender, EventArgs e)
        {
            Log.Message("ClientDisconnectedEvent");
            try
            {
                if (btn_ClientConnect.enabled)
                    btn_ClientConnect.Enable();
                if (btn_ServerStart.enabled)
                    btn_ServerStart.Enable();
            }
            catch (Exception ex)
            {
                Log.Error("ClientDisconnectedEvent error: " + ex.ToString());
            }
        }
    }
}
//Info https://media.readthedocs.org/pdf/skylines-modding-docs/master/skylines-modding-docs.pdf
// http://skylines-modding-docs.readthedocs.org/en/latest/modding/Development/How-to-Use-ColossalFramework.UI.html