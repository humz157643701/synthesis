﻿using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Threading.Tasks;

namespace Assets.Scripts.GUI
{
    /// <summary>
    /// This state controls the emulation toolbar and interface to starting emulation robot code.
    /// </summary>
    public class EmulationToolbarState : State
    {
        public static bool exiting = false;

        EmulationDriverStation emulationDriverStation;

        bool loaded = false;
        float lastAdditionalDot = 0;
        int dotCount = 0;

        GameObject canvas;
        GameObject tabs;
        GameObject emulationToolbar;
        GameObject loadingPanel = null;

        GameObject helpMenu;
        GameObject overlay;
        Text helpBodyText;

        public override void Start()
        {
            emulationDriverStation = StateMachine.SceneGlobal.GetComponent<EmulationDriverStation>();
            canvas = GameObject.Find("Canvas");
            tabs = Auxiliary.FindObject(canvas, "Tabs");
            emulationToolbar = Auxiliary.FindObject(canvas, "EmulationToolbar");
            loadingPanel = Auxiliary.FindObject(canvas, "LoadingPanel");

            helpMenu = Auxiliary.FindObject(canvas, "Help");
            overlay = Auxiliary.FindObject(canvas, "Overlay");
            helpBodyText = Auxiliary.FindObject(canvas, "BodyText").GetComponent<Text>();

            Button helpButton = Auxiliary.FindObject(helpMenu, "CloseHelpButton").GetComponent<Button>();
            helpButton.onClick.RemoveAllListeners();
            helpButton.onClick.AddListener(CloseHelpMenu);
        }

        public override void FixedUpdate()
        {
            if (loadingPanel.activeSelf)
            {
                Text t = loadingPanel.transform.Find("Text").GetComponent<Text>();

                if (loaded)
                {
                    t.text = "Loading...";
                    loadingPanel.SetActive(false);
                    loaded = false;
                } else
                {
                    if (Time.unscaledTime >= lastAdditionalDot + 0.75)
                    {
                        dotCount = (dotCount + 1) % 4;
                        t.text = "Loading";
                        for (int i = 0; i < dotCount; i++)
                        {
                            t.text += ".";
                        }
                        lastAdditionalDot = Time.unscaledTime;
                    }
                }
            }
        }

        /// <summary>
        /// Selects robot code and starts VM. 
        /// </summary>
        public void OnSelectRobotCodeButtonClicked()
        {
            if (!Synthesis.EmulatorManager.IsVMConnected())
            {
                return;
            }
            LoadCode();
        }

        public async void LoadCode()
        {
            string[] selectedFiles = SFB.StandaloneFileBrowser.OpenFilePanel("Robot Code Executable", "C:\\", "", false);
            if (selectedFiles.Length != 1)
            {
                Debug.Log("No files selected for robot code upload");
            }
            else
            {
                Synthesis.EmulatorManager.UserProgram userProgram = new Synthesis.EmulatorManager.UserProgram(selectedFiles[0]);
                if (userProgram.type == Synthesis.EmulatorManager.UserProgram.UserProgramType.JAVA) // TODO remove this once support is added
                {
                    emulationDriverStation.ShowJavaNotSupportedPopUp();
                }
                else
                {
                    if(Synthesis.EmulatorManager.IsRunningRobotCode())
                        emulationDriverStation.ToggleRobotCodeButton();
                    loadingPanel.SetActive(true);
                    Task Upload = Task.Factory.StartNew(() =>
                    {
                        Synthesis.EmulatorManager.SCPFileSender(userProgram);
                        loaded = true;
                    });
                    await Upload;
                }
            }

            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.SelectCode,
                AnalyticsLedger.EventAction.Clicked,
                "",
                AnalyticsLedger.getMilliseconds().ToString());
        }

        /// <summary>
        /// Opens the Synthesis Driver Station for emulation
        /// </summary>
        public void OnDriverStationButtonClicked()
        {
            emulationDriverStation.ToggleDriverStation();

            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.DriverStation,
                AnalyticsLedger.EventAction.Clicked,
                "",
                AnalyticsLedger.getMilliseconds().ToString());
        }

        public void OnStartRobotCodeButtonClicked()
        {
            emulationDriverStation.ToggleRobotCodeButton();

            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.RunCode,
                AnalyticsLedger.EventAction.Start,
                "",
                AnalyticsLedger.getMilliseconds().ToString());
        }

        public void OnVMConnectionStatusClicked()
        {
            if (!Synthesis.EmulatorManager.IsVMRunning() && !Synthesis.EmulatorManager.IsVMConnected())
            {
                Synthesis.EmulatorManager.StartEmulator();
                emulationDriverStation.BeginTrackingVMConnectionStatus();
            }
        }

        #region Help Button and Menu
        public void OnHelpButtonClicked()
        {
            helpMenu.SetActive(true);

            // Used to change the text of emulation help menu
            helpBodyText.GetComponent<Text>().text = "\n\nSelect Code: Select the user program file to upload. Uploading may take a couple seconds." +
                "\n\nDriver Station: Access an FRC driver station-like tool to manipulate robot running state." +
                "\n\nStart Code / Stop Code: Run or kill user program in VM. It may take a second to start and connect to the user program." +
                "\n\nVM Connection status: Shows connection status to VM and user program. Running user program is disabled until connection is established.";

            Auxiliary.FindObject(helpMenu, "Type").GetComponent<Text>().text = "EmulationToolbar";
            overlay.SetActive(true);
            tabs.transform.Translate(new Vector3(300, 0, 0));
            foreach (Transform t in emulationToolbar.transform)
            {
                if (t.gameObject.name != "HelpButton") t.Translate(new Vector3(300, 0, 0));
                else t.gameObject.SetActive(false);
            }

            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.EmulationHelp,
                AnalyticsLedger.EventAction.Clicked,
                "",
                AnalyticsLedger.getMilliseconds().ToString());
        }

        private void CloseHelpMenu()
        {
            helpMenu.SetActive(false);
            overlay.SetActive(false);
            tabs.transform.Translate(new Vector3(-300, 0, 0));
            foreach (Transform t in emulationToolbar.transform)
            {
                if (t.gameObject.name != "HelpButton") t.Translate(new Vector3(-300, 0, 0));
                else t.gameObject.SetActive(true);
            }
        }
        #endregion

    }
}
