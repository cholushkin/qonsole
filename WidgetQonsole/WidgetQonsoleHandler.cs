using Events;
using UnityEngine;

namespace Qonsole
{
    public class WidgetQonsoleHandler : MonoBehaviour, IHandle<WidgetQonsoleView.EventConsoleEnteredText>
    {
        public WidgetQonsoleController Controller;
        void Awake()
        {
            GlobalEventAggregator.EventAggregator.Subscribe(this);
        }

        public void Handle(WidgetQonsoleView.EventConsoleEnteredText message)
        {
            Controller.ExecuteString(message.Text);
        }
    }
}

