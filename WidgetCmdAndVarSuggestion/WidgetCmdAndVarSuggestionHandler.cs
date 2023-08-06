using Events;
using UnityEngine;

public class WidgetCmdAndVarSuggestionHandler: MonoBehaviour, IHandle<WidgetQonsoleView.EventConsoleTextChange>
{
    public WidgetCmdAndVarSuggestionController Controller;

    void Awake()
    {
        GlobalEventAggregator.EventAggregator.Subscribe(this);
    }

    public void Handle(WidgetQonsoleView.EventConsoleTextChange message)
    {
        Controller.UpdateSuggestionList(message.Text);
    }
}
