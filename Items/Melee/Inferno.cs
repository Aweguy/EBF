using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Melee
{
	public class Inferno : ModItem
	{
     
        public override void SetStaticDefaults()
		{
			base.DisplayName.WithFormatArgs("Inferno");//Name of the Item
			base.Tooltip.WithFormatArgs("Wreathed in scorching flames.\nBurns foes.");//Tooltip of the item
		}

		public override void SetDefaults()
		{
			Item.width = 60;//Width of the hitbox of the item (usually the item's sprite width)
			Item.height = 60;//Height of the hitbox of the item (usually the item's sprite height)

			Item.damage = 20;//Item's base damage value
			Item.knockBack = 1f;//Float, the item's knockback value. How far the enemy is launched when hit
			Item.DamageType = DamageClass.Melee;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
			Item.useStyle = ItemUseStyleID.Rapier;//The animation of the item when used
			Item.useTime = 10;//How fast the item is used
			Item.useAnimation = 30;//How long the animation lasts. For swords it should stay the same as UseTime

			Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 0, platinum: 0);//Item's value when sold
			Item.rare = ItemRarityID.Cyan;//Item's name colour, this is hardcoded by the modder and should be based on progression
			Item.UseSound = SoundID.Item1;//The item's sound when it's used
			Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
			Item.useTurn = true;//Boolean, if the player's direction can change while using the item

			Item.shoot = ProjectileID.GladiusStab;
			Item.shootSpeed = 2;
		}

		/*
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
		{
			for(int i = 0; i < 4; i++)
			{
				float randomRotation = Main.rand.NextFloat(0, 360);
				Projectile.NewProjectile(Item.GetSource_FromThis(), target.Center, new Vector2(1f, 0).RotatedBy(randomRotation), ModContent.ProjectileType<Inferno_Fireball>(), Item.damage, 0f, player.whoAmI);
			}

			target.AddBuff(BuffID.OnFire, 60 * 2);
		}
		*/
	}
	public class Inferno_Fireball: ModProjectile
	{
		public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;

            Projectile.friendly = true;
            Projectile.penetrate = -1;

            Projectile.light = 1f;
            Projectile.tileCollide = true;

			Projectile.penetrate = -1;
			Projectile.timeLeft = 60 * 3;

			Projectile.scale = 0.5f;

            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
			target.AddBuff(BuffID.OnFire, 60 * 2);

            if (hit.Damage >= target.life)
            {
                for (int i = 0; i < 4; i++)
                {
                    float randomRotation = Main.rand.NextFloat(0, 360);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, new Vector2(1f, 0).RotatedBy(randomRotation), ModContent.ProjectileType<Inferno_Fireball>(), Projectile.damage, 0f, Projectile.whoAmI);
                }
            }
        }

        public override bool PreAI()
        {
            int dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 0, Color.Orange, 1f);
			Main.dust[dustIndex].noGravity = true;
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 25; i++)
            {
                int dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 0, Color.Orange, 1f);
                Main.dust[dustIndex].noGravity = true;
            }
        }
    }
}
