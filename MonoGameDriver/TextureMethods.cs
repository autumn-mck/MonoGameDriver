using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoGameDriver
{
	/// <summary>
	/// A number of static methods for doing stuff with textures
	/// </summary>
	public static class TextureMethods
	{
		private static Random random = new Random();

		/// <summary>
		/// Generate randomly coloured textures for all cars
		/// </summary>
		public static void GenerateRandomCarTexture(Car c, GraphicsDevice graphicsDevice)
		{
			byte[] col = new byte[3];
			random.NextBytes(col);
			c.Colour = col;
			c.Texture = TextureMethods.EditCarTexture(col, Rendering.CarTexture, graphicsDevice);
		}

		public static void RecolourTexture(ComputerCar original, ComputerCar clone, GraphicsDevice graphicsDevice)
		{
			byte[] newCol = (byte[])original.Colour.Clone();
			newCol[0] += (byte)(random.Next(0, 61) - 30);
			newCol[1] += (byte)(random.Next(0, 61) - 30);
			newCol[2] += (byte)(random.Next(0, 61) - 30);

			clone.Colour = newCol;

			clone.Texture = TextureMethods.EditCarTexture(newCol, Rendering.CarTexture, graphicsDevice);
		}

		/// <summary>
		/// A method that changes the colours of the given texture pixel by pixel so that the cars can be visually differentiated
		/// </summary>
		private static Texture2D EditCarTexture(byte[] newColours, Texture2D toEdit, GraphicsDevice graphicsDevice)
		{
			byte[] data = new byte[toEdit.Width * toEdit.Height * 4];
			toEdit.GetData(data);
			Texture2D textClone = new Texture2D(graphicsDevice, toEdit.Width, toEdit.Height);
			for (int i = 0; i < toEdit.Width * toEdit.Height * 4; i += 4)
			{
				if (data[i] == 192)
				{
					data[i] = newColours[0];
					data[i + 1] = newColours[1];
					data[i + 2] = newColours[2];
				}
				else if (data[i] == 128)
				{
					data[i] = (byte)(newColours[0] / 3 * 2);
					data[i + 1] = (byte)(newColours[1] / 3 * 2);
					data[i + 2] = (byte)(newColours[2] / 3 * 2);
				}
				else if (data[i] == 64)
				{
					data[i] = (byte)(newColours[0] / 3);
					data[i + 1] = (byte)(newColours[1] / 3);
					data[i + 2] = (byte)(newColours[2] / 3);
				}
			}
			textClone.SetData(data);
			return textClone;
		}

		/// <summary>
		/// Return a 2D byte array representing the given texture
		/// </summary>
		public static byte[,] GetTetureByteArray(Texture2D texture)
		{
			// Gets the terrain data and stores it
			// Create a byte array of the same size of the given terrain image
			int height = texture.Height;
			int width = texture.Width;
			int nStride = (width * 32 + 7) / 8;
			byte[] byteArrayFromImage = new byte[height * nStride];

			byte[,] terrainData = new byte[width, height];

			texture.GetData(byteArrayFromImage);

			// Copy the terain data from the image to the terrainData byte array
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					terrainData[i, j] = byteArrayFromImage[i * 4 + j * width * 4];
				}
			}

			return terrainData;
		}
	}
}
