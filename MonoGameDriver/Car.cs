using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameDriver.Network;
using System;
using System.Collections.Generic;

namespace MonoGameDriver
{
	public abstract class Car
	{
		public bool IsSelected { get; set; } = false;
		public bool WasSelected { get; set; } = false;
		public bool IsDisabled { get; set; } = false;

		public float DistTravelledTarmac { get; set; }
		public float DistTravelledGrass { get; set; }

		public Vector2 Velocity { get; set; }

		private Vector2 position;
		public Vector2 Position
		{
			get { return position; }
			set
			{
				position = value;
				Vector2 tlPos = Position - Size / 2;
				CarCorners[0] = new Vector2(tlPos.X + Width / 2 - Width / 2 * FacingDirection.X + Height / 2 * FacingDirection.Y, tlPos.Y + Height / 2 - Width / 2 * FacingDirection.Y - Height / 2 * FacingDirection.X);
				CarCorners[1] = new Vector2(tlPos.X + Width / 2 - Width / 2 * FacingDirection.X - Height / 2 * FacingDirection.Y, tlPos.Y + Height / 2 - Width / 2 * FacingDirection.Y + Height / 2 * FacingDirection.X);
				CarCorners[2] = new Vector2(tlPos.X + Width / 2 + Width / 2 * FacingDirection.X + Height / 2 * FacingDirection.Y, tlPos.Y + Height / 2 + Width / 2 * FacingDirection.Y - Height / 2 * FacingDirection.X);
				CarCorners[3] = new Vector2(tlPos.X + Width / 2 + Width / 2 * FacingDirection.X - Height / 2 * FacingDirection.Y, tlPos.Y + Height / 2 + Width / 2 * FacingDirection.Y + Height / 2 * FacingDirection.X);
			}
		}
		public Vector2 FacingDirection { get; set; } // A vector with length 1 representing the direction the player is facing
		public Vector2[] CarCorners { get; set; } // The positions of the 4 corners of the car; used for collision detection

		public float Width { get; set; }
		public float Height { get; set; }
		public Vector2 Size
		{
			get { return new Vector2(Width, Height); }
			set { Width = value.X; Height = value.Y; }
		}

		public float ExternalTurningForces { get; set; }
		public float AngularVelocity { get; set; } // The angular velocity of the car in degrees

		public float Mass { get; set; }
		public float MaxEngineForce { get; set; } // A simplification of how cars work - the engine acts as a constant force on the car
		public Vector2 ExternalForces { get; set; } // External forces acting on the car

		public float AngleBetweenWheelsAndVelocity { get; set; }

		public byte[] Colour { get; set; }
		public Texture2D Texture { get; set; }

		private float rotation; // The current rotation of the car in radians
		public float Rotation
		{
			get { return rotation; }
			set
			{
				rotation = value;
				if (rotation > MathF.PI * 2) rotation -= MathF.PI * 2;
				else if (rotation < 0) rotation += MathF.PI * 2;

				FacingDirection = new Vector2(MathF.Cos(rotation), MathF.Sin(rotation));

			}
		}

		public Car(Vector2 _position, Vector2 _velocity, float _mass)
		{
			CarCorners = new Vector2[4];
			AngularVelocity = 0;
			Velocity = _velocity;
			FacingDirection = new Vector2(0, 1);
			Rotation = 0;
			ExternalForces = Vector2.Zero;
			CarCorners = new Vector2[4] { Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero };
			Position = _position;

			Mass = _mass;
			MaxEngineForce = 18000; // Could probably use some tweaking, this was just chosen arbitrarily

			Width = 8;
			Height = 4;

			Size = new Vector2(Width, Height);
		}

		protected float GetTurnMult()
		{
			// A larger value decreases the maximum turn speed
			float speedMaxTurnRate = 18f;
			return (Math.Clamp(Velocity.Length(), 0, speedMaxTurnRate) / speedMaxTurnRate);
		}

		protected abstract bool IsAccelerating();

		protected abstract bool IsBraking();

		protected abstract bool IsTurningLeft();

		protected abstract bool IsTurningRight();


		/// <summary>
		/// Updates the rotation of the car. TODO: Update to rotational velocity
		/// </summary>
		public void UpdateRotation(float timePassed)
		{
			// These numbers seem arbitrary
			float turnForce = 0;
			turnForce += IsTurningLeft() ? -3000000 : 0;
			turnForce += IsTurningRight() ? 3000000 : 0;
			turnForce *= Math.Clamp(Velocity.LengthSquared() / 49, 0, 1);

			turnForce -= AngularVelocity * MathF.Abs(AngularVelocity) * 160f;
			turnForce -= AngularVelocity * 2000;

			turnForce += ExternalTurningForces;
			ExternalTurningForces = 0;

			float turnAcc = turnForce / Mass;
			AngularVelocity += turnAcc * timePassed;
			Rotation += AngularVelocity * timePassed / 180f * MathF.PI;
		}

		/// <summary>
		/// Updates the velocity of the car based on the time passed since the last update
		/// </summary>
		public virtual void UpdateVelocity(float timePassed, Material material, float engineForce = 20000)
		{
			// TODO: Figure out what any of my code actually does. Why did I not comment this earlier?
			float forceX = 0f;
			float forceY = 0f;
			if (IsAccelerating())
			{
				forceX += FacingDirection.X * engineForce;
				forceY += FacingDirection.Y * engineForce;
			}

			Vector2 wheelDirection = new Vector2(MathF.Cos(Rotation), MathF.Sin(Rotation));
			Vector2 friction = Friction(new Vector2(forceX, forceY), wheelDirection, Velocity.Length(), material);

			forceX += friction.X;
			forceY += friction.Y;

			forceX += ExternalForces.X;
			forceY += ExternalForces.Y;

			ExternalForces = Vector2.Zero;

			Vector2 acc = new Vector2(forceX / Mass, forceY / Mass);

			float newX = Velocity.X + acc.X * timePassed;
			float newY = Velocity.Y + acc.Y * timePassed;

			// A car cannot turn while it is stopped
			if (Velocity.LengthSquared() != 0)
			{
				float angleBetweenWheelsAndVelocity = MathF.Abs(Rotation - MathF.Atan2(Velocity.Y, Velocity.X));
				angleBetweenWheelsAndVelocity = MathF.Acos(Vector2.Dot(wheelDirection, Velocity) / (wheelDirection.Length() * Velocity.Length())) * AngularVelocity / MathF.Abs(AngularVelocity);
				AngleBetweenWheelsAndVelocity = angleBetweenWheelsAndVelocity;
				if (angleBetweenWheelsAndVelocity != 0 && !float.IsNaN(angleBetweenWheelsAndVelocity) && Velocity.LengthSquared() > 1)
				{
					float rotMult = 0.7f / timePassed;
					float oldNewX = newX;
					float oldNewY = newY;
					newX = ((oldNewX * MathF.Cos(angleBetweenWheelsAndVelocity)) - (oldNewY * MathF.Sin(angleBetweenWheelsAndVelocity)) + oldNewX * rotMult) / (rotMult + 1);
					newY = ((oldNewX * MathF.Sin(angleBetweenWheelsAndVelocity)) + (oldNewY * MathF.Cos(angleBetweenWheelsAndVelocity)) + oldNewY * rotMult) / (rotMult + 1);
				}
			}

			if ((newX < 0 && Velocity.X > 0 && !IsAccelerating()) || (newX > 0 && Velocity.X < 0 && !IsAccelerating()))
			{
				newX = 0;
			}
			if ((newY < 0 && Velocity.Y > 0 && !IsAccelerating()) || (newY > 0 && Velocity.Y < 0 && !IsAccelerating()))
			{
				newY = 0;
			}

			Velocity = new Vector2(newX, newY);
		}

		/// <summary>
		/// Calculates friction from rolling resistance, braking, air resistance and other friction
		/// </summary>
		private Vector2 Friction(Vector2 forces, Vector2 wheelDirection, float speed, Material material)
		{
			if (Velocity.LengthSquared() == 0) return Vector2.Zero;

			// Rolling Resistance
			Vector2 direction = Vector2.Normalize(Velocity);

			// Dividing by 34 seems to be an arbitrary decision
			float fMag = material.RollingResistance * Mass * Constants.g / 34f;
			float fX = -Velocity.X * fMag;
			float fY = -Velocity.Y * fMag;

			if (IsBraking())
			{
				fX += -40000 * direction.X;
				fY += -40000 * direction.Y;
			}

			if (fX > forces.X && Velocity.X == 0) fX = forces.X;
			if (fY > forces.Y && Velocity.Y == 0) fY = forces.Y;

			// Air resistance
			// These numbers are partially based on reality, partially based on whatever feels right
			float airRes = 0.5f * 0.3f * 2.2f * 1.29f * speed * speed * 20 * material.AirResMult;
			fX -= airRes * direction.X;
			fY -= airRes * direction.Y;

			// Standard Friction
			float wheelMovementAngle = 1 - (Vector2.Dot(direction, wheelDirection) / (direction.Length() * wheelDirection.Length()));
			fX -= (material.GroundResistance * Mass * Constants.g * Math.Abs(wheelMovementAngle) * direction.X);
			fY -= (material.GroundResistance * Mass * Constants.g * Math.Abs(wheelMovementAngle) * direction.Y);

			Vector2 force = new Vector2(fX, fY);

			return force;
		}

		public virtual void UpdatePosition(float timePassed, Material mat)
		{
			// Currently only using the euler method for integration
			float newX = (Position.X + Velocity.X * timePassed);
			float newY = (Position.Y + Velocity.Y * timePassed);

			// Prevents the car from driving off the edge of the screen
			newX = Math.Clamp(newX, 0, Constants.courseWidth);
			newY = Math.Clamp(newY, 0, Constants.courseHeight);

			// Update distance travelled
			float dist = (Position - new Vector2(newX, newY)).LengthSquared();
			if (mat == Materials.Tarmac) DistTravelledTarmac += dist;
			else DistTravelledGrass += dist;

			Position = new Vector2(newX, newY);
		}
	}

	public class ComputerCar : Car
	{
		public List<float> Sensors { get; set; }
		public NeuralNetwork NeuralNetwork { get; set; }

		public ComputerCar(Vector2 _position, Vector2 _velocity, float _mass, Random random) : base(_position, _velocity, _mass)
		{
			NeuralNetwork = new NeuralNetwork(5, new int[] { 4 }, 4, random);
			Sensors = new List<float>() { 0, 0, 0, 0, 0 };
		}

		public override void UpdateVelocity(float timePassed, Material material, float engineForce)
		{
			base.UpdateVelocity(timePassed, material, (MathF.Min(MathF.Max(NeuralNetwork.GetOutput(Sensors)[2] - 0.2f, 0.0f) + 0.4f, 1) - 0.2f) * 20000);
		}

		protected override bool IsAccelerating()
		{
			return !IsDisabled;
		}

		protected override bool IsBraking()
		{
			return NeuralNetwork.GetOutput(Sensors)[3] > 0.5f;
		}

		protected override bool IsTurningLeft()
		{
			return NeuralNetwork.GetOutput(Sensors)[0] > 0.5f;
		}

		protected override bool IsTurningRight()
		{
			return NeuralNetwork.GetOutput(Sensors)[1] > 0.5f;
		}
	}
}
