using UnityEngine;

public class DialogueTestRunner : MonoBehaviour
{
    [SerializeField] private DialogueView _view;

    private DialogueModel _model;
    private DialoguePresenter _presenter;

    private void Start()
    {
        _model = new DialogueModel();
        _presenter = new DialoguePresenter(_model, _view);

        _presenter.OnEnable(); // 직접 호출 (중요)
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("테스트: 스페이스 입력");

            // View의 클릭 이벤트 트리거
            _view.OnClick();
        }
    }

    private void OnDestroy()
    {
        _presenter.OnDisable();
    }
}