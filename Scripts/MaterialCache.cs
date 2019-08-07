using System.Collections.Generic;
using UnityEngine;

namespace WCGL
{
    public class MaterialCache
    {
        Dictionary<Renderer, Material[]> sharedMaterialsCache = new Dictionary<Renderer, Material[]>();

        public Material[] GetMaterials(Renderer renderer, bool enableCustomPerspective)
        {
            if (Application.isPlaying == true) return renderer.materials;

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
                    tempMaterials[i] = new Material(sharedMaterials[i]);
                }

                renderer.materials = tempMaterials;
                this.sharedMaterialsCache[renderer] = sharedMaterials;
                return tempMaterials;
            }
            else
            {
                renderer.sharedMaterials = this.sharedMaterialsCache[renderer];
                return renderer.sharedMaterials;
            }
        }
    }
}