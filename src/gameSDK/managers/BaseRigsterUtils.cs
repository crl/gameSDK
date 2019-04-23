using clayui;
using foundation;

namespace gameSDK
{
    public class BaseRigsterUtils
    {
        public static void init()
        {
            ObjectFactory.registerClassAlias<SkillTimeLineVO>("vo.skillVO");
            ObjectFactory.registerClassAlias<SkillLineVO>("vo.SkillLineVO");
            ObjectFactory.registerClassAlias<SkillPointVO>("vo.SkillPointVO");

            ObjectFactory.registerClassAlias<EffectCreateEvent>("vo.EffectCreateEvent");
            ObjectFactory.registerClassAlias<EffectFollowEvent>("vo.EffectFollowEvent");

            ObjectFactory.registerClassAlias<MoveEvent>("vo.MoveEvent");
            ObjectFactory.registerClassAlias<PlayAnimEvent>("vo.PlayAnimEvent");
            ObjectFactory.registerClassAlias<SetAnimationBoolEvent>("vo.SetAnimationBoolEvent");

            ObjectFactory.registerClassAlias<TimeScaleEvent>("vo.TimeScaleEvent");
            ObjectFactory.registerClassAlias<CameraMoveEvent>("vo.CameraMoveEvent");
            ObjectFactory.registerClassAlias<CameraShakeEvent>("vo.CameraShakeEvent");
            ObjectFactory.registerClassAlias<PlaySoundEvent>("vo.PlaySoundEvent");
            ObjectFactory.registerClassAlias<TrigerEvent>("vo.TriggerEvent");
            ObjectFactory.registerClassAlias<SkillEvent>("vo.SkillEvent");
            ObjectFactory.registerClassAlias<EmptyEvent>("vo.EmptyEvent");
        
            ObjectFactory.registerClassAlias<FlashShowEvent>("vo.FlashShowEvent");
            ObjectFactory.registerClassAlias<GhostEffectEvent>("vo.GhostEffectEvent");
        }
    }
}