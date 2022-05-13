using System;
using System.Collections.Generic;
using System.Text;

namespace MonoGameDriver
{
	public static class Materials
	{
		public static Material Grass = new Material(0.04f, 2f, 2, -2f, "Grass"); // Although 2 times air resistance is unrealistic, it provides a greater penalty for driving off the track
		public static Material Tarmac = new Material(0.03f, 0.9f, 1, 1.5f, "Tarmac");
	}

	public class Material
	{
		public float RollingResistance { get; set; }
		public float GroundResistance { get; set; }
		public float AirResMult { get; set; }
		public float BoostMult { get; set; }
		public string Name { get; set; }

		public Material(float _rollingResistance, float _groundResistance, float _airResMult, float _boostMult, string _name)
		{
			RollingResistance = _rollingResistance;
			GroundResistance = _groundResistance;
			AirResMult = _airResMult;
			BoostMult = _boostMult;
			Name = _name;
		}
	}
}
