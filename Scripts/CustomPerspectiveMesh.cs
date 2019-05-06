using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace WCGL
{
    [ExecuteInEditMode]
    public class CustomPerspectiveMesh : MonoBehaviour
    {
        [System.Serializable]
        struct CullCancel
        {
            #pragma warning disable 649
            public uint materialIdx;
            public uint meshIdx;
            public uint passIdx;
            public bool transparent;
            #pragma warning restore 649
        };

        [SerializeField] CustomPerspectiveModel customPerspectiveModel = null;
        [SerializeField] List<CullCancel> cullCancel = new List<CullCancel>();

        public Renderer Renderer { get; private set; }
        MaterialCache materialChace;

        public void enableCustomMatrix(ref Matrix4x4 unityCustomProj, Camera camera,
            ref Matrix4x4 glCustomProj, ref Matrix4x4 glInvVP, ref Vector3 viewDirectionCorrectWorld,
            Texture screenSpaceShadowMap, CommandBuffer afterOpaque, CommandBuffer afterAlpha,
            bool correctShadow, bool correctRimLight)
        {
            if (Renderer == null) return;

            setCustomMatrix(ref glCustomProj, ref glInvVP, screenSpaceShadowMap, correctShadow, correctRimLight, ref viewDirectionCorrectWorld);
            cancelCull(ref unityCustomProj, camera, afterOpaque, afterAlpha);
        }

        bool isCulled(ref Matrix4x4 proj, Camera camera)
        {
            var bounds = Renderer.bounds;
            var minMax = new Vector3[] { bounds.min, bounds.max };

            var screenPosMin = new Vector2(float.MaxValue, float.MaxValue);
            var screenPosMax = new Vector2(float.MinValue, float.MinValue);

            for (int i = 0; i <= 1; i++)
            {
                for (int j = 0; j <= 1; j++)
                {
                    for (int k = 0; k <= 1; k++)
                    {
                        var worldPos = new Vector4(minMax[i].x, minMax[j].y, minMax[k].z);
                        var viewPos = camera.worldToCameraMatrix.MultiplyPoint3x4(worldPos);
                        var screenPos = proj.MultiplyPoint(viewPos);
                        screenPosMin = Vector2.Min(screenPosMin, screenPos);
                        screenPosMax = Vector2.Max(screenPosMax, screenPos);
                    }
                }
            }

            var boundRect = new Rect(screenPosMin, screenPosMax - screenPosMin);
            var screenRect = new Rect(-1, -1, 2, 2);

            return !screenRect.Overlaps(boundRect);
        }

        void cancelCull(ref Matrix4x4 unityCustomProj, Camera camera, CommandBuffer afterOpaque, CommandBuffer afterAlpha)
        {
            if (cullCancel.Count == 0) return;

            var cameraProj = camera.projectionMatrix;
            if (isCulled(ref cameraProj, camera) == true && isCulled(ref unityCustomProj, camera) == false)
            {
                var materials = Renderer.sharedMaterials;
                foreach(var cc in cullCancel)
                {
                    var command = cc.transparent == true ? afterAlpha : afterOpaque;
                    command.DrawRenderer(Renderer,materials[cc.materialIdx], (int)cc.meshIdx, (int)cc.passIdx );
                }
            }
        }

        void setCustomMatrix(ref Matrix4x4 glCustomProj, ref Matrix4x4 glInvVP, Texture screenSpaceShadowMap,
            bool correctShadow, bool correctRimLight, ref Vector3 viewDirectionCorrectWorld)
        { 
            var mateials = materialChace.GetMaterials(true);
            foreach (var material in mateials)
            {
                material.EnableKeyword("CUSTOM_PERSPECTIVE_ON");
                material.SetMatrix("CUSTOM_MATRIX_P", glCustomProj);
                material.SetMatrix("MATRIX_I_VP", glInvVP);
                if (correctShadow == true) material.SetTexture("_ShadowMapTexture", screenSpaceShadowMap);
                if (correctRimLight == true) material.SetVector("ViewDirectionCorrectWorld", viewDirectionCorrectWorld);
            }
        }

        public void disableCustomMatrix(bool correctShadow)
        {
            if (Renderer == null) return;

            var mateials = materialChace.GetMaterials(false);
            foreach (var material in mateials)
            {
                material.DisableKeyword("CUSTOM_PERSPECTIVE_ON");
                if (correctShadow == true) material.SetTexture("_ShadowMapTexture", null);
            }
        }

        void Start() { } //for GUI

        void Awake()
        {
            Renderer = GetComponent<Renderer>();
            materialChace = new MaterialCache(Renderer);
            if (customPerspectiveModel != null) customPerspectiveModel.Meshes.Add(this);
        }

        void OnDestroy()
        {
            if (customPerspectiveModel != null) customPerspectiveModel.Meshes.Remove(this);
        }
    }
}
