using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;

namespace WCGL
{
    class ScreenspaceShadowMap
    {
        CommandBuffer commad;
        RenderTexture viewPosTexture;

        public ScreenspaceShadowMap(Camera camera)
        {
            commad = new CommandBuffer();
            commad.name = "CustomPerspective Meshes ViewPos";

            resetTexture(camera);
        }

        void resetTexture(Camera camera)
        {
            viewPosTexture = new RenderTexture(camera.pixelWidth, camera.scaledPixelHeight, 0, RenderTextureFormat.ARGBHalf);
            viewPosTexture.name = "CustomPerspective_ViewPos";
            viewPosTexture.filterMode = FilterMode.Point;
        }

        public Texture updateBuffer(Camera camera)
        {
            commad.Clear();

            if (viewPosTexture.width != camera.pixelWidth || viewPosTexture.height != camera.pixelHeight) resetTexture(camera);

            commad.SetRenderTarget(viewPosTexture, BuiltinRenderTextureType.Depth);
            commad.ClearRenderTarget(false, true, Color.clear);

            foreach (var cpm in CustomPerspectiveModel.GetActiveInstances())
            {
                if (cpm.correctShadow == false) continue;

                var viewPosMaterial = cpm.ViewPosMaterial;
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

            return viewPosTexture;
        }

        public void enableCommandBuffer(Camera camera)
        {
            camera.AddCommandBuffer(CameraEvent.AfterDepthTexture, commad);
        }

        public void disableCommandBuffer(Camera camera)
        {
            camera.RemoveCommandBuffer(CameraEvent.AfterDepthTexture, commad);
        }
    }
}
