using System.Linq;
using Gamelib.DataStructures;
using UnityEngine;

public class WidgetCommandHistoryController : MonoBehaviour
{
    public int CircularBufferCapacity;
    public WidgetQonsoleView View;
    private CircularBuffer<string> _historyInput;

    public void Reset()
    {
        CircularBufferCapacity = 32;
    }

    public void Awake()
    {
        _historyInput = new CircularBuffer<string>(CircularBufferCapacity);
        View.SetHistoryBuffer(_historyInput);
    }

    public void AddToHistory(string messageText)
    {
        if(string.IsNullOrEmpty(messageText))
            return;
        
        messageText = messageText.Trim();
        
        if (_historyInput.FirstOrDefault(x => x == messageText)!=null)
            return;

        _historyInput.Enqueue(messageText);
    }
}
