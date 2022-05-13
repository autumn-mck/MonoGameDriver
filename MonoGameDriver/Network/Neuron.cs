using System;

namespace MonoGameDriver.Network
{
	[Serializable]
	public abstract class Neuron
	{
		// Each neuron stores which layer it is on
		public int Layer { get; set; }

		public Neuron(int _layer)
		{
			Layer = _layer;
		}

		public abstract float GetValue();
	}
}
