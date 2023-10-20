using System;
using UnityEngine;

[Serializable]
public class NodeData
{
    public string title;
    [HideInInspector] public string guid;
    public string value;
    [HideInInspector] public Rect pos;
}