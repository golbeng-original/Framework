using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Golbeng.Framework.Commons
{
	public interface ILogger
	{
		string FlushString { get; }
		void AddLog(string message);
		void ClearLog();
	}

	public class BaseLogger : ILogger
	{
		public string FlushString { get => ""; }

		public void AddLog(string message)
		{
			System.Console.WriteLine(message);
		}

		public void ClearLog()
		{
			//
		}
	}
}
