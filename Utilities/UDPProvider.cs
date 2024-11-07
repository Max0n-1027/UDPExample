using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

/// <summary>
/// UDPサーバーとして機能し、受信したデータをリアクティブに提供するクラスです。
/// </summary>
public class UDPProvider : IDisposable
{
    private readonly int _port;
    private readonly UdpClient _udpClient;
    private readonly ISubject<string> _subject;
    private bool _disposed = false;

    /// <summary>
    /// 指定されたポートでUDPサーバーを初期化し、受信を開始します。
    /// </summary>
    /// <param name="port">受信するポート番号。</param>
    public UDPProvider(int port)
    {
        _port = port;
        _udpClient = new UdpClient(_port);
        _subject = new Subject<string>();

        // 受信を開始
        StartReceiving();
    }

    /// <summary>
    /// 受信したデータをリアクティブに通知するObservableプロパティ。
    /// </summary>
    public IObservable<string> DataReceived => _subject.AsObservable();

    /// <summary>
    /// 非同期でデータの受信を開始し、データが受信されるたびに購読者へ通知します。
    /// </summary>
    private void StartReceiving()
    {
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, _port);

        Observable.FromAsync(() => _udpClient.ReceiveAsync())
            .Repeat()
            .Subscribe(
                result =>
                {
                    string receivedData = Encoding.UTF8.GetString(result.Buffer);
                    _subject.OnNext(receivedData); // データが到着したら通知
                },
                ex =>
                {
                    Console.WriteLine($"エラー: {ex.Message}");
                    _subject.OnError(ex);
                },
                () => _subject.OnCompleted()
            );
    }

    /// <summary>
    /// UDPサーバーを停止し、リソースを解放します。
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// ファイナライザ。ガベージコレクション時に未解放のリソースを解放します。
    /// </summary>
    ~UDPProvider()
    {
        Dispose(false);
    }

    /// <summary>
    /// リソースを解放します。
    /// </summary>
    /// <param name="disposing">
    /// マネージドリソースを解放する場合はtrue。アンマネージドリソースのみ解放する場合はfalse。
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // マネージドリソースの解放
            _udpClient?.Close();
            _subject?.OnCompleted();
        }

        // アンマネージドリソースの解放がある場合はここで行う

        _disposed = true;
    }
}
