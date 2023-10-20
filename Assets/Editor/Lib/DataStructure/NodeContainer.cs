using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

[Serializable]
public class NodeContainer : ScriptableObject
{
    public NodeData entryNode;
    public List<NodeData> nodeData = new();
    public List<NodeLinkData> nodeLinkData = new();
    public List<ExportedProperty> nodePropertyData = new();

    [OnOpenAsset(OnOpenAssetAttributeMode.Execute)]
    public static bool OnOpenAsset(int instanceID)
    {
        var isOpen = EditorWindow.HasOpenInstances<DialogTreeEditor>();
        DialogTreeEditor dte;
        if (!isOpen)
        {
            dte = EditorWindow.CreateWindow<DialogTreeEditor>();
            var str = AssetDatabase.GetAssetPath(instanceID);
            dte.RequestDataOperation(false, str);
        }
        else
        {
            EditorWindow.FocusWindowIfItsOpen<DialogTreeEditor>();
            dte = EditorWindow.GetWindow<DialogTreeEditor>();
            var str = AssetDatabase.GetAssetPath(instanceID);
            if (dte.DialogGraphView.Current == AssetDatabase.LoadAssetAtPath<NodeContainer>(str) || 
                dte.DialogGraphView.Current == null)
            {
                dte.RequestDataOperation(false, str);
                return false;
            }
            var result = EditorUtility.DisplayDialog("저장되지 않음", "경고 : 파일이 저장되지 않았습니다. 정말 불러오시겠습니까?", "예", "아니오");
            if (result) dte.RequestDataOperation(false, str);
        }
        return false;
    }
}