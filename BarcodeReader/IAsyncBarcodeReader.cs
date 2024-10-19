namespace BarcodeReader;

public interface IAsyncBarcodeReader : IBarcodeReader
{
    Task StartAsync();

    Task Stop();
}