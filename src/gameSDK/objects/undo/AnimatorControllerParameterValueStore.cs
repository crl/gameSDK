
using System.Collections.Generic;
using UnityEngine;

namespace gameSDK
{
    public class AnimatorControllerParameterValueStore
    {
        public int nameHash;
        public AnimatorControllerParameterType type;
        public float valueFloat;
        public int valueInt;
        public bool valueBool;

        public static List<AnimatorControllerParameterValueStore> GetValue(Animator animator)
        {
            List<AnimatorControllerParameterValueStore> result = new List<AnimatorControllerParameterValueStore>();
            AnimatorControllerParameter[] parameters = animator.parameters;

            foreach (AnimatorControllerParameter parameter in parameters)
            {
                AnimatorControllerParameterValueStore store = new AnimatorControllerParameterValueStore();
                store.nameHash = parameter.nameHash;
                store.type = parameter.type;

                switch (store.type)
                {
                    case AnimatorControllerParameterType.Bool:
                        store.valueBool = animator.GetBool(store.nameHash);
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        //animator.ResetTrigger(store.nameHash);
                        break;
                    case AnimatorControllerParameterType.Float:
                        store.valueFloat = animator.GetFloat(store.nameHash);
                        break;
                    case AnimatorControllerParameterType.Int:
                        store.valueFloat = animator.GetInteger(store.nameHash);
                        break;
                }

                result.Add(store);
            }
            return result;
        }

        public static void SetValue(List<AnimatorControllerParameterValueStore> parameters, Animator animator)
        {
            foreach (AnimatorControllerParameterValueStore parameter in parameters)
            {
                switch (parameter.type)
                {
                    case AnimatorControllerParameterType.Bool:
                        animator.SetBool(parameter.nameHash, parameter.valueBool);
                        break;
                    case AnimatorControllerParameterType.Int:
                        animator.SetInteger(parameter.nameHash, parameter.valueInt);
                        break;

                    case AnimatorControllerParameterType.Float:
                        animator.SetFloat(parameter.nameHash, parameter.valueFloat);
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        //animator.ResetTrigger(parameter.nameHash);
                        break;
                }
            }
        }
    }
}