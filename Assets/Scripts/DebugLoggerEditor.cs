#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DebugLogger))]
public class DebugLoggerEditor : Editor {
    private SerializedProperty tagSettingsProp;
    private SerializedProperty classFiltersProp;
    private SerializedProperty showStackTraceProp;
    private SerializedProperty whitelistModeProp;
    private bool showTagSettings = true;
    private bool showClassFilters = true;
    private Vector2 classListScrollPosition;
    private Vector2 tagListScrollPosition;
    private string classSearchString = "";
    private string tagSearchString = "";

    private void OnEnable() {
        tagSettingsProp = serializedObject.FindProperty("tagSettings");
        classFiltersProp = serializedObject.FindProperty("classFilters");
        showStackTraceProp = serializedObject.FindProperty("showStackTrace");
        whitelistModeProp = serializedObject.FindProperty("whitelistMode");

        // Initialize default tag settings if empty
        if (!Application.isPlaying && (tagSettingsProp.arraySize == 0 || tagSettingsProp == null)) {
            InitializeDefaultTagSettings();
        }
    }

    private void InitializeDefaultTagSettings() {
        serializedObject.Update();
        tagSettingsProp.ClearArray();

        // Define the exact order and values we want
        var orderedTags = new[] {
            DebugLogger.LogTag.UI,
            DebugLogger.LogTag.Actions,
            DebugLogger.LogTag.Effects,
            DebugLogger.LogTag.Creatures,
            DebugLogger.LogTag.Players,
            DebugLogger.LogTag.Cards,
            DebugLogger.LogTag.Combat,
            DebugLogger.LogTag.Initialization
        };

        foreach (var tag in orderedTags) {
            if (DebugLogger.DefaultColors.ContainsKey(tag)) {
                tagSettingsProp.InsertArrayElementAtIndex(tagSettingsProp.arraySize);
                var element = tagSettingsProp.GetArrayElementAtIndex(tagSettingsProp.arraySize - 1);

                // Set the raw integer value directly
                element.FindPropertyRelative("tagValue").intValue = (int)tag;
                element.FindPropertyRelative("isEnabled").boolValue = true;
                element.FindPropertyRelative("color").colorValue = DebugLogger.DefaultColors[tag];
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    public override void OnInspectorGUI() {
        if (serializedObject == null) return;
        
        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug Logger Settings", EditorStyles.boldLabel);

        // Stack Trace Toggle
        EditorGUILayout.PropertyField(showStackTraceProp);

        // Tag Settings
        EditorGUILayout.Space();
        showTagSettings = EditorGUILayout.Foldout(showTagSettings, "Tag Settings", true);
        if (showTagSettings) {
            EditorGUI.indentLevel++;

            // Tag search bar
            tagSearchString = EditorGUILayout.TextField("Search Tags", tagSearchString ?? "");

            // Tag list with scrollview
            EditorGUILayout.LabelField("Available Tags");
            tagListScrollPosition = EditorGUILayout.BeginScrollView(tagListScrollPosition, GUILayout.Height(200));

            GUI.enabled = !Application.isPlaying;

            if (tagSettingsProp != null) {
                // Filter and display tag settings
                for (int i = 0; i < tagSettingsProp.arraySize; i++) {
                    SerializedProperty tagSetting = tagSettingsProp.GetArrayElementAtIndex(i);
                    if (tagSetting == null) continue;

                    SerializedProperty tagValueProp = tagSetting.FindPropertyRelative("tagValue");
                    SerializedProperty isEnabledProp = tagSetting.FindPropertyRelative("isEnabled");
                    SerializedProperty colorProp = tagSetting.FindPropertyRelative("color");

                    if (tagValueProp == null || isEnabledProp == null || colorProp == null) continue;

                    // Get the current tag name
                    var currentTag = (DebugLogger.LogTag)tagValueProp.intValue;
                    string tagName = currentTag.ToString();

                    // Apply tag search filter
                    if (!string.IsNullOrEmpty(tagSearchString) &&
                        !tagName.ToLower().Contains(tagSearchString.ToLower())) {
                        continue;
                    }

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(isEnabledProp, GUIContent.none, GUILayout.Width(10));
                    EditorGUILayout.LabelField(tagName, GUILayout.ExpandWidth(true));
                    EditorGUILayout.PropertyField(colorProp, GUIContent.none, GUILayout.Width(100));
                    EditorGUILayout.EndHorizontal();
                }
            }

            GUI.enabled = true;
            EditorGUILayout.EndScrollView();

            // Add a button to reset to default colors
            if (GUILayout.Button("Reset to Default Colors")) {
                ResetTagColors();
            }

            EditorGUI.indentLevel--;
        }

        // Class Filters
        EditorGUILayout.Space();
        showClassFilters = EditorGUILayout.Foldout(showClassFilters, "Class Filters", true);
        if (showClassFilters) {
            EditorGUI.indentLevel++;

            // Whitelist/Blacklist mode toggle
            EditorGUILayout.PropertyField(whitelistModeProp, new GUIContent("Whitelist Mode",
                "When enabled, only selected classes will be logged. When disabled, selected classes will be blocked from logging."));

            // Search bar
            classSearchString = EditorGUILayout.TextField("Search Classes", classSearchString ?? "");

            // Class list with checkboxes
            EditorGUILayout.LabelField("Available Classes");
            classListScrollPosition = EditorGUILayout.BeginScrollView(classListScrollPosition,
                GUILayout.Height(300));

            var filteredClasses = DebugLogger.AvailableClasses
                .Where(c => string.IsNullOrEmpty(classSearchString) ||
                           c.ToLower().Contains(classSearchString.ToLower()));

            foreach (var className in filteredClasses) {
                bool isEnabled = IsClassEnabled(className);
                bool newEnabled = EditorGUILayout.ToggleLeft(className, isEnabled);

                if (newEnabled != isEnabled) {
                    if (newEnabled) {
                        AddClassFilter(className);
                    } else {
                        RemoveClassFilter(className);
                    }
                }
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button(whitelistModeProp.boolValue ? "Select All Classes" : "Deselect All Classes")) {
                ToggleAllClasses(whitelistModeProp.boolValue);
            }

            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void ResetTagColors() {
        if (tagSettingsProp == null) return;

        for (int i = 0; i < tagSettingsProp.arraySize; i++) {
            SerializedProperty tagSetting = tagSettingsProp.GetArrayElementAtIndex(i);
            if (tagSetting == null) continue;

            SerializedProperty tagValueProp = tagSetting.FindPropertyRelative("tagValue");
            SerializedProperty colorProp = tagSetting.FindPropertyRelative("color");
            
            if (tagValueProp == null || colorProp == null) continue;

            var currentTag = (DebugLogger.LogTag)tagValueProp.intValue;
            if (DebugLogger.DefaultColors.TryGetValue(currentTag, out Color defaultColor)) {
                colorProp.colorValue = defaultColor;
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private bool IsClassEnabled(string className) {
        if (classFiltersProp == null) return !whitelistModeProp.boolValue;
        
        for (int i = 0; i < classFiltersProp.arraySize; i++) {
            var filter = classFiltersProp.GetArrayElementAtIndex(i);
            var classNameProp = filter.FindPropertyRelative("className");
            var isEnabledProp = filter.FindPropertyRelative("isEnabled");
            
            if (classNameProp.stringValue == className) {
                return isEnabledProp.boolValue;
            }
        }
        
        // If no explicit setting is found, return true for whitelist mode (all selected by default)
        return whitelistModeProp.boolValue;
    }

    private void AddClassFilter(string className) {
        // Check if class filter already exists
        for (int i = 0; i < classFiltersProp.arraySize; i++) {
            var filter = classFiltersProp.GetArrayElementAtIndex(i);
            if (filter.FindPropertyRelative("className").stringValue == className) {
                filter.FindPropertyRelative("isEnabled").boolValue = true;
                return;
            }
        }

        // Add new class filter
        classFiltersProp.InsertArrayElementAtIndex(classFiltersProp.arraySize);
        var newFilter = classFiltersProp.GetArrayElementAtIndex(classFiltersProp.arraySize - 1);
        newFilter.FindPropertyRelative("className").stringValue = className;
        newFilter.FindPropertyRelative("isEnabled").boolValue = true;
    }

    private void RemoveClassFilter(string className) {
        // Always add an explicit entry when removing in whitelist mode
        if (whitelistModeProp.boolValue) {
            // Check if class filter already exists
            for (int i = 0; i < classFiltersProp.arraySize; i++) {
                var filter = classFiltersProp.GetArrayElementAtIndex(i);
                if (filter.FindPropertyRelative("className").stringValue == className) {
                    filter.FindPropertyRelative("isEnabled").boolValue = false;
                    return;
                }
            }

            // If no entry exists, create one with disabled state
            classFiltersProp.InsertArrayElementAtIndex(classFiltersProp.arraySize);
            var newFilter = classFiltersProp.GetArrayElementAtIndex(classFiltersProp.arraySize - 1);
            newFilter.FindPropertyRelative("className").stringValue = className;
            newFilter.FindPropertyRelative("isEnabled").boolValue = false;
        } else {
            // In blacklist mode, just set to disabled if exists
            for (int i = 0; i < classFiltersProp.arraySize; i++) {
                var filter = classFiltersProp.GetArrayElementAtIndex(i);
                if (filter.FindPropertyRelative("className").stringValue == className) {
                    filter.FindPropertyRelative("isEnabled").boolValue = false;
                    return;
                }
            }
        }
    }

    private void ToggleAllClasses(bool enable) {
        // Clear existing filters
        classFiltersProp.ClearArray();

        // Add all classes with the specified enabled state
        foreach (var className in DebugLogger.AvailableClasses) {
            classFiltersProp.InsertArrayElementAtIndex(classFiltersProp.arraySize);
            var filter = classFiltersProp.GetArrayElementAtIndex(classFiltersProp.arraySize - 1);
            filter.FindPropertyRelative("className").stringValue = className;
            filter.FindPropertyRelative("isEnabled").boolValue = enable;
        }
    }
}
#endif