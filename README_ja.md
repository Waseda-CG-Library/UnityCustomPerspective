# Custom Perspective マニュアル

---
## シーン構築

### オブジェクト作成
Vanishing Point, Point Of View, Focus のうち必要なものを作成。

### カメラ用スクリプト
対象のカメラに CustomPerspectiveCamera をアタッチ。

### モデル用スクリプト
対象モデルのルートに CustomPerspectiveModelをアタッチし、Vanishing Point, Point Of View, Focus のうち必要なものを設定。

対象モデルの各メッシュに CustomPerspectiveMesh をアタッチし、CustomPerspectiveModel を設定。（※廃止予定）

---
## シェーダ書き換え

### CustomPerspective.cginc を include
`#include "UnityCG.cginc"` の後ろあたりに追加。
```
#pragma multi_compile _ CUSTOM_PERSPECTIVE_ON
#include "CustomPerspective.cginc"
```

### 座標変換を置換
* 元：```o.vertex = UnityObjectToClipPos(v.vertex);```
* 新：```o.vertex = ObjectToCustomClipPos(v.vertex);```

### Fallback を置換
* 元：```Fallback "Legacy Shaders/VertexLit"```
* 新：```Fallback "Hidden/CustomPerspective/Vertex"```

---
## パラメータ設定

### CustomPerspectiveCamera.ViewVolumeScale
画面の端付近でオブジェクトが消えたり不正な影が生じた場合に設定します。
この値により、カリングとシャドウマップ生成に用いられるビューボリュームが縦横方向に拡大されます。
理論上は僅かに負荷が増加するため、できるだけ小さな値が推奨です。
