using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;

namespace WCGL
{
    class ScreenspaceShadowMap
    {
        CommandBuffer commad;

        RenderTexture viewPosTexture;
        Material viewPosMaterial;

        RenderTexture shadowTexture;
        Material shadowMaterial;

        Light light;

        public ScreenspaceShadowMap(Camera camera)
        {
            commad = new CommandBuffer();
            commad.name = "CustomPerspective Meshes ViewPos";

            var viewPosShader = Shader.Find("Hidden/CustomPerspective/ViewPos");
            viewPosMaterial = new Material(viewPosShader);
            viewPosMaterial.name = "CustomPerspective ViewPos";

            var shadowShader = Shader.Find("Hidden/CustomPerspective/Internal-ScreenSpaceShadows");
            shadowMaterial = new Material(shadowShader);
            shadowShader.name = "CustomPerspective Screenspace ShadowMap";

            light = Object.FindObjectOfType<Light>();

            resetTexture(camera);
        }

        void resetTexture(Camera camera)
        {
            var desc = new RenderTextureDescriptor(camera.pixelWidth, camera.scaledPixelHeight, RenderTextureFormat.ARGBHalf, 16);
            viewPosTexture = new RenderTexture(desc);
            viewPosTexture.name = "CustomPerspective_ViewPos";

            desc.colorFormat = RenderTextureFormat.ARGB32;
            shadowTexture = new RenderTexture(desc);
            shadowTexture.name = "CustomPerspective_ScreenspaceShadowMap";

            shadowMaterial.SetTexture("ViewPosTexture", viewPosTexture);
        }

        public Texture updateBuffer(Camera camera)
        {
            commad.Clear();

            if (light == null || light.isActiveAndEnabled == false || light.shadows == LightShadows.None) return null;
            int pass = light.shadows == LightShadows.Hard ? 0 : 1;

            if (viewPosTexture.width != camera.pixelWidth || viewPosTexture.height != camera.pixelHeight) resetTexture(camera);

            commad.SetRenderTarget(viewPosTexture);
            commad.ClearRenderTarget(true, true, Color.clear);

            foreach (var cpm in CustomPerspectiveModel.GetInstances().Distinct())
            {
                if (cpm == null || cpm.isActiveAndEnabled == false) continue;

                viewPosMaterial.SetMatrix("CUSTOM_MATRIX_P", cpm.CustomMatrix);
                foreach (var mesh in cpm.Meshes)
                {
                    if (mesh.isActiveAndEnabled == false) continue;

                    int count = mesh.Renderer.sharedMaterials.Count();
                    for (int i = 0; i < count; i++)
                    {
                        commad.DrawRenderer(mesh.Renderer, viewPosMaterial, i);
                    }
                }
            }

            commad.Blit(null, shadowTexture, shadowMaterial, pass);

            return shadowTexture;
        }

        public void enableCommandBuffer(Camera camera)
        {
            camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, commad);
        }

        public void disableCommandBuffer(Camera camera)
        {
            camera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, commad);
        }
    }
}
