using BepInEx;
using UnityEngine;
using System;
using System.Collections.Generic;

[BepInPlugin("blueguy.uipanel", "UI Panel", "1.3")]
public class Main : BaseUnityPlugin
{
    private Rect windowRect;

    private Vector2 scrollPos = Vector2.zero;
    private List<LogEntry> logs = new List<LogEntry>();

    private int currentPage = 0;
    private Color menuColor = Color.blue;

    private string inputText = "";
    private bool inputFocused = false;

    class LogEntry
    {
        public string Message;
        public LogType Type;

        public LogEntry(string message, LogType type)
        {
            Message = message;
            Type = type;
        }
    }

    void Awake()
    {
        Application.logMessageReceived += HandleLog;
    }

    void Start()
    {
        windowRect = new Rect(Screen.width - 420, 20, 400, 500);
        Log("Panel loaded");
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void OnGUI()
    {
        try
        {
            GUI.backgroundColor = menuColor;
            GUI.color = Color.white;

            windowRect = GUI.Window(0, windowRect, DrawWindow, "DEBUG PANEL");
        }
        catch (Exception ex)
        {
            Debug.LogError("GUI Exception: " + ex.Message);
        }
    }

    void DrawWindow(int id)
    {
        GUI.Box(new Rect(0, 0, windowRect.width, windowRect.height), "");

        GUILayout.Space(25);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Customization"))
            currentPage = 0;

        if (GUILayout.Button("Console"))
            currentPage = 1;

        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        if (currentPage == 0)
            DrawCustomizationPage();
        else
            DrawConsolePage();

        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }

    void DrawCustomizationPage()
    {
        GUILayout.Label("=== MENU COLOR ===");

        if (GUILayout.Button("Red")) menuColor = Color.red;
        if (GUILayout.Button("Orange")) menuColor = new Color(1f, 0.5f, 0f);
        if (GUILayout.Button("Yellow")) menuColor = Color.yellow;
        if (GUILayout.Button("Green")) menuColor = Color.green;
        if (GUILayout.Button("Blue")) menuColor = Color.blue;
        if (GUILayout.Button("Purple")) menuColor = new Color(0.6f, 0f, 1f);
        if (GUILayout.Button("Black")) menuColor = Color.black;
    }

    void DrawConsolePage()
    {
        GUILayout.Label("=== CONSOLE ===");

        bool opened = false;

        try
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(280));
            opened = true;

            for (int i = 0; i < logs.Count; i++)
            {
                if (logs[i] != null)
                    GUILayout.Label(logs[i].Message);
            }
        }
        finally
        {
            if (opened)
                GUILayout.EndScrollView();
        }

        GUILayout.Space(5);

        GUI.SetNextControlName("ConsoleInput");
        inputText = GUILayout.TextField(inputText);

        if (!inputFocused)
        {
            GUI.FocusControl("ConsoleInput");
            inputFocused = true;
        }

        Event e = Event.current;
        if (e.isKey && e.keyCode == KeyCode.Return)
        {
            SendConsoleInput();
            e.Use();
        }

        if (GUILayout.Button("Clear Logs"))
            logs.Clear();
    }

    void SendConsoleInput()
    {
        if (string.IsNullOrWhiteSpace(inputText))
            return;

        string text = inputText.Trim();

        if (text.StartsWith("{error}"))
        {
            Debug.LogError(text.Replace("{error}", "").Trim());
        }
        else if (text.StartsWith("{warning}"))
        {
            Debug.LogWarning(text.Replace("{warning}", "").Trim());
        }
        else
        {
            Debug.Log(text);
        }

        inputText = "";
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (string.IsNullOrEmpty(logString))
            return;

        if (logString.Contains("GUI Error") ||
            logString.Contains("GUIClip") ||
            logString.Contains("ArgumentException"))
            return;

        string prefix;

        switch (type)
        {
            case LogType.Warning:
                prefix = "WARNING: ";
                break;

            case LogType.Error:
            case LogType.Assert:
            case LogType.Exception:
                prefix = "ERROR: ";
                break;

            default:
                prefix = "PRINT: ";
                break;
        }

        logs.Add(new LogEntry(prefix + logString, type));

        if (logs.Count > 100)
            logs.RemoveAt(0);
    }

    void Log(string message)
    {
        logs.Add(new LogEntry("PRINT: " + message, LogType.Log));

        if (logs.Count > 100)
            logs.RemoveAt(0);
    }
}