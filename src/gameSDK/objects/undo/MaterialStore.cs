using System.Collections.Generic;
using UnityEngine;

namespace gameSDK
{
    public class MaterialStore
    {
        protected List<Renderer> renderers=new List<Renderer>(); 
        protected List<Material[]> materials=new List<Material[]>();
        public static List<MaterialStore> GetValue(params GameObject[] gos)
        {
            List<MaterialStore> result=new List<MaterialStore>();
            foreach (GameObject go in gos)
            {
                MaterialStore store = new MaterialStore();
                store.get(go);
                result.Add(store);
            }
            return result;
        }

        public Renderer[] get(GameObject go,bool resetOld=true)
        {
            undo();
            SkinnedMeshRenderer[] skinnedMeshRenderers= go.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
            {
                renderers.Add(skinnedMeshRenderer);
                materials.Add(skinnedMeshRenderer.materials);
            }
            MeshRenderer[] meshRenderers = go.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                renderers.Add(meshRenderer);
                materials.Add(meshRenderer.materials);
            }

            return skinnedMeshRenderers;
        }

        public static void SetValue(List<MaterialStore> list)
        {
            foreach (MaterialStore materialStore in list)
            {
                materialStore.undo();
            }
        }

        public virtual void undo()
        {
            int len = renderers.Count;
            for (int i = 0; i < len; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer != null)
                {
                    renderer.materials = materials[i];
                }
            }
            renderers.Clear();
            materials.Clear();
        }
    }


}