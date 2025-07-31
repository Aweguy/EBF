using EBF.Abstract_Classes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Magic
{
    public class Jubileus : EBFStaff, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";

        private const int spread = 250;
        public override void SetDefaultsSafe()
        {
            Item.width = 60;
            Item.height = 64;
            Item.damage = 388;
            Item.useTime = 4;
            Item.useAnimation = 8;
            Item.rare = ItemRarityID.Red;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.DD2LightningBugZap;
            Item.shootSpeed = 25f;
            Item.mana = 8;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //Spawn position
            float offsetX = Main.rand.NextFloat(-spread, spread);
            position = new Vector2(Main.MouseWorld.X + offsetX, Main.screenPosition.Y - 200);

            //Velocity towards cursor
            velocity = Vector2.Normalize(Main.MouseWorld - position) * velocity.Length();
            velocity.X += Main.rand.NextFloat(-2, 2);

            //Spawn the projecile
            damage += Main.rand.Next(-20, 21);
            var proj = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI, 0f);
            proj.timeLeft = 300;
            proj.friendly = true;
            proj.hostile = false;
            proj.aiStyle = -1;
            proj.velocity = velocity;
            proj.rotation = proj.velocity.ToRotation();

            //Create sound every few uses
            if(player.itemAnimation == player.itemAnimationMax)
            {
                SoundEngine.PlaySound(SoundID.DD2_LightningBugZap, position);
            }

            return false;
        }
    }
}
