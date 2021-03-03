using CommonPackage.String;
using Golbeng.Framework.Managers;
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
		protected CResourceManager ResourceManager { get; }

		public IFileLoader(CResourceManager resourceManager)
		{
			ResourceManager = resourceManager;
		}

		public Stream Load(string resourcePath)
		{
			/*
			var task = _Load(fullPath);
			task.Wait();

			return task.Result;
			*/

			return _Load(resourcePath);
		}

		protected abstract Stream _Load(string resourcePath);
	}
}
