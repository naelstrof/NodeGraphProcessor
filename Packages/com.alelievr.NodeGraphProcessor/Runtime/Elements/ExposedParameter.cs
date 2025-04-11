using System;
using System.Collections.Generic;
using UnityEngine;

namespace GraphProcessor {
[Serializable]
public class ExposedParameter : ISerializationCallbackReceiver {
    [Serializable]
    public class Settings {
        public bool isHidden = false;
        public bool expanded = false;

        [SerializeField] internal string guid = null;

        public override bool Equals(object obj) {
            if (obj is Settings s && s != null)
                return Equals(s);
            else
                return false;
        }

        public virtual bool Equals(Settings param) {
            return isHidden == param.isHidden && expanded == param.expanded;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }

    public string guid; // unique id to keep track of the parameter
    public string name;
    [Obsolete("Use GetValueType()")] public string type;
    [Obsolete("Use value instead")] public SerializableObject serializedValue;
    public bool input = true;
    [SerializeReference] public Settings settings;
    public string shortType => GetValueType()?.Name;

    public void Initialize(string name, object value) {
        guid = Guid.NewGuid().ToString(); // Generated once and unique per parameter
        settings = CreateSettings();
        settings.guid = guid;
        this.name = name;
        this.value = value;
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize() {
        // SerializeReference migration step:
#pragma warning disable CS0618
        if (serializedValue?.value != null) // old serialization system can't serialize null values
        {
            value = serializedValue.value;
            Debug.Log("Migrated: " + serializedValue.value + " | " + serializedValue.serializedName);
            serializedValue.value = null;
        }
#pragma warning restore CS0618
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize() {
    }

    protected virtual Settings CreateSettings() {
        return new Settings();
    }

    public virtual object value { get; set; }

    public virtual Type GetValueType() {
        return value == null ? typeof(object) : value.GetType();
    }

    private static Dictionary<Type, Type> exposedParameterTypeCache = new();

    internal ExposedParameter Migrate() {
        if (exposedParameterTypeCache.Count == 0)
            foreach (var type in AppDomain.CurrentDomain.GetAllTypes())
                if (type.IsSubclassOf(typeof(ExposedParameter)) && !type.IsAbstract) {
                    var paramType = Activator.CreateInstance(type) as ExposedParameter;
                    exposedParameterTypeCache[paramType.GetValueType()] = type;
                }
#pragma warning disable CS0618 // Use of obsolete fields
        var oldType = Type.GetType(type);
#pragma warning restore CS0618
        if (oldType == null || !exposedParameterTypeCache.TryGetValue(oldType, out var newParamType))
            return null;

        var newParam = Activator.CreateInstance(newParamType) as ExposedParameter;

        newParam.guid = guid;
        newParam.name = name;
        newParam.input = input;
        newParam.settings = newParam.CreateSettings();
        newParam.settings.guid = guid;

        return newParam;
    }

    public static bool operator ==(ExposedParameter param1, ExposedParameter param2) {
        if (ReferenceEquals(param1, null) && ReferenceEquals(param2, null))
            return true;
        if (ReferenceEquals(param1, param2))
            return true;
        if (ReferenceEquals(param1, null))
            return false;
        if (ReferenceEquals(param2, null))
            return false;

        return param1.Equals(param2);
    }

    public static bool operator !=(ExposedParameter param1, ExposedParameter param2) {
        return !(param1 == param2);
    }

    public bool Equals(ExposedParameter parameter) {
        return guid == parameter.guid;
    }

    public override bool Equals(object obj) {
        if (obj == null || !GetType().Equals(obj.GetType()))
            return false;
        else
            return Equals((ExposedParameter)obj);
    }

    public override int GetHashCode() {
        return guid.GetHashCode();
    }

    public ExposedParameter Clone() {
        var clonedParam = Activator.CreateInstance(GetType()) as ExposedParameter;

        clonedParam.guid = guid;
        clonedParam.name = name;
        clonedParam.input = input;
        clonedParam.settings = settings;
        clonedParam.value = value;

        return clonedParam;
    }
}

// Due to polymorphic constraints with [SerializeReference] we need to explicitly create a class for
// every parameter type available in the graph (i.e. templating doesn't work)
[Serializable]
public class ColorParameter : ExposedParameter {
    public enum ColorMode {
        Default,
        HDR
    }

    [Serializable]
    public class ColorSettings : Settings {
        public ColorMode mode;

        public override bool Equals(Settings param) {
            return base.Equals(param) && mode == ((ColorSettings)param).mode;
        }
    }

    [SerializeField] private Color val;

    public override object value {
        get => val;
        set => val = (Color)value;
    }

    protected override Settings CreateSettings() {
        return new ColorSettings();
    }
}

[Serializable]
public class FloatParameter : ExposedParameter {
    public enum FloatMode {
        Default,
        Slider
    }

    [Serializable]
    public class FloatSettings : Settings {
        public FloatMode mode;
        public float min = 0;
        public float max = 1;

        public override bool Equals(Settings param) {
            return base.Equals(param) && mode == ((FloatSettings)param).mode && min == ((FloatSettings)param).min &&
                   max == ((FloatSettings)param).max;
        }
    }

    [SerializeField] private float val;

    public override object value {
        get => val;
        set => val = (float)value;
    }

    protected override Settings CreateSettings() {
        return new FloatSettings();
    }
}

[Serializable]
public class Vector2Parameter : ExposedParameter {
    public enum Vector2Mode {
        Default,
        MinMaxSlider
    }

    [Serializable]
    public class Vector2Settings : Settings {
        public Vector2Mode mode;
        public float min = 0;
        public float max = 1;

        public override bool Equals(Settings param) {
            return base.Equals(param) && mode == ((Vector2Settings)param).mode &&
                   min == ((Vector2Settings)param).min && max == ((Vector2Settings)param).max;
        }
    }

    [SerializeField] private Vector2 val;

    public override object value {
        get => val;
        set => val = (Vector2)value;
    }

    protected override Settings CreateSettings() {
        return new Vector2Settings();
    }
}

[Serializable]
public class Vector3Parameter : ExposedParameter {
    [SerializeField] private Vector3 val;

    public override object value {
        get => val;
        set => val = (Vector3)value;
    }
}

[Serializable]
public class Vector4Parameter : ExposedParameter {
    [SerializeField] private Vector4 val;

    public override object value {
        get => val;
        set => val = (Vector4)value;
    }
}

[Serializable]
public class IntParameter : ExposedParameter {
    public enum IntMode {
        Default,
        Slider
    }

    [Serializable]
    public class IntSettings : Settings {
        public IntMode mode;
        public int min = 0;
        public int max = 10;

        public override bool Equals(Settings param) {
            return base.Equals(param) && mode == ((IntSettings)param).mode && min == ((IntSettings)param).min &&
                   max == ((IntSettings)param).max;
        }
    }

    [SerializeField] private int val;

    public override object value {
        get => val;
        set => val = (int)value;
    }

    protected override Settings CreateSettings() {
        return new IntSettings();
    }
}

[Serializable]
public class Vector2IntParameter : ExposedParameter {
    [SerializeField] private Vector2Int val;

    public override object value {
        get => val;
        set => val = (Vector2Int)value;
    }
}

[Serializable]
public class Vector3IntParameter : ExposedParameter {
    [SerializeField] private Vector3Int val;

    public override object value {
        get => val;
        set => val = (Vector3Int)value;
    }
}

[Serializable]
public class DoubleParameter : ExposedParameter {
    [SerializeField] private double val;

    public override object value {
        get => val;
        set => val = (double)value;
    }
}

[Serializable]
public class LongParameter : ExposedParameter {
    [SerializeField] private long val;

    public override object value {
        get => val;
        set => val = (long)value;
    }
}

[Serializable]
public class StringParameter : ExposedParameter {
    [SerializeField] private string val;

    public override object value {
        get => val;
        set => val = (string)value;
    }

    public override Type GetValueType() {
        return typeof(string);
    }
}

[Serializable]
public class RectParameter : ExposedParameter {
    [SerializeField] private Rect val;

    public override object value {
        get => val;
        set => val = (Rect)value;
    }
}

[Serializable]
public class RectIntParameter : ExposedParameter {
    [SerializeField] private RectInt val;

    public override object value {
        get => val;
        set => val = (RectInt)value;
    }
}

[Serializable]
public class BoundsParameter : ExposedParameter {
    [SerializeField] private Bounds val;

    public override object value {
        get => val;
        set => val = (Bounds)value;
    }
}

[Serializable]
public class BoundsIntParameter : ExposedParameter {
    [SerializeField] private BoundsInt val;

    public override object value {
        get => val;
        set => val = (BoundsInt)value;
    }
}

[Serializable]
public class AnimationCurveParameter : ExposedParameter {
    [SerializeField] private AnimationCurve val;

    public override object value {
        get => val;
        set => val = (AnimationCurve)value;
    }

    public override Type GetValueType() {
        return typeof(AnimationCurve);
    }
}

[Serializable]
public class GradientParameter : ExposedParameter {
    public enum GradientColorMode {
        Default,
        HDR
    }

    [Serializable]
    public class GradientSettings : Settings {
        public GradientColorMode mode;

        public override bool Equals(Settings param) {
            return base.Equals(param) && mode == ((GradientSettings)param).mode;
        }
    }

    [SerializeField] private Gradient val;
    [SerializeField] [GradientUsage(true)] private Gradient hdrVal;

    public override object value {
        get => val;
        set => val = (Gradient)value;
    }

    public override Type GetValueType() {
        return typeof(Gradient);
    }

    protected override Settings CreateSettings() {
        return new GradientSettings();
    }
}

[Serializable]
public class GameObjectParameter : ExposedParameter {
    [SerializeField] private GameObject val;

    public override object value {
        get => val;
        set => val = (GameObject)value;
    }

    public override Type GetValueType() {
        return typeof(GameObject);
    }
}

[Serializable]
public class BoolParameter : ExposedParameter {
    [SerializeField] private bool val;

    public override object value {
        get => val;
        set => val = (bool)value;
    }
}

[Serializable]
public class Texture2DParameter : ExposedParameter {
    [SerializeField] private Texture2D val;

    public override object value {
        get => val;
        set => val = (Texture2D)value;
    }

    public override Type GetValueType() {
        return typeof(Texture2D);
    }
}

[Serializable]
public class RenderTextureParameter : ExposedParameter {
    [SerializeField] private RenderTexture val;

    public override object value {
        get => val;
        set => val = (RenderTexture)value;
    }

    public override Type GetValueType() {
        return typeof(RenderTexture);
    }
}

[Serializable]
public class MeshParameter : ExposedParameter {
    [SerializeField] private Mesh val;

    public override object value {
        get => val;
        set => val = (Mesh)value;
    }

    public override Type GetValueType() {
        return typeof(Mesh);
    }
}

[Serializable]
public class MaterialParameter : ExposedParameter {
    [SerializeField] private Material val;

    public override object value {
        get => val;
        set => val = (Material)value;
    }

    public override Type GetValueType() {
        return typeof(Material);
    }
}
}