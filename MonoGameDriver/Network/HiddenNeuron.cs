using System;
using System.Collections.Generic;

namespace MonoGameDriver.Network
{
	/// <summary>
	/// A neuron in one of the hidden layers or output layer
	/// </summary>
	[Serializable]
	public class HiddenNeuron : Neuron
	{
		// A neuron will have weighted connections to neurons on the previous layer
		public List<NeuralConnection> Connections { get; set; }
		// Along with a final bias
		public float Bias { get; set; }
		private float value = float.NaN;

		public HiddenNeuron(int _layer) : base(_layer) { }

		/// <summary>
		/// Returns the given input after being passed through the sigmoid function
		/// </summary>
		private static float SigmoidFunction(float input)
		{
			return 1.0f / (1.0f + (float)Math.Exp(-input));
		}

		/// <summary>
		/// Calculated and returns the value of the neuron
		/// </summary>
		public override float GetValue()
		{
			// If the value has already been calculated, don't calculate it again to increase performance
			if (float.IsNaN(value))
			{
				value = 0;

				foreach (NeuralConnection c in Connections) value += c.ConnectionValue();

				if (Double.IsNaN(SigmoidFunction(value - Bias)))
				{
					if (value > 0) value = 1;
					else value = 0;
				}
				else
					value = SigmoidFunction(value - Bias);
			}
			return value;
		}

		/// <summary>
		/// Reset the neuron's cached value
		/// </summary>
		public void ResetValue()
		{
			value = float.NaN;
		}
	}
}
