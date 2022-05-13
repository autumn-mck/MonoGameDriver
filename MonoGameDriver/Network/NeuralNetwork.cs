using System;
using System.Collections.Generic;

namespace MonoGameDriver.Network
{
	// Class should be serialisable so that it can be saved later on
	[Serializable]
	public class NeuralNetwork
	{
		// A 2D List of all the neurons
		public List<List<Neuron>> Network { get; set; }

		[NonSerialized]
		private readonly Random random;

		public NeuralNetwork(int inputs, int[] hiddenLayers, int outputs, Random _random)
		{
			random = _random;

			Network = new List<List<Neuron>>();
			Network.Add(new List<Neuron>());

			// Add the input neurons
			for (int i = 0; i < inputs; i++)
			{
				Network[0].Add(new InputNeuron());
			}

			// Add the hidden layers
			for (int i = 1; i <= hiddenLayers.Length; i++)
			{
				Network.Add(new List<Neuron>());
				// Add the neurons on this hidden layer
				for (int j = 0; j < hiddenLayers[i - 1]; j++)
				{
					Network[i].Add(new HiddenNeuron(i));

					// Create a random bias between -1 and 1
					// I think this was just an arbitrary choice of numbers
					((HiddenNeuron)Network[i][j]).Bias = (float)((random.NextDouble() - 0.5) * 2.0);

					// Connect the neuron to all the neurons on the previous layer
					List<NeuralConnection> connections = new List<NeuralConnection>();
					foreach (Neuron n in Network[i - 1])
					{
						connections.Add(new NeuralConnection());
						connections[^1].Neuron = n;

						// Create a random bias between -1 and 1
						// I think this was just an arbitrary choice of numbers
						connections[^1].Weight = (float)((random.NextDouble() - 0.5) * 2.0);
					}
					((HiddenNeuron)Network[i][j]).Connections = connections;

				}
			}

			// Output layer
			Network.Add(new List<Neuron>());
			for (int i = 0; i < outputs; i++)
			{
				Network[hiddenLayers.Length + 1].Add(new HiddenNeuron(hiddenLayers.Length + 1));
				((HiddenNeuron)Network[hiddenLayers.Length + 1][i]).Bias = (float)random.Next(-100, 100) / 100;
				List<NeuralConnection> connections = new List<NeuralConnection>();
				foreach (Neuron n in Network[hiddenLayers.Length])
				{
					connections.Add(new NeuralConnection());
					connections[^1].Neuron = n;
					connections[^1].Weight = random.Next(-100, 100) / 100f;
				}
				((HiddenNeuron)Network[hiddenLayers.Length + 1][i]).Connections = connections;
			}
		}

		/// <summary>
		/// Get the outputs of the network from the given set of inputs
		/// </summary>
		public List<float> GetOutput(List<float> input)
		{
			// Give each input neuron its corresponding input value
			for (int i = 0; i < Network[0].Count; i++)
			{
				((InputNeuron)Network[0][i]).Value = input[i];
			}

			// Evaluate each layer of the network
			for (int i = 1; i < Network.Count - 1; i++)
			{
				foreach (Neuron n in Network[i])
				{
					n.GetValue();
				}
			}

			// Get the outputs from the last layer of the network
			List<float> toReturn = new List<float>();
			foreach (Neuron n in Network[^1])
			{
				toReturn.Add(n.GetValue());
			}

			// Reset each neuron in the network
			for (int i = 1; i < Network.Count; i++)
			{
				foreach (Neuron n in Network[i])
				{
					((HiddenNeuron)n).ResetValue();
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Slightly randomise the neurons in the network
		/// </summary>
		public void SlightlyRandomiseSome()
		{
			for (int i = 1; i < Network.Count; i++)
			{
				foreach (Neuron neuron in Network[i])
				{
					((HiddenNeuron)neuron).Bias += ((float)random.NextDouble() - 0.5f) / 0.2f;
					foreach (NeuralConnection c in ((HiddenNeuron)neuron).Connections)
					{
						c.Weight += ((float)random.NextDouble() - 0.5f) / 0.2f;
					}
				}
			}
		}

		// Currently unused code that will be used when I eventually move towards stochastic gradient descent
		//public void CalcCost(List<float> guess, int actual)
		//{
		//	float cost = 0;

		//	for (int i = 0; i < guess.Count; i++)
		//	{
		//		if (i == actual) cost += (guess[i] - 1) * (guess[i] - 1);
		//		else cost += guess[i] * guess[i];
		//	}

		//	Cost = (Cost * Count + cost) / (Count + 1);
		//	Count++;
		//}
	}
}
