namespace RemoteLlama.Helpers;

internal interface IConsoleHelper
{
    Task RunWithProgressAsync(string description, Func<Action<long, long>, Task> action);
    void ShowError(string errorMessage);
    void WriteTable(IEnumerable<string> headers, IEnumerable<IEnumerable<string>> rows);
}