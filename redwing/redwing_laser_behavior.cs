using System.Collections;
using System.Collections.Generic;
using ModCommon;
using UnityEngine;

namespace redwing
{
	public class redwing_laser_spawner_behavior : MonoBehaviour
	{
		private const float lifespan = 0.7f;
		public static AudioClip laserFX;
		
		public void Start()
		{
			StartCoroutine(despawn());
			AudioSource a = gameObject.GetComponent<AudioSource>();
			a.clip = laserFX;
			a.volume = (GameManager.instance.gameSettings.masterVolume *
			            GameManager.instance.gameSettings.soundVolume * 0.01f);
			a.Play();
			
		}

		private IEnumerator despawn()
		{
			yield return new WaitForSecondsRealtime(lifespan);
			Destroy(this.gameObject);
		}
	}
	
	
	public class redwing_laser_behavior : MonoBehaviour
	{
		private const float LIFESPAN = 0.25f;

		public SpriteRenderer drawEm;

		public Texture2D spriteUsed;
		
		public bool soloLaser;

		private float yPivot;

		// const float ANIMATION_SPEED = 60f;


		public void Start()
		{
			yPivot = 0f;
			StartCoroutine(playAnimation());
		}

		private IEnumerator hurtEnemies()
		{
			const float waitTime = 0.15f;
			yield return new WaitForSecondsRealtime(waitTime);

			log("there are " + enteredColliders.Count + " enemies in your list");
			
			foreach (Collider2D collider in enteredColliders)
			{
                
				GameObject target = collider.gameObject;
				log("Doing laser damage to target with name " + target.name);
                
                
				redwing_game_objects.applyHitInstance(collider.gameObject,
					redwing_hooks.laserDamageBase + redwing_hooks.laserDamagePerNail *
					PlayerData.instance.GetInt("nailSmithUpgrades"),
					AttackTypes.Spell, redwing_game_objects.voidKnight);
			}

			soloLaser = false;

		}

		private IEnumerator playAnimation()
		{
			if (soloLaser)
			{
				StartCoroutine(hurtEnemies());
				yPivot = 0.5f;
			}

			float currentTime = 0f;
			int currentFrame = 0;
			while (currentFrame < 10)
			{
				drawFrame(currentFrame);
				currentTime += Time.unscaledDeltaTime;
				currentFrame = (int) (10.0 * (currentTime) / (LIFESPAN));
				yield return null;
			}

			while (soloLaser)
			{
				yield return null;
			}

			Destroy(this.gameObject);
		}
		
		
		/*
		 * To be honest, you need a hecking good GPU to make this function run at full speed
		 * On most computers it will probably be so laggy as to only play around 3 or 4 of the 10 frames.
		 *
		 * I'm not too worried about this though. It's just an effect. The solution is to make the
		 * sprite much smaller. Like maybe only 250 px long. And then use transform to scale it by 12x or something
		 * crazy.
		 * 
		 */
		private void drawFrame(int currentFrame)
		{
			const int laserMiddle = redwing_flame_gen.LASERTEXTURE_WIDTH / 2;
			Rect r;
			Color c;
			switch (currentFrame)
			{
				case 0:
					r = new Rect(laserMiddle - 4, 0, 8, redwing_flame_gen.LASERTEXTURE_HEIGHT);
					drawEm.sprite = Sprite.Create(spriteUsed, r, new Vector2(0.5f, yPivot), 50);
					drawEm.color = Color.white;
					break;
				case 1:
					r = new Rect(laserMiddle - 5, 0, 10, redwing_flame_gen.LASERTEXTURE_HEIGHT);
					drawEm.sprite = Sprite.Create(spriteUsed, r, new Vector2(0.5f, yPivot), 50);
					break;
				case 2:
					r = new Rect(laserMiddle - 6, 0, 12, redwing_flame_gen.LASERTEXTURE_HEIGHT);
					drawEm.sprite = Sprite.Create(spriteUsed, r, new Vector2(0.5f, yPivot), 50);
					break;
				case 3:
					r = new Rect(laserMiddle - 7, 0, 14, redwing_flame_gen.LASERTEXTURE_HEIGHT);
					drawEm.sprite = Sprite.Create(spriteUsed, r, new Vector2(0.5f, yPivot), 50);
					break;
				case 4:
					r = new Rect(laserMiddle - 8, 0, 16, redwing_flame_gen.LASERTEXTURE_HEIGHT);
					drawEm.sprite = Sprite.Create(spriteUsed, r, new Vector2(0.5f, yPivot), 50);
					break;
				case 5:
					r = new Rect(0, 0, redwing_flame_gen.LASERTEXTURE_WIDTH, redwing_flame_gen.LASERTEXTURE_HEIGHT);
					drawEm.sprite = Sprite.Create(spriteUsed, r, new Vector2(0.5f, yPivot), 50);
					break;
				case 6:
					r = new Rect(0, 0, redwing_flame_gen.LASERTEXTURE_WIDTH, redwing_flame_gen.LASERTEXTURE_HEIGHT);
					drawEm.sprite = Sprite.Create(spriteUsed, r, new Vector2(0.5f, yPivot), 50);
					c = drawEm.color;
					c.a = 0.8f;
					drawEm.color = c;
					break;
				case 7:
					r = new Rect(0, 0, redwing_flame_gen.LASERTEXTURE_WIDTH, redwing_flame_gen.LASERTEXTURE_HEIGHT);
					drawEm.sprite = Sprite.Create(spriteUsed, r, new Vector2(0.5f, yPivot), 50);
					c = drawEm.color;
					c.a = 0.6f;
					drawEm.color = c;
					break;
				case 8:
					r = new Rect(0, 0, redwing_flame_gen.LASERTEXTURE_WIDTH, redwing_flame_gen.LASERTEXTURE_HEIGHT);
					drawEm.sprite = Sprite.Create(spriteUsed, r, new Vector2(0.5f, yPivot), 50);
					c = drawEm.color;
					c.a = 0.4f;
					drawEm.color = c;
					break;
				case 9:
					r = new Rect(0, 0, redwing_flame_gen.LASERTEXTURE_WIDTH, redwing_flame_gen.LASERTEXTURE_HEIGHT);
					drawEm.sprite = Sprite.Create(spriteUsed, r, new Vector2(0.5f, yPivot), 50);
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
			if (layer != 11) return;

			if (collision.CompareTag("Geo"))
			{
				return;
			}
			
			if (!this.enteredColliders.Contains(collision))
			{
				enteredColliders.Add(collision);
			}
		}

		public List<Collider2D> enteredColliders;


		private static void log(string str)
		{
			Modding.Logger.Log("[Redwing] " + str);
		}
	}
}