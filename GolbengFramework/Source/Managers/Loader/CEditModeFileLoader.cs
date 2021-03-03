using Golbeng.Framework.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Golbeng.Framework.Loader
{
	public class CEditModeFileLoader : IFileLoader
	{
		public CEditModeFileLoader(CResourceManager resourceManager) : base(resourceManager) { }

		protected override Stream _Load(string resourcePath)
		{
			return ResourceManager.FindStreamResource(resourcePath);
		}
	}
}
