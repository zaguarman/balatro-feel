using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using static DebugLogger;

[Serializable]
public class TagSettings {
    [SerializeField] private int tagValue;
    public bool isEnabled = true;
    public Color color;

    public LogTag tag {
        get => (LogTag)tagValue;
        set => tagValue = (int)value;
    }

    public string HexColor => "#" + ColorUtility.ToHtmlStringRGB(color);
}

[Serializable]
public class ClassFilter {
    public string className;
    public bool isEnabled = true;
}

public class DebugLogger : MonoBehaviour {
    [Flags]
    public enum LogTag {
        None = 0,
        UI = 1,
        Actions = 2,
        Effects = 4,
        Creatures = 8,
        Players = 16,
        Cards = 32,
        Combat = 64,
        Initialization = 128,
        All = ~0
    }

    public static readonly Dictionary<LogTag, Color> DefaultColors = new Dictionary<LogTag, Color>() {
        { LogTag.UI, GetColorFromHex("#80FFFF") },
        { LogTag.Actions, GetColorFromHex("#FFE066") },
        { LogTag.Effects, GetColorFromHex("#FF99FF") },
        { LogTag.Creatures, GetColorFromHex("#90EE90") },
        { LogTag.Players, GetColorFromHex("#ADD8E6") },
        { LogTag.Cards, GetColorFromHex("#E0E0E0") },
        { LogTag.Combat, GetColorFromHex("#FF9999") },
        { LogTag.Initialization, GetColorFromHex("#DEB887") },
    };

    public static readonly string[] AvailableClasses = new string[] {
        "ActionsQueue",
        "ArrowIndicator",
        "BattlefieldArrowManager",
        "BattlefieldCombatHandler",
        "BattlefieldUI",
        "Card",
        "CardContainer",
        "CardController",
        "CardData",
        "CardDealingService",
        "CardDropZone",
        "CardFactory",
        "ContainerSettings",
        "Creature",
        "Deck",
        "Entity",
        "GameActions",
        "GameBootstrap",
        "GameManager",
        "GameMediator",
        "GameReferences",
        "GameUI",
        "HandUI",
        "HealthHandler",
        "Player",
        "PlayerUI",
        "Target",
        "TestSetup",
        "UIComponent"
    };

    [SerializeField] private List<TagSettings> tagSettings = new List<TagSettings>();
    [SerializeField] private List<ClassFilter> classFilters = new List<ClassFilter>();
    [SerializeField] private bool showStackTrace = false;
    [SerializeField] private bool whitelistMode = false;

    private Dictionary<LogTag, string> _tagColorMap;
    private HashSet<string> _enabledClasses;
    private static DebugLogger _instance;

    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeLogger();
    }

    private void InitializeStackTraceLogTypes() {
        var stackTraceType = showStackTrace ? StackTraceLogType.ScriptOnly : StackTraceLogType.None;

        Application.SetStackTraceLogType(LogType.Log, stackTraceType);
        Application.SetStackTraceLogType(LogType.Warning, stackTraceType);
        Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.Full);
        Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.Full);
        Application.SetStackTraceLogType(LogType.Assert, StackTraceLogType.Full);
    }

    private void OnValidate() {
        if (Application.isPlaying) {
            InitializeLogger();
        }
    }

    private static Color GetColorFromHex(string hex) {
        Color color;
        ColorUtility.TryParseHtmlString(hex, out color);
        return color;
    }

    public static void Log(
        object message,
        LogTag tags = LogTag.All,
        [CallerFilePath] string sourceFilePath = "") {
        _instance?.LogWithType(LogType.Log, message, tags, sourceFilePath);
    }

    public static void LogWarning(
        object message,
        LogTag tags = LogTag.All,
        [CallerFilePath] string sourceFilePath = "") {
        _instance?.LogWithType(LogType.Warning, message, tags, sourceFilePath);
    }

    public static void LogError(
        object message,
        LogTag tags = LogTag.All,
        [CallerFilePath] string sourceFilePath = "") {
        _instance?.LogWithType(LogType.Error, message, tags, sourceFilePath);
    }

    private void LogWithType(
        LogType logType,
        object message,
        LogTag tags,
        string sourceFilePath) {
        if (!ShouldLog(tags, sourceFilePath)) return;

        string formattedMessage = FormatMessage(message, tags, sourceFilePath);

        switch (logType) {
            case LogType.Log:
                Debug.Log(formattedMessage);
                break;
            case LogType.Warning:
                Debug.LogWarning(formattedMessage);
                break;
            case LogType.Error:
                Debug.LogError(formattedMessage);
                break;
        }
    }

    private void InitializeLogger() {
        // Initialize tag settings if empty
        if (tagSettings == null || tagSettings.Count == 0) {
            tagSettings = new List<TagSettings>();
            foreach (LogTag tag in Enum.GetValues(typeof(LogTag))) {
                // Explicitly exclude None and All
                if (tag != LogTag.None && tag != LogTag.All && DefaultColors.ContainsKey(tag)) {
                    tagSettings.Add(new TagSettings {
                        tag = tag,
                        isEnabled = true,
                        color = DefaultColors[tag]
                    });
                }
            }
        }

        // Create tag color map ensuring no duplicates
        _tagColorMap = new Dictionary<LogTag, string>();
        foreach (var setting in tagSettings) {
            if (!_tagColorMap.ContainsKey(setting.tag)) {
                _tagColorMap[setting.tag] = setting.HexColor;
            }
        }

        // Initialize class filters if empty
        if (classFilters == null) {
            classFilters = new List<ClassFilter>();
        }

        // Create enabled classes set
        _enabledClasses = new HashSet<string>(
            classFilters
                .Where(cf => cf.isEnabled && !string.IsNullOrEmpty(cf.className))
                .Select(cf => cf.className)
        );

        InitializeStackTraceLogTypes();
    }

    private bool ShouldLog(LogTag tags, string sourceFilePath) {
        // Safety check for initialization
        if (_tagColorMap == null || _enabledClasses == null) {
            InitializeLogger();
        }

        // Check if any of the tags are enabled
        bool anyTagEnabled = false;
        foreach (var setting in tagSettings) {
            if (setting.isEnabled && (tags & setting.tag) != 0) {
                anyTagEnabled = true;
                break;
            }
        }

        if (!anyTagEnabled) return false;

        string className = GetClassName(sourceFilePath);

        // If whitelist mode is enabled and no classes are in the enabled set, block all logging
        if (whitelistMode && _enabledClasses.Count == 0) {
            return false;
        }

        // If whitelist mode is enabled, only allow listed classes
        if (whitelistMode) {
            return _enabledClasses.Contains(className);
        }
        // If blacklist mode is enabled, block listed classes
        else {
            return !_enabledClasses.Contains(className);
        }
    }

    private string FormatMessage(
        object message,
        LogTag tags,
        string sourceFilePath) {
        string className = GetClassName(sourceFilePath);
        string tagList = GetTagList(tags);
        string coloredTags = ColorizeTags(tagList, tags);

        return $"{className}: [{coloredTags}] {message}";
    }

    private string GetClassName(string sourceFilePath) {
        if (string.IsNullOrEmpty(sourceFilePath)) return "Unknown";
        return System.IO.Path.GetFileNameWithoutExtension(sourceFilePath);
    }

    private string GetTagList(LogTag tags) {
        // Explicitly filter out None and All
        return string.Join("|",
            Enum.GetValues(typeof(LogTag))
                .Cast<LogTag>()
                .Where(tag => tag != LogTag.None && tag != LogTag.All && (tags & tag) != 0)
                .Select(tag => tag.ToString())
        );
    }

    private string ColorizeTags(string tagList, LogTag tags) {
        string[] individualTags = tagList.Split('|');

        return string.Join("|", individualTags.Select(tag =>
            Enum.TryParse(tag, out LogTag currentTag) && _tagColorMap.TryGetValue(currentTag, out string color)
                ? $"<color={color}>{tag}</color>"
                : tag
        ));
    }
}