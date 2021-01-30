using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Golbeng.Framework.Commons.Camera
{
	class CCameraResolution : MonoBehaviour
	{
		public float WidthResolution = 9.0f;
		public float HeightResolution = 16.0f;


		void Awake()
		{
			UnityEngine.Camera camera = GetComponent<UnityEngine.Camera>();
			var rect = camera.rect;

			camera.aspect = WidthResolution / HeightResolution;

			float widthRatio = (float)Screen.width / WidthResolution;
			float heightRatio = (float)Screen.height / HeightResolution;

			float heightAdd = ((widthRatio / (heightRatio / 100.0f)) - 100) / 200;
			float widthAdd = ((heightRatio / (widthRatio / 100.0f)) - 100) / 200;

			if (heightRatio > widthRatio)
				widthAdd = 0.0f;
			else
				heightAdd = 0.0f;

			rect.x += Mathf.Abs(widthAdd);
			rect.y += Mathf.Abs(heightAdd);
			rect.width += widthAdd * 2.0f;
			rect.height += heightAdd * 2.0f;

			camera.rect = rect;
		}
	}
}
