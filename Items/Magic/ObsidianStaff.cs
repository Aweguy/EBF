using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Magic
{
    public class ObsidianStaff : EBFStaff, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";
        public override void SetDefaultsSafe()
        {
            Item.width = 60;
            Item.height = 64;
            Item.damage = 66;
            Item.useTime = 75;
            Item.useAnimation = 75;
            Item.value = Item.sellPrice(platinum: 0, gold: 4, silver: 0, copper: 0);
            Item.rare = ItemRarityID.LightRed;
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item88;
            Item.shoot = ModContent.ProjectileType<ObsidianStaff_DarkPulse>();
            Item.shootSpeed = 0.01f;
            Item.mana = 18;
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = Main.MouseWorld;
        }
        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.Obsidian, stack: 30)
                .AddIngredient(ItemID.Ruby, stack: 10)
                .AddIngredient(ItemID.SoulofNight, stack: 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class ObsidianStaff_DarkPulse : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 10;
        }
        public override void SetDefaults()
        {
            Projectile.width = 76;
            Projectile.height = 76;

            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Magic;
        }
        public override void OnSpawn(IEntitySource source)
        {
            // Spawn dust above and below
            var pos = Projectile.Center;
            for (int i = 0; i < 10; i++)
            {
                var offset = new Vector2(Main.rand.Next(-2, 2), Main.rand.Next(64, 128));
                var dust = Dust.NewDustPerfect(pos + offset, DustID.Obsidian);
                dust.noGravity = true;
                
                dust = Dust.NewDustPerfect(pos - offset, DustID.Obsidian);
                dust.noGravity = true;
            }

            // Spawn dust around
            for (float theta = 0.01f; theta < MathF.Tau; theta += MathF.Tau / 24)
            {
                var offset = theta.ToRotationVector2() * Main.rand.Next(32, 96);
                var dust = Dust.NewDustPerfect(pos + offset, DustID.RedTorch, Scale: 1.8f);
                dust.noGravity = true;
            }
        }
        public override void AI()
        {
            if (Main.GameUpdateCount % 4 == 0)
            {
                Projectile.frame++;
                if (Projectile.frame > 10)
                    Projectile.Kill();
            }

            SuckNPCs(150, suckingStrength: 20);
            SuckGore(150, suckingStrength: 20);
            SuckDust(150, suckingStrength: 20);
        }
        private void SuckNPCs(float suckingRange, float suckingStrength = 20)
        {
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.boss || npc.immortal || npc.dontTakeDamage) //immortal is target dummy
                    continue;

                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist <= suckingRange)
                {
                    float gravityMagnitude = Projectile.scale * suckingStrength / (dist * 0.5f + 10f); //Won't divide by 0 :)
                    npc.velocity += npc.DirectionTo(Projectile.Center) * gravityMagnitude;
                }
            }
        }
        private void SuckGore(float suckingRange, float suckingStrength = 20)
        {
            foreach (Gore gore in Main.gore)
            {
                if (!gore.active)
                    continue;

                float dist = Vector2.Distance(Projectile.Center, gore.position);
                if (dist <= suckingRange)
                {
                    float gravityMagnitude = Projectile.scale * suckingStrength / (dist * 0.5f + 10f); // Won't divide by 0 :)
                    gore.velocity += Vector2.Normalize(Projectile.Center - gore.position) * gravityMagnitude;
                }
            }
        }
        private void SuckDust(float suckingRange, float suckingStrength = 20)
        {
            foreach (Dust dust in Main.dust)
            {
                if (!dust.active)
                    continue;

                float dist = Vector2.Distance(Projectile.Center, dust.position);
                if (dist <= suckingRange)
                {
                    float gravityMagnitude = Projectile.scale * suckingStrength / (dist * 0.5f + 10f); //Won't divide by 0 :)
                    dust.velocity += Vector2.Normalize(Projectile.Center - dust.position) * gravityMagnitude;
                }
            }
        }
    }
}
