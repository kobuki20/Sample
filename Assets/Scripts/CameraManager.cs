using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.WebCam;
using TMPro;

public class CameraManager : MonoBehaviour
{
    // 撮影したJPEGデータをほかのスクリプトへ渡すイベント
    public event Action<byte[]> PhotoCaptured;

    // カメラ関連
    private PhotoCapture photoCaptureObject = null;
    private CameraParameters cameraParameters;
    private Texture2D targetTexture;
    private Resolution cameraResolution;

    // 撮影中かどうか
    private bool cameraIsBusy = false;

    [SerializeField] private Texture2D testImage;

    // Start is called before the first frame update
    void Start()
    {
        // 撮影成功時に画像送信処理を実行する
        PhotoCaptured += OnPhotoCaptured;

#if UNITY_EDITOR
        // Unity Editorではカメラを初期化しない
        Debug.Log("Editorモード：テスト画像を使用します");
#else
        Debug.Log("Hololens実機：撮影待機中");
#endif
    }

    private void OnPhotoCaptured(byte[] imageData)
    {
        if (NetworkManager.instance == null)
        {
            Debug.LogError("NetworkManagerが見つかりません");
            return;
        }

        Debug.Log("CameraManagerからNetworkManagerへ画像を渡します");

        NetworkManager.instance.SendImageToServer(imageData);
    }

    // ボタンから呼び出す
    public void OnButtonPressedCapturePhoto()
    {
        if (cameraIsBusy)
        {
            Debug.LogWarning("現在撮影中です");
            return;
        }

#if UNITY_EDITOR
        // Unity Playモードではテスト画像を使用
        Debug.Log("現在撮影中です");
        CaptureTestImage();
#else
        // Hololens実機ではカメラ撮影
        Debug.Log("Hololens実機用の撮影処理を実行します");
        CapturePhoto();
#endif
    }

    private void CapturePhoto()
    {
        Debug.Log("撮影開始");
        cameraIsBusy = true;

        PhotoCapture.CreateAsync(false, captureObject =>
        {
            if (captureObject == null)
            {
                Debug.LogError("PhotoCapture作成失敗");
                cameraIsBusy = false;
                return;
            }

            photoCaptureObject = captureObject;
            Debug.Log("PhotoCapture作成成功");

            try
            {
                // 撮影オブジェクト作成後に解像度を取得
                Resolution[] resolutions =
                    PhotoCapture.SupportedResolutions.ToArray();

                Debug.Log(
                    "対応解像度数: " + resolutions.Length
                );

                if (resolutions.Length == 0)
                {
                    Debug.LogError(
                        "PhotoCapture作成後も解像度が取得できません"
                    );

                    CleanupCamera();
                    return;
                }

                Resolution cameraResolution = resolutions
                    .OrderByDescending(
                        res => res.width * res.height
                    )
                    .First();

                Debug.Log(
                    "選択解像度: " +
                    cameraResolution.width + "x" +
                    cameraResolution.height
                );

                cameraParameters = new CameraParameters();
                cameraParameters.hologramOpacity = 0.0f;
                cameraParameters.cameraResolutionWidth =
                    cameraResolution.width;
                cameraParameters.cameraResolutionHeight =
                    cameraResolution.height;
                cameraParameters.pixelFormat =
                    CapturePixelFormat.BGRA32;

                // 撮影用のTexture2Dを生成
                if (targetTexture != null)
                {
                    Destroy(targetTexture);
                }

                targetTexture = new Texture2D(
                    cameraResolution.width,
                    cameraResolution.height,
                    TextureFormat.BGRA32,
                    false
                );

                // 撮影モード開始
                photoCaptureObject.StartPhotoModeAsync(
                    cameraParameters,
                    startResult =>
                    {
                        if (!startResult.success)
                        {
                            Debug.LogError("撮影モード開始失敗");
                            CleanupCamera();
                            return;
                        }

                        Debug.Log("撮影モード開始成功");

                        photoCaptureObject.TakePhotoAsync(
                            (takeResult, photoCaptureFrame) =>
                            {
                                try
                                {
                                    if (!takeResult.success ||
                                        photoCaptureFrame == null)
                                    {
                                        Debug.LogError("写真撮影失敗");
                                        return;
                                    }

                                    photoCaptureFrame
                                        .UploadImageDataToTexture(
                                            targetTexture
                                        );

                                    byte[] imageData =
                                        targetTexture.EncodeToJPG(85);

                                    if (imageData == null ||
                                        imageData.Length == 0)
                                    {
                                        Debug.LogError("JPEG変換失敗");
                                        return;
                                    }

                                    Debug.Log(
                                        "撮影成功: " +
                                        imageData.Length + " bytes"
                                    );

                                    PhotoCaptured?.Invoke(imageData);
                                }
                                catch (Exception e)
                                {
                                    Debug.LogError(
                                        "撮影処理エラー: " + e
                                    );
                                }
                                finally
                                {
                                    StopPhotoMode();
                                }
                            }
                        );
                    }
                );
            }
            catch (Exception e)
            {
                Debug.LogError(
                    "カメラ設定エラー: " + e
                );

                CleanupCamera();
            }
        });
    }

    private void StopPhotoMode()
    {
        if (photoCaptureObject == null)
        {
            cameraIsBusy = false;
            return;
        }

        photoCaptureObject.StopPhotoModeAsync(result =>
        {
            if (!result.success)
            {
                Debug.LogWarning("撮影モードの終了に失敗しました");
            }

            CleanupCamera();
        });
    }

    private void CleanupCamera()
    {
        if (photoCaptureObject != null)
        {
            photoCaptureObject.Dispose();
            photoCaptureObject = null;
        }

        cameraIsBusy = false;

        Debug.Log("カメラ解放完了");
    }

    private void OnDestroy()
    {
        if (photoCaptureObject != null)
        {
            photoCaptureObject.Dispose();
            photoCaptureObject = null;
        }

        if (targetTexture != null)
        {
            Destroy(targetTexture);
        }
    }

    private void CaptureTestImage()
    {
        if (testImage == null)
        {
            Debug.LogError("テスト画像が設定されていません");
            return;
        }

        cameraIsBusy = true;

        try
        {
            byte[] imageData = testImage.EncodeToJPG(85);

            if (imageData == null || imageData.Length == 0)
            {
                Debug.LogError("テスト画像のJPEG変換に失敗");
                return;
            }

            Debug.Log("テスト画像取得成功: " + imageData.Length + " bytes");

            PhotoCaptured?.Invoke(imageData);
        }
        catch (Exception e)
        {
            Debug.LogError("テスト画像処理エラー: " + e);
        }
        finally
        {
            cameraIsBusy = false;
        }
    }
}
