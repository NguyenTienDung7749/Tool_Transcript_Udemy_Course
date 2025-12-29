using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UdemyTranscriptExtractor.Helpers;

public class EasterEggHandler
{
    private readonly List<Key> _konamiCode = new()
    {
        Key.Up, Key.Up, Key.Down, Key.Down,
        Key.Left, Key.Right, Key.Left, Key.Right,
        Key.B, Key.A
    };
    
    private readonly List<Key> _inputSequence = new();
    private readonly Window _window;
    
    public event EventHandler? KonamiCodeActivated;
    
    public EasterEggHandler(Window window)
    {
        _window = window;
        _window.KeyDown += OnKeyDown;
    }
    
    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        _inputSequence.Add(e.Key);
        
        // Keep only the last 10 keys
        if (_inputSequence.Count > 10)
            _inputSequence.RemoveAt(0);
        
        // Check if sequence matches Konami code
        if (_inputSequence.Count == 10 && SequenceMatches())
        {
            KonamiCodeActivated?.Invoke(this, EventArgs.Empty);
            _inputSequence.Clear();
        }
    }
    
    private bool SequenceMatches()
    {
        for (int i = 0; i < _konamiCode.Count; i++)
        {
            if (_inputSequence[i] != _konamiCode[i])
                return false;
        }
        return true;
    }
}
