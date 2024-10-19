using System.Buffers;
using System.Text;

namespace BarcodeReader;

public class BarcodeReaderAsync(string devicePath, int timeoutMs)
    : BaseBarcodeReader(devicePath, timeoutMs), IAsyncBarcodeReader
{
    private readonly StringBuilder _barcode = new(26);
    private readonly object _lock = new();
    private CancellationTokenSource _cancellationTokenSource = new();
    private Task? _readTask; // Переменная для хранения задачи чтения
    private Timer? _timer;

    public async Task StartAsync()
    {
        if (_readTask != null && !_readTask.IsCompleted) throw new InvalidOperationException("Чтение уже запущено.");

        _readTask = Task.Factory.StartNew(() => ReadBarcodeAsync(_cancellationTokenSource.Token),
            TaskCreationOptions.LongRunning);
    }

    public async Task Stop()
    {
        await _cancellationTokenSource.CancelAsync();
        StopTimer();
        await _readTask; // Ждать завершения задачи чтения, если запущена
        _cancellationTokenSource.Dispose(); // Освободить ресурсы
        _cancellationTokenSource = new CancellationTokenSource(); // Создать новый токен для следующего запуска
    }

    private async Task ReadBarcodeAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (!File.Exists(InputDevicePath))
            {
                RaiseIOError("Устройство не найдено.");
                await Task.Delay(1000, token); // Задержка при отсутствии устройства
                continue; // Возвращаемся к началу цикла для повторной проверки
            }

            try
            {
                using (var fs = new FileStream(InputDevicePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite,
                           4096, true))
                {
                    var buffer = ArrayPool<byte>.Shared.Rent(24);
                    var memoryBuffer = new Memory<byte>(buffer);
                    try
                    {
                        while (!token.IsCancellationRequested)
                        {
                            var bytesRead = await fs.ReadAsync(memoryBuffer, token);
                            if (bytesRead == 0) break;

                            ProcessBuffer(memoryBuffer.Span[..bytesRead]);
                        }
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(buffer); // Освобождаем пул
                    }
                }
            }
            catch (IOException ex)
            {
                RaiseIOError("Ошибка ввода/вывода: " + ex.Message);
                await Task.Delay(1000, token); // Задержка при ошибке
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Ошибка доступа: {ex.Message}. Проверьте разрешения на устройство.");
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Неизвестная ошибка: {ex.Message}.");
                break; // Останавливаем выполнение
            }
        }
    }

    private void ProcessBuffer(Span<byte> memoryBuffer)
    {
        foreach (var b in memoryBuffer)
            if (b >= 32 && b <= 126) // Печатаемые символы
            {
                lock (_lock) // Блокируем при изменении _barcode
                {
                    _barcode.Append((char)b);
                }

                ResetTimer();
            }
            else if (b == 13) // Enter
            {
                string barcodeValue;
                lock (_lock) // Блокируем при чтении и очистке _barcode
                {
                    barcodeValue = _barcode.ToString();
                    _barcode.Clear();
                }

                RaiseBarcodeScanned(barcodeValue);
                StopTimer(); // Останавливаем таймер после успешного сканирования
            }
    }

    private void ResetTimer()
    {
        lock (_lock) // Блокируем при доступе к таймеру
        {
            if (_timer == null)
                _timer = new Timer(_ =>
                {
                    string barcodeValue;
                    lock (_lock) // Блокируем при доступе к _barcode
                    {
                        barcodeValue = _barcode.ToString();
                        _barcode.Clear();
                    }

                    RaiseBarcodeScanned(barcodeValue);
                }, null, Timeout, System.Threading.Timeout.Infinite);
            else
                _timer.Change(Timeout, System.Threading.Timeout.Infinite);
        }
    }

    private void StopTimer()
    {
        lock (_lock) // Блокируем при остановке таймера
        {
            if (_timer != null)
            {
                _timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                _timer.Dispose();
                _timer = null;
            }
        }
    }
}