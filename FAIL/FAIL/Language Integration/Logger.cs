namespace FAIL.Language_Integration;
internal class Logger : IDisposable
{
    private bool disposedValue;

    public LogLevel Level { get; set; }
    public StreamWriter? Stream { get; init; }


    public Logger(LogLevel level = LogLevel.Warn, StreamWriter? stream = null)
    {
        Level = level;
        Stream = stream;

        if (stream is not null) stream.AutoFlush = true;
    }


    public bool Log(dynamic value, LogLevel level)
    {
        if (level < Level) return false;
        if (Stream is null) return false;

        Stream.WriteLine($"{DateTime.Now,19} | {level,-8} | {value}");
        return true;
    }
    public bool Log(dynamic value, dynamic sender, LogLevel level)
    {
        if (level < Level) return false;
        if (Stream is null) return false;

        Stream.WriteLine($"{DateTime.Now,19} | {level,-8} | Element of type '{sender.GetType().Name}' has exited with value '{value}'.");
        return true;
    }


    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Stream?.Dispose();
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }
    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~Logger()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
