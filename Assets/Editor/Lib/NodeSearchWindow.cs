using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
{
    private DialogGraphView _view;
    private EditorWindow _window;

    public void Init(DialogGraphView view, EditorWindow window)
    {
        _view = view;
        _window = window;
    }
    
    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        var tree = new List<SearchTreeEntry>
        {
            new SearchTreeGroupEntry(new GUIContent("Create Elements")),
            new SearchTreeGroupEntry(new GUIContent("Text Nodes"), 1),
            new SearchTreeEntry(new GUIContent("Dialog Node"))
            {
                userData = new NodeElement(), level = 2
            }
        };
        return tree;
    }

    public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
    {
        var mousePos = _window.rootVisualElement.ChangeCoordinatesTo(_window.rootVisualElement.parent,
            context.screenMousePosition - _window.position.position);
        var localMousePos = _view.contentViewContainer.WorldToLocal(mousePos);
        switch (searchTreeEntry.userData)
        {
            case NodeElement element:
                _view.CreateNode("Dialog Node", mousePos);
                return true;
            default:
                return false;
        }
    }
}