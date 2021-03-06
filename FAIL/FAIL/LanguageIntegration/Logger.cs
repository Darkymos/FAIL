using static System.FormattableString;

namespace FAIL.LanguageIntegration;
internal class Logger : IDisposable
{
    private bool disposedValue;

    public LogLevel Level { get; set; }
    public StreamWriter Stream { private get; init; }


    public Logger(StreamWriter stream, LogLevel level = LogLevel.Warn)
    {
        Level = level;
        Stream = stream;

        stream.AutoFlush = true;
    }

    
    public bool Log(dynamic value, LogLevel level)
    {
        if (level < Level) return false;

        Stream.WriteLine(Invariant($"{DateTime.Now,19} | {level,-8} | {value}"));
        return true;
    }
    public bool Log(dynamic value, dynamic sender, LogLevel level)
    {
        if (level < Level) return false;

        Stream.WriteLine(Invariant($"{DateTime.Now,19} | {level,-8} | Element of type '{sender.GetType().Name}' has exited with value '{value}'."));
        return true;
    }


    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Stream.Close();
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
