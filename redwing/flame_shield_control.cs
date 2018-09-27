using UnityEngine;

namespace redwing
{
    public class flame_shield_control : MonoBehaviour
    {
        private const double FLAME_SHIELD_CD = 15;
        private float shieldCooldown = 0f;
        private const float FS_UPDATE_TIME = 0.08f;
        private float fsLastUpdate = 0f;
        private int lastFSState = 3;
        private int currentFlameShieldTexture = 0;
        public double fsCharge { get; private set; } = 0;
        
        
        private SpriteRenderer flameShieldSprite;
        private AudioSource flameShieldAudio;


        public bool canCharge { get; private set; } = true;

        public void discharge()
        {
            fsCharge = FLAME_SHIELD_CD;
            if (lastFSState > 0)
            {
                lastFSState = -1;
            }
        }

        public void enableCharge()
        {
            canCharge = true;
        }

        public void disableCharge()
        {
            canCharge = false;
            fsCharge = FLAME_SHIELD_CD;
            if (lastFSState > 0)
            {
                lastFSState = -1;
            }
        }
        
        private void Start()
        {
            flameShieldSprite = gameObject.GetComponent<SpriteRenderer>();
            
            flameShieldSprite.color = Color.white;
            flameShieldAudio = gameObject.GetComponent<AudioSource>();
            flameShieldAudio.loop = false;
        }

        private void Update()
        {
            // This will increment to infinity if health binding is on. so be it. it can't overflow because deltaTime is too small
            // to push it past the larger epsilons.
            fsLastUpdate += Time.deltaTime;
            
            if (gng_bindings.hasHealthBinding())
            {
                flameShieldSprite.color = Color.clear;
                fsCharge = FLAME_SHIELD_CD;
                return;
            }
            
            if (canCharge && fsCharge > 0)
            {
                fsCharge -= Time.deltaTime;
            }
            
            if (!(fsLastUpdate > FS_UPDATE_TIME)) return;
            
            fsLastUpdate = 0f;
            const float yPivot = 0.5f;
            
            if (lastFSState < 0)
            {
                if (lastFSState == -1)
                {
                    flameShieldSprite.sprite = Sprite.Create(load_textures.FLAME_SHIELD_LOST[0],
                        new Rect(0, 0, load_textures.FLAME_SHIELD_LOST[0].width, load_textures.FLAME_SHIELD_LOST[0].height - load_textures.flameYOffset),
                        new Vector2(0.5f, yPivot));
                    lastFSState--;
                } else if (lastFSState == -2)
                {
                    flameShieldSprite.sprite = Sprite.Create(load_textures.FLAME_SHIELD_LOST[1],
                        new Rect(0, 0, load_textures.FLAME_SHIELD_LOST[1].width, load_textures.FLAME_SHIELD_LOST[1].height - load_textures.flameYOffset),
                        new Vector2(0.5f, yPivot));
                    lastFSState--;
                } else if (lastFSState == -3)
                {
                    flameShieldSprite.sprite = Sprite.Create(load_textures.FLAME_SHIELD_LOST[2],
                        new Rect(0, 0, load_textures.FLAME_SHIELD_LOST[2].width, load_textures.FLAME_SHIELD_LOST[2].height - load_textures.flameYOffset),
                        new Vector2(0.5f, yPivot));
                    lastFSState--;
                }
                else
                {
                    flameShieldSprite.sprite = Sprite.Create(load_textures.FLAME_SHIELD_LOST[3],
                        new Rect(0, 0, load_textures.FLAME_SHIELD_LOST[3].width, load_textures.FLAME_SHIELD_LOST[3].height - load_textures.flameYOffset),
                        new Vector2(0.5f, yPivot));
                    
                    lastFSState = 0;
                    currentFlameShieldTexture = 0;
                }

                return;
            }

            if (fsCharge > 20.0)
            {
                return;
            }

            if (fsCharge > 10.0)
            {
                if (lastFSState == 0)
                {
                    currentFlameShieldTexture = -load_textures.FLAME_SHIELD_CHARGE1_INTRO_FRAMES;
                }
                int i = load_textures.FLAME_SHIELD_CHARGE1_INTRO_FRAMES + currentFlameShieldTexture;

                if (i >= load_textures.FLAME_SHIELD_CHARGE1.Length)
                {
                    currentFlameShieldTexture = 0;
                    i = load_textures.FLAME_SHIELD_CHARGE1_INTRO_FRAMES;
                }
                flameShieldSprite.sprite = Sprite.Create(load_textures.FLAME_SHIELD_CHARGE1[i],
                    new Rect(0, 0, load_textures.FLAME_SHIELD_CHARGE1[i].width, load_textures.FLAME_SHIELD_CHARGE1[i].height - load_textures.flameYOffset),
                    new Vector2(0.5f, yPivot));
                lastFSState = 1;
                currentFlameShieldTexture++;
            } else if (fsCharge > 0.0)
            {
                if (lastFSState == 1)
                {
                    currentFlameShieldTexture = -load_textures.FLAME_SHIELD_CHARGE2_INTRO_FRAMES;
                }
                
                int i = load_textures.FLAME_SHIELD_CHARGE2_INTRO_FRAMES + currentFlameShieldTexture;
                if (i >= load_textures.FLAME_SHIELD_CHARGE2.Length)
                {
                    currentFlameShieldTexture = 0;
                    i = load_textures.FLAME_SHIELD_CHARGE2_INTRO_FRAMES;
                }
                
                flameShieldSprite.sprite = Sprite.Create(load_textures.FLAME_SHIELD_CHARGE2[i],
                    new Rect(0, 0, load_textures.FLAME_SHIELD_CHARGE2[i].width, load_textures.FLAME_SHIELD_CHARGE2[i].height - load_textures.flameYOffset),
                    new Vector2(0.5f, yPivot));
                lastFSState = 2;
                currentFlameShieldTexture++;
            }
            else if (fsCharge <= 0.0)
            {
                if (lastFSState == 2)
                {
                    currentFlameShieldTexture = -load_textures.FLAME_SHIELD_CHARGED_INTRO_FRAMES;
                }
                
                int i = load_textures.FLAME_SHIELD_CHARGED_INTRO_FRAMES + currentFlameShieldTexture;
                if (i >= load_textures.FLAME_SHIELD_CHARGED.Length)
                {
                    currentFlameShieldTexture = 0;
                    i = load_textures.FLAME_SHIELD_CHARGED_INTRO_FRAMES;
                }
                
                flameShieldSprite.sprite = Sprite.Create(load_textures.FLAME_SHIELD_CHARGED[i],
                    new Rect(0, 0, load_textures.FLAME_SHIELD_CHARGED[i].width, load_textures.FLAME_SHIELD_CHARGED[i].height - load_textures.flameYOffset),
                    new Vector2(0.5f, 0.5f));
                lastFSState = 3;
                currentFlameShieldTexture++;
            }

        }
    }
}