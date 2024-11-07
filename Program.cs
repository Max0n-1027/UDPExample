using UDPExample.Models;

using (UDPProvider udpServer = new(5000))
{
    // データを購読
    udpServer.DataReceived.Subscribe(
        data => ReceiveDataHandler.Handle(data),
        ex => Console.WriteLine($"エラー: {ex.Message}"),
        () => Console.WriteLine("受信が完了しました")
    );

    Console.WriteLine("UDPサーバーが実行中です。Enterキーで終了します...");
    Console.ReadLine();
}

