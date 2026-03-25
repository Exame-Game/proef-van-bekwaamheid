using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;

public class QRCodeGenerator : MonoBehaviour
{

#if UNITY_STANDALONE || UNITY_EDITOR
    [SerializeField] private RawImage rawImageReceiver;
    [SerializeField] private NetworkHost network;

    private Texture2D _storeEncodedTexture;

    void Start()
    {
        Debug.Log("<color=cyan>[QRCodeGenerator] Initializing — creating 256x256 texture...</color>");
        _storeEncodedTexture = new Texture2D(256, 256);
        InitializeAction();
    }

    private void InitializeAction()
    {
        if (network == null)
        {
            Debug.LogError("<color=red>[QRCodeGenerator] InitializeAction() — NetworkHost reference is null! QR won't auto-update on connection.</color>");
            return;
        }

        network.OnConnectionChanged += OnConnectionChanged;
        OnConnectionChanged(true);
        Debug.Log("<color=cyan>[QRCodeGenerator] Subscribed to NetworkHost.OnConnectionChanged.</color>");
    }

    private void OnDestroy()
    {
        if (network == null)
            return;

        network.OnConnectionChanged -= OnConnectionChanged;
        Debug.Log("<color=orange>[QRCodeGenerator] OnDestroy — unsubscribed from NetworkHost.OnConnectionChanged.</color>");
    }

    private Color32[] Encode(string textForEncoding, int width, int height)
    {
        Debug.Log($"<color=cyan>[QRCodeGenerator] Encoding QR — text: \"{textForEncoding}\" | size: {width}x{height}</color>");

        BarcodeWriter writer = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = height,
                Width = width
            }
        };

        return writer.Write(textForEncoding);
    }

    private void OnConnectionChanged(bool isConnected)
    {
        Debug.Log($"<color=cyan>[QRCodeGenerator] Connection state changed: {(isConnected ? "Connected" : "Disconnected")}</color>");

        if (!isConnected)
        {
            Debug.Log("<color=orange>[QRCodeGenerator] Disconnected — skipping QR generation.</color>");
            return;
        }

        if (string.IsNullOrEmpty(network.CurrentIPAddress))
        {
            Debug.LogWarning("<color=orange>[QRCodeGenerator] OnConnectionChanged — CurrentIPAddress is null or empty, QR may be invalid.</color>");
        }
        else
        {
            Debug.Log($"<color=green>[QRCodeGenerator] IP received: {network.CurrentIPAddress} — generating QR...</color>");
        }

        EncodeTextToQRode();
    }

    public void OnClickEncode()
    {
        Debug.Log("<color=cyan>[QRCodeGenerator] OnClickEncode() called manually.</color>");
        EncodeTextToQRode();
    }

    private void EncodeTextToQRode()
    {
        string textWrite = string.IsNullOrEmpty(network.CurrentIPAddress)
            ? "You shoud write something"
            : network.CurrentIPAddress;

        if (textWrite == "You shoud write something")
            Debug.LogWarning("<color=orange>[QRCodeGenerator] No IP address found — encoding fallback placeholder text.</color>");

        Color32[] _convertPixelToTexture = Encode(textWrite, _storeEncodedTexture.width, _storeEncodedTexture.height);

        if (_convertPixelToTexture == null || _convertPixelToTexture.Length == 0)
        {
            Debug.LogError("<color=red>[QRCodeGenerator] Encode() returned empty pixel array — QR texture not updated!</color>");
            return;
        }

        _storeEncodedTexture.SetPixels32(_convertPixelToTexture);
        _storeEncodedTexture.Apply();
        rawImageReceiver.texture = _storeEncodedTexture;

        Debug.Log($"<color=green>[QRCodeGenerator] QR code generated and applied to RawImage for: \"{textWrite}\"</color>");
    }
#endif
}