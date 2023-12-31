﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogGraphView : GraphView
{
    public DialogTreeEditor Editor;
    public Blackboard Blackboard;
    public NodeContainer Current;
    public string CurrentPath;
    private NodeSearchWindow _search;
    public List<ExportedProperty> ExportedProperties = new();
    
    public DialogGraphView(DialogTreeEditor window)
    {
        Editor = window;
        AddGrid();
        AddPermissions();
        AddElement(GenerateEntryNode());
        AddSearchWindow(window);
        
    }
    private void AddSearchWindow(EditorWindow window)
    {
        _search = ScriptableObject.CreateInstance<NodeSearchWindow>();
        nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _search);
        _search.Init(this, window);
    }
    private void AddGrid()
    {
        var grid = new GridBackground();
        grid.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/DialogTreeEditor.uss"));
        Insert(0, grid);
        grid.StretchToParentSize();
    }
    private void AddPermissions()
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
    }

    public Port GeneratePort(NodeElement node, Direction direction, Port.Capacity capacity, Type portType) => node.InstantiatePort(Orientation.Horizontal, direction, capacity, portType);

    private NodeElement GenerateEntryNode()
    {
        var node = new NodeElement
        {
            title = "시작",
            Guid = GUID.Generate().ToString(),
            DialogText = "ENTRYPOINT",
            IsEntry = true
        };
        
        var port = GeneratePort(node, Direction.Output, Port.Capacity.Single, typeof(bool));
        port.portName = "Next";
        node.outputContainer.Add(port);
        
        UpdateNode(node);
        
        node.SetPosition(new Rect(100, 200, 100, 150));
        return node;
    }

    public void UpdateNode(NodeElement node)
    {
        node.RefreshExpandedState();
        node.RefreshPorts();
    }

    public void CreateNode(string nodeName, Vector2 pos)
    {
        AddElement(CreateNodeElement(nodeName, nodeName, string.Empty, pos));
    }
    public NodeElement CreateNodeElement(string nodeTitle, string dText, string dialog, Vector2 pos)
    {
        var node = new NodeElement
        {
            title = nodeTitle,
            DialogText = dText,
            Guid = GUID.Generate().ToString()
        };
        var input = GeneratePort(node, Direction.Input, Port.Capacity.Single, typeof(bool));
        input.portName = "Prev";
        node.inputContainer.Add(input);

        var btn = new Button(() => AddChoicePort(node)) { text = "new Choice" };
        node.titleContainer.Add(btn);

        var textField = new TextField(string.Empty)
        {
            value = dialog
        };
        textField.RegisterValueChangedCallback(evt =>
        {
            node.DialogText = evt.newValue;
            Editor.IsSaved = false;
        });
        node.mainContainer.Add(textField);
        
        UpdateNode(node);
        node.SetPosition(new Rect(pos, new Vector2(100, 150)));
        return node;
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();
        ports.ForEach(port =>
        {
            if (startPort != port && startPort.node != port.node) compatiblePorts.Add(port);
        });
        return compatiblePorts;
    }

    public void AddChoicePort(NodeElement element, string overridePortName = "")
    {
        var generatedPort = GeneratePort(element, Direction.Output, Port.Capacity.Single, typeof(bool));
        
        generatedPort.contentContainer.Remove(generatedPort.contentContainer.Q<Label>("type"));
        
        var outCount = element.outputContainer.Query("connector").ToList().Count;
        var outPortName = string.IsNullOrEmpty(overridePortName) ?  $"Choice {outCount}" : overridePortName;
        generatedPort.portName = outPortName;

        var textField = new TextField
        {
            name = string.Empty,
            value = outPortName
        };
        textField.RegisterValueChangedCallback(evt =>
        {
            generatedPort.portName = evt.newValue;
            Editor.IsSaved = false;
        });
        textField.RemoveFromClassList("unity-base-field");
        
        var deleteBtn = new Button(() => RemovePort(element, generatedPort))
        {
            text = "X"
        };
        generatedPort.contentContainer.Add(new Label(" "));
        generatedPort.contentContainer.Add(textField);
        generatedPort.contentContainer.Add(deleteBtn);
        
        element.outputContainer.Add(generatedPort);
        UpdateNode(element);
        Editor.IsSaved = false;
    }

    private void RemovePort(NodeElement element, Port gPort)
    {
        var target = edges.ToList().Where(x => x.output.portName == gPort.portName && x.output.node == gPort.node).ToList();
        
        element.outputContainer.Remove(gPort);
        UpdateNode(element);
        
        if (!target.Any()) return;

        var edge = target.First();
        edge.input.DisconnectAll();
        RemoveElement(edge);
        UpdateNode(element);
        Editor.IsSaved = false;
    }

    public void ClearBBandEp()
    {
        ExportedProperties.Clear();
        Blackboard.Clear();
    }
    public void AddPropertyToBlackBoard(ExportedProperty p)
    {
        var localName = p.propertyName;
        var localValue = p.propertyValue;
        while (ExportedProperties.Any(x => x.propertyName == localName))
        {
            localName = $"{localName}(1)";
        }
        var property = new ExportedProperty
        {
            propertyName = localName,
            propertyValue = localValue
        };
        ExportedProperties.Add(property);
        
        var bbField = new BlackboardField
        {
            text = property.propertyName,
            typeText = "string property"
        };
        
        Blackboard.Add(bbField);
    }
}