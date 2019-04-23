using UnityEngine;

namespace gameSDK
{
    public class MeshStore
    {
        public Renderer render;

        public SkinnedMeshRenderer skinnedMeshRenderer;

        public Mesh mesh;
        public Transform owner;

        public Matrix4x4 matrix4X4;

        public virtual bool renderEnabled
        {
            get
            {
                if (render)
                {
                    return render.enabled;
                }
                return false;
            }
        }

        private Mesh bakeMesh;
        public virtual Mesh bake()
        {
            if (skinnedMeshRenderer == null)
            {
                return mesh;
            }
            if (bakeMesh == null)
            {
                bakeMesh = new Mesh();
            }

            skinnedMeshRenderer.BakeMesh(bakeMesh);
            return bakeMesh;
        }


        public void dispose()
        {
            if (bakeMesh != null)
            {
                UnityEngine.Object.Destroy(bakeMesh);
                bakeMesh = null;
            }
        }
    }
}