namespace ConnectionWatcher.App.Localization;

public sealed record StartupPresentation(
    string Tagline,
    IReadOnlyList<string> Messages);

public static class StartupText
{
    public static StartupPresentation Get(string language) => language switch
    {
        "zh-CN" => new StartupPresentation(
            "看见真正重要的连接。",
            [
                "正在加载你的监视规则……",
                "只观察你选择关注的目标。",
                "日志始终保存在这台电脑上。",
                "SocketSight负责观察，不负责瞎猜。",
                "正在擦亮网络放大镜……",
                "本次启动没有伤害任何数据包。"
            ]),
        "zh-TW" => new StartupPresentation(
            "看見真正重要的連線。",
            [
                "正在載入你的監視規則……",
                "只觀察你選擇關注的目標。",
                "記錄始終保存在這台電腦上。",
                "SocketSight 負責觀察，不負責亂猜。",
                "正在擦亮網路放大鏡……",
                "本次啟動沒有傷害任何資料封包。"
            ]),
        "es" => new StartupPresentation(
            "Ve las conexiones que importan.",
            [
                "Cargando tus reglas de monitoreo…",
                "Solo observamos los objetivos que elegiste.",
                "Tus registros permanecen en este equipo.",
                "SocketSight observa; no adivina.",
                "Preparando la lupa de red…",
                "Ningún paquete sufrió daños durante este inicio."
            ]),
        "fr" => new StartupPresentation(
            "Voyez les connexions qui comptent.",
            [
                "Chargement de vos règles de surveillance…",
                "Seules les cibles que vous choisissez sont observées.",
                "Vos journaux restent sur cet ordinateur.",
                "SocketSight observe sans tirer de conclusions hâtives.",
                "Préparation de la loupe réseau…",
                "Aucun paquet n’a été maltraité pendant ce démarrage."
            ]),
        "de" => new StartupPresentation(
            "Behalten Sie die wichtigen Verbindungen im Blick.",
            [
                "Ihre Überwachungsregeln werden geladen…",
                "Es werden nur die von Ihnen gewählten Ziele beobachtet.",
                "Ihre Protokolle bleiben auf diesem Computer.",
                "SocketSight beobachtet, ohne zu raten.",
                "Die Netzwerk-Lupe wird vorbereitet…",
                "Bei diesem Start kamen keine Pakete zu Schaden."
            ]),
        "pt-BR" => new StartupPresentation(
            "Veja as conexões que importam.",
            [
                "Carregando suas regras de monitoramento…",
                "Observamos apenas os alvos que você escolheu.",
                "Seus registros permanecem neste computador.",
                "O SocketSight observa; não faz suposições.",
                "Preparando a lupa da rede…",
                "Nenhum pacote foi ferido durante esta inicialização."
            ]),
        _ => new StartupPresentation(
            "See the connections that matter.",
            [
                "Loading your monitoring rules…",
                "Watching only the targets you choose.",
                "Your logs stay on this computer.",
                "SocketSight observes; it does not guess.",
                "Polishing the network magnifying glass…",
                "No packets were harmed during this startup."
            ])
    };
}
