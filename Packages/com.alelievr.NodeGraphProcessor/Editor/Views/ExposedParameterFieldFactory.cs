using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphProcessor {
// So, this is a workaround class to add a wrapper around PropertyFields applied on [SerializeReference].
// Because Property Fields binding being extremely slow (https://forum.unity.com/threads/propertyfield-extremely-slow.966191/)
// and AppliedModifiedProperties() re-creating the ScriptableObject when called (which in NGP causes the graph to be re-built)
// we can't use PropertyFields directly. This class provides a set of function to create PropertyFields for Exposed Parameters
// but without being attached to the graph, so when we call AppliedModifiedProperties, the graph is not re-built.
// The drawback is that we have to check ourselves for value changes and then apply them on the graph parameters,
// but it's far better than having to re-create the graph every time a parameter or a setting is changed.
public class ExposedParameterFieldFactory : IDisposable {
    [SerializeField] private readonly ExposedParameterWorkaround exposedParameterObject;
    private readonly BaseGraph graph;
    private readonly Dictionary<ExposedParameter, ExposedParameter.Settings> oldParameterSettings = new();

    private readonly Dictionary<ExposedParameter, object> oldParameterValues = new();
    private readonly SerializedObject serializedObject;
    private SerializedProperty serializedParameters;

    public ExposedParameterFieldFactory(BaseGraph graph, List<ExposedParameter> customParameters = null) {
        this.graph = graph;

        exposedParameterObject = ScriptableObject.CreateInstance<ExposedParameterWorkaround>();
        exposedParameterObject.graph = graph;
        exposedParameterObject.hideFlags = HideFlags.HideAndDontSave ^ HideFlags.NotEditable;
        serializedObject = new SerializedObject(exposedParameterObject);
        UpdateSerializedProperties(customParameters);
    }

    public void Dispose() {
        GameObject.DestroyImmediate(exposedParameterObject);
    }

    public void UpdateSerializedProperties(List<ExposedParameter> parameters = null) {
        if (parameters != null)
            exposedParameterObject.parameters = parameters;
        else
            exposedParameterObject.parameters = graph.exposedParameters;
        serializedObject.Update();
        serializedParameters = serializedObject.FindProperty(nameof(ExposedParameterWorkaround.parameters));
    }

    public VisualElement GetParameterValueField(ExposedParameter parameter, Action<object> valueChangedCallback) {
        serializedObject.Update();
        var propIndex = FindPropertyIndex(parameter);
        var field = new PropertyField(serializedParameters.GetArrayElementAtIndex(propIndex));
        field.Bind(serializedObject);

        var view = new VisualElement();
        view.Add(field);

        oldParameterValues[parameter] = parameter.value;
        view.Add(new IMGUIContainer(() => {
            if (oldParameterValues.TryGetValue(parameter, out var value))
                if (parameter.value != null && !parameter.value.Equals(value))
                    valueChangedCallback(parameter.value);
            oldParameterValues[parameter] = parameter.value;
        }));

        // Disallow picking scene objects when the graph is not linked to a scene
        if (!graph.IsLinkedToScene()) {
            var objectField = view.Q<ObjectField>();
            if (objectField != null)
                objectField.allowSceneObjects = false;
        }

        return view;
    }

    public VisualElement GetParameterSettingsField(ExposedParameter parameter, Action<object> valueChangedCallback) {
        serializedObject.Update();
        var propIndex = FindPropertyIndex(parameter);
        var serializedParameter = serializedParameters.GetArrayElementAtIndex(propIndex);
        serializedParameter.managedReferenceValue = exposedParameterObject.parameters[propIndex];
        var serializedSettings = serializedParameter.FindPropertyRelative(nameof(ExposedParameter.settings));
        serializedSettings.managedReferenceValue = exposedParameterObject.parameters[propIndex].settings;
        var settingsField = new PropertyField(serializedSettings);
        settingsField.Bind(serializedObject);

        var view = new VisualElement();
        view.Add(settingsField);

        // TODO: see if we can replace this with an event
        oldParameterSettings[parameter] = parameter.settings;
        view.Add(new IMGUIContainer(() => {
            if (oldParameterSettings.TryGetValue(parameter, out var settings))
                if (!settings.Equals(parameter.settings))
                    valueChangedCallback(parameter.settings);
            oldParameterSettings[parameter] = parameter.settings;
        }));

        return view;
    }

    public void ResetOldParameter(ExposedParameter parameter) {
        oldParameterValues.Remove(parameter);
        oldParameterSettings.Remove(parameter);
    }

    private int FindPropertyIndex(ExposedParameter param) {
        return exposedParameterObject.parameters.FindIndex(p => p == param);
    }
}
}