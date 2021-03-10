using System.Collections.Generic;
using UnityEngine;

namespace WCGL
{
    public class MaterialCache
    {
        Dictionary<Renderer, Material[]> sharedMaterialsCache = new Dictionary<Renderer, Material[]>();
        List<Material> dst = new List<Material>();

        public List<Material> GetMaterials(Renderer renderer, bool enableCustomPerspective)
        {
            if (Application.isPlaying == true)
            {
                renderer.GetMaterials(dst);
                return dst;
            }

            //Before render: cache sharedMaterils & change sharedMaterials to other instances
            //After render: restore sharedMaterials
            //Due to this implementation, solve editor mode problems memory leak and
            //impossible to change material values because of creating other material instances 
            if (enableCustomPerspective == true)
            {
                var sharedMaterials = renderer.sharedMaterials;
                var tempMaterials = new Material[sharedMaterials.Length];
                for (int i = 0; i < tempMaterials.Length; i++)
                {
                    if (sharedMaterials[i] != null) tempMaterials[i] = new Material(sharedMaterials[i]);
                }

                renderer.materials = tempMaterials;
                sharedMaterialsCache[renderer] = sharedMaterials;

                dst.Clear();
                dst.AddRange(tempMaterials);
            }
            else
            {
                renderer.sharedMaterials = sharedMaterialsCache[renderer];
                renderer.GetSharedMaterials(dst);
            }

            return dst;
        }
    }
}