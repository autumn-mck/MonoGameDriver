using System;

namespace MonoGameDriver.Network
{
	/// <summary>
	/// A neuron on the input layer
	/// </summary>
	[Serializable]
	public class InputNeuron : Neuron
	{
		// An input neuron should have some way of having its value directly set
		public float Value { get; set; }

		// Currently assuming all input neurons are on layer 0
		public InputNeuron() : base(0) { }

		public override float GetValue()
		{
			return Value;
		}
	}
}
