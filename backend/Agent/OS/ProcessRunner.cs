using System.Diagnostics;
using System.Text;

namespace Agent.OS;

public static class ProcessRunner
{
    public static async Task<string> RunAsync(string fileName, string arguments,
        Func<string, Task>? sendSseMessage = null, string? workingDirectory = null, short? timeoutMilliseconds = null)
    {
        var sseAppended = "\n";
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory ?? Constants.Execution.Directory
            }
        };
        var output = new StringBuilder();
        process.OutputDataReceived += async (sender, e) =>
        {
            if (e.Data == null) return;
            output.AppendLine(e.Data);
            if (sendSseMessage != null)
            {
                await sendSseMessage(e.Data + sseAppended);
            }
        };
        process.ErrorDataReceived += async (sender, e) =>
        {
            if (e.Data == null) return;
            output.AppendLine(e.Data);
            if (sendSseMessage != null)
            {
                await sendSseMessage(e.Data + sseAppended);
            }
        };
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        if (timeoutMilliseconds.HasValue)
        {
            using var cts = new CancellationTokenSource();
            var waitTask = process.WaitForExitAsync(cts.Token);
            var timeoutTask = Task.Delay(timeoutMilliseconds.Value, cts.Token);
            
            var completedTask = await Task.WhenAny(waitTask, timeoutTask);
            if (completedTask == timeoutTask)
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch (InvalidOperationException)
                {
                    // Process might have exited just before we tried to kill it
                }
                sendSseMessage?.Invoke($"Process runner message: Terminated process '${fileName}' with arguments '${arguments}' after" +
                                       $" {timeoutMilliseconds.Value}ms");
            }
            
            await cts.CancelAsync(); // Cancel the timeout task if the process completed normally
        }
        else
        {
            await process.WaitForExitAsync();
        }

        var outputString = output.ToString();
        return outputString;
    }
}