using CommonPackage.String;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Golbeng.Framework.Loader
{
	public abstract class IFileLoader
	{
		public Stream Load(string fullPath)
		{
			var task = _Load(fullPath);
			task.Wait();

			return task.Result;
		}

		protected abstract Task<Stream> _Load(string fullPath);
	}
}
