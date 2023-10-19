using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogTreeEditor : EditorWindow
{
    private DialogGraphView _dialogGraphView;
    private string _fileName = "new Tree";
    
    [MenuItem("DialogTreeEditor/Editor...")]
    public static void ShowWindow()
    {
        var wnd = GetWindow<DialogTreeEditor>();
        wnd.titleContent = new GUIContent("DialogTreeEditor");
    }

    public void CreateGUI()
    {
        CreateGraphView();
        CreateToolbar();
        CreateBlackBoard();
    }

    private void CreateBlackBoard()
    {
        var blackBoard = new Blackboard(_dialogGraphView);
        blackBoard.Add(new BlackboardSection {title = "Exposed Properties"});
        blackBoard.addItemRequested = board =>
        {
            Debug.Log("생성");
            _dialogGraphView.AddPropertyToBlackBoard(new ExportedProperty());
        };
        blackBoard.editTextRequested = (bb, e, arg) =>
        {
            var old = ((BlackboardField)e).text;
            
            if (_dialogGraphView.ExportedProperties.Any(x => x.propertyName == arg))
            {
                EditorUtility.DisplayDialog("에러", "이미 해당 이름을 갖고있는 프로퍼티가 있습니다.", "확인");
                return;
            }
            
            var index = _dialogGraphView.ExportedProperties.FindIndex(x => x.propertyName == old);
            Debug.Log($"old : {old}");
            foreach (var ep in _dialogGraphView.ExportedProperties)
            {
                Debug.Log($"ep : {ep.propertyName}");
            }
            Debug.Log($"index : {index}, all : {_dialogGraphView.ExportedProperties.Count}");
            _dialogGraphView.ExportedProperties[index].propertyName = arg;
            ((BlackboardField)e).text = arg;
        };
        blackBoard.SetPosition(new Rect(10, 30, 200, 300));
        _dialogGraphView.Add(blackBoard);
        _dialogGraphView.Blackboard = blackBoard;
    }
    private void CreateGraphView()
    {
        _dialogGraphView = new DialogGraphView(this)
        {
            name = "DialogGraphView"
        };
        _dialogGraphView.StretchToParentSize();
        rootVisualElement.Add(_dialogGraphView);
    }

    private void CreateToolbar()
    {
        var toolbar = new Toolbar();

        var textField = new TextField("File Name :");
        textField.SetValueWithoutNotify("new Tree");
        textField.MarkDirtyRepaint();
        textField.RegisterValueChangedCallback(evt => _fileName = evt.newValue);
        toolbar.Add(textField);

        var saveBtn = new Button(() => RequestDataOperation(true)) {text = "save tree"};
        var loadBtn = new Button(() => RequestDataOperation(false)) {text = "load tree"};
        toolbar.Add(saveBtn);
        toolbar.Add(loadBtn);
        
        rootVisualElement.Add(toolbar);
    }

    private void RequestDataOperation(bool isSave)
    {
        if (string.IsNullOrEmpty(_fileName))
        {
            EditorUtility.DisplayDialog("파일 이름 없음", "파일 이름을 입력하세요.", "확인");
            return;
        }

        var saveUtil = TreeSaveUtil.Get(_dialogGraphView);
        if (isSave)
        {
            saveUtil.SaveTree(_fileName);
        }
        else
        {
            saveUtil.LoadTree(_fileName);
        }
    }
}