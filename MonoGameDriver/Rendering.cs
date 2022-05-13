using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System.Collections.Generic;
using System.Linq;

namespace MonoGameDriver
{
	public static class Rendering
	{
		private static GraphicsDeviceManager _graphics;
		private static SpriteBatch _spriteBatch;

		private static float renderScale;
		private static float scaleMod;

		public static Texture2D TerrainTexture { get; set; }
		public static Texture2D CarTexture { get; set; }
		public static Texture2D UIRect { get; set; }

		public static SpriteFont DebugTextFont { get; set; }

		public static void Initialise(SpriteBatch __spriteBatch, GraphicsDeviceManager __graphics)
		{
			_graphics = __graphics;
			_spriteBatch = __spriteBatch;

			renderScale = _graphics.PreferredBackBufferWidth / 1920f;
			scaleMod = (float)_graphics.PreferredBackBufferWidth / Constants.courseWidth;
		}

		public static void Render(IEnumerable<Car> cars, DebugInfo debugInfo)
		{
			_spriteBatch.Begin();

			// Draw the background terrain
			_spriteBatch.Draw(TerrainTexture, Vector2.Zero, Color.White);
			_spriteBatch.Draw(TerrainTexture, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, (int)(_graphics.PreferredBackBufferWidth / 16f * 9f)), Color.White);

			DrawCars(cars);
			DrawDebugInfo(debugInfo);
			_spriteBatch.End();
		}

		private static void DrawCars(IEnumerable<Car> cars)
		{
			try
			{
				foreach (Car car in cars)
				{
					DrawCar(car);
					if (!car.IsDisabled)
						_spriteBatch.DrawLine(car.Position * scaleMod, 20, car.AngleBetweenWheelsAndVelocity + car.Rotation, Color.Red, 5);
				}
			}
			catch { }
		}

		private static void DrawCar(Car car)
		{
			Color colour = Color.White;
			if (car.IsDisabled)
			{
				colour = new Color(0.2f, 0.2f, 0.2f);
			}

			_spriteBatch.Draw(car.Texture,
				new Rectangle((int)(car.Position.X * scaleMod), (int)(car.Position.Y * scaleMod), (int)(car.Width * scaleMod), (int)(car.Height * scaleMod)),
				null,
				colour,
				car.Rotation,
				new Vector2(car.Texture.Width / 2, car.Texture.Height / 2),
				SpriteEffects.None, 0f);

			// Draw a yellow box around a car if it was previously selected
			if (car.WasSelected)
			{
				_spriteBatch.DrawLine(car.CarCorners[0] * scaleMod, car.CarCorners[1] * scaleMod, Color.Yellow, 2);
				_spriteBatch.DrawLine(car.CarCorners[1] * scaleMod, car.CarCorners[3] * scaleMod, Color.Yellow, 2);
				_spriteBatch.DrawLine(car.CarCorners[3] * scaleMod, car.CarCorners[2] * scaleMod, Color.Yellow, 2);
				_spriteBatch.DrawLine(car.CarCorners[2] * scaleMod, car.CarCorners[0] * scaleMod, Color.Yellow, 2);
			}
		}

		private static void DrawDebugInfo(DebugInfo info)
		{
			if (!(info is null))
			{
				string str = info.ToString();
				int numLines = str.Count(c => c.Equals('\n')) + 1;
				_spriteBatch.Draw(UIRect, new Rectangle(0, 0, (int)(400 * renderScale), (int)(numLines * 40 * renderScale)), Color.White);
				DrawDebugText(str, new Vector2(0, 0));
			}
		}

		private static void DrawDebugText(string text, Vector2 position)
		{
			float textScale = 0.5f * renderScale;
			_spriteBatch.DrawString(DebugTextFont, text, position * renderScale, Color.Black, 0, Vector2.Zero, textScale, SpriteEffects.None, 0f);
		}
	}
}
