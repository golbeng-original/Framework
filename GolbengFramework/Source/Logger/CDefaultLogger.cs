using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Golbeng.Framework.Logger
{
	public class CDefaultLogger : ILogger
	{
		private string MakeLogEntry(LogLevels level, string tag, string msg)
		{
			string levelStr = "";
			switch (level)
			{
				case LogLevels.All: levelStr = "D"; break;
				case LogLevels.Information: levelStr = "I"; break;
				case LogLevels.Error: levelStr = "Err"; break;
				case LogLevels.Warning: levelStr = "Warn"; break;
				case LogLevels.Exception: levelStr = "Ex"; break;
			}


			return $"[{DateTime.Now.ToString("hh:mm:ss")}][{levelStr}][{tag}] {msg}";
		}

		public CDefaultLogger()
		{
			AddLogOutput(new CUnityLogOutput());
		}

		public override void Error(string tag, string msg)
		{
			var targetLevel = LogLevels.Error;

			if (Level <= targetLevel)
			{
				try
				{
					Write(targetLevel, MakeLogEntry(targetLevel, tag, msg));
				}
				catch { }
			}
		}

		public override void Exception(string tag, string msg, Exception exception)
		{
			var targetLevel = LogLevels.Exception;

			if (Level <= targetLevel)
			{
				try
				{
					System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(true);

					Write(targetLevel, MakeLogEntry(targetLevel, tag, msg + "\n" + $"file : {trace.GetFrame(1).GetFileName()}[line:{trace.GetFrame(1).GetFileLineNumber()}]"));
					Write(targetLevel, MakeLogEntry(targetLevel, tag, "===== Ex Message ====="));
					Write(targetLevel, MakeLogEntry(targetLevel, tag, exception.Message));
					Write(targetLevel, MakeLogEntry(targetLevel, tag, "===== Ex Stacktrace ====="));
					Write(targetLevel, MakeLogEntry(targetLevel, tag, exception.StackTrace));
				}
				catch { }
			}
		}

		public override void Information(string tag, string msg)
		{
			var targetLevel = LogLevels.Information;

			if (Level <= targetLevel)
			{
				try
				{
					Write(targetLevel, MakeLogEntry(targetLevel, tag, msg));
				}
				catch { }
			}
		}

		public override void Verbose(string tag, string msg)
		{
			var targetLevel = LogLevels.All;

			if (Level <= targetLevel)
			{
				try
				{
					Write(targetLevel, MakeLogEntry(targetLevel, tag, msg));
				}
				catch { }
			}
		}

		public override void Warning(string tag, string msg)
		{
			var targetLevel = LogLevels.Warning;

			if (Level <= targetLevel)
			{
				try
				{
					Write(targetLevel, MakeLogEntry(targetLevel, tag, msg));
				}
				catch { }
			}
		}
	}
}
