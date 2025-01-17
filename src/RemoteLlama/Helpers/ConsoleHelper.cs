using Spectre.Console;

namespace RemoteLlama.Helpers;

internal class ConsoleHelper : IConsoleHelper
{
    private Progress? _progress;
    private ProgressTask? _currentTask;

    public async Task RunWithProgressAsync(string description, Func<Action<long, long>, Task> action)
    {
        _progress = AnsiConsole.Progress()
            .AutoClear(false)
            .HideCompleted(false)
            .Columns(
            [
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn(),
            ]);

        await _progress.StartAsync(async ctx => 
        {
            _currentTask = ctx.AddTask(description);

            // Create an update action that will be passed to the caller
            void updateAction(long completed, long total) => UpdateProgress(completed, total);

            // Execute the provided action with the update callback
            await action(updateAction).ConfigureAwait(false);

            // Refresh the progress bar
            ctx.Refresh();
            
            // Complete the task
            CompleteProgress();
        }).ConfigureAwait(false);
    }

    private void UpdateProgress(long completed, long total)
    {
        if (_currentTask == null) return;
        
        _currentTask.MaxValue = total;
        _currentTask.Value = completed;
    }

    private void CompleteProgress()
    {
        _currentTask?.StopTask();
    }

    public void ShowError(string errorMessage) => AnsiConsole.MarkupLine($"[red]{errorMessage}[/]");

    public Task<T> RunWithSpinner<T>(Func<StatusContext, Task<T>> func, string status = "|")
    {
        return AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync(status, func);
    }

    public void WriteTable(IEnumerable<string> headers, IEnumerable<IEnumerable<string>> rows)
    {
        var table = new Table();

        foreach (var header in headers)
        {
            table.AddColumn(header);
        }

        foreach (var row in rows)
        {
            table.AddRow(row.ToArray());
        }

        AnsiConsole.Write(table);
    }

    public void WriteColumns(IEnumerable<IEnumerable<string>> rows)
    {
        var grid = new Grid();

        // Get the number of items per row and ensure they are the same
        var itemCount = rows.First().Count();
        if (rows.Any(row => row.Count() != itemCount))
        {
            throw new ArgumentException("All rows must have the same number of items.");
        }

        // Create itemCount number of columns
        for (var i = 0; i < itemCount; i++)
        {
            grid.AddColumn();
        }

        // Add each row to the grid
        foreach (var row in rows)
        {
            grid.AddRow(row.ToArray());
        }

        AnsiConsole.Write(grid);
    }

    public void WriteWord(string word)
    {
        AnsiConsole.Write(word);
    }

    public void WriteLine(string word) => AnsiConsole.WriteLine(word);

    public string? Prompt(string prompt) => AnsiConsole.Ask<string>(prompt);
}
