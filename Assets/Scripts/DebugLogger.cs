using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System;
using UnityEngine;

public static class DebugLogger {

    static DebugLogger() {
        _enabledTags = LogTag.UI | LogTag.Cards | LogTag.Actions | LogTag.Effects;
        _tagColors = new Dictionary<LogTag, string>(_defaultTagColors);
        InitializeStackTraceLogTypes();
    }

    private static void InitializeStackTraceLogTypes() {
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
        Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.Full);
        Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.Full);
        Application.SetStackTraceLogType(LogType.Assert, StackTraceLogType.Full);
    }

    [Flags]
    public enum LogTag {
        None = 0,
        UI = 1 << 0,
        Actions = 1 << 1,
        Effects = 1 << 2,
        Creatures = 1 << 3,
        Players = 1 << 4,
        Cards = 1 << 5,
        Combat = 1 << 6,
        Initialization = 1 << 7,
        Network = 1 << 8,
        Economy = 1 << 9,
        All = ~0
    }

    private static readonly Dictionary<LogTag, string> _defaultTagColors = new Dictionary<LogTag, string>
    {
        { LogTag.UI, "#80FFFF" },
        { LogTag.Actions, "#FFE066" },
        { LogTag.Effects, "#FF99FF" },
        { LogTag.Creatures, "#90EE90" },
        { LogTag.Players, "#ADD8E6" },
        { LogTag.Cards, "#E0E0E0" },
        { LogTag.Combat, "#FF9999" },
        { LogTag.Initialization, "#DEB887" },
        { LogTag.Network, "#98FB98" },
        { LogTag.Economy, "#DDA0DD" }
    };

    private static LogTag _enabledTags;
    private static Dictionary<LogTag, string> _tagColors;

    public static void EnableTags(LogTag tags) {
        _enabledTags |= tags;
    }

    public static void DisableTags(LogTag tags) {
        _enabledTags &= ~tags;
    }

    public static void SetTagColor(LogTag tag, string hexColor) {
        _tagColors[tag] = hexColor;
    }

    #region Log Methods
    public static void Log(
        object message,
        LogTag tags = LogTag.All,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "") {
        LogWithType(LogType.Log, message, tags, memberName, sourceFilePath);
    }

    public static void LogWarning(
        object message,
        LogTag tags,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "") {
        LogWithType(LogType.Warning, message, tags, memberName, sourceFilePath);
    }

    public static void LogError(
        object message,
        LogTag tags,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "") {
        LogWithType(LogType.Error, message, tags, memberName, sourceFilePath);
    }

    private static void LogWithType(
        LogType logType,
        object message,
        LogTag tags,
        string memberName,
        string sourceFilePath) {
        if (!ShouldLog(tags)) return;

        string formattedMessage = FormatMessage(message, tags, memberName, sourceFilePath);

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
    #endregion

    private static bool ShouldLog(LogTag tags) {
        return (_enabledTags & tags) != 0;
    }

    private static string FormatMessage(
        object message,
        LogTag tags,
        string memberName,
        string sourceFilePath) {
        string className = GetClassName(sourceFilePath);
        string tagList = GetTagList(tags);
        string coloredTags = ColorizeTags(tagList, tags);

        return $"{className}: [{coloredTags}] {message}";
    }

    private static string GetClassName(string sourceFilePath) {
        if (string.IsNullOrEmpty(sourceFilePath)) return "Unknown";
        return System.IO.Path.GetFileNameWithoutExtension(sourceFilePath);
    }

    private static string GetTagList(LogTag tags) {
        return string.Join("|",
            Enum.GetValues(typeof(LogTag))
                .Cast<LogTag>()
                .Where(tag => tag != LogTag.None && tag != LogTag.All && (tags & tag) == tag)
                .Select(tag => tag.ToString())
        );
    }

    private static string ColorizeTags(string tagList, LogTag tags) {
        string[] individualTags = tagList.Split('|');

        return string.Join("|", individualTags.Select(tag =>
            Enum.TryParse(tag, out LogTag currentTag) && _tagColors.TryGetValue(currentTag, out string color)
                ? $"<color={color}>{tag}</color>"
                : tag
        ));
    }
}