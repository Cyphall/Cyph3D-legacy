using System;
using System.Diagnostics;

namespace Renderer
{
	public class DeltaStopwatch : Stopwatch
	{
		private double _previousTime;
		
		public double DeltaTime
		{
			get
			{
				if (Math.Abs(_previousTime) < double.Epsilon)
				{
					_previousTime = Elapsed.TotalSeconds;
					return 0;
				}
				else
				{
					double currentTime = Elapsed.TotalSeconds;
					double deltaTime = currentTime - _previousTime;
					_previousTime = currentTime;
					return deltaTime;
				}
			}
		}
		
		public new static DeltaStopwatch StartNew()
		{
			DeltaStopwatch s = new DeltaStopwatch();
			s.Start();
			return s;
		}
	}
}