using System.Text;

namespace BarcodeReader;

public class BarcodeReader : BaseBarcodeReader, ISyncBarcodeReader
{
    private readonly StringBuilder _barcode;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private Timer _timer;

    public BarcodeReader(string devicePath, int timeoutMs = 250) : base(devicePath, timeoutMs)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _barcode = new StringBuilder(26);
    }


    public void Start()
    {
        throw new NotImplementedException();
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }

    private void ReadBarcode(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    private void ProcessBuffer(byte[] buffer, int bytesRead)
    {
        throw new NotImplementedException();
    }

    private void ResetTimer()
    {
        throw new NotImplementedException();
    }

    private void StopTimer()
    {
        throw new NotImplementedException();
    }
}