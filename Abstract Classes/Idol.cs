using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Abstract_Classes
{
	public abstract class Idol : ModNPC
	{
        public SoundStyle HitSound;
        public SoundStyle JumpSound;
        public SoundStyle HighJumpSound;

		public float PrivateMoveSpeedMult;
		public float Division;
		public int PrivateMoveSpeedBalance;

        private bool Left = false;
        private bool Right = true;
        private bool Spin = false;


        public virtual void SetSafeDefaults()
		{

		}

		public override void SetDefaults()
		{
			NPC.aiStyle = -1;
			NPC.noGravity = false;
			if (!Main.dedServ)
				NPC.HitSound = HitSound;

			if (Main.hardMode)
			{
				NPC.lifeMax *= 2;
				NPC.damage *= 2;
				NPC.defense *= 2;
			}
			SetSafeDefaults();
		}

		public override void AI()
		{
			Rotation(NPC);
			MovementSpeed(NPC);
			Jumping(NPC);

			NPC.spriteDirection = NPC.direction;
		}

		private void MovementSpeed(NPC NPC)
		{
			NPC.TargetClosest(true);

			Vector2 target = Main.player[NPC.target].Center - NPC.Center;

			if (Spin)
			{
				float num1276 = target.Length();
				float MoveSpeedMult = PrivateMoveSpeedMult * 2f; //Speed Multiplier
				MoveSpeedMult += num1276 / (Division/2); //Balancing the speed. Lowering the division value makes it have more sharp turns.
				int MoveSpeedBal = PrivateMoveSpeedBalance/2;
				target.Normalize(); //Makes the vector2 for the target have a lenghth of one facilitating in the calculation
				target *= MoveSpeedMult;

				NPC.velocity.X = (NPC.velocity.X * (float)(MoveSpeedBal - 1) + target.X) / (float)MoveSpeedBal;
			}
			else
			{
				float num1276 = target.Length();
				float MoveSpeedMult = PrivateMoveSpeedMult;
				MoveSpeedMult += num1276 / Division; //Balancing the speed. Lowering the division value makes it have more sharp turns.
				int MoveSpeedBal = PrivateMoveSpeedBalance;
				target.Normalize(); //Makes the vector2 for the target have a lenghth of one facilitating in the calculation
				target *= MoveSpeedMult;

				NPC.velocity.X = (NPC.velocity.X * (float)(MoveSpeedBal - 1) + target.X) / (float)MoveSpeedBal;
			}
		}

		private void Jumping(NPC NPC)
		{
			Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.ai[0], ref NPC.ai[1]);

			if (NPC.collideY)
			{
				if (Main.rand.NextFloat() < .1f)
				{
					NPC.velocity = new Vector2(NPC.velocity.X, -10f);
					if (!Main.dedServ)
						SoundEngine.PlaySound(HighJumpSound, NPC.Center);

					if (!Left && Right && !Spin)
					{
						Left = true;
						Right = false;
					}
					else if (Left && !Right && !Spin)
					{
						Left = false;
						Right = true;
					}
				}
				else
				{
					NPC.velocity = new Vector2(NPC.velocity.X, -5f);
					if (!Main.dedServ)
						SoundEngine.PlaySound(JumpSound, NPC.Center);

					if (!Left && Right && !Spin)
					{
						Left = true;
						Right = false;
					}
					else if (Left && !Right && !Spin)
					{
						Left = false;
						Right = true;
					}
				}
			}
		}

		private void Rotation(NPC NPC)
		{
			if (Right && !Spin)
			{
				NPC.rotation += MathHelper.ToRadians(1);

				NPC.rotation = MathHelper.Clamp(NPC.rotation, MathHelper.ToRadians(-10), MathHelper.ToRadians(10));
			}
			else if (Left && !Spin)
			{
				NPC.rotation -= MathHelper.ToRadians(1);

				NPC.rotation = MathHelper.Clamp(NPC.rotation, MathHelper.ToRadians(-10), MathHelper.ToRadians(10));
			}

			if (NPC.life <= NPC.lifeMax * .25f)
			{
				NPC.rotation += MathHelper.ToRadians(30) * NPC.spriteDirection;
				Spin = true;
			}
		}
	}
}
