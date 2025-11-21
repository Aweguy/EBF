using EBF.Abstract_Classes;
using EBF.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Magic
{
    public class Jubileus : EBFStaff, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";

        private const int spread = 200;
        public override void SetDefaultsSafe()
        {
            Item.width = 60;
            Item.height = 64;
            Item.damage = 318;
            Item.useTime = 8;
            Item.useAnimation = 8;
            Item.value = Item.sellPrice(platinum: 0, gold: 30, silver: 0, copper: 0);
            Item.rare = ItemRarityID.Red;
            Item.autoReuse = true;
            Item.UseSound = SoundID.DD2_LightningBugZap;
            Item.shoot = ProjectileID.DD2LightningBugZap;
            Item.shootSpeed = 25f;
            Item.mana = 6;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 2; i++)
            {
                //Spawn position
                float offsetX = Main.rand.NextFloat(-spread, spread);
                position = new Vector2(Main.MouseWorld.X + offsetX, Main.screenPosition.Y - 200 - (100 * i));

                //Velocity towards cursor
                velocity = Vector2.Normalize(Main.MouseWorld - position) * velocity.Length();
                velocity.X += Main.rand.NextFloat(-2, 2);

                //Spawn the projecile
                var proj = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI, 0f);
                proj.timeLeft = 300;
                proj.friendly = true;
                proj.hostile = false;
                proj.aiStyle = -1;
                proj.velocity = velocity;
                proj.rotation = proj.velocity.ToRotation();
            }

            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe(amount: 1)
                .AddIngredient(ItemID.BlizzardStaff, stack: 1)
                .AddIngredient<HolyGrail>(stack: 1)
                .AddIngredient<ElixirOfLife>(stack: 1)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
