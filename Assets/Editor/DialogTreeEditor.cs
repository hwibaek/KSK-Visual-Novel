using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogTreeEditor : EditorWindow
{
    public DialogGraphView DialogGraphView;
    public string currentTreeName = "DialogTreeEditor";

    private bool _isSaved = true;
    private bool _isAutoSave;

    public bool IsSaved
    {
        get => _isSaved;
        set
        {
            _isSaved = value;
            titleContent.text = IsSaved ? currentTreeName : $"{currentTreeName}*";
            if (_isAutoSave && !IsSaved)
            {
                RequestDataOperation(true, DialogGraphView.CurrentPath);
            }
        }
    }
    public void CreateGUI()
    {
        CreateGraphView();
        CreateToolbar();
        CreateBlackBoard();
        titleContent.text = currentTreeName;
        
    }
    private void CreateBlackBoard()
    {
        var blackBoard = new Blackboard(DialogGraphView);
        blackBoard.Add(new BlackboardSection {title = "Exposed Properties"});
        blackBoard.addItemRequested = board =>
        {
            DialogGraphView.AddPropertyToBlackBoard(new ExportedProperty());
        };
        blackBoard.editTextRequested = (bb, e, arg) =>
        {
            var old = ((BlackboardField)e).text;
            
            if (DialogGraphView.ExportedProperties.Any(x => x.propertyName == arg))
            {
                EditorUtility.DisplayDialog("에러", "이미 해당 이름을 갖고있는 프로퍼티가 있습니다.", "확인");
                return;
            }
            
            var index = DialogGraphView.ExportedProperties.FindIndex(x => x.propertyName == old);
            DialogGraphView.ExportedProperties[index].propertyName = arg;
            ((BlackboardField)e).text = arg;
        };
        blackBoard.SetPosition(new Rect(10, 30, 200, 300));
        DialogGraphView.Add(blackBoard);
        DialogGraphView.Blackboard = blackBoard;
    }
    private void CreateGraphView()
    {
        DialogGraphView = new DialogGraphView(this)
        {
            name = "DialogGraphView",
            graphViewChanged = OnGraphChange
        };
        DialogGraphView.StretchToParentSize();
        rootVisualElement.Add(DialogGraphView);
    }
    private GraphViewChange OnGraphChange(GraphViewChange change)
    {
        IsSaved = change.edgesToCreate == null && change.elementsToRemove == null && change.movedElements == null;
        return change;
    }
    private void CreateToolbar()
    {
        var toolbar = new Toolbar();

        var saveBtn = new Button(() => RequestDataOperation(true, DialogGraphView.CurrentPath)) { text = "save tree" };
        var saveAtBtn = new Button(() => RequestDataOperation(true)) { text = "save at..." };
        var loadBtn = new Button(() => RequestDataOperation(false)) { text = "load tree..." };
        var autoSaveToggle = new Toggle("auto save");
        autoSaveToggle.RegisterValueChangedCallback(evt => _isAutoSave = evt.newValue);
        toolbar.Add(saveBtn);
        toolbar.Add(saveAtBtn);
        toolbar.Add(loadBtn);
        toolbar.Add(autoSaveToggle);
        
        rootVisualElement.Add(toolbar);
    }
    
    public void RequestDataOperation(bool isSave, string path = "")
    {
        var saveUtil = TreeSaveUtil.Get(DialogGraphView);
        if (isSave)
        {
            var isSaved = saveUtil.SaveTree(string.IsNullOrEmpty(path) ? EditorUtility.SaveFilePanel("Save Tree", "Assets/", "new Tree", "asset") : path);
            if (isSaved) IsSaved = true;
        }
        else
        {
            var isLoaded = saveUtil.LoadTree(string.IsNullOrEmpty(path) ? EditorUtility.OpenFilePanel("Load Tree", "Assets/", "asset") : path);
            if (isLoaded) IsSaved = true;
        }
    }
}