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
	    private float drawTime = 0;//The current charge value.
	    private const int MinimumDrawTime = 30; // The minimum charge required before the bow can release the arrow. Set to be the bow's usetime.
	    protected virtual int ArrowType => ProjectileID.None;

	    /// <summary>
	    /// Determines held item's distance from the player center, and where the projectile is spawned.
	    /// <para>Defaults to 10 pixels.</para>
	    /// </summary>
	    protected int HoldoutDistance { get; set; } = 10;

	    /// <summary>
	    /// How far away the arrow should be drawn. Higher values means the arrow will be drawn closer to the bow.
	    /// <para>Defaults to 0 pixels.</para>
	    /// </summary>
	    protected int ArrowDrawOffset { get; set; } = 0;
	    
	    /// <summary>
	    /// The sound that plays once the projectile has been released.
	    /// <para>Defaults to Item5 (bow shoot sound).</para>
	    /// </summary>
	    protected SoundStyle ReleaseSound { get; set; } = SoundID.Item5;
	    
	    /// <summary>
        /// The maximum amount of charge an arrow can have, at which point draw time will stop increasing. Draw time starts at 0 and ticks up by 1 every update while an arrow exists.
        /// <br>If you wish to check if the arrow is fully charged, use the FullyCharged property instead.</br>
        /// <para>Defaults to 80.</para>
        /// </summary>
        protected int MaximumDrawTime { get; set; } = 80;

        /// <summary>
        /// True when the arrow is fully charged.
        /// </summary>
        protected bool FullyCharged => (int)drawTime == MaximumDrawTime;

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
            set
            {
                if (value < 1) damageScale = 1;
                else damageScale = value;
            }
        }
        private float damageScale = 2;

        /// <summary>
        /// How much the velocity should be multiplied based on its charging percentage. The value cannot be set below 1.
        /// <para>Defaults to 2 times increase.</para>
        /// </summary>
        protected float VelocityScale
        {
            get => velocityScale;
            set
            {
                if (value < 1) velocityScale = 1;
                else velocityScale = value;
            }
        }
        private float velocityScale = 2;

        /// <summary>
        /// This method controls what happens upon shooting. By default, it spawns an instance of ArrowType.
        /// <br>Override this to change shooting logic as well as shot projectile.</br>
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

        public override bool PreAI()
        {
			var player = Main.player[Projectile.owner];
			var playerCenter = player.RotatedRelativePoint(player.MountedCenter);
	        var canShoot = drawTime >= MinimumDrawTime && !CanUse(player) || FullyCharged && AutoRelease;
			
			// Update holding direction
			var holdoutOffset = HoldoutDistance * Vector2.Normalize(Main.MouseWorld - playerCenter);
			
	        // Net sync
			if (holdoutOffset != Projectile.velocity) 
				Projectile.netUpdate = true;
			Projectile.velocity = holdoutOffset;
			
			HandleDrawTime(player);
			UpdatePlayer(player, playerCenter);
	        
			// Shoot logic
			if (canShoot && Main.myPlayer == Projectile.owner) {
				Shoot(player, playerCenter, holdoutOffset);
				Projectile.Kill();
			}

			return true;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
	        // Draw wooden or specified arrow
	        var arrowTexture = ArrowType == ProjectileID.None 
		        ? TextureAssets.Projectile[ProjectileID.WoodenArrowFriendly].Value
		        : TextureAssets.Projectile[ArrowType].Value;

	        // In case player uses a different arrow, draw that one instead
	        if ((int)Projectile.ai[0] != ProjectileID.WoodenArrowFriendly)
		        arrowTexture = TextureAssets.Projectile[(int)Projectile.ai[0]].Value;
	        
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
	        var ammoConsumed = player.PickAmmo(heldItem, out var projToShoot, out var speed, out var damage, out var knockback, out var usedAmmoItemId);

	        if (!ammoConsumed)
		        return;
	        
			// Set up arguments for shot
	        var source = player.GetSource_ItemUse_WithPotentialAmmo(heldItem, usedAmmoItemId);
	        var (boostedDamage, boostedVelocity) = GetBoostedStats();
	        var type = ArrowType == ProjectileID.None ? projToShoot : ArrowType;
			var position = playerCenter + (holdoutOffset / 2);
			
			// Shoot
			if (projToShoot == ProjectileID.WoodenArrowFriendly)
				OnShoot(source, position, boostedVelocity * heldItem.shootSpeed, type, boostedDamage, knockback, Projectile.owner, FullyCharged ? 1 : 0, 0, 0);
			else
				Projectile.NewProjectile(source, position, boostedVelocity * heldItem.shootSpeed, projToShoot, boostedDamage, knockback, Projectile.owner);
			
			SoundEngine.PlaySound(ReleaseSound, Projectile.position);
        }

        private (int damage, Vector2 velocity) GetBoostedStats()
        {
	        // Calculate boosts from the arrow's draw time.
	        var drawPercentage = (drawTime - MinimumDrawTime) / (MaximumDrawTime - MinimumDrawTime); // Begin scaling after exceeding minimum draw time
	        var damageBoost = 1 + (damageScale - 1) * drawPercentage;
	        var velocityBoost = 1 + (velocityScale - 1) * drawPercentage;
	        
	        var newDamage = (int)(Projectile.damage * damageBoost);
	        var newVelocity = Vector2.Normalize(Projectile.velocity) * velocityBoost;

	        return (newDamage, newVelocity);
        }
        
		private void HandleDrawTime(Player player)
		{
			if (drawTime < MaximumDrawTime)
			{
				drawTime++;
				if (drawTime >= MaximumDrawTime && !AutoRelease)
					SoundEngine.PlaySound(SoundID.MaxMana, player.position);
			}
			else if (!AutoRelease)
			{
				//Light the tip of the arrow
				var magnitude = 8 - DrawOriginOffsetY;
				var offset = (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * magnitude;
				var dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.AncientLight, Vector2.Zero);
				dust.noGravity = true;
			}
		}
		
        private void UpdatePlayer(Player player, Vector2 playerCenter)
        {
	        // Make player acts as if the holdout projectile is their item
	        Projectile.direction = Projectile.velocity.X < 0 ? -1 : 1;
	        Projectile.spriteDirection = Projectile.direction;
	        player.ChangeDir(Projectile.direction);
	        player.heldProj = Projectile.whoAmI;
	        player.SetDummyItemTime(2);
	        Projectile.Center = playerCenter;
	        var rotationOffset = Projectile.spriteDirection == -1 ? MathHelper.Pi : 0;
	        Projectile.rotation = Projectile.velocity.ToRotation() + rotationOffset;
	        player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
	        Projectile.timeLeft = 2;
        }
        
        private static bool CanUse(Player player, bool mustChannel = true) 
	        => player is { active: true, dead: false, CCed: false, noItems: false } && (!mustChannel || player.channel);

    }
}

