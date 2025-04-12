using System;
using GraphProcessor;
using UnityEngine;

[Serializable]
[NodeMenuItem("Primitives/Color")]
public class ColorNode : BaseNode {
    [Output(name = "Color")] [SerializeField]
    public new Color color;

    public override string name => "Color";
}