// Chutoki Free - traductions (italien, espagnol, chinois simplifié, japonais)
// Copyright (c) 2026 Radjouh Slim - Licence MIT
// Le français et l'anglais sont écrits directement dans le code (App.T("français", "english")).
// Ici, chaque texte anglais est traduit dans les autres langues : { anglais, italien, espagnol, chinois, japonais }.
using System.Collections.Generic;

namespace Chutoki {

static class Story {
    public const string Fr =
        "J'ai créé Chutoki parce qu'aucune application ne rendait vraiment mon PC silencieux : le bruit des ventilateurs réveillait mon fils. " +
        "Alors je l'ai fait moi-même, avec l'aide de l'intelligence artificielle.\n\n" +
        "Si moi j'ai pu le faire, toi aussi : lance-toi et crée tes propres outils !   — Slim";
    public const string En =
        "I made Chutoki because no app could make my PC truly quiet: the fan noise kept waking up my son. " +
        "So I built it myself, with the help of artificial intelligence.\n\n" +
        "If I could do it, so can you: go for it and build your own tools!   — Slim";
}

static class Tr {
    static readonly Dictionary<string, string[]> d = new Dictionary<string, string[]>();
    static readonly string[] order = { "it", "es", "zh", "ja" };

    public static string Get(string en, string lang) {
        int i = System.Array.IndexOf(order, lang); if (i < 0) return null;
        string[] t; return d.TryGetValue(en, out t) ? t[i] : null;
    }
    static void A(string en, string it, string es, string zh, string ja) { d[en] = new[] { it, es, zh, ja }; }

    static Tr() {
        // --- modes
        A("Quiet", "Calmo", "Calma", "安静", "静か");
        A("Normal", "Normale", "Normal", "标准", "標準");
        A("Strong", "Forte", "Fuerte", "强力", "強力");
        A("Auto", "Auto", "Auto", "自动", "自動");
        A("I handle everything", "ci penso io", "yo me encargo de todo", "全部交给我", "おまかせ");
        A("shhh, baby's asleep", "shhh, il bimbo dorme", "chis, el bebé duerme", "嘘，宝宝在睡觉", "しーっ、赤ちゃんが寝てるよ");
        A("good balance", "buon equilibrio", "buen equilibrio", "均衡", "バランス良好");
        A("maximum cooling", "raffreddamento massimo", "máxima refrigeración", "最大散热", "最大冷却");
        A("(boosted)", "(rinforzato)", "(reforzado)", "（已增强）", "（強化中）");
        A("safeguard on", "protezione attiva", "protección activa", "保护已启动", "保護機能オン");

        // --- alertes
        A("Fan failure?", "Ventola guasta?", "¿Ventilador averiado?", "风扇故障？", "ファンの故障？");
        A("{0} stopped spinning. Check its cable or the fan.", "{0} non gira più. Controlla il cavo o la ventola.", "{0} ha dejado de girar. Revisa su cable o el ventilador.", "{0} 停止转动。请检查线缆或风扇。", "{0} が回転していません。ケーブルかファンを確認してください。");
        A("Quiet mode is boosting", "La modalità Calmo si rinforza", "El modo Calma se refuerza", "安静模式正在增强", "静かモードを強化します");
        A("Your PC has been warm for 2 minutes: switching to Normal until it cools down.", "Il PC è caldo da 2 minuti: passo a Normale finché non si raffredda.", "Tu PC lleva 2 minutos caliente: paso a Normal hasta que se enfríe.", "电脑已经发热 2 分钟：切换到标准模式，直到温度下降。", "PCが2分間熱いままです。冷えるまで標準モードにします。");
        A("Too hot!", "Troppo caldo!", "¡Demasiado calor!", "太热了！", "熱すぎます！");
        A("Fans set to full speed to cool your PC down.", "Ventole al massimo per raffreddare il PC.", "Ventiladores al máximo para enfriar tu PC.", "风扇全速运转为电脑降温。", "PCを冷やすためファンを全開にしました。");
        A("Sensor lost", "Sensore perso", "Sensor perdido", "传感器丢失", "センサーが見つかりません");
        A("CPU temperature can't be read: fans at full speed for safety.", "Impossibile leggere la temperatura della CPU: ventole al massimo per sicurezza.", "No se puede leer la temperatura de la CPU: ventiladores al máximo por seguridad.", "无法读取 CPU 温度：为安全起见风扇全速运转。", "CPU温度を読み取れません。安全のためファンを全開にします。");
        A("CPU very hot", "CPU molto calda", "CPU muy caliente", "CPU 非常热", "CPUがとても熱い");
        A("Try Strong mode, or check the CPU cooler.", "Prova la modalità Forte o controlla il dissipatore della CPU.", "Prueba el modo Fuerte o revisa el disipador de la CPU.", "试试强力模式，或检查 CPU 散热器。", "強力モードを試すか、CPUクーラーを確認してください。");
        A("Check your cooling", "Controlla il raffreddamento", "Revisa la refrigeración", "请检查散热", "冷却を確認してください");
        A("The CPU is hot while almost idle. Dust or a loose cooler?", "La CPU è calda anche quasi a riposo. Polvere o dissipatore allentato?", "La CPU está caliente casi sin hacer nada. ¿Polvo o un disipador suelto?", "CPU 几乎空闲却很热。是灰尘还是散热器松动？", "ほぼ待機中なのにCPUが熱いです。ホコリかクーラーのゆるみ？");

        // --- rapport / graphique
        A("Start;Duration (min);CPU avg (°C);CPU max (°C);GPU max (°C);Fans max (%);Quiet (min);Normal (min);Strong (min);Full-speed events;Alerts",
          "Inizio;Durata (min);CPU media (°C);CPU max (°C);GPU max (°C);Ventole max (%);Calmo (min);Normale (min);Forte (min);Volte al massimo;Avvisi",
          "Inicio;Duración (min);CPU media (°C);CPU máx (°C);GPU máx (°C);Ventiladores máx (%);Calma (min);Normal (min);Fuerte (min);Veces al máximo;Alertas",
          "开始;时长 (分钟);CPU 平均 (°C);CPU 最高 (°C);GPU 最高 (°C);风扇最高 (%);安静 (分钟);标准 (分钟);强力 (分钟);全速次数;警报",
          "開始;時間 (分);CPU平均 (°C);CPU最高 (°C);GPU最高 (°C);ファン最大 (%);静か (分);標準 (分);強力 (分);全開回数;警告");
        A("Last 5 minutes (°C and %)", "Ultimi 5 minuti (°C e %)", "Últimos 5 minutos (°C y %)", "最近 5 分钟（°C 和 %）", "直近5分間（°C と %）");
        A("CPU", "Processore", "Procesador", "处理器", "CPU");
        A("Graphics card", "Scheda video", "Tarjeta gráfica", "显卡", "グラフィックボード");
        A("Fans", "Ventole", "Ventiladores", "风扇", "ファン");
        A("Report", "Rapporto", "Informe", "报告", "レポート");
        A("Sessions (first row = now)", "Sessioni (prima riga = adesso)", "Sesiones (primera fila = ahora)", "会话（第一行 = 当前）", "セッション（1行目 = 現在）");
        A("Latest alerts", "Ultimi avvisi", "Últimas alertas", "最近的警报", "最近の警告");
        A("No alerts. All good.", "Nessun avviso. Tutto bene.", "Sin alertas. Todo bien.", "没有警报，一切正常。", "警告なし。順調です。");
        A("Open folder", "Apri cartella", "Abrir carpeta", "打开文件夹", "フォルダーを開く");

        // --- bienvenue
        A("Welcome!", "Benvenuto!", "¡Bienvenido!", "欢迎！", "ようこそ！");
        A("Chutoki, the fan that goes shhh: your PC gets quiet while staying nice and cool.", "Chutoki, la ventola che fa shhh: il tuo PC diventa silenzioso e resta fresco.", "Chutoki, el ventilador que hace chis: tu PC se vuelve silencioso y sigue fresco.", "Chutoki，会说“嘘”的风扇：让电脑安静又凉爽。", "Chutoki は「しーっ」と言うファン：PCを静かに、そして涼しく保ちます。");
        A("•  It adjusts your fan speeds automatically.\n•  If things heat up fast or too much, it reacts right away.\n•  When you close it, your PC goes back to its usual settings.",
          "•  Regola da solo la velocità delle ventole.\n•  Se scalda in fretta o troppo, reagisce subito.\n•  Quando lo chiudi, il PC torna alle sue impostazioni abituali.",
          "•  Ajusta solo la velocidad de los ventiladores.\n•  Si se calienta rápido o demasiado, reacciona al instante.\n•  Al cerrarlo, tu PC vuelve a sus ajustes habituales.",
          "•  自动调节风扇转速。\n•  温度上升过快或过高时立即响应。\n•  关闭后，电脑恢复原来的设置。",
          "•  ファンの速度を自動で調整します。\n•  急に、または熱くなりすぎたらすぐに反応します。\n•  終了すると、PCはいつもの設定に戻ります。");
        A("Your CPU is cooled by:", "La tua CPU è raffreddata da:", "Tu CPU se refrigera con:", "你的 CPU 散热方式：", "CPUの冷却方法：");
        A("Air (a fan on the CPU)", "Aria (una ventola sulla CPU)", "Aire (un ventilador sobre la CPU)", "风冷（CPU 上有风扇）", "空冷（CPUの上にファン）");
        A("Water cooling (with a pump)", "Raffreddamento a liquido (con pompa)", "Refrigeración líquida (con bomba)", "水冷（带水泵）", "水冷（ポンプ付き）");
        A("I don't know", "Non lo so", "No lo sé", "我不知道", "わからない");
        A("I understand: this free software is provided as is, without warranty, and I use it at my own risk.", "Ho capito: questo software gratuito è fornito così com'è, senza garanzia, e lo uso a mio rischio.", "Entiendo: este software gratuito se ofrece tal cual, sin garantía, y lo uso bajo mi responsabilidad.", "我明白：本免费软件按“原样”提供，不含任何保证，使用风险由我自行承担。", "了解しました：この無料ソフトは現状のまま無保証で提供され、自己責任で使用します。");
        A("Let's go!", "Andiamo!", "¡Vamos!", "开始吧！", "はじめよう！");

        // --- à propos
        A("About", "Informazioni", "Acerca de", "关于", "情報");
        A("Version ", "Versione ", "Versión ", "版本 ", "バージョン ");
        A(" — free", " — gratuita", " — gratuita", " — 免费", " — 無料");
        A("Made by ", "Creato da ", "Creado por ", "作者：", "作者：");
        A("Why Chutoki?", "Perché Chutoki?", "¿Por qué Chutoki?", "为什么做 Chutoki？", "Chutoki を作った理由");
        A(Story.En,
          "Ho creato Chutoki perché nessuna app rendeva davvero silenzioso il mio PC: il rumore delle ventole svegliava mio figlio. " +
          "Così l'ho fatto da solo, con l'aiuto dell'intelligenza artificiale.\n\n" +
          "Se ci sono riuscito io, puoi farlo anche tu: lanciati e crea i tuoi strumenti!   — Slim",
          "Creé Chutoki porque ninguna aplicación hacía que mi PC fuera realmente silencioso: el ruido de los ventiladores despertaba a mi hijo. " +
          "Así que lo hice yo mismo, con la ayuda de la inteligencia artificial.\n\n" +
          "Si yo pude hacerlo, tú también: ¡anímate y crea tus propias herramientas!   — Slim",
          "我做 Chutoki，是因为没有一款软件能让我的电脑真正安静：风扇的噪音总是吵醒我的儿子。" +
          "于是我在人工智能的帮助下，自己动手做了它。\n\n" +
          "我能做到，你也能：大胆去做，创造属于你自己的工具吧！   — Slim",
          "Chutoki を作ったのは、PCを本当に静かにしてくれるアプリがなかったからです。ファンの音で息子が目を覚ましてしまうのです。" +
          "そこで、AIの力を借りて自分で作りました。\n\n" +
          "私にできたのだから、あなたにもできます。自分だけのツールを作ってみましょう！   — Slim");
        A("This software is free, ad-free, and provided \"as is\", without any warranty. It changes your motherboard fan speeds: you use it at your own risk. " +
          "Safeguards make the fans react if things heat up, and your PC returns to its own settings when the app is closed.",
          "Questo software è gratuito, senza pubblicità e fornito \"così com'è\", senza alcuna garanzia. Modifica la velocità delle ventole della scheda madre: lo usi a tuo rischio. " +
          "Le protezioni fanno reagire le ventole se scalda, e il PC torna alle sue impostazioni quando l'app è chiusa.",
          "Este software es gratuito, sin anuncios y se ofrece \"tal cual\", sin ninguna garantía. Cambia la velocidad de los ventiladores de tu placa base: lo usas bajo tu responsabilidad. " +
          "Las protecciones hacen reaccionar a los ventiladores si se calienta, y tu PC vuelve a sus ajustes al cerrar la aplicación.",
          "本软件免费、无广告，按“原样”提供，不含任何保证。它会改变主板风扇的转速：使用风险由你自行承担。" +
          "温度升高时，保护机制会让风扇及时响应；关闭软件后，电脑恢复原来的设置。",
          "このソフトは無料・広告なしで、「現状のまま」無保証で提供されます。マザーボードのファン速度を変更するため、自己責任でご使用ください。" +
          "熱くなると保護機能がファンを反応させ、アプリを終了するとPCは元の設定に戻ります。");
        A("Privacy: no personal data is collected. Once a day, the app reads a small file on the Chutoki website to check for a new version or a message (can be turned off in ⚙); nothing about you is sent. Your settings and reports stay on your PC (%APPDATA%\\Chutoki).",
          "Privacy: nessun dato personale viene raccolto. Una volta al giorno l'app legge un piccolo file sul sito di Chutoki per sapere se c'è una nuova versione o un messaggio (disattivabile in ⚙); nulla su di te viene inviato. Impostazioni e rapporti restano sul tuo PC (%APPDATA%\\Chutoki).",
          "Privacidad: no se recoge ningún dato personal. Una vez al día, la aplicación lee un pequeño archivo en la web de Chutoki para saber si hay una nueva versión o un mensaje (se puede desactivar en ⚙); no se envía nada sobre ti. Tus ajustes e informes se quedan en tu PC (%APPDATA%\\Chutoki).",
          "隐私：不收集任何个人数据。软件每天读取一次 Chutoki 网站上的一个小文件，以检查是否有新版本或消息（可在 ⚙ 中关闭）；不会发送任何关于你的信息。你的设置和报告保存在你的电脑上（%APPDATA%\\Chutoki）。",
          "プライバシー：個人データは一切収集しません。1日1回、新しいバージョンやお知らせがあるか Chutoki のサイト上の小さなファイルを読み込みます（⚙ でオフにできます）。あなたに関する情報は何も送信されません。設定とレポートはPC内（%APPDATA%\\Chutoki）に保存されます。");
        A("Components: LibreHardwareMonitor (MPL-2.0), PawnIO driver (namazso), HidSharp (Apache-2.0), .NET libraries (MIT) and others — see \"Third-party\".",
          "Componenti: LibreHardwareMonitor (MPL-2.0), driver PawnIO (namazso), HidSharp (Apache-2.0), librerie .NET (MIT) e altri — vedi \"Componenti di terzi\".",
          "Componentes: LibreHardwareMonitor (MPL-2.0), controlador PawnIO (namazso), HidSharp (Apache-2.0), bibliotecas .NET (MIT) y otros — ver \"Componentes de terceros\".",
          "组件：LibreHardwareMonitor (MPL-2.0)、PawnIO 驱动 (namazso)、HidSharp (Apache-2.0)、.NET 库 (MIT) 等 — 详见“第三方组件”。",
          "コンポーネント：LibreHardwareMonitor (MPL-2.0)、PawnIO ドライバー (namazso)、HidSharp (Apache-2.0)、.NET ライブラリ (MIT) など — 「サードパーティ」を参照。");

        // --- nouveautés
        A("New version {0} available! Click to download.", "Nuova versione {0} disponibile! Clicca per scaricarla.", "¡Nueva versión {0} disponible! Haz clic para descargarla.", "新版本 {0} 已发布！点击下载。", "新しいバージョン {0} があります！クリックしてダウンロード。");
        A("News from Chutoki", "Novità da Chutoki", "Novedades de Chutoki", "来自 Chutoki 的消息", "Chutoki からのお知らせ");
        A("Hide", "Nascondi", "Ocultar", "隐藏", "非表示");
        A("Check for news once a day", "Cerca novità una volta al giorno", "Buscar novedades una vez al día", "每天检查一次更新", "1日1回お知らせを確認");
        A("License", "Licenza", "Licencia", "许可证", "ライセンス");
        A("Third-party", "Componenti di terzi", "Componentes de terceros", "第三方组件", "サードパーティ");

        // --- réglages avancés
        A("Advanced settings", "Impostazioni avanzate", "Ajustes avanzados", "高级设置", "詳細設定");
        A("Don't touch (BIOS)", "Non toccare (BIOS)", "No tocar (BIOS)", "不调整 (BIOS)", "触らない (BIOS)");
        A("CPU + graphics card", "Processore + scheda video", "Procesador + tarjeta gráfica", "处理器 + 显卡", "CPU + グラフィックボード");
        A("Header", "Connettore", "Conector", "接口", "コネクター");
        A("Speed (RPM)", "Velocità (giri/min)", "Velocidad (RPM)", "转速 (RPM)", "回転数 (RPM)");
        A("Power", "Potenza", "Potencia", "功率", "出力");
        A("Follows the temperature of", "Segue la temperatura di", "Sigue la temperatura de", "跟随温度", "連動する温度");
        A("Windows balanced plan", "Piano Windows bilanciato", "Plan equilibrado de Windows", "Windows 平衡计划", "Windows バランス プラン");
        A("Balanced power plan enabled: your CPU will run cooler when idle.", "Piano bilanciato attivato: la CPU scalderà meno a riposo.", "Plan equilibrado activado: tu CPU se calentará menos en reposo.", "已启用平衡电源计划：CPU 空闲时温度更低。", "バランス プランを有効にしました：待機中のCPU温度が下がります。");
        A("Could not change the power plan.", "Impossibile cambiare il piano di alimentazione.", "No se pudo cambiar el plan de energía.", "无法更改电源计划。", "電源プランを変更できませんでした。");
        A("Closing keeps the app near the clock", "La X tiene l'app vicino all'orologio", "La X deja la app junto al reloj", "关闭时保留在时钟旁", "×で閉じても時計の横に常駐");
        A("Close", "Chiudi", "Cerrar", "关闭", "閉じる");
        A("The language will change the next time Chutoki starts.", "La lingua cambierà al prossimo avvio di Chutoki.", "El idioma cambiará la próxima vez que se inicie Chutoki.", "语言将在下次启动 Chutoki 时更改。", "言語は次回 Chutoki の起動時に変わります。");
        A("Warning: if a water-cooling pump is connected to \"{0}\", slowing it down can overheat your CPU.\n\nIs it really a fan?",
          "Attenzione: se una pompa del raffreddamento a liquido è collegata a \"{0}\", rallentarla può surriscaldare la CPU.\n\nÈ davvero una ventola?",
          "Atención: si una bomba de refrigeración líquida está conectada a \"{0}\", ralentizarla puede sobrecalentar tu CPU.\n\n¿Seguro que es un ventilador?",
          "注意：如果“{0}”上连接的是水冷泵，降低转速可能导致 CPU 过热。\n\n确定这是风扇吗？",
          "注意：「{0}」に水冷ポンプが接続されている場合、速度を下げるとCPUが過熱するおそれがあります。\n\n本当にファンですか？");
        A("CPU: ", "Processore: ", "Procesador: ", "处理器：", "CPU：");
        A("Graphics: ", "Scheda video: ", "Tarjeta gráfica: ", "显卡：", "グラフィック：");
        A("Motherboard: ", "Scheda madre: ", "Placa base: ", "主板：", "マザーボード：");
        A("Mode: ", "Modalità: ", "Modo: ", "模式：", "モード：");
        A("Resume control", "Riprendi il controllo", "Retomar el control", "恢复控制", "制御を再開");
        A("Pause (back to BIOS)", "Pausa (torna al BIOS)", "Pausa (volver a la BIOS)", "暂停（交还 BIOS）", "一時停止（BIOSに戻す）");
        A("stopped / none", "ferma / assente", "parado / ausente", "停止 / 无", "停止 / なし");

        // --- fenêtre principale
        A("Start with Windows", "Avvia con Windows", "Iniciar con Windows", "随 Windows 启动", "Windows と一緒に起動");
        A("Could not change autostart.", "Impossibile modificare l'avvio automatico.", "No se pudo cambiar el inicio automático.", "无法更改开机启动。", "自動起動を変更できませんでした。");
        A("Open", "Apri", "Abrir", "打开", "開く");
        A("Quit", "Esci", "Salir", "退出", "終了");
        A("To control fans, Chutoki needs a small free and safe component: PawnIO.\n\nOpen its official download page? (install \"PawnIO_setup.exe\", then restart Chutoki)",
          "Per regolare le ventole, Chutoki ha bisogno di un piccolo componente gratuito e sicuro: PawnIO.\n\nAprire la sua pagina ufficiale di download? (installa \"PawnIO_setup.exe\", poi riavvia Chutoki)",
          "Para controlar los ventiladores, Chutoki necesita un pequeño componente gratuito y seguro: PawnIO.\n\n¿Abrir su página oficial de descarga? (instala \"PawnIO_setup.exe\" y vuelve a abrir Chutoki)",
          "要控制风扇，Chutoki 需要一个免费且安全的小组件：PawnIO。\n\n打开它的官方下载页面吗？（安装“PawnIO_setup.exe”，然后重新启动 Chutoki）",
          "ファンを制御するには、無料で安全な小さなコンポーネント PawnIO が必要です。\n\n公式ダウンロードページを開きますか？（「PawnIO_setup.exe」をインストールし、Chutoki を再起動してください）");
        A("Sensors unavailable", "Sensori non disponibili", "Sensores no disponibles", "传感器不可用", "センサーを利用できません");
        A("Too hot! Fans at full speed", "Troppo caldo! Ventole al massimo", "¡Demasiado calor! Ventiladores al máximo", "太热了！风扇全速", "熱すぎ！ファン全開");
        A("Paused", "In pausa", "En pausa", "已暂停", "一時停止中");
        A("All good", "Tutto bene", "Todo bien", "一切正常", "順調です");
        A("Getting warm", "Si scalda un po'", "Se calienta un poco", "有点热了", "少し熱くなってきた");
        A("Very hot!", "Molto caldo!", "¡Mucho calor!", "非常热！", "とても熱い！");
        A("Temperatures unknown", "Temperature sconosciute", "Temperaturas desconocidas", "温度未知", "温度不明");
        A("CPU ", "Processore ", "Procesador ", "处理器 ", "CPU ");
        A("Graphics ", "Scheda video ", "Gráfica ", "显卡 ", "グラフィック ");
        A("Laptop: its fans are managed by the manufacturer.\nChutoki only monitors temperatures.", "PC portatile: le ventole sono gestite dal produttore.\nChutoki controlla solo le temperature.", "Portátil: sus ventiladores los gestiona el fabricante.\nChutoki solo vigila las temperaturas.", "笔记本电脑：风扇由厂商管理。\nChutoki 仅监控温度。", "ノートPC：ファンはメーカーが管理しています。\nChutoki は温度の監視のみ行います。");
        A("PawnIO component missing: monitoring only.\nInstall it, then restart the app.", "Componente PawnIO mancante: solo monitoraggio.\nInstallalo, poi riavvia l'app.", "Falta el componente PawnIO: solo vigilancia.\nInstálalo y vuelve a abrir la app.", "缺少 PawnIO 组件：仅监控。\n请安装后重新启动软件。", "PawnIO がありません：監視のみ。\nインストールしてアプリを再起動してください。");
        A("Run the app as administrator.", "Avvia l'app come amministratore.", "Ejecuta la app como administrador.", "请以管理员身份运行本软件。", "管理者としてアプリを実行してください。");
        A("Your motherboard isn't supported yet.\nChutoki only monitors temperatures.", "La tua scheda madre non è ancora supportata.\nChutoki controlla solo le temperature.", "Tu placa base aún no es compatible.\nChutoki solo vigila las temperaturas.", "你的主板暂不支持。\nChutoki 仅监控温度。", "お使いのマザーボードはまだ非対応です。\nChutoki は温度の監視のみ行います。");
        A("Your PC is managing its fans (paused).", "Il PC gestisce da solo le ventole (pausa).", "Tu PC gestiona sus ventiladores (pausa).", "电脑正在自行管理风扇（已暂停）。", "PCが自分でファンを管理しています（一時停止）。");
        A("No fan under control (see ⚙).", "Nessuna ventola controllata (vedi ⚙).", "Ningún ventilador controlado (ver ⚙).", "没有受控的风扇（见 ⚙）。", "制御中のファンはありません（⚙ を参照）。");
        A("Heating up: fans are reacting ({0} %)", "Si scalda: le ventole reagiscono ({0} %)", "Se calienta: los ventiladores reaccionan ({0} %)", "温度上升：风扇正在响应（{0} %）", "温度上昇中：ファンが反応しています（{0} %）");
        A("Fans at {0} % (Quiet boosted while cooling down)", "Ventole al {0} % (Calmo rinforzato finché si raffredda)", "Ventiladores al {0} % (Calma reforzada mientras se enfría)", "风扇 {0} %（降温期间安静模式已增强）", "ファン {0} %（冷えるまで静かモードを強化）");
        A("Fans are running at {0} %", "Le ventole girano al {0} %", "Los ventiladores giran al {0} %", "风扇转速 {0} %", "ファンは {0} % で回転中");
        A("Still running near the clock. Right-click > Quit to stop me.", "Resto attivo vicino all'orologio. Clic destro > Esci per fermarmi.", "Sigo activo junto al reloj. Clic derecho > Salir para detenerme.", "我仍在时钟旁运行。右键 > 退出 即可关闭。", "時計の横で動作中です。右クリック >「終了」で停止します。");
        A("Chutoki is already running (icon near the clock).", "Chutoki è già in esecuzione (icona vicino all'orologio).", "Chutoki ya se está ejecutando (icono junto al reloj).", "Chutoki 已在运行（时钟旁的图标）。", "Chutoki はすでに起動しています（時計の横のアイコン）。");
    }
}
}
