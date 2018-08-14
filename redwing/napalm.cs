using System;
using System.Collections;
using ModCommon;
using UnityEngine;

namespace redwing
{
    public class napalm_self_destroy : MonoBehaviour
    {
        public GameObject destroySelfWhenNull;

        private void Update()
        {
            if (destroySelfWhenNull == null)
            {
                Destroy(gameObject);
            }
        }
    }
    
    
    public class napalm : MonoBehaviour
    {
        private ParticleSystem fieryParticles;
        private ParticleSystemRenderer fieryParticleRenderer;
        private GameObject fireGenerator;

        public HealthManager cachedEnemyHM;
        public tk2dSprite cachedEnemySprite;

        private double napalmStrength = 0.0;
        private Color flameColor = Color.gray;
        private bool visible;

        public static double damageExponent;
        public static double damageMultiplier;
        

        public napalm()
        {
            //Modding.Logger.Log("Trying to add napalm... wish me luck.");
            
            fireGenerator = new GameObject(gameObject.name + " napalm", typeof(ParticleSystem), 
                typeof(ParticleSystemRenderer), typeof(napalm_self_destroy));
            fireGenerator.GetComponent<napalm_self_destroy>().destroySelfWhenNull = gameObject;
            fireGenerator.transform.localPosition = gameObject.transform.position;
            fieryParticles = fireGenerator.GetComponent<ParticleSystem>();

            ParticleSystem.MainModule partMain = fieryParticles.main;
            partMain.loop = true;
            fieryParticles.useAutoRandomSeed = true;
            partMain.gravityModifier = 0f;
            //partMain.startColor = new 
            //    ParticleSystem.MinMaxGradient(new Color(1f, 0f, 0f), new Color(1f, 1f, 0.3f));
            //partMain.startColor = new 
            //    ParticleSystem.MinMaxGradient(new Color(1f, 1f, 0.3f));
            partMain.startSize = new ParticleSystem.MinMaxCurve(1f);
            partMain.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.75f);
            partMain.maxParticles = 300;
            partMain.startSpeed = new ParticleSystem.MinMaxCurve(16f, 20f);
            partMain.startRotation = new ParticleSystem.MinMaxCurve(0, (float) (Math.PI * 2.0));
            
            ParticleSystem.EmissionModule partEmission = fieryParticles.emission;
            partEmission.enabled = true;
            partEmission.rateOverTime = new ParticleSystem.MinMaxCurve(16f);

            fieryParticleRenderer = fireGenerator.GetComponent<ParticleSystemRenderer>();
            
            
            fieryParticleRenderer.material.shader = Shader.Find("Sprites/Default");
            fieryParticleRenderer.material.mainTexture = load_textures.spark;
            fieryParticleRenderer.material.color = flameColor;
            fieryParticleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            partMain.startColor = new ParticleSystem.MinMaxGradient(Color.white);
            
            cachedEnemyHM = gameObject.GetComponent<HealthManager>();
            cachedEnemySprite = gameObject.GetComponent<tk2dSprite>();
            visible = true;
            
            if (cachedEnemyHM != null && cachedEnemyHM.hp > 0)
            {
                if (gameObject.name.StartsWith("Nightmare Grimm Boss") || gameObject.name.StartsWith("Grimm Boss"))
                {
                    DestroyImmediate(fireGenerator);
                    return;
                }
                
                StartCoroutine(dealFireDamage());
            }
            else
            {
                DestroyImmediate(fireGenerator);
                return;
            }
            fireGenerator.SetActive(true);
        }

        private void Update()
        {
            if (fireGenerator == null) return;
            
            if (cachedEnemyHM.isDead)
            {
                Destroy(fireGenerator);
                return;
            }
            
            Vector3 newPos = gameObject.transform.position;
            newPos.z = -1f;
            fireGenerator.transform.localPosition = newPos;
            
            ParticleSystem.Particle[] particleList = new ParticleSystem.Particle[fieryParticles.particleCount];
            fieryParticles.GetParticles(particleList);
            for(int i = 0; i < particleList.Length; ++i)
            {
                float lifePercentage = (particleList[i].remainingLifetime / particleList[i].startLifetime);
                particleList[i].startColor = Color.Lerp(Color.clear, Color.white, lifePercentage);
            }
            fieryParticles.SetParticles(particleList, fieryParticles.particleCount);
            
            if (cachedEnemySprite == null) return;
            
            if (visible && (!cachedEnemySprite.isActiveAndEnabled || cachedEnemySprite.color.a < 0.05f))
            {
                Modding.Logger.Log("Found invisible enemy, making invis!");
                fieryParticleRenderer.material.color = Color.clear;
                visible = false;
            } else if (!visible && (cachedEnemySprite.isActiveAndEnabled || cachedEnemySprite.color.a >= 0.05f))
            {
                Modding.Logger.Log("Enemy now visible!");
                fieryParticleRenderer.material.color = flameColor;
                visible = true;
            }
        }

        public void addNapalm(double napalm, Color napalmColor)
        {
            if (fireGenerator == null) return;
            
            if (napalmStrength > 0.0)
            {
                double netNapalm = napalm + napalmStrength;

                float avgRed = (float) ((napalmColor.r * napalm + flameColor.r * napalmStrength) / netNapalm);
                float avgGrn = (float) ((napalmColor.g * napalm + flameColor.g * napalmStrength) / netNapalm);
                float avgBlu = (float) ((napalmColor.b * napalm + flameColor.b * napalmStrength) / netNapalm);
                
                flameColor = new Color(avgRed, avgGrn, avgBlu);

                if (napalmStrength < 10.0)
                {
                    flameColor.a = (float) (napalmStrength / 10.0);
                }
                else
                {
                    flameColor.a = 1.0f;
                }
                napalmStrength += napalm;
            }
            else
            {
                flameColor = napalmColor;
                napalmStrength = napalm;
            }

            if (visible)
            {
                fieryParticleRenderer.material.color = flameColor;
            }
        }

        private IEnumerator dealFireDamage()
        {
            while (cachedEnemyHM.hp > 0)
            {
                if (napalmStrength >= 1.0)
                {
                    //Modding.Logger.Log("Doing " + calculateNapalmDamage() + " napalm dmg to " + gameObject.name);
                    cachedEnemyHM.hp -= calculateNapalmDamage();
                    if (cachedEnemyHM.hp <= 0)
                    {
                        cachedEnemyHM.Die(0f, AttackTypes.Generic, true);
                        DestroyImmediate(fireGenerator);
                    }
                    napalmStrength = (napalmStrength * 0.95) - 1.0;
                    setNapalmParts();
                }
                else
                {
                    fieryParticleRenderer.material.color = Color.clear;
                }
                yield return new WaitForSeconds((float) (Math.Pow(Math.E, -0.05*napalmStrength)) + 0.05f);
            }
            DestroyImmediate(fireGenerator);
        }

        private void setNapalmParts()
        {
            if (fireGenerator == null) return;
            ParticleSystem.MainModule partMain = fieryParticles.main;
            partMain.startSize = napalmStrength < 30.0 ?
                new ParticleSystem.MinMaxCurve((float) ((napalmStrength) / 40.0),
                    (float) ((napalmStrength) / 40.0) + 0.25f) : 
                new ParticleSystem.MinMaxCurve(0.75f, 1f);

            ParticleSystem.EmissionModule partEmission = fieryParticles.emission;
            partEmission.rateOverTime = new ParticleSystem.MinMaxCurve((float) ((napalmStrength) / 2.0));
        }


        private int calculateNapalmDamage()
        {
            return (int) (Math.Pow( (napalmStrength * damageMultiplier), damageExponent));
        }


        public void addParticles(int particlesToAdd, Color fireColor)
        {
            for (int i = 0; i < particlesToAdd; i++)
            {
                fieryParticles.Emit(new ParticleSystem.EmitParams()
                {
                    angularVelocity = 0f,
                    angularVelocity3D = Vector3.zero,
                    applyShapeToPosition = false,
                    axisOfRotation = Vector3.fwd,
                    position = Vector3.zero,
                    randomSeed = (uint) redwing_flame_gen.rng.Next(0, int.MaxValue),
                    rotation = 0f,
                    rotation3D = Vector3.zero,
                    startColor = fireColor,
                    startLifetime = 2f,
                    startSize = 1.0f,
                    startSize3D = Vector3.one,
                    velocity = new Vector3((float)(redwing_flame_gen.rng.NextDouble()- 0.5) * 12f,
                        (float)(redwing_flame_gen.rng.NextDouble()- 0.5) * 12f)
                }, 1);
            }
            
            
        }
    }
}