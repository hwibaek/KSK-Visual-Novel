using UnityEngine;
using UnityEngine.UI;

namespace Lobby
{
    #region Singleton

    public partial class LobbyManager : MonoBehaviour
    {
        private static LobbyManager _instance;

        public static LobbyManager Instance
        {
            get => _instance;
            private set
            {
                if (_instance == null) _instance = value;
                else if (_instance != value) Destroy(value);
            }
        }
    }

    #endregion

    #region Const

    public partial class LobbyManager
    {
        
    }

    #endregion
    
    #region Variables

    public partial class LobbyManager
    {
        
    }

    #endregion
    
    #region References

    public partial class LobbyManager
    {
        [Header("References")]
        [SerializeField] private Button newGameBtn;
        public Button NewGameBtn => newGameBtn;

        [SerializeField] private Button continueBtn;
        public Button ContinueBtn => continueBtn;

        [SerializeField] private Button optionBtn;
        public Button OptionBtn => optionBtn;

        [SerializeField] private Button exitBtn;
        public Button ExitBtn => exitBtn;
    }

    #endregion
    
    #region Event Methods

    public partial class LobbyManager
    {
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            NewGameBtn.onClick.AddListener(NewGame);
            ContinueBtn.onClick.AddListener(Continue);
            OptionBtn.onClick.AddListener(Option);
            ExitBtn.onClick.AddListener(Exit);
        }
    }

    #endregion
    
    #region Methods

    public partial class LobbyManager
    {
        private void NewGame()
        {
            Debug.Log("새 게임 버튼 눌림");
        }

        private void Continue()
        {
            Debug.Log("이어하기 버튼 눌림");
        }

        private void Option()
        {
            Debug.Log("옵션 버튼 눌림");
        }

        private void Exit()
        {
            Debug.Log("종료 버튼 눌림");
            Application.Quit();
        }
    }

    #endregion
}