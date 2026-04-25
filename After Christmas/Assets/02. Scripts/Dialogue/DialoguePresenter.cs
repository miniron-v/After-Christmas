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

    private void OnEnable()
    {
        Debug.Log("Presenter: 적절한 로그");
        _view._onClicked += _model.LoadNextSentence;
        _model._onLoadedNextSentence += _view.ShowNextSentence;
    }

    private void OnDisable()
    {
        _view._onClicked -= _model.LoadNextSentence;
        _model._onLoadedNextSentence -= _view.ShowNextSentence;
    }
}