using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Golbeng.Framework.Logger
{
	public enum LogLevels : int
	{
		None,
		All,
		Warning,
		Information,
		Error,
		Exception
	}

	public interface ILogOutput : IDisposable
	{
		void Write(LogLevels level, string logEntry);
	}

	public abstract class ILogger : IDisposable
	{
		private bool _disposed = false;
		private IList<ILogOutput> _outputs = new List<ILogOutput>();

		public LogLevels Level { get; set; } = LogLevels.All;

		~ILogger()
		{
			_Dispose();
		}

		private void _Dispose()
		{
			if (_disposed == true)
				return;

			foreach(var output in _outputs)
			{
				output.Dispose();
			}
		}

		public void Dispose()
		{
			_Dispose();
			GC.SuppressFinalize(this);
		}

		public void AddLogOutput(ILogOutput logOutput)
		{
			foreach(var output in _outputs)
			{
				if (output.GetType() == logOutput.GetType())
					return;
			}

			_outputs.Add(logOutput);
		}

		protected void Write(LogLevels level, string logEntry)
		{
			if (_outputs.Count == 0)
				return;

			foreach(var output in _outputs)
			{
				output.Write(level, logEntry);
			}
		}

		public abstract void Verbose(string tag, string msg);
		public abstract void Information(string tag, string msg);
		public abstract void Warning(string tag, string msg);
		public abstract void Error(string tag, string msg);
		public abstract void Exception(string tag, string msg, Exception exception);
	}
}
