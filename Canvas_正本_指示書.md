//--見本　ここから。

**Canvas（正本/指示書）**

目的：
・PrimaryCompositeViewModelの大改修
　PrimaryCompositeViewModelの簡素化とソースコードを薄くする
 （無理だけど、できるだけ）一目で全て理解できるぐらいが目標。

## 固定前提（沈んでは困るお約束）

### A. 開発環境・技術スタック（変更が少ない前提）
- UI: WPF
- アーキテクチャ: MVVM
- 言語/実行基盤: C# 8.0 / .NET（※ターゲットTFMは別途明記）
- IDE/OS: Visual Studio / Windows（現状 Win12想定）
- DB: SQL Server

### B. このプロジェクト固有の「上位ルール」（ブレると破綻しやすい）
- **禁止概念：タブ**（設計検討・実装・会話でも極力持ち込まない／代替は「テンプレート登録で VM ⇔ UC を対応」）
- **テンプレート登録で VM ⇔ UC を対応**させる（既存方針を尊重）
- DIコンテナではなく、**自作DIぽい仕組み（PropertyInjector 等）**を前提にする（既存方針を尊重）
- **JSON駆動**：子アプリ／UC／メニュー等は **JSON由来**。システム側の「JSON読み込み→注入→ウインドウオープン」フローは、原則 **ハードコーディングしない／していない**
- **登録点ではなくチェーン**：JSONは概ね **3層（Suite → Shell → Utility）** で `ChildElementIds` により辿る。末端（Utility）で `ViewModelPath` / `ControlPath` を指定し、上位（Suite/Shell）は子IDの列挙に徹する
- **Entry → 保持 → 起動パイプライン**：エントリポイントでJSONを読み込んで保持し、その後は BuildPipeline により「辞書生成→各Builderでインスタンス化→各Registryへ登録→PropertyInjectorで注入→Window生成→DataContext接続→Show」の順で起動する（PrimaryShellLifecycleController / PrimaryShellBuildPipeline）


### C. 作業の役割分担
- Canvas: 上位ルール／前提／読む対象URL／決定事項（正本）
- チャット: 試行錯誤・相談・比較検討（沈んでもOK）

読む対象（raw URL一覧）


TODO（順番）
//--見本　ここまで


##下準備のTODO

１-「何が起きるとブレるのか？」を言語化
・チャットが長くなると何が困る？
・どのタイミングで「もう一度説明」が発生する？

２-「ブレないために最低限必要なもの」を洗い出す
・キャンバスに何が書いてあれば良いか
・チャットとキャンバスの役割分担

３-「1回の作業単位（セッション）」の定義
・どこまでを1チャットでやるか
・どこで必ず区切るか

この3つを、会話しながら少しずつ固める
それがこのチャットの役割で良さそうです。

## この方式になった理由（短く・正本）
- 起点は **禁止概念：タブ** 周りの破綻。
  - 「JSONから全自動でVM/UCを生成し、（禁止概念）側で利用する」方針を試したが、
    **VM/UCの連携（インスタンス化・DataContext・テンプレート解決等）が安定せず**、
    アセンブリ内部の挙動まで追ってヘルパー実装を試しても解決しなかった。
  - さらに当初「AIができる」と言った方針が最終的に成立せず、
    **“成立する設計”へ全面的に自作寄りへ舵を切った**。
- その結果として採用したのが **JSON駆動＋チェーン参照＋ビルドパイプライン＋Registry登録**。
- 目的は **JSONから動的に構築**し、生成物を **Registry登録**して windowUniqueId 単位で管理し、Primary/Secondary（将来Tertiary）を連結できる土台を作ること。
- 代償として **OPEN/CLOSEの後始末が必須**（Registry解除・参照切り）。

## ウインドウ起動の要点（30秒で思い出す用・正本）

### 0) 目的
- 「WPFの一般的作法」ではなく、**JSON駆動＋チェーン参照＋ビルドパイプライン**でウインドウを組み立てる。
- 最優先の価値：
  - **AppGenerator（構築の土台）** と **Runtime（起動・運用）** により、
    **ウインドウ構築の土台** と、その恩恵である **ウインドウの自由度** を得る。
- 生成された各要素は **Registry登録** され、同じShellでも **複数ウインドウを同時に開ける**（windowUniqueId単位）。
- Primary／Secondary（将来はTertiary等）を **つなげる設計**の土台。
- その反面、Registry登録型なので **OPEN/CLOSE（破棄・解除）処理が必須**（リーク/参照残りを防ぐ）。
- ここが抜けると、以降の議論・改修が全部ズレる。

### 1) 構造データの読み込みと保持（Entry）
- `App.xaml.cs` で **ConfigReader.LoadAll() / InitialValueReader.LoadAll()** を呼び、
  Suite/Shell/Utility を読み込んで **ConfigStore に保持**。

### 2) どの画面を開くか（Entry決定）
- SuiteRootId → ShellEntryId を Resolver で決め、
  `PrimaryShellLifecycleController.HandleWindowLifecycle("OPEN", shellId, ...)` に渡す。

### 3) 起動パイプライン（BuildPipeline）
- `PrimaryShellBuildPipeline.Process(...)` が中心。
- まず **BitDictionaryBuilder** で Shell.ChildElementIds の順に「基礎辞書（SSOT）」を作る。
- 次に typeFlag で振り分けて Builder 群を順に実行：
  - ViewModelBuilder / UserControlBuilder / TemplateBuilder / PrimaryCompositeBuilder / NavigationList / Dashboard
- 各Builderは生成物を **各Registryへ登録**。
- その後 WindowBuilder で Window を生成 → PropertyInjector で注入 → DataContext 接続 → Show。

### 3.2) typeFlag 対応表（正本）
- `1` = ViewModel
- `2` = UserControl
- `4` = DataTemplate
- `8` = CompositeViewModel
- `16` = Navigation / Dashboard

### 3.3) Registries（正本・種類だけ）
- `ShellBitDictionaryRegistry`（SSOT）
- `ViewModelRegistry`（windowUniqueId → elementId → VM）
- `UserControlRegistry`（windowUniqueId → elementId → UC）
- `CompositeViewModelRegistry`
- `NavigationListRegistry` / `DashboardEntriesRegistry`
- `WindowRegistry`（windowUniqueId → Window）
- `PairingRegistry`（Primary-Secondary のペア）
- `RegistryCleaner`（CLOSE時の掃除）

※ベースクラス：`OneLevelRegistryBase` / `TwoLevelRegistryBase` / `ThreeLevelRegistryBase`（レベル＝キー階層）。

### 3.5) クローズの基準点（X対策）
- スタート地点：`PrimaryBaseShell.xaml.cs`
- `[X]` で閉じられても拾えるように `Closing` / `Closed` をフック。
- `Closing` では多重実行防止フラグ `IsClosingInProgress` を立てて
  `PrimaryShellLifecycleController.HandleWindowLifecycle("CLOSE", "", WindowUniqueId, null, null)` を呼ぶ。
- `Closed` では `RegistryCleaner.ClearOne(WindowUniqueId)` を実行し、
  RESTART でなければ全Registry空で `Application.Current.Shutdown()`。

### 4) 会話上の強いガード（ブレ防止）
- **禁止概念：タブ**（検討・実装・会話でも持ち込まない）
- 「登録点で一括登録」ではなく、**辞書化＋レジストリ＋チェーン**が前提。
- 「画面追加＝コード追加」ではなく、原則 **JSON追加**。

## Primary→Secondary の関係（要整備・正本）
- 目的：Primary から Secondary を開き、必要なら相互に連結（親子関係）を維持する。
- 最低限決めるべきこと：
  - 識別子：Primary/Secondary それぞれの `WindowUniqueId` と、親子リンクのキー
  - 生成責務：誰が Secondary を OPEN し、誰が CLOSE を起動するか
  - 参照の持ち方：直接参照か、Registry 経由か、イベント/メッセージ経由か
  - 後始末：CLOSE 時に「どのRegistryから何を消すか」「親子リンクをどう切るか」

## 端折りがちな重要点（正本化候補・最小）

### 1) 注入（PropertyInjector 系）
- 目的：Builderで生成した VM/UC/Composite/Window に対し、必要な依存・参照・設定値を注入する。
- 入口：`PrimaryShellBuildPipeline` → `PrimaryWindowPropertyInjector.Inject(windowUniqueId, composite, parentWindowId, parentSelectedElementId)`
- 注入の種類（最小）：
  - `composite.WindowUniqueId = windowUniqueId`
  - 初期表示：`CurrentContentViewModel`/`SideContentViewModel` を既定（Dashboard / NavigationList）に設定
  - NavigationList/Dashboard へ `SetCompositeViewModel(composite)` 注入（Registry経由＋AVM経由の二段）
  - 各AVM要素（BaseViewModel系）へ `InitializeFromSetting(windowUniqueId)` を **reflection** で呼び出し
  - Window（`IShell`）へ `WindowUniqueId` 注入（`WindowRegistry` 経由）
- メモ：引数 `parentWindowId` / `parentSelectedElementId` は現状 Inject 内で未使用（将来の親子連結用の余地）。
- 読む対象URL：
  - PrimaryWindowPropertyInjector：
    https://raw.githubusercontent.com/kikurinkei/ApplicationSuite/refs/heads/master/Runtime/Injection/PrimaryWindowPropertyInjector.cs
- SecondaryWindowPropertyInjector（差分の要点）：
  - `parentWindowId` / `parentSelectedElementId` を **必須**扱い（null/空は例外）
  - 初期表示：`composite.CurrentContentViewModel = composite.AVM["ManualView"]`
  - NavigationList：`SetItems(navItems)` + `SetCompositeViewModel(composite)`
  - Dashboard：Primary同様に Registry経由＋AVM経由で `SetCompositeViewModel(composite)`
  - 親子ペア：
    - 親PrimaryCompositeを取得し `PrimaryComposite.PairedWindowUniqueId = windowUniqueId`
    - 子ManualViewViewModelに `PairedWindowUniqueId = parentWindowId`
    - `AnchorId = parentSelectedElementId` は現状コメントアウト（スクロールフロー抑制目的）
  - `InitializeFromSetting(windowUniqueId)` を reflection 呼び出し（Primary同様）
  - Window（`IShell`）へ `WindowUniqueId` 注入（Primary同様）
  - URL：
    https://raw.githubusercontent.com/kikurinkei/ApplicationSuite/refs/heads/master/Runtime/Injection/SecondaryWindowPropertyInjector.cs

### 2) WindowAction（新規ウインドウの開閉・ルーティング）
- 目的：UC/VM側から「OPEN/CLOSE/RESTART/SHUTDOWN」等を安全に起動し、LifecycleControllerへ流す。
- 呼び出し元（最小）：`WindowActionViewModel`（ListBox選択で実行）
  - `ISelectedAware.OnSelected(windowUniqueId, elementId)` で `CommandItems` を再構築
  - `SelectedCommand` の setter で Dispatcher（switch）を実行
- 入口（最小）：`PrimaryShellLifecycleController.HandleWindowLifecycle(status, shellId, windowUniqueId, parentWindowId, parentSelectedElementId)`
  - OPEN: `("OPEN", shellId, "", null, null)`
  - CLOSE: `("CLOSE", "", windowUniqueId, null, null)`
  - RESTART: `("RESTART", shellId, requestorWindowId, null, null)`
  - SHUTDOWN: `("SHUTDOWN", "", requestorWindowId, null, null)`
  - 重要：**shellId（種別）と windowUniqueId（個体）を混ぜない**
- コマンド一覧の生成：`CommandItemBuilder.Build()`
  - 先頭固定：`SHUTDOWN AllWindows`
  - Shell構成（`ConfigStore.GetShellElements()`）から `RESTART {shellId}`
  - 実行中Window（`WindowRegistry.Instance.GetKeys()`）から `CLOSE {windowUniqueId}`
  - Shell構成から `OPEN {shellId}`
- 読む対象URL：
  - CommandItemBuilder：
    https://raw.githubusercontent.com/kikurinkei/ApplicationSuite/refs/heads/master/WindowModules/AppShared/Utilities/WindowAction/CommandItemBuilder.cs
  - WindowActionViewModel：
    https://raw.githubusercontent.com/kikurinkei/ApplicationSuite/refs/heads/master/WindowModules/AppShared/Utilities/WindowAction/WindowActionViewModel.cs
  - ISelectedAware：
    https://raw.githubusercontent.com/kikurinkei/ApplicationSuite/refs/heads/master/WindowModules/AppShared/Base/ISelectedAware.cs

### 3) ログ（DebugTextWriter/Log出力）
- 目的：運用・デバッグ時に「何が起きたか」を追えるようにする。
- 正本に最低限書く：
  - 出力先（VS出力/ファイル/DB等）
  - ログの粒度（OPEN/CLOSE/Builder/RegistryCleaner など）
- 読む対象URL：
  - （ここに貼る）

### 4) マニュアルウインドウ（Behaviors）
- 目的：説明書/マニュアルをアプリ内で表示し、操作導線（コマンド/リンク）を提供する。
- 正本に最低限書く：
  - どのShell/Utilityとして組み込まれるか（JSON上の位置）
  - Behaviorが担う責務（クリック→アクション等）
- 読む対象URL：
  - （ここに貼る）

### 5) CLOSEの最小後始末セット（宣言だけ）
- 宣言：CLOSE時は windowUniqueId 配下の生成物を、対象Registryから必ず解除し、親子リンクも切る。
- 対象Registry一覧（最小セット）：
  - （ここに貼る）

## セッション運用テンプレ（このチャットの進め方）

### 1. 今回のゴール（1行）
- 

### 2. 触る範囲（ファイル/クラス/URL）
- 

### 3. 変えない前提（B.上位ルールから抜粋）
- 

### 4. 決定事項（今回の結論）
- 

### 5. 未決（次回へ持ち越し）
- 
