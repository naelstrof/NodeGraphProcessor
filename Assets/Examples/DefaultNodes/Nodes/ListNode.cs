using System;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;

[Serializable]
[NodeMenuItem("Custom/List")]
public class ListNode : BaseNode {
    [Output(name = "Out")] public Vector4 output;

    [Input(name = "In")] [SerializeField] public Vector4 input;

    public List<GameObject> objs = new();

    public override string name => "List";

    protected override void Process() {
        output = input;
    }
}