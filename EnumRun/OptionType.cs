namespace EnumRun
{
    [Flags]
    internal enum OptionType
    {
        None = 0,                   //  オプション無し
        NoRun = 1,                  //  [n] 実行しない
        WaitForExit = 2,            //  [w] 終了待ち
        RunAsAdmin = 4,             //  [a] 管理者として実行
        DomainPCOnly = 8,           //  [m] ドメイン参加PCの場合のみ実行
        WorkgroupPCOnly = 16,       //  [k] ワークグループPCの場合のみ実行
        SystemAccountOnly = 32,     //  [s] システムアカウント(SYSTEM等)の場合のみ実行
        DomainUserOnly = 64,        //  [d] ドメインユーザーの場合のみ実行
        LocalUserOnly = 128,        //  [l] ローカルユーザーの場合のみ実行
        DGReachableOnly = 256,      //  [p] デフォルトゲートウェイとの通信可能な場合のみ実行
        TrustedOnly = 512,          //  [t] 管理者に昇格している場合のみ実行
        Output = 1024,              //  [o] 実行中の標準/エラーをファイルに出力
        BeforeWait = 2048,          //  [\dr] 実行前待機
        AfterWait = 4096,           //  [r\d] 実行後待機
    }
}
