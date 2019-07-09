# Custom Perspective

## シーン構築

### オブジェクト作成
Vanishing Point, Point Of View, Focus のうち必要なものを作成。

### カメラ用スクリプト
対象のカメラに CustomPerspectiveCamera をアタッチ。

### モデル用スクリプト
対象モデルのルートに CustomPerspectiveModelをアタッチし、Vanishing Point, Point Of View, Focus のうち必要なものを設定。

対象モデルの各メッシュに CustomPerspectiveMesh をアタッチし、CustomPerspectiveModel を設定。（※廃止予定）

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