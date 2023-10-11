using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public static class ProcessUtil
{
	public class ProcessResult
	{
		public Queue<string> standardOutput { get; private set; }

		public Queue<string> standardError { get; private set; }

		public ProcessResult(Process process, bool redirectStandardOutput, bool redirectStandardError, Action<StreamReader> readStandardOutput)
		{
			standardOutput = new Queue<string>();
			standardError = new Queue<string>();
			using (process)
			{
				if (redirectStandardOutput)
				{
					using (process.StandardOutput)
					{
						readStandardOutput?.Invoke(process.StandardOutput);
						while (!process.StandardOutput.EndOfStream)
						{
							standardOutput.Enqueue(process.StandardOutput.ReadLine());
						}
					}
				}
				if (redirectStandardError)
				{
					process.BeginErrorReadLine();
					process.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs args)
					{
						standardError.Enqueue(args.Data);
					};
				}
				process.WaitForExit();
			}
		}
	}

	private const bool DEBUG = true;

	public static ProcessResult Run(string exeName, string args, bool redirectStandardInput = false, bool redirectStandardOutput = false, bool redirectStandardError = false, bool createNoWindow = true, bool useShellExecute = false, Action<StreamReader> readStandardOutput = null)
	{
		redirectStandardOutput = redirectStandardOutput || readStandardOutput != null;
		return new ProcessResult(Process.Start(new ProcessStartInfo
		{
			FileName = Path.Combine(Directory.GetCurrentDirectory(), exeName),
			Arguments = args,
			UseShellExecute = useShellExecute,
			RedirectStandardInput = redirectStandardInput,
			RedirectStandardOutput = redirectStandardOutput,
			RedirectStandardError = redirectStandardError,
			CreateNoWindow = createNoWindow
		}), redirectStandardOutput, redirectStandardError, readStandardOutput);
	}
}
