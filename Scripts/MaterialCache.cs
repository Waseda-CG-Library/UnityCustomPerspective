using System.Collections.Generic;
using UnityEngine;

namespace WCGL
{
    public class MaterialCache
    {
        Dictionary<Renderer, Material[]> sharedMaterials = new Dictionary<Renderer, Material[]>();

        public Material[] GetMaterials(Renderer renderer, bool enableCustomPerspective)
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
                this.sharedMaterials[renderer] = sharedMaterials;
                return tempMaterials;
            }
            else
            {
                renderer.sharedMaterials = this.sharedMaterials[renderer];
                this.sharedMaterials.Remove(renderer);
                return renderer.sharedMaterials;
            }
        }
    }
}