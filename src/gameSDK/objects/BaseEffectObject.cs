using foundation;
using UnityEngine;

namespace gameSDK
{

    public class BaseEffectObject : BaseObject
    {
        public static bool isParticleScalingBug = false;

        public float playbackSpeed = 1.0f;
        public BaseEffectObject()
        {
            prefix = "effect";
            resourceRootDir = PathDefine.effectPath;
        }

        private float __prePlaybackSpeed = 1.0f;
        protected override void bindComponents()
        {
            if (layer != -1)
            {
                setContentLayer(_skin.transform,layer);
            }
            _animator=_skin.GetComponent<Animator>();
            if (playbackSpeed != __prePlaybackSpeed)
            {
                __prePlaybackSpeed = playbackSpeed;
                ParticleSystem[] particleSystems = _skin.GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem particleSystem in particleSystems)
                {
                    if (particleSystem.main.scalingMode == ParticleSystemScalingMode.Local)
                    {
                        ParticleSystem.MainModule mainModule =particleSystem.main;
                        mainModule.simulationSpeed = playbackSpeed;
                    }
                }
            }

            
            if (isParticleScalingBug)
            {
                ParticleSystem[] particleSystems=_skin.GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem particleSystem in particleSystems)
                {
                    if (particleSystem.main.scalingMode == ParticleSystemScalingMode.Local)
                    {
                        particleSystem.gameObject.AddComponent<ParticleScalingFixBug>();
                    }
                }
            }
        }
    }
}
