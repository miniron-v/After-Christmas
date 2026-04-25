using UnityEngine;

public class DialoguePresenter
{
    // view와 model 선언
    private DialogueView _view;
    private DialogueModel _model;

    public DialoguePresenter(DialogueModel model, DialogueView view)
    {
        _view = view;
        _model = model;
    }

    public void OnEnable()
    {
        Debug.Log("Presenter: 이벤트 연결 완료");
        _view._onClicked += _model.LoadNextSentence;
        _model._onLoadedNextSentence += _view.ShowNextSentence;
    }

    public void OnDisable()
    {
        _view._onClicked -= _model.LoadNextSentence;
        _model._onLoadedNextSentence -= _view.ShowNextSentence;
    }
}