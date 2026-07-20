using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EBF.Abstract_Classes
{
    /// <summary>
    /// Represents a holdout weapon such that arrows can be charged.
    /// </summary>
    public abstract class EBFHoldoutBow : ModProjectile
    {
	    private Vector2 ownerMouseWorld; // For net sync
	    private float drawTime;//The current charge value.
	    private const int MinimumDrawTime = 30; // The minimum charge required before the bow can release the arrow. Set to be the bow's usetime.
	    private int ConsumedArrowType => (int)Projectile.ai[0]; // Which arrow item was consumed upon use. Sent by EBFBow.
	    
	    /// <summary>
	    /// Determines what arrow to shoot.
	    /// <para>Defaults to Wooden Arrow.</para>
	    /// </summary>
	    protected virtual int ArrowType => ProjectileID.WoodenArrowFriendly;

	    /// <summary>
	    /// Determines held item's distance from the player center, and where the projectile is spawned.
	    /// <para>Defaults to 10 pixels.</para>
	    /// </summary>
	    protected int HoldoutDistance { get; set; } = 10;

	    /// <summary>
	    /// How far away the arrow should be drawn. Higher values means the arrow will be drawn closer to the bow.
	    /// <para>Defaults to 0 pixels.</para>
	    /// </summary>
	    protected int ArrowDrawOffset { get; set; }
	    
	    /// <summary>
	    /// The sound that plays once the projectile has been released.
	    /// <para>Defaults to Item5 (bow shoot sound).</para>
	    /// </summary>
	    protected SoundStyle ShootSound { get; set; } = SoundID.Item5;
	    
	    /// <summary>
        /// The maximum amount of charge an arrow can have, at which point draw time will stop increasing. Draw time starts at 0 and ticks up by 1 every update while an arrow exists.
        /// <br>If you wish to check if the arrow is fully charged, use the FullyCharged property instead.</br>
        /// <para>Defaults to 80.</para>
        /// </summary>
        protected int MaximumDrawTime { get; set; } = 80;

        protected bool FullyCharged => (int)drawTime >= MaximumDrawTime;

        /// <summary>
        /// Automatically release arrows that are fully charged.
        /// <para>Defaults to true.</para>
        /// </summary>
        protected bool AutoRelease { get; set; } = true;

        /// <summary>
        /// How much the damage should be multiplied based on its charging percentage. The value cannot be set below 1.
        /// <para>Defaults to 2 times increase.</para>
        /// </summary>
        protected float DamageScale
        {
            get => damageScale;
            set => damageScale = value < 1 ? 1 : value;
        }
        private float damageScale = 2;

        /// <summary>
        /// How much the velocity should be multiplied based on its charging percentage. The value cannot be set below 1.
        /// <para>Defaults to 2 times increase.</para>
        /// </summary>
        protected float VelocityScale
        {
            get => velocityScale;
            set => velocityScale = value < 1 ? 1 : value;
        }
        private float velocityScale = 2;

        /// <summary>
        /// This method controls what happens upon shooting a wooden arrow.
        /// Override this to change shooting logic as well as shot projectile.
        /// <para>By default, it spawns an instance of ArrowType, or wooden arrow if ArrowType isn't set.</para>
        /// </summary>
        /// <param name="velocity">Projectile velocity after draw boost is applied.</param>
        /// <param name="damage">Projectile damage after draw boost is applied.</param>
        protected virtual void OnShoot(IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockback, int owner, float ai0 = 0, float ai1 = 0, float ai2 = 0)
        {
	        Projectile.NewProjectile(source, position, velocity, type, damage, knockback, owner, ai0, ai1, ai2);
        }
        
        public override void SetStaticDefaults() {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }

        public override void SetDefaults() {
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.hide = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = true;
        }
        
        public override bool? CanDamage() => false;

        public sealed override bool PreAI()
        {
			var player = Main.player[Projectile.owner];
			var playerCenter = player.RotatedRelativePoint(player.MountedCenter);
			
			// Get mouse input only from owner
			if (Main.myPlayer == Projectile.owner)
				ownerMouseWorld = Main.MouseWorld;
			
			// Update holding direction
			var holdoutOffset = HoldoutDistance * Vector2.Normalize(ownerMouseWorld - playerCenter);
			
	        // Net sync
			if (holdoutOffset != Projectile.velocity) 
				Projectile.netUpdate = true;
			Projectile.velocity = holdoutOffset;
			
			HandleChargeup(player);
			UpdatePlayerVisuals(player, playerCenter);
			ForceItemStay(player);
	        
			// Shoot logic
			if (CanShoot(player)) {
				Shoot(player, playerCenter, holdoutOffset);
				Projectile.Kill();
				return false;
			}

			return true;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
	        // Choose texture based on consumed arrow
	        var arrowTexture = ConsumedArrowType == ProjectileID.WoodenArrowFriendly 
		        ? TextureAssets.Projectile[ArrowType].Value 
		        : TextureAssets.Projectile[ConsumedArrowType].Value;
	        
	        // Draw arrow
	        var drawPercentage = drawTime / MaximumDrawTime;
	        var drawOffset = 16 - ArrowDrawOffset - (8f * drawPercentage);
	        var position = Projectile.Center - Main.screenPosition + Vector2.Normalize(Projectile.velocity) * drawOffset;
	        var sourceRect = arrowTexture.Frame();
	        var origin = sourceRect.Size() / 2f;
	        var rotationOffset = MathHelper.PiOver2 * Projectile.spriteDirection; // Account for arrow sprite facing up
	        var rotation = Main.player[Projectile.owner].itemRotation + rotationOffset;
	        
	        Main.EntitySpriteDraw(arrowTexture, position, sourceRect, Color.White,rotation, 
		        origin, Projectile.scale, SpriteEffects.None);
	        
	        // Draw bow
	        return true;
        }

        private void Shoot(Player player, Vector2 playerCenter, Vector2 holdoutOffset)
        {
	        var heldItem = player.HeldItem;
	        var ammoConsumed = player.PickAmmo(heldItem, out var projToShoot, out _, out _, out var knockback, out var usedAmmoItemId);

	        if (!ammoConsumed)
		        return;
	        
			// Set up arguments for shot
	        var source = player.GetSource_ItemUse_WithPotentialAmmo(heldItem, usedAmmoItemId);
	        var (boostedDamage, boostedVelocity) = GetBoostedStats();
	        var type = ArrowType == ProjectileID.WoodenArrowFriendly ? projToShoot : ArrowType;
			var position = playerCenter + (holdoutOffset / 2); // the /2 is to prevent arrow from spawning past walls
			
			// Shoot
			if (type == ProjectileID.WoodenArrowFriendly)
				OnShoot(source, position, boostedVelocity * heldItem.shootSpeed, type, boostedDamage, knockback, Projectile.owner, FullyCharged ? 1 : 0);
			else
				Projectile.NewProjectile(source, position, boostedVelocity * heldItem.shootSpeed, type, boostedDamage, knockback, Projectile.owner);
			
			SoundEngine.PlaySound(ShootSound, Projectile.position);
        }

        private (int damage, Vector2 velocity) GetBoostedStats()
        {
	        // Get boosting percentage
	        var range = MaximumDrawTime - MinimumDrawTime; // Begin scaling after exceeding minimum draw time
	        var drawPercentage = range <= 0 ? 1f : (drawTime - MinimumDrawTime) / range; // Condition handles /0 edge case
	        
	        // Apply boosts
	        var damageBoost = 1 + (damageScale - 1) * drawPercentage;
	        var velocityBoost = 1 + (velocityScale - 1) * drawPercentage;
	        
	        // Return boosted values
	        var newDamage = (int)(Projectile.damage * damageBoost);
	        var newVelocity = Vector2.Normalize(Projectile.velocity) * velocityBoost;
	        return (newDamage, newVelocity);
        }
        
		private void HandleChargeup(Player player)
		{
			// Only increment drawTime on owner, as other clients will read from ExtraAI
			if (Projectile.owner == Main.myPlayer && !FullyCharged)
			{
				drawTime++;
				if (FullyCharged && !AutoRelease)
					SoundEngine.PlaySound(SoundID.MaxMana, player.position);
			}
			else if (FullyCharged && !AutoRelease)
			{
				//Light the tip of the arrow
				var magnitude = 8 - DrawOriginOffsetY;
				var offset = (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * magnitude;
				var dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.AncientLight, Vector2.Zero);
				dust.noGravity = true;
			}
		}
		
        private void UpdatePlayerVisuals(Player player, Vector2 playerCenter)
        {
	        Projectile.direction = Projectile.velocity.X < 0 ? -1 : 1;
	        Projectile.spriteDirection = Projectile.direction;
	        player.ChangeDir(Projectile.direction);
	        player.heldProj = Projectile.whoAmI;
	        Projectile.Center = playerCenter;
	        var rotationOffset = Projectile.spriteDirection == -1 ? MathHelper.Pi : 0;
	        Projectile.rotation = Projectile.velocity.ToRotation() + rotationOffset;
	        player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
        }

        private void ForceItemStay(Player player)
        {
	        player.SetDummyItemTime(2);
	        Projectile.timeLeft = 2;
        }
        
        private static bool CanUse(Player player, bool mustChannel = true) 
	        => player is { active: true, dead: false, CCed: false, noItems: false } && (!mustChannel || player.channel);

        private bool CanShoot(Player player) => Main.myPlayer == Projectile.owner 
	        && drawTime >= MinimumDrawTime && !CanUse(player) || FullyCharged && AutoRelease;
        
        public override void SendExtraAI(BinaryWriter writer)
        {
	        writer.Write(drawTime);
	        writer.WritePackedVector2(ownerMouseWorld);
        }
        
        public override void ReceiveExtraAI(BinaryReader reader)
        {
	        drawTime = reader.ReadSingle();
	        ownerMouseWorld = reader.ReadPackedVector2();
        }
    }
}

