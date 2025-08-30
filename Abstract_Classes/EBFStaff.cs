using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Abstract_Classes
{
    public abstract class EBFStaff : ModItem
    {
        /// <summary>
        /// This property returns the position of the staff's head, adjusted for the custom staff usestyle.
        /// <br>It's pretty hacky, so it doesn't look good with really big staves</br>
        /// </summary>
        public Vector2 StaffHead
        {
            get
            {
                Player player = Main.LocalPlayer;
                Vector2 dir = player.DirectionTo(Main.MouseWorld) * (Item.width / 4);
                
                //player.direction updates a frame late
                int sign = dir.X <= 0 ? 1 : -1;

                //Similarly, player.itemPosition and player.itemRotation updates a frame late, so I gotta use a workaround.
                return dir + player.Center + new Vector2(32, (Item.height / 1.66f) * sign).RotatedBy(player.AngleTo(Main.MouseWorld));
            }
        }
        public virtual void SetDefaultsSafe()
        { }
        public sealed override void SetDefaults()
        {
            Item.DamageType = DamageClass.Magic;
            Item.useStyle = ItemUseStyleID.Shoot; //Shoot animation style allows the staff to be rotated, instead of pointing directly at the cursor
            Item.shootSpeed = 0.01f; //Must be > 0 to make the held item rotate when used
            Item.noMelee = true; //And of course, we don't want the staff itself to deal damage

            SetDefaultsSafe();
        }
        public sealed override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            player.itemLocation -= new Vector2(player.direction * Item.width / 4, Item.height / 6).RotatedBy(player.itemRotation);
        }

        //Uncomment this code if you want to test StaffHead
        //public override bool? UseItem(Player player)
        //{
        //    Dust dust = Dust.NewDustPerfect(StaffHead, DustID.Torch, Velocity: Vector2.Zero, Scale: 5);
        //    dust.noGravity = true;
        //    return true;
        //}
    }
}
