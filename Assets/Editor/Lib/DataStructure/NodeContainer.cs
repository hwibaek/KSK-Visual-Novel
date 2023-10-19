using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NodeContainer : ScriptableObject
{
    public NodeData entryNode;
    public List<NodeData> nodeData = new();
    public List<NodeLinkData> nodeLinkData = new();
    public List<ExportedProperty> nodePropertyData = new();
}