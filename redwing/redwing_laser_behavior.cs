using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace redwing
{
	public class redwing_laser_spawner_behavior : MonoBehaviour
	{
		private const float lifespan = 1.2f;

		public void Start()
		{
			StartCoroutine(despawn());
		}

		private IEnumerator despawn()
		{
			float currentTime = 0f;
			while (currentTime < lifespan)
			{
				yield return null;
				currentTime += Time.unscaledDeltaTime;
			}
			Modding.Logger.Log("[REDWING] Despawning laser spawner because time ran out");
			Destroy(this.gameObject);
		}
	}
	
	
	public class redwing_laser_behavior : MonoBehaviour
	{
		private const float LIFESPAN = 1f;
		private const int LASER_WIDTH = 80;
		private const int LASER_HEIGHT = 1500;

		public int damageDealt;
		public DamageEnemies hurtEm;
		public SpriteRenderer drawEm;

		public Texture2D spriteUsed;

		// const float ANIMATION_SPEED = 60f;
		private bool didDamage;


		public void Start()
		{
			enteredColliders = new List<Collider2D>();
			StartCoroutine(playAnimation());
		}

		private IEnumerator playAnimation()
		{
			float currentTime = 0f;
			int currentFrame = 0;
			while (currentFrame < 10)
			{
				drawFrame(currentFrame);
				currentTime += Time.unscaledDeltaTime;
				currentFrame = (int) (10.0 * (currentTime) / (LIFESPAN));
				if (currentFrame == 5 && !didDamage)
				{
					hurtEnemies();
					didDamage = true;
				}
				else
				{
					yield return null;
				}
			}

			Destroy(this.gameObject);
		}

		private void drawFrame(int currentFrame)
		{
			const int laserMiddle = LASER_WIDTH / 2;
			Rect r;
			Color c;
			switch (currentFrame)
			{
				case 0:
					r = new Rect(laserMiddle - 4, 0, 8, LASER_HEIGHT);
					drawEm.sprite = Sprite.Create(spriteUsed, r, new Vector2(0.5f, 0f));
					drawEm.color = Color.white;
					break;
				case 1:
					r = new Rect(laserMiddle - 5, 0, 10, LASER_HEIGHT);
					drawEm.sprite = Sprite.Create(spriteUsed, r, new Vector2(0.5f, 0f));
					break;
				case 2:
					r = new Rect(laserMiddle - 6, 0, 12, LASER_HEIGHT);
					drawEm.sprite = Sprite.Create(spriteUsed, r, new Vector2(0.5f, 0f));
					break;
				case 3:
					r = new Rect(laserMiddle - 7, 0, 14, LASER_HEIGHT);
					drawEm.sprite = Sprite.Create(spriteUsed, r, new Vector2(0.5f, 0f));
					break;
				case 4:
					r = new Rect(laserMiddle - 8, 0, 16, LASER_HEIGHT);
					drawEm.sprite = Sprite.Create(spriteUsed, r, new Vector2(0.5f, 0f));
					break;
				case 5:
					r = new Rect(0, 0, LASER_WIDTH, LASER_HEIGHT);
					drawEm.sprite = Sprite.Create(spriteUsed, r, new Vector2(0.5f, 0f));
					break;
				case 6:
					r = new Rect(0, 0, LASER_WIDTH, LASER_HEIGHT);
					drawEm.sprite = Sprite.Create(spriteUsed, r, new Vector2(0.5f, 0f));
					c = drawEm.color;
					c.a = 0.8f;
					drawEm.color = c;
					break;
				case 7:
					r = new Rect(0, 0, LASER_WIDTH, LASER_HEIGHT);
					drawEm.sprite = Sprite.Create(spriteUsed, r, new Vector2(0.5f, 0f));
					c = drawEm.color;
					c.a = 0.6f;
					drawEm.color = c;
					break;
				case 8:
					r = new Rect(0, 0, LASER_WIDTH, LASER_HEIGHT);
					drawEm.sprite = Sprite.Create(spriteUsed, r, new Vector2(0.5f, 0f));
					c = drawEm.color;
					c.a = 0.4f;
					drawEm.color = c;
					break;
				case 9:
					r = new Rect(0, 0, LASER_WIDTH, LASER_HEIGHT);
					drawEm.sprite = Sprite.Create(spriteUsed, r, new Vector2(0.5f, 0f));
					c = drawEm.color;
					c.a = 0.2f;
					drawEm.color = c;
					break;
				default:

					break;
			}
		}


		private void OnTriggerEnter2D(Collider2D collision)
		{
			int layer = collision.gameObject.layer;
			if (layer == 20 || layer == 9 || layer == 26 || layer == 31)
			{
				return;
			}

			if (collision.CompareTag("Geo"))
			{
				return;
			}

			if (!this.enteredColliders.Contains(collision))
			{
				enteredColliders.Add(collision);
			}
		}

		// Token: 0x060024BB RID: 9403 RVA: 0x000C21FA File Offset: 0x000C03FA
		private void OnTriggerExit2D(Collider2D collision)
		{
			if (this.enteredColliders.Contains(collision))
			{
				this.enteredColliders.Remove(collision);
			}
		}

		private void hurtEnemies()
		{
			log("Hurting enemies. There are " + enteredColliders.Count + " enemies to hurt");
			for (int i = this.enteredColliders.Count - 1; i >= 0; i--)
			{
				Collider2D collider2D = this.enteredColliders[i];
				if (collider2D == null || !collider2D.isActiveAndEnabled)
				{
					this.enteredColliders.RemoveAt(i);
				}
				else
				{
					this.doDamage(collider2D.gameObject);
				}
			}
		}

		private void doDamage(GameObject target)
		{
			if (this.damageDealt <= 0)
			{
				return;
			}

			FSMUtility.SendEventToGameObject(target, "TAKE DAMAGE", false);
			HitTaker.Hit(target, new HitInstance
			{
				Source = base.gameObject,
				AttackType = AttackTypes.Generic,
				CircleDirection = this.circleDirection,
				DamageDealt = damageDealt,
				Direction = this.direction,
				IgnoreInvulnerable = ignoreInvuln,
				MagnitudeMultiplier = magnitudeMult,
				MoveAngle = 0f,
				MoveDirection = moveDirection,
				Multiplier = 1f,
				SpecialType = specialType,
				IsExtraDamage = false
			}, 3);
		}

		public AttackTypes attackType = AttackTypes.Generic;
		public bool circleDirection = false;
		public float direction = 0f;
		public bool ignoreInvuln = true;
		public float magnitudeMult = 1.0f;
		public bool moveDirection = false;
		public SpecialTypes specialType = SpecialTypes.None;
		
		private List<Collider2D> enteredColliders;


		private static void log(string str)
		{
			Modding.Logger.Log("[Redwing] " + str);
		}
	}
}