# Polar Distortion

## is何

* 画像の座標を直交座標から極座標に変換、あるいはその逆を行い、変形します。
* 画像サイズが 4096x4096 以上でも、画像全体に対して適用することが出来ます。

## ダウンロード

* Releasesからzipファイルをダウンロードしてください。

## インストール・アンインストール

* dllファイルをpluginディレクトリにコピー、または削除します。

## 使用方法

* 映像エフェクトの加工からPolar Distortionを適用します。

### パラメータ

* 変換
    * 座標変換の割合です。
* 極座標から長方形へ
    * 変換の方向を、極座標から直交座標に変換するかどうかを表します。
* 前処理/後処理用
    * 極座標の状態に対しエフェクトを適用したい場合に使用します。
    * 適用順序としては以下のようになります
        * Polar Distortion (極座標から長方形へ ON, 前処理/後処理用 ON)
        * エフェクト1
        * エフェクト2
        * ...
        * Polar Distortion (極座標から長方形へ OFF, 前処理/後処理用 ON)
    * 間のエフェクトの適用によって画像サイズが変わると、上手く元の直交座標に戻せなくなるため、サイズを維持するオプションなどがある場合は使用してください。

## 注意点等

* 大きな画像全体に適用できるようにするため、内部でビットマップを生成している関係上、エフェクトの組み合わせなどによってYMMが落ちる可能性があります。
    * YMMが落ちる場合はそのエフェクトの組み合わせやこのエフェクト自体を使用しないようにしてください。
* あまりにも大きすぎる画像(1辺が16384px以上)の場合、画像が途切れるため、これ以下に押さえてください。
    * 使用している素材自体は小さい場合でも、エフェクトの適用によってサイズが大きくなる場合があります。

## ライセンス

* MIT