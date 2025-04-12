using System;
using GraphProcessor;
using UnityEngine;

[Serializable]
[NodeMenuItem("Primitives/Text")]
public class TextNode : BaseNode {
    [Output(name = "Label")] [SerializeField]
    public string output;

    public override string name => "Text";
}