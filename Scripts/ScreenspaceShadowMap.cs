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

            var shadowShader = Shader.Find("Hidden/CustomPerspective/Internal-ScreenSpaceShadows");
            shadowMaterial = new Material(shadowShader);

            light = Object.FindObjectsOfType<Light>().FirstOrDefault(l => l.type == LightType.Directional);
            if(light != null)
            {
                var textureSize = getTexelSize(light);
                shadowMaterial.SetVector("_ShadowMapTexture_TexelSize", textureSize);
            }

            resetTexture(camera);
        }

        static Vector4 getTexelSize(Light light)
        {
            int type = (int)light.shadowResolution;
            if (type == (int)LightShadowResolution.FromQualitySettings) type = (int)QualitySettings.shadowResolution;

            float size = 512 * Mathf.Pow(2, type);
            return new Vector4(1 / size, 1 / size, size, size);
        }

        void resetTexture(Camera camera)
        {
            viewPosTexture = new RenderTexture(camera.pixelWidth, camera.scaledPixelHeight, 16, RenderTextureFormat.ARGBHalf);
            viewPosTexture.name = "CustomPerspective_ViewPos";

            shadowTexture = new RenderTexture(camera.pixelWidth, camera.scaledPixelHeight, 0, RenderTextureFormat.ARGB32);
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

            foreach (var cpm in CustomPerspectiveModel.GetActiveInstances())
            {
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
