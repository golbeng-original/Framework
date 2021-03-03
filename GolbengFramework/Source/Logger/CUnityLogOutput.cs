using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Golbeng.Framework.Logger
{
	public class CUnityLogOutput : ILogOutput
	{
		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		public void Write(LogLevels level, string logEntry)
		{
			switch (level)
			{
				case LogLevels.All:
				case LogLevels.Information:
					UnityEngine.Debug.Log(logEntry);
					break;
				case LogLevels.Warning:
					UnityEngine.Debug.LogWarning(logEntry);
					break;
				case LogLevels.Error:
				case LogLevels.Exception:
					UnityEngine.Debug.LogError(logEntry);
					break;
			}

		}
	}
}
