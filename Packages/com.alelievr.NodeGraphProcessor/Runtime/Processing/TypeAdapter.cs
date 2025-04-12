using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GraphProcessor {
/// <summary>
///     Implement this interface to use the inside your class to define type convertions to use inside the graph.
///     Example:
///     <code>
/// public class CustomConvertions : ITypeAdapter
/// {
///     public static Vector4 ConvertFloatToVector(float from) => new Vector4(from, from, from, from);
///     ...
/// }
/// </code>
/// </summary>
public abstract class ITypeAdapter // TODO: turn this back into an interface when we have C# 8
{
    public virtual IEnumerable<(Type, Type)> GetIncompatibleTypes() {
        yield break;
    }
}

public static class TypeAdapter {
    private static readonly Dictionary<(Type from, Type to), Func<object, object>> adapters = new();
    private static readonly Dictionary<(Type from, Type to), MethodInfo> adapterMethods = new();
    private static readonly List<(Type from, Type to)> incompatibleTypes = new();

    [NonSerialized] private static bool adaptersLoaded;

#if !ENABLE_IL2CPP
    private static Func<object, object> ConvertTypeMethodHelper<TParam, TReturn>(MethodInfo method) {
        // Convert the slow MethodInfo into a fast, strongly typed, open delegate
        var func = (Func<TParam, TReturn>)Delegate.CreateDelegate
            (typeof(Func<TParam, TReturn>), method);

        // Now create a more weakly typed delegate which will call the strongly typed one
        Func<object, object> ret = param => func((TParam)param);
        return ret;
    }
#endif

    private static void LoadAllAdapters() {
        foreach (var type in AppDomain.CurrentDomain.GetAllTypes())
            if (typeof(ITypeAdapter).IsAssignableFrom(type)) {
                if (type.IsAbstract)
                    continue;

                if (Activator.CreateInstance(type) is ITypeAdapter adapter)
                    foreach (var types in adapter.GetIncompatibleTypes()) {
                        incompatibleTypes.Add((types.Item1, types.Item2));
                        incompatibleTypes.Add((types.Item2, types.Item1));
                    }

                foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public |
                                                       BindingFlags.NonPublic)) {
                    if (method.GetParameters().Length != 1) {
                        Debug.LogError(
                            $"Ignoring conversion method {method} because it does not have exactly one parameter");
                        continue;
                    }

                    if (method.ReturnType == typeof(void)) {
                        Debug.LogError($"Ignoring conversion method {method} because it does not returns anything");
                        continue;
                    }

                    var from = method.GetParameters()[0].ParameterType;
                    var to = method.ReturnType;

                    try {
#if ENABLE_IL2CPP
// IL2CPP doesn't support calling generic functions via reflection (AOT can't generate templated code)
                            Func<object, object> r =
 (object param) => { return (object)method.Invoke(null, new object[]{ param }); };
#else
                        var genericHelper = typeof(TypeAdapter).GetMethod("ConvertTypeMethodHelper",
                            BindingFlags.Static | BindingFlags.NonPublic);

                        // Now supply the type arguments
                        var constructedHelper = genericHelper.MakeGenericMethod(from, to);

                        var ret = constructedHelper.Invoke(null, new object[] { method });
                        var r = (Func<object, object>)ret;
#endif

                        adapters.Add((method.GetParameters()[0].ParameterType, method.ReturnType), r);
                        adapterMethods.Add((method.GetParameters()[0].ParameterType, method.ReturnType), method);
                    } catch (Exception e) {
                        Debug.LogError($"Failed to load the type convertion method: {method}\n{e}");
                    }
                }
            }

        // Ensure that the dictionary contains all the convertions in both ways
        // ex: float to vector but no vector to float
        foreach (var kp in adapters)
            if (!adapters.ContainsKey((kp.Key.to, kp.Key.from)))
                Debug.LogError(
                    $"Missing convertion method. There is one for {kp.Key.from} to {kp.Key.to} but not for {kp.Key.to} to {kp.Key.from}");

        adaptersLoaded = true;
    }

    public static bool AreIncompatible(Type from, Type to) {
        var count = incompatibleTypes.Count;
        for (var i = 0; i < count; i++) {
            var type = incompatibleTypes[i];
            if (type.from == from && type.to == to) return true;
        }

        return false;
    }

    public static bool AreAssignable(Type from, Type to) {
        if (!adaptersLoaded)
            LoadAllAdapters();

        if (AreIncompatible(from, to))
            return false;

        return adapters.ContainsKey((from, to));
    }

    public static MethodInfo GetConversionMethod(Type from, Type to) {
        return adapterMethods[(from, to)];
    }

    public static object Convert(object from, Type targetType) {
        if (!adaptersLoaded)
            LoadAllAdapters();

        if (adapters.TryGetValue((from.GetType(), targetType), out var conversionFunction))
            return conversionFunction?.Invoke(from);

        return null;
    }
}
}