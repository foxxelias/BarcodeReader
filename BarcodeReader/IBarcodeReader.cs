namespace BarcodeReader;

public interface IBarcodeReader
{
    public delegate void BarcodeScannedEventHandler(object sender, string barcode);

    public delegate void IoErrorEventHandler(object sender, string error);

    void Start();
    void Stop();

    event BarcodeScannedEventHandler BarcodeScanned;
    event IoErrorEventHandler IOError;
}