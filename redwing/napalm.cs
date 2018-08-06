using System;
using System.Collections;
using ModCommon;
using UnityEngine;

namespace redwing
{
    public class napalm : MonoBehaviour
    {
        private ParticleSystem fieryParticles;
        private ParticleSystemRenderer fieryParticleRenderer;
        private GameObject fireGenerator;

        public HealthManager cachedEnemyHM;

        private double napalmStrength = 0.0;
        private Color flameColor = Color.gray;

        private const double damageExponent = 0.7;

        public napalm()
        {
            Modding.Logger.Log("Trying to add napalm... wish me luck.");
            
            fireGenerator = new GameObject(gameObject.name + " napalm", typeof(ParticleSystem), typeof(ParticleSystemRenderer));
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
            partMain.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            partMain.maxParticles = 300;
            partMain.startSpeed = new ParticleSystem.MinMaxCurve(10f, 16f);
            
            ParticleSystem.EmissionModule partEmission = fieryParticles.emission;
            partEmission.enabled = true;
            partEmission.rateOverTime = new ParticleSystem.MinMaxCurve(8f);

            fieryParticleRenderer = fireGenerator.GetComponent<ParticleSystemRenderer>();
            
            
            fieryParticleRenderer.material.shader = Shader.Find("Particles/Blend");
            fieryParticleRenderer.material.mainTexture = redwing_flame_gen.perfectWhiteCircle;
            fieryParticleRenderer.material.color = Color.white;
            fieryParticleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            
            ParticleSystem.ColorOverLifetimeModule fadeOut = fieryParticles.colorOverLifetime;
            fadeOut.color = new ParticleSystem.MinMaxGradient(flameColor, new Color(0.5f, 0.5f, 0.5f, 0f));
            fadeOut.enabled = true;

            cachedEnemyHM = gameObject.GetComponent<HealthManager>();
            
            if (cachedEnemyHM != null && cachedEnemyHM.hp > 0)
            {
                StartCoroutine(dealFireDamage());
            }
            else
            {
                DestroyImmediate(fireGenerator);
            }
            
            fireGenerator.SetActive(true);

            
        }

        private void Update()
        {
            if (fireGenerator != null)
                fireGenerator.transform.localPosition = gameObject.transform.position;
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

            Color blankFlameColor = flameColor;
            blankFlameColor.a = 0;
            
            ParticleSystem.ColorOverLifetimeModule fadeOut = fieryParticles.colorOverLifetime;
            fadeOut.color = new ParticleSystem.MinMaxGradient(flameColor, blankFlameColor);
        }

        private IEnumerator dealFireDamage()
        {
            while (cachedEnemyHM.hp > 0)
            {
                if (napalmStrength >= 1.0)
                {
                    Modding.Logger.Log("Doing " + calculateNapalmDamage() + " napalm dmg to " + gameObject.name);
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
                    ParticleSystem.ColorOverLifetimeModule fadeOut = fieryParticles.colorOverLifetime;
                    fadeOut.color = new ParticleSystem.MinMaxGradient(Color.clear);
                }
                yield return new WaitForSeconds(0.5f);
            }
            DestroyImmediate(fireGenerator);
        }

        private void setNapalmParts()
        {
            if (fireGenerator == null) return;
            ParticleSystem.MainModule partMain = fieryParticles.main;
            partMain.startSize = napalmStrength < 30.0 ?
                new ParticleSystem.MinMaxCurve((float) ((napalmStrength) / 40.0)) : new ParticleSystem.MinMaxCurve(0.75f);

            ParticleSystem.EmissionModule partEmission = fieryParticles.emission;
            partEmission.rateOverTime = new ParticleSystem.MinMaxCurve((float) ((napalmStrength) / 2.0));
        }


        private int calculateNapalmDamage()
        {
            return (int) (Math.Pow(napalmStrength, damageExponent));
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