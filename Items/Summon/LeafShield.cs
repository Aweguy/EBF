using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Summon
{
    public class LeafShield : EBFCatToy, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";
        public override void SetDefaultsSafe()
        {
            Item.width = 32;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 40;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 16;//Item's base damage value
            Item.knockBack = 3f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.useTime = 15;//How fast the item is used
            Item.useAnimation = 15;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.buyPrice(copper: 0, silver: 0, gold: 2, platinum: 0);//Item's value when bought
            Item.rare = ItemRarityID.Blue;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item1;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.defense = 2;

            Item.shoot = ModContent.ProjectileType<LeafShieldStab>();
            BonusMinion = ModContent.ProjectileType<WoodenIdolMinion>();
        }
        public override void HoldItemSafe(Player player)
        {
            player.statDefense += 2;
        }

        //Sold by dryad
    }

    public class LeafShieldStab : EBFToyStab
    {
        public override void SetDefaultsSafe()
        {
            DrawOffsetX = -4;
            DrawOriginOffsetY = -6;

            ProjOffset = 2;
            BoostDuration = 0;
            TagDamage = 2;
        }
        public override void OnHitNPCSafe(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!target.immortal)
            {
                Main.player[Projectile.owner].AddBuff(BuffID.DryadsWard, 300);
            }
        }
    }

    public class WoodenIdolMinion : EBFMinion
    {
        public override string Texture => "EBF/Items/Summon/LeafShield_WoodenIdolMinion";
        public override bool MinionContactDamage() => true;
        public override void SetStaticDefaultsSafe()
        {
            Main.projFrames[Projectile.type] = 3;
        }
        public override void SetDefaultsSafe()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            UseHoverAI = false;
        }
        public override void OnSpawnSafe(IEntitySource source)
        {
            Projectile.frame = Main.rand.Next(3);

            SoundEngine.PlaySound(SoundID.Item140 with { Pitch = 1.0f }, Projectile.Center);
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.WoodFurniture);
            }
        }
        public override void AISafe()
        {
            if (Math.Abs(Projectile.velocity.X) > 2f && Target == null)
            {
                JumpTo(Projectile.Center - Vector2.UnitY * 32);
            }

            Projectile.friendly = Target != null;
        }
    }
}
