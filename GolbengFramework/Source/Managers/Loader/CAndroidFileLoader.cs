using Golbeng.Framework.Commons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Golbeng.Framework.Loader
{
	public class CAndroidFileLoader : IFileLoader
	{
		protected override async Task<Stream> _Load(string fullPath)
		{
			MemoryStream memoryStream = null;

			try
			{
				using (var unityWebRequest = UnityWebRequest.Get(fullPath))
				{
					unityWebRequest.timeout = 1000;

					await Task.Run(() =>
					{
						var request = unityWebRequest.SendWebRequest();

						while (request.isDone == false) { }

					});

					if (string.IsNullOrEmpty(unityWebRequest.error) == true)
						memoryStream = new MemoryStream(unityWebRequest.downloadHandler.data);

				}

				return memoryStream;
			}
			catch(Exception e)
			{
				ManagerProvider.Logger?.AddLog($"CAndroidFileLoader.Load error : {e.Message}");
				return null;
			}
		}
	}
}
