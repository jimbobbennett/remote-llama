using Spectre.Console;

namespace RemoteLlama.Helpers;

internal interface IConsoleHelper
{
    Task RunWithProgressAsync(string description, Func<Action<long, long>, Task> action);
    void ShowError(string errorMessage);
    void WriteTable(IEnumerable<string> headers, IEnumerable<IEnumerable<string>> rows);
    void WriteColumns(IEnumerable<IEnumerable<string>> rows);
    Task<T> RunWithSpinner<T>(Func<StatusContext, Task<T>> func, string status = "|");
    void WriteWord(string word);
    void WriteLine(string word);
    string? Prompt(string prompt);
}