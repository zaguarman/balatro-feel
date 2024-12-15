#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor(typeof(DebugLogger))]
public class DebugLoggerEditor : Editor {
    private SerializedProperty settingsProp;
    private Editor settingsEditor;

    private void OnEnable() {
        settingsProp = serializedObject.FindProperty("settings");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        EditorGUILayout.PropertyField(settingsProp);

        if (settingsProp.objectReferenceValue != null) {
            CreateCachedEditor(settingsProp.objectReferenceValue, null, ref settingsEditor);
            settingsEditor.OnInspectorGUI();
        }

        serializedObject.ApplyModifiedProperties();
    }
}

[CustomEditor(typeof(DebugLoggerSettings))]
public class DebugLoggerSettingsEditor : Editor {
    private SerializedProperty tagSettingsProp;
    private SerializedProperty classFiltersProp;
    private SerializedProperty showStackTraceProp;
    private SerializedProperty whitelistModeProp;
    private SerializedProperty tagWhitelistModeProp;
    private bool showTagSettings = true;
    private bool showClassFilters = true;
    private Vector2 classListScrollPosition;
    private Vector2 tagListScrollPosition;
    private string classSearchString = "";
    private string tagSearchString = "";
    private bool allTagsSelected = false;
    private bool allClassesSelected = false;

    private void OnEnable() {
        tagSettingsProp = serializedObject.FindProperty("tagSettings");
        classFiltersProp = serializedObject.FindProperty("classFilters");
        showStackTraceProp = serializedObject.FindProperty("showStackTrace");
        whitelistModeProp = serializedObject.FindProperty("whitelistMode");
        tagWhitelistModeProp = serializedObject.FindProperty("tagWhitelistMode");

        // Initialize default tag settings if empty
        if (!Application.isPlaying && (tagSettingsProp.arraySize == 0 || tagSettingsProp == null)) {
            InitializeDefaultTagSettings();
        }
    }

