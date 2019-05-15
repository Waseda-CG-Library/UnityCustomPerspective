using System.Collections.Generic;
using UnityEngine;

namespace WCGL
{
    public class MaterialCache
    {
        Material[] sharedMaterialsCache = null;
        Renderer renderer;

        public MaterialCache(Renderer _renderer)
        {
            this.renderer = _renderer;
        }

        public Material[] GetMaterials(bool enableCustomPerspective)
        {
            if (Application.isPlaying == true) return renderer.materials;

            //エディタモードではsharedMaterialsを使わないと何故かメモリリークする
            //http://answers.unity3d.com/questions/283271/material-leak-in-editor.html

            //描画前：sharedMaterialを別インスタンスに変更
            //描画後：sharedMaterialを元に戻す
            //この実装により、マテリアルのメモリリーク問題と
            //マテリアルが別インスタンス化して内容を変更できない問題を同時に解決できる。
            if (enableCustomPerspective == true)
            {
                var sharedMaterials = renderer.sharedMaterials;
                var tempMaterials = new Material[sharedMaterials.Length];
                for (int i = 0; i < tempMaterials.Length; i++)
                {
                    tempMaterials[i] = new Material(sharedMaterials[i]);
                }
                renderer.materials = tempMaterials;
                this.sharedMaterialsCache = sharedMaterials;
                return tempMaterials;
            }
            else
            {
                renderer.sharedMaterials = this.sharedMaterialsCache;
                this.sharedMaterialsCache = null;
                return renderer.sharedMaterials;
            }
        }
    }
}