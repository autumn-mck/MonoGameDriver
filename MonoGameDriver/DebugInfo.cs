using System;

namespace MonoGameDriver
{
	public class DebugInfo
	{
		public float MaxAvgSpeed { get; set; }
		public float TimeSinceLastGen { get; set; }
		public float NextGenTime { get; set; }
		public int Generation { get; set; }

		public override string ToString()
		{
			return $"Max Avg Speed: {MathF.Round(MaxAvgSpeed, 2)}\n" +
				$"Generation: {Generation}\n" +
				$"Time since prev. gen: {MathF.Round(TimeSinceLastGen, 1)}\n" +
				$"Time to next gen: {MathF.Round(NextGenTime - TimeSinceLastGen, 1)}";
		}
	}
}
