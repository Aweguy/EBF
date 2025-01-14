using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Items.Melee
{
    public class LoveBlade : ModItem, ILocalizedModType
    {
        private bool playSound; //Ensures the magic sound only plays once per hit target
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public override void SetDefaults()
        {
            Item.width = 64;//Width of the hitbox of the item (usually the item's sprite width)
            Item.height = 64;//Height of the hitbox of the item (usually the item's sprite height)

            Item.damage = 85;//Item's base damage value
            Item.knockBack = 3f;//Float, the item's knockback value. How far the enemy is launched when hit
            Item.DamageType = DamageClass.Melee;//Item's damage type, Melee, Ranged, Magic and Summon. Custom damage are also a thing
            Item.useStyle = ItemUseStyleID.Swing;//The animation of the item when used
            Item.useTime = 25;//How fast the item is used
            Item.useAnimation = 25;//How long the animation lasts. For swords it should stay the same as UseTime

            Item.value = Item.sellPrice(copper: 0, silver: 0, gold: 30, platinum: 0);//Item's value when sold
            Item.rare = ItemRarityID.Lime;//Item's name colour, this is hardcoded by the modder and should be based on progression
            Item.UseSound = SoundID.Item1;//The item's sound when it's used
            Item.autoReuse = true;//Boolean, if the item auto reuses if the use button is held
            Item.useTurn = true;//Boolean, if the player's direction can change while using the item
        }
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            playSound = true;

            //Spawn an aura on every nearby enemy if you do damage to something other than a target dummy
            if (!target.immortal && GetNearbyTargets(target, range: 200, out NPC[] nearbyTargets))
            {
                foreach (NPC npc in nearbyTargets)
                {
                    //Only use some of them
                    if (Main.rand.NextBool(4))
                    {
                        //Don't play multiple sounds
                        if (playSound)
                        {
                            playSound = false;
                            SoundEngine.PlaySound(SoundID.Item30, npc.position);
                        }

                        //Spawn aura
                        Projectile.NewProjectileDirect(Item.GetSource_FromThis(), npc.position, Vector2.Zero, ProjectileID.PrincessWeapon, Item.damage / 2, 0);
                    }
                }
            }
        }
        private static bool GetNearbyTargets(NPC hitTarget, float range, out NPC[] nearbyTargets)
        {
            List<NPC> targetsList = new List<NPC>();
            foreach (NPC npc in Main.npc)
            {
                if (npc == hitTarget)
                    continue;

                if (!npc.friendly && npc.active && !npc.dontTakeDamage && npc.lifeMax > 5 && npc.WithinRange(hitTarget.position, range))
                {
                    targetsList.Add(npc);
                }
            }

            nearbyTargets = targetsList.ToArray();
            return targetsList.Count > 0;
        }

        //Sold by The Princess TownNPC
    }
}
