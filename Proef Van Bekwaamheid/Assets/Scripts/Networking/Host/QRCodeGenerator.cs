using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;

public class QRCodeGenerator : MonoBehaviour
{
    [SerializeField] private RawImage rawImageReceiver;
    [SerializeField] private TextMeshProUGUI IPText;

    private Texture2D _storeEncodedTexture;

    void Start()
    {
        Debug.Log("<color=cyan>[QRCodeGenerator] Initializing — creating 256x256 texture...</color>");
        _storeEncodedTexture = new Texture2D(256, 256);
    }

    public void GenerateQRCode(string IPAddress)
    {
        Debug.Log("<color=cyan>[QRCodeGenerator] GenerateQRCode() called — starting QR code generation process...</color>");
        EncodeTextToQRode(IPAddress);
        IPText.text = IPAddress;
    }

    private void EncodeTextToQRode(string IPAddress)
    {
        string textWrite = string.IsNullOrEmpty(IPAddress)
            ? "0.0.0.0"
            : IPAddress;

        if (textWrite == " 0.0.0.0")
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
}