    private void InitializeDefaultTagSettings() {
        serializedObject.Update();
        tagSettingsProp.ClearArray();

        var orderedTags = new[]
        {
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

                element.FindPropertyRelative("tagValue").intValue = (int)tag;
                element.FindPropertyRelative("isEnabled").boolValue = true;
                element.FindPropertyRelative("color").colorValue = DebugLogger.DefaultColors[tag];
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    public override void OnInspectorGUI() {
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
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(tagWhitelistModeProp, new GUIContent("Whitelist"));
            if (GUILayout.Button(allTagsSelected ? "Deselect All" : "Select All", GUILayout.Width(100))) {
                ToggleAllTags();
            }
            EditorGUILayout.EndHorizontal();

            // Tag search bar
            tagSearchString = EditorGUILayout.TextField("Search", tagSearchString ?? "");

            // Tag list with scrollview
            EditorGUILayout.LabelField("Available Tags");
            tagListScrollPosition = EditorGUILayout.BeginScrollView(tagListScrollPosition, GUILayout.Height(200));
            if (tagSettingsProp != null) {
                for (int i = 0; i < tagSettingsProp.arraySize; i++) {
                    SerializedProperty tagSetting = tagSettingsProp.GetArrayElementAtIndex(i);
                    if (tagSetting == null) continue;

                    SerializedProperty tagValueProp = tagSetting.FindPropertyRelative("tagValue");
                    SerializedProperty isEnabledProp = tagSetting.FindPropertyRelative("isEnabled");
                    SerializedProperty colorProp = tagSetting.FindPropertyRelative("color");
                    if (tagValueProp == null || isEnabledProp == null || colorProp == null) continue;

                    var currentTag = (DebugLogger.LogTag)tagValueProp.intValue;
                    string tagName = currentTag.ToString();

                    if (!string.IsNullOrEmpty(tagSearchString) &&
                        !tagName.ToLower().Contains(tagSearchString.ToLower())) {
                        continue;
                    }

                    EditorGUILayout.BeginHorizontal();
                    float minTextWidth = GUI.skin.toggle.CalcSize(new GUIContent(tagName)).x + 15;
                    bool newEnabled = EditorGUILayout.ToggleLeft(tagName, isEnabledProp.boolValue, GUILayout.Width(minTextWidth));
                    if (newEnabled != isEnabledProp.boolValue) {
                        isEnabledProp.boolValue = newEnabled;
                    }

                    GUILayout.FlexibleSpace();
                    EditorGUILayout.PropertyField(colorProp, GUIContent.none, GUILayout.Width(100));
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Reset Colors")) {
                ResetTagColors();
            }
            EditorGUI.indentLevel--;
        }

        // Class Filters
        EditorGUILayout.Space();
        showClassFilters = EditorGUILayout.Foldout(showClassFilters, "Class Filters", true);
        if (showClassFilters) {
            EditorGUI.indentLevel++;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(whitelistModeProp, new GUIContent("Whitelist"));
            if (GUILayout.Button(allClassesSelected ? "Deselect All" : "Select All", GUILayout.Width(100))) {
                ToggleAllClasses();
            }
            EditorGUILayout.EndHorizontal();

            classSearchString = EditorGUILayout.TextField("Search", classSearchString ?? "");

            EditorGUILayout.LabelField("Available Classes");
            classListScrollPosition = EditorGUILayout.BeginScrollView(classListScrollPosition, GUILayout.Height(300));

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

            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void ToggleAllTags() {
        if (tagSettingsProp == null) return;

        allTagsSelected = !allTagsSelected;

        for (int i = 0; i < tagSettingsProp.arraySize; i++) {
            var tagSetting = tagSettingsProp.GetArrayElementAtIndex(i);
            if (tagSetting != null) {
                var isEnabledProp = tagSetting.FindPropertyRelative("isEnabled");
                if (isEnabledProp != null) {
                    isEnabledProp.boolValue = allTagsSelected;
                }
            }
        }
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

        return whitelistModeProp.boolValue;
    }

    private void AddClassFilter(string className) {
        for (int i = 0; i < classFiltersProp.arraySize; i++) {
            var filter = classFiltersProp.GetArrayElementAtIndex(i);
            if (filter.FindPropertyRelative("className").stringValue == className) {
                filter.FindPropertyRelative("isEnabled").boolValue = true;
                return;
            }
        }

        classFiltersProp.InsertArrayElementAtIndex(classFiltersProp.arraySize);
        var newFilter = classFiltersProp.GetArrayElementAtIndex(classFiltersProp.arraySize - 1);
        newFilter.FindPropertyRelative("className").stringValue = className;
        newFilter.FindPropertyRelative("isEnabled").boolValue = true;
    }

    private void RemoveClassFilter(string className) {
        if (whitelistModeProp.boolValue) {
            for (int i = 0; i < classFiltersProp.arraySize; i++) {
                var filter = classFiltersProp.GetArrayElementAtIndex(i);
                if (filter.FindPropertyRelative("className").stringValue == className) {
                    filter.FindPropertyRelative("isEnabled").boolValue = false;
                    return;
                }
            }

            classFiltersProp.InsertArrayElementAtIndex(classFiltersProp.arraySize);
            var newFilter = classFiltersProp.GetArrayElementAtIndex(classFiltersProp.arraySize - 1);
            newFilter.FindPropertyRelative("className").stringValue = className;
            newFilter.FindPropertyRelative("isEnabled").boolValue = false;
        } else {
            for (int i = 0; i < classFiltersProp.arraySize; i++) {
                var filter = classFiltersProp.GetArrayElementAtIndex(i);
                if (filter.FindPropertyRelative("className").stringValue == className) {
                    filter.FindPropertyRelative("isEnabled").boolValue = false;
                    return;
                }
            }
        }
    }

    private void ToggleAllClasses() {
        allClassesSelected = !allClassesSelected;
        classFiltersProp.ClearArray();

        foreach (var className in DebugLogger.AvailableClasses) {
            classFiltersProp.InsertArrayElementAtIndex(classFiltersProp.arraySize);
            var filter = classFiltersProp.GetArrayElementAtIndex(classFiltersProp.arraySize - 1);
            filter.FindPropertyRelative("className").stringValue = className;
            filter.FindPropertyRelative("isEnabled").boolValue = allClassesSelected;
        }
    }
}
#endif