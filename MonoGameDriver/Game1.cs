using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameDriver.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace MonoGameDriver
{
	public class Game1 : Game
	{
		private readonly GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;

		private readonly DebugInfo debugInfo = new DebugInfo();

		private float timeSinceLastGen;
		private float TimeSinceLastGen
		{
			get { return timeSinceLastGen; }
			set { timeSinceLastGen = value; debugInfo.TimeSinceLastGen = timeSinceLastGen; }
		}

		private int generation;
		private int Generation
		{
			get { return generation; }
			set { generation = value;debugInfo.Generation = generation; }
		}

		private float nextGenTime;
		private float NextGenTime
		{
			get { return nextGenTime; }
			set { nextGenTime = value; debugInfo.NextGenTime = nextGenTime; }
		}

		private readonly Vector2 screenSize = new Vector2(1920, 1080);
		private readonly Vector2 startPos = new Vector2(380, 80);
		private readonly float startRotation = MathF.PI;

		// A list of all cars/players
		private readonly List<Car> cars = new List<Car>();

		// Used to keep track of time elapsed since the previous physics calculation
		private float timeElapsed = 0;

		private byte[,] terrainData;

		private Random random;

		public Game1()
		{
			_graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
			IsFixedTimeStep = false;
		}

		protected override void Initialize()
		{
			random = new Random();

			_graphics.PreferredBackBufferWidth = (int)(screenSize.X);
			_graphics.PreferredBackBufferHeight = (int)(screenSize.Y);
			_graphics.ApplyChanges();

			InitialCarSetup();

			base.Initialize();
		}

		private void InitialCarSetup()
		{
			for (int i = 0; i < 1000; i++)
			{
				ComputerCar toAdd = new ComputerCar(startPos, Vector2.Zero, 1500, random)
				{ Texture = Rendering.CarTexture, Rotation = startRotation };
				cars.Add(toAdd);
			}
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			// Content loading below
			Rendering.DebugTextFont = Content.Load<SpriteFont>("DebugText");
			Rendering.CarTexture = Content.Load<Texture2D>("RedCar");
			Rendering.TerrainTexture = Content.Load<Texture2D>("terrain");
			
			Rendering.UIRect = new Texture2D(_graphics.GraphicsDevice, 1, 1);
			Rendering.UIRect.SetData(new byte[] { 50, 255, 50, 0 });

			Rendering.Initialise(_spriteBatch, _graphics);

			foreach (Car car in cars)
			{
				TextureMethods.GenerateRandomCarTexture(car, GraphicsDevice);
			}
			terrainData = TextureMethods.GetTetureByteArray(Rendering.TerrainTexture);
		}

		protected override void Update(GameTime gameTime)
		{
			MouseState mState = Mouse.GetState();
			KeyboardState kState = Keyboard.GetState();

			// Exit if the user has pressed down the escape key
			if (kState.IsKeyDown(Keys.Escape))
				Exit();

			// For more consistant results, the timestep needs to be constant.
			// TODO: A timestep any larger slows the simulation to a crawl. Why?
			timeElapsed = 0.064f;//(float)gameTime.ElapsedGameTime.TotalSeconds;

			UpdateCarSensors();
			DisableCarsOnGrass();

			TimeSinceLastGen += timeElapsed;
			NextGenTime = 30.0f + 5.0f * Generation;
			bool shouldNewGen = TimeSinceLastGen > NextGenTime || cars.All(car => car.IsDisabled);

			if (shouldNewGen) StartNextGen();
			else PhysicsUpdate();

			base.Update(gameTime);
		}

		private void PhysicsUpdate()
		{
			Parallel.ForEach(cars, car =>
			{
				if (!car.IsDisabled)
				{
					car.UpdateRotation(timeElapsed);
					(car).UpdateVelocity(timeElapsed, GetMaterial(car.Position));
					car.UpdatePosition(timeElapsed, GetMaterial(car.Position));
				}
			});
		}

		private void StartNextGen()
		{
			SelectCarsForNextGen();
			Generation++;
			TimeSinceLastGen = 0.0f;

			// Only keep the cars that have been selected
			List<ComputerCar> nextGen = cars.Where(x => x is ComputerCar car && car.IsSelected).Select(c => (ComputerCar)c).ToList();
			cars.Clear();

			foreach (ComputerCar c in nextGen)
			{
				// Reset the car's values
				c.IsDisabled = false;
				c.IsSelected = false;
				c.WasSelected = true;
				c.Position = startPos;
				c.Rotation = startRotation;
				c.Velocity = Vector2.Zero;
				c.DistTravelledTarmac = 0f;
				c.DistTravelledGrass = 0f;

				for (int i = 0; i < 1000 / nextGen.Count; i++)
				{
					// Create a shallow copy of the car's network and give it to the clone
					ComputerCar clone = new ComputerCar(startPos, Vector2.Zero, 1500, random);
					using (MemoryStream ms = new MemoryStream())
					{
						BinaryFormatter formatter = new BinaryFormatter();
						formatter.Serialize(ms, c.NeuralNetwork.Network);
						ms.Position = 0;
						clone.NeuralNetwork.Network = (List<List<Neuron>>)formatter.Deserialize(ms);
					}
					// Slightly randomise the network of the clone
					clone.NeuralNetwork.SlightlyRandomiseSome();

					TextureMethods.RecolourTexture(c, clone, GraphicsDevice);

					clone.Rotation = startRotation;
					cars.Add(clone);
				}
			}
			cars.AddRange(nextGen);
		}

		private void SelectCarsForNextGen()
		{
			List<ComputerCar> toSelect = cars.Where(c => c is ComputerCar).Select(c => (ComputerCar)c).OrderByDescending(x => x.DistTravelledTarmac).Take(40).ToList();
			debugInfo.MaxAvgSpeed = toSelect[0].DistTravelledTarmac / TimeSinceLastGen;
			foreach (ComputerCar toSel in toSelect) toSel.IsSelected = true;
		}

		private void DisableCarsOnGrass()
		{
			// Be lenient and allow cars to drive up to 1/50th the distance on grass before stopping them
			IEnumerable<Car> carsL = cars.Where(x => x is ComputerCar car && car.DistTravelledTarmac < car.DistTravelledGrass * 50f);
			foreach (Car car in carsL)
			{
				car.Velocity = Vector2.Zero;
				car.IsDisabled = true;
			}
		}

		private void UpdateCarSensors()
		{
			Parallel.ForEach(cars, c =>
			{
				if (c is ComputerCar car && !car.IsDisabled)
				{
					// For each computer, update its sensors
					car.Sensors[0] = DistanceToGrass(car.Position, car.Rotation);
					car.Sensors[1] = DistanceToGrass(car.Position, car.Rotation + MathF.PI / 4);
					car.Sensors[2] = DistanceToGrass(car.Position, car.Rotation - MathF.PI / 4);
					car.Sensors[3] = DistanceToGrass(car.Position, car.Rotation + MathF.PI / 2);
					car.Sensors[4] = DistanceToGrass(car.Position, car.Rotation - MathF.PI / 2);
				}
			});
		}

		private float DistanceToGrass(Vector2 startPos, float angle)
		{
			float distSoFar = 0;
			float floatIncrement = 1;
			Vector2 stepIncrement = new Vector2(floatIncrement * MathF.Cos(angle), floatIncrement * MathF.Sin(angle)); ;
			while (GetMaterial(startPos) != Materials.Grass)
			{
				distSoFar += floatIncrement;
				startPos += stepIncrement;
			}
			return distSoFar;
		}

		/// <summary>
		/// Returns the track material at the given location
		/// </summary>
		private Material GetMaterial(Vector2 location)
		{
			try
			{
				int currentPixelColour = terrainData[(int)(location.X / Constants.courseWidth * 1920), (int)(location.Y / Constants.courseHeight * 1080)];
				if (currentPixelColour == 104)
					return Materials.Tarmac;
				else return Materials.Grass;
			}
			catch
			{
				return Materials.Grass;
			}
		}

		// Draw the current state to the screen
		protected override void Draw(GameTime gameTime)
		{
			Rendering.Render(cars, debugInfo);

			base.Draw(gameTime);
		}
	}
}
