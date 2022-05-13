using System;

namespace MonoGameDriver.Network
{
	/// <summary>
	/// Represents a connection between a neuron and a neuron on the previous layer
	/// </summary>
	[Serializable]
	public class NeuralConnection
	{
		// The neuron on the previous layer
		public Neuron Neuron { get; set; }
		// The weight of the connection
		public float Weight { get; set; }

		// Get the weighted value of the neuron
		public float ConnectionValue()
		{
			return Neuron.GetValue() * Weight;
		}

		public override string ToString()
		{
			return Math.Round(ConnectionValue(), 4).ToString();
		}
	}
}
