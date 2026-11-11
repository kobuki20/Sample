using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;
    public string serverIP = ""; // サーバーのIPアドレス
    public int port = 50000; // サーバーのポート番号

    private TcpClient client;
    private NetworkStream stream;
    private bool isConnected = false;

    private CancellationTokenSource tokenSource;

    private TouchScreenKeyboard touchScreenKeyboard;

    public TMP_Text receivedText;
    private string receivedMessage = "";
    private bool hasNewMessage = false;

    // 画像送信用のポート
    public int imagePort = 50001;

    // シングルトン
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    private async void Start()
    {
        // CancellationTokenSourceの生成
        tokenSource = new CancellationTokenSource();
        var token = tokenSource.Token;

        ConnectToServer(token);
        await UniTask.WaitForSeconds(3);
        SendMessageToServer("Test"); // 3秒後にサーバー側にメッセージ送る
    }

    void Update()
    {
        if (hasNewMessage)
        {
            receivedText.text = receivedMessage;
            hasNewMessage = false;
        }
    }

    void OnDestroy()
    {
        DisconnectFromServer();
    }

    // 接続の確立
    public async void ConnectToServer(CancellationToken token = default)
    {
        try
        {
            client = new TcpClient();
            await client.ConnectAsync(serverIP, port);
            stream = client.GetStream();
            isConnected = true;

            // 非同期で受信するスレッドを開始
            await UniTask.RunOnThreadPool(ReceiveData, cancellationToken: token);

            Debug.Log("サーバーに接続しました");
        }
        catch (Exception e)
        {
            Debug.LogError("サーバーへの接続に失敗しました:　" + e.Message);
        }
    }

    // サーバーサイドへメッセージの送信
    public void SendMessageToServer(string message)
    {
        if (isConnected && stream != null)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                Debug.Log("メッセージを送信しました:　" + message);
            }
            catch (Exception e)
            {
                Debug.LogError("メッセージ送信に失敗しました:　" + e.Message);
            }
        }
    }

    // サーバーシドからのメッセージの受信（非同期）
    private void ReceiveData()
    {
        while (isConnected)
        {
            try
            {
                if (stream != null && stream.DataAvailable)
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Debug.Log("サーバーからのメッセージ:　" + message);
                        receivedMessage = message;
                        hasNewMessage = true;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("メッセージ受信に失敗しました:　" + e.Message);
                isConnected = false;
            }
        }
    }

    // 接続の切断
    public void DisconnectFromServer()
    {
        isConnected = false;

        if (stream != null)
        {
            tokenSource.Cancel();
            stream.Close();
            stream = null;
        }

        if (client != null)
        {
            tokenSource.Cancel();
            client.Close();
            client = null;
        }

        Debug.Log("サーバーとの接続を切断しました。");
    }

    // キーボードを開く
    public void OpenKeyboard()
    {
        Debug.Log("OpenKeyboard Called");

        touchScreenKeyboard = TouchScreenKeyboard.Open(
            "",
            TouchScreenKeyboardType.Default,
            false,
            false,
            false,
            false
        );

        Debug.Log("TouchScreenKeyboard.Open called");
    }

    // Ubuntuへの送信
    public void SendKeyboardText()
    {
        Debug.Log("SendKeyboardText called");

        if (touchScreenKeyboard == null)
        {
            Debug.LogWarning("キーボードが開かれていません");
            return;
        }

        SendMessageToServer(touchScreenKeyboard.text);
    }

    // JPEG画像をUbuntuに送信
    public async void SendImageToServer(byte[] imageData)
    {
        if (imageData == null || imageData.Length == 0)
        {
            Debug.LogError("送信する画像データがありません");
            return;
        }
        Debug.Log("画像送信開始: " + imageData.Length + " bytes");

        try
        {
            await Task.Run(async () =>
            {
                using (TcpClient imageClient = new TcpClient())
                {
                    // 画像送信用ポートに接続
                    await imageClient.ConnectAsync(serverIP, imagePort);

                    using (NetworkStream imageStream = imageClient.GetStream())
                    {
                        // 画像サイズを4バイトで表現
                        byte[] sizeData = BitConverter.GetBytes(imageData.Length);

                        // ネットワークバイトオーダーに変換
                        if (BitConverter.IsLittleEndian)
                        {
                            Array.Reverse(sizeData);
                        }

                        // 画像サイズを送信
                        await imageStream.WriteAsync(sizeData, 0, sizeData.Length);

                        // 画像本体を送信
                        await imageStream.WriteAsync(imageData, 0, imageData.Length);
                        await imageStream.FlushAsync();
                    }
                }
            });
            Debug.Log("画像送信完了");
        }
        catch (Exception e)
        {
            Debug.LogError("画像送信失敗: " + e.Message);
        }
    }
}
