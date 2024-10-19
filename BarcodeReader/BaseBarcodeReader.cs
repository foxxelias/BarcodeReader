namespace BarcodeReader;

public abstract class BaseBarcodeReader : IBarcodeReader
{
    protected readonly string InputDevicePath;
    protected readonly int Timeout;

    protected BaseBarcodeReader(string devicePath, int timeoutMs = 250)
    {
        InputDevicePath = devicePath;
        Timeout = timeoutMs;
    }

    public event IBarcodeReader.BarcodeScannedEventHandler BarcodeScanned;
    public event IBarcodeReader.IoErrorEventHandler IOError;

    public abstract void Start();
    public abstract void Stop();

    protected void RaiseBarcodeScanned(string barcode)
    {
        BarcodeScanned?.Invoke(this, barcode);
    }

    protected void RaiseIOError(string error)
    {
        IOError?.Invoke(this, error);
    }
}