using UnityEngine;
using Events;

namespace Qonsole
{
    public class WidgetCommandHistoryHandler : MonoBehaviour, IHandle<WidgetQonsoleView.EventConsoleEnteredText>
    {
        public WidgetCommandHistoryController Controller;
        void Awake()
        {
            GlobalEventAggregator.EventAggregator.Subscribe(this);
        }

        public void Handle(WidgetQonsoleView.EventConsoleEnteredText message)
        {
            Controller.AddToHistory(message.Text);
        }
    }
}

