using UnityEngine;

namespace gameSDK
{
    public class GhostObject:MonoBehaviour
    {
        //持续时间
        public float duration = 2f;
        //创建新残影间隔
        public float interval = 0.1f;
        public bool onPositionChange=true;

        public Shader ghostShader;
        //网格数据
        SkinnedMeshRenderer[] skinnedMeshRenderers;
        private float lastTime = 0;
        private Vector3 lastPosition = Vector3.zero;
        void Start()
        {
            ///获取身上所有的Mesh
            skinnedMeshRenderers = this.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
        }

        void Update()
        {
            ///人物有位移才创建残影
            if (onPositionChange && lastPosition == this.transform.position)
            {
                return;
            }
            lastPosition = this.transform.position;
            if (Time.time - lastTime < interval)
            {
                ///残影间隔时间
                return;
            }
            lastTime = Time.time;
            if (skinnedMeshRenderers == null)
            {
                return;
            }
            for (int i = 0,len= skinnedMeshRenderers.Length; i < len; i++)
            {
                Mesh mesh = new Mesh();
                skinnedMeshRenderers[i].BakeMesh(mesh);

                GameObject go = new GameObject();
                go.hideFlags = HideFlags.HideAndDontSave;

                GhostItem item = go.AddComponent<GhostItem>();
                ///控制残影消失
                item.duration = duration;
                item.recycleTime = Time.time + duration;

                MeshFilter filter = go.AddComponent<MeshFilter>();
                filter.mesh = mesh;

                MeshRenderer meshRen = go.AddComponent<MeshRenderer>();
                meshRen.material = skinnedMeshRenderers[i].material;

                Shader shader = meshRen.material.shader;
                if (ghostShader == null)
                {
                    ghostShader = Shader.Find(shader.name + " Ghost");
                }
                meshRen.material.shader = ghostShader;
                go.transform.localScale = skinnedMeshRenderers[i].transform.localScale;
                go.transform.position = skinnedMeshRenderers[i].transform.position;
                go.transform.rotation = skinnedMeshRenderers[i].transform.rotation;

                item.meshRenderer = meshRen;
            }
        }
    }

    public class GhostItem:MonoBehaviour
    {
        ///持续时间
        public float duration;
        ///销毁时间
        public float recycleTime;
        public MeshRenderer meshRenderer;
        void Update()
        {
            float deltaTime = recycleTime - Time.time;
            if (deltaTime <= 0)
            {
                ///到时间就销毁
                GameObject.Destroy(this.gameObject);
            }
            else if (meshRenderer.material)
            {
                float rate = deltaTime / duration;///计算生命周期的比例
                Color clr = meshRenderer.material.color;
                clr.a *= rate;///设置透明通道
                meshRenderer.material.color = clr;
            }
        }
    }
}