using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;

namespace WCGL
{
    class ScreenspaceShadowMap
    {
        CommandBuffer command;
        RenderTexture viewPosTexture;

        public ScreenspaceShadowMap(Camera camera)
        {
            command = new CommandBuffer();
            command.name = "CustomPerspective Meshes ViewPos";

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
            command.Clear();

            if (viewPosTexture.width != camera.pixelWidth || viewPosTexture.height != camera.pixelHeight) resetTexture(camera);

            command.SetRenderTarget(viewPosTexture, BuiltinRenderTextureType.Depth);
            command.ClearRenderTarget(false, true, Color.clear);

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
                        command.DrawRenderer(mesh.Renderer, viewPosMaterial, i);
                    }
                }
            }

            return viewPosTexture;
        }

        public void enableCommandBuffer(Camera camera)
        {
            camera.AddCommandBuffer(CameraEvent.AfterDepthTexture, command);
        }

        public void disableCommandBuffer(Camera camera)
        {
            camera.RemoveCommandBuffer(CameraEvent.AfterDepthTexture, command);
        }
    }
}
