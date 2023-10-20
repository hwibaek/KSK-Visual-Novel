using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class TreeSaveUtil
{
    private DialogGraphView _view;
    private NodeContainer _cache;
    private List<Edge> Edges => _view.edges.ToList();
    private List<NodeElement> Nodes => _view.nodes.ToList().Cast<NodeElement>().ToList();
    public static TreeSaveUtil Get(DialogGraphView view)
    {
        return new TreeSaveUtil
        {
            _view = view
        };
    }

    private bool SaveNodes(NodeContainer container)
    {
        if (!Edges.Any()) return false;
        var connectedPorts = Edges.Where(x => x.input.node != null).ToArray();
        foreach (var t in connectedPorts)
        {
            var outNode = t.output.node as NodeElement;
            var inputNode = t.input.node as NodeElement;
            
            container.nodeLinkData.Add(new NodeLinkData
            {
                baseGuid = outNode?.Guid,
                portName = t.output.portName,
                targetGuid = inputNode?.Guid
            });
        }

        var entry = Nodes.First(node => node.IsEntry);
        container.entryNode = new NodeData
        {
            guid = entry.Guid,
            pos = entry.GetPosition(),
            title = "대화 노드"
        };
        foreach (var node in Nodes.Where(node => !node.IsEntry))
        {
            container.nodeData.Add(new NodeData
            {
                guid = node.Guid,
                pos = node.GetPosition(),
                title = "대화 노드",
                value = node.DialogText
            });
        }

        return true;
    }

    private void SaveProperties(NodeContainer container)
    {
        container.nodePropertyData.AddRange(_view.ExportedProperties);
    }
    public bool SaveTree(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return false;
        if (!filePath.Contains("Assets/"))
        {
            EditorUtility.DisplayDialog("경로 에러", "불러올 모든 파일은 에셋 폴더 안에 있어야합니다!", "확인");
            return false;
        }

        var folders = filePath.Split('/');
        var finalPath = "";
        foreach (var str in folders)
        {
            if (string.IsNullOrEmpty(finalPath) && str != "Assets")
            {
                continue;
            }

            finalPath = $"{finalPath}/{str}";
        }

        if (finalPath[0].Equals('/')) finalPath = finalPath.Remove(0, 1);
        var container = ScriptableObject.CreateInstance<NodeContainer>();
        if (!SaveNodes(container)) return false;   

        SaveProperties(container);
        
        AssetDatabase.CreateAsset(container, finalPath);
        AssetDatabase.SaveAssets();
        return true;
    }

    public bool LoadTree(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return false;
        if (!filePath.Contains("Assets/"))
        {
            EditorUtility.DisplayDialog("경로 에러", "불러올 모든 파일은 에셋 폴더 안에 있어야합니다!", "확인");
            return false;
        }

        var folders = filePath.Split('/');
        var finalPath = "";
        foreach (var str in folders)
        {
            if (string.IsNullOrEmpty(finalPath) && str != "Assets")
            {
                continue;
            }

            finalPath = $"{finalPath}/{str}";
        }

        if (finalPath[0].Equals('/')) finalPath = finalPath.Remove(0, 1);
        _cache = AssetDatabase.LoadAssetAtPath<NodeContainer>(finalPath);
        if (_cache == null)
        {
            EditorUtility.DisplayDialog("파일 없음", "파일이 존재하지 않습니다!", "확인");
            return false;
        }
        
        ClearGraph();
        CreateNodes();
        ConnectNodes();
        CreateProperties();
        _view.Current = _cache;
        return true;
    }

    private void CreateProperties()
    {
        _view.ClearBBandEp();
        foreach (var ep in _cache.nodePropertyData)
        {
            _view.AddPropertyToBlackBoard(ep);
        }
    }
    private void ClearGraph()
    {
        var entry = Nodes.Find(x => x.IsEntry);
        entry.Guid = _cache.entryNode.guid;

        foreach (var node in Nodes.Where(node => !node.IsEntry))
        {
            Edges.Where(x => x.input.node == node).ToList().ForEach(edge => _view.RemoveElement(edge));
            _view.RemoveElement(node);
        }
    }

    private void CreateNodes()
    {
        foreach (var node in _cache.nodeData)
        {
            var temp = _view.CreateNodeElement(node.title, node.title, node.value, Vector2.zero);
            temp.Guid = node.guid;
            temp.DialogText = node.value;
            _view.AddElement(temp);

            var ports = _cache.nodeLinkData.Where(x => x.baseGuid == node.guid).ToList();
            ports.ForEach(x =>
            {
                _view.AddChoicePort(temp, x.portName);
            });
        }
    }

    private void ConnectNodes()
    {
        foreach (var t in Nodes)
        {
            var connections = _cache.nodeLinkData.Where(x => x.baseGuid == t.Guid).ToList();
            for (var j = 0; j < connections.Count; j++)
            {
                var target = connections[j].targetGuid;
                var targetNode = Nodes.First(x => x.Guid == target);
                LinkNodes(t.outputContainer[j].Q<Port>(), (Port)targetNode.inputContainer[0]);
                targetNode.SetPosition(_cache.nodeData.First(x => x.guid == target).pos);
            }
        }
    }

    private void LinkNodes(Port output, Port input)
    {
        var tEdge = new Edge
        {
            output = output,
            input = input
        };
        tEdge.input?.Connect(tEdge);
        tEdge.output?.Connect(tEdge);
        _view.Add(tEdge);
    }
}