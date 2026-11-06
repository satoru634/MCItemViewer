# MCItemViewer

✨ [English](doc/README_english.md)

Minecraftを起動せずにアイテムを閲覧できるツールです。

表示したアイテムを画像として出力することもできます。

![screenshot](./doc/screenshot.png)

## 🌟 Features

- 指定されたMinecraftのバージョンにあるアイテムを表示します。
- 表示されたアイテムを画像(PNG形式)に出力できます。
- 日本語/英語の表示言語切替（設定ページ）。アイテム名も選択した言語で表示されます。

## 🖥️ System requirements

- Windows 10 64bit
- Windows 11 64bit

## ⭕ Supported Minecraft versions

Minecraft Java Edition 1.13以降

## ☑️ Operation confirmed versions

- 1.13
- 1.20.1
- 1.21.3
- 1.21.4
- 1.21.5

## 🧩 Usage

1. Minecraftゲームディレクトリとバージョンを設定します。

   ![usage01](./doc/usage01.png)

2. アイテムID or アイテム名を入力します。

   ※1 サジェスト機能付きです。

   ※2 現在のバージョンでは、名前空間は「minecraft」固定です。

   ![usage02](./doc/usage02.png)

3. 右側に入力したアイテムが表示されます。

   ※アイテムID or アイテム名を入力しても、実際のアイテムが表示されない場合もあります。

4. 表示中のアイテムを画像化する場合は、出力サイズ(ピクセル)を選択し、右側にある「PNG出力」ボタンを押してください。

   「PNG画像出力先フォルダ」で指定した場所に画像が出力されます。

   ![usage03](./doc/usage03.png)

## 😔 Not supported (Will be supported in the future)

- チェストやベッドなどのブロックエンティティは表示されません。
- 1.21.4以降では、草などのアイテムにバイオームの色が反映されていない場合があります。
- MODで追加されるアイテムは未対応です。
