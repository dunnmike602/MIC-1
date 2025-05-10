namespace MLDComputing.Emulators.MIC1.Controller.Components;

public class SimpleAutoComplete(string[] commands) : IAutoCompleteHandler
{
    public char[] Separators { get; set; } = [' '];

    public string[] GetSuggestions(string text, int index)
    {
        if (string.IsNullOrEmpty(text))
        {
            return commands;
        }

        var suggestions = commands
            .Where(c => c.StartsWith(text, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        return suggestions;
    }
}