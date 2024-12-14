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
    private SerializedProperty tagWhitelistModeProp;
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
        tagWhitelistModeProp = serializedObject.FindProperty("tagWhitelistMode");

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

            // Tag whitelist mode toggle and select all button
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(tagWhitelistModeProp, new GUIContent("Tag Whitelist Mode"));
            if (GUILayout.Button(allTagsSelected ? "Deselect All" : "Select All", GUILayout.Width(120))) {
                ToggleAllTags();
            }
            EditorGUILayout.EndHorizontal();

            // Tag search bar
            tagSearchString = EditorGUILayout.TextField("Search", tagSearchString ?? "");

            // Tag list with scrollview
            EditorGUILayout.LabelField("Available Tags");
            tagListScrollPosition = EditorGUILayout.BeginScrollView(tagListScrollPosition, GUILayout.Height(200));

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

            EditorGUILayout.EndScrollView();

            // Reset colors button
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

            // Class whitelist mode toggle and select all button
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(whitelistModeProp, new GUIContent("Whitelist Mode"));
            if (GUILayout.Button(allClassesSelected ? "Deselect All" : "Select All", GUILayout.Width(120))) {
                ToggleAllClasses();
            }
            EditorGUILayout.EndHorizontal();

            // Search bar
            classSearchString = EditorGUILayout.TextField("Search", classSearchString ?? "");

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



            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private bool allTagsSelected = true;

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

    private bool allClassesSelected = true;

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