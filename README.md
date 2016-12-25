Face-Exchanger
==============

## ダウンロード
クリックするとダウンロードが開始されます。
[Download](https://github.com/0V/Face-Exchanger/releases/download/0.2.0/FaceExchanger0.2.0.zip)
  
## 概要  
指定したファイルや、カメラから取り込んだ映像の顔の部分に他の画像を重ねる C# 製 WPF アプリケーションです。  
雑コラが自動で作れます。  
画像処理には OpenCV のラッパーライブラリ [OpenCvSharp](http://schima.hatenablog.com/entry/2014/01/30/105406) を使用しています。  
OpenCV バージョンは 3.1 です。  
  

## 機能  
顔の上に指定した画像を被せる
* 「画像ファイル」を読み込んで処理  
* 「Web カメラからの画像」を処理  
* 「Web カメラからの動画」をリアルタイムで処理  
* 処理した画像の保存 

## 使い方  
* File / Camera ボタンでファイルモードとカメラモードの切り替え
* Image / Movie ボタンで画像を読み込むか映像を読み込むかを切り替え
* Set Face ボタンで重ねる画像の変更
* Start ボタンで処理の開始
* Save  ボタンで画像を保存
  
  

## 既知の問題
* カメラから動画を取得する際、停止できない  

## アップデートログ
2016/12/25 0.2.0
* OpenCV3.1に対応
* 画像のかぶせ方をPoisson Image Editingを利用する方法に変更
