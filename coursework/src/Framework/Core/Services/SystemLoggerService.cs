using System;
using System.Text;

namespace CourseWork.Framework.Core.Services;

internal sealed class SystemLoggerService
{
    private const int AUTOMATIC_FLUSH_ENTRIES_COUNT = 2;

    private const char LOG_ENTRY_DELIMITER = ' ';
    private const char LOG_ENTRY_TERMINATOR = '\n';

    private const string LOG_ENTRY_TYPE_WRAPPERS = "()";
    private const string LOG_ENTRY_PREFIX_WRAPPERS = "||";

    private const int LOG_ENTRY_WRAPPER_INDEX_START = 0;
    private const int LOG_ENTRY_WRAPPER_INDEX_FINISH = 1;

    private static readonly SystemLoggerService instance;

    private readonly StringBuilder buffer;

    int logEntriesInBufferCount;

    static SystemLoggerService()
    {
        SystemLoggerService.instance = new SystemLoggerService();
    }

    private SystemLoggerService()
    {
        this.logEntriesInBufferCount = 0;
        this.buffer = new StringBuilder();
    }

    internal static SystemLoggerService Instance => SystemLoggerService.instance;

    internal void AppendLogEntry(string logPrefix, string logType, string logMessage)
    {
        buffer.Append(SystemLoggerService.LOG_ENTRY_PREFIX_WRAPPERS[SystemLoggerService.LOG_ENTRY_WRAPPER_INDEX_START]);
        buffer.Append(logPrefix ?? throw new ArgumentNullException($"{nameof(logPrefix)} cannot be null."));
        buffer.Append(SystemLoggerService.LOG_ENTRY_PREFIX_WRAPPERS[SystemLoggerService.LOG_ENTRY_WRAPPER_INDEX_FINISH]);

        buffer.Append(SystemLoggerService.LOG_ENTRY_DELIMITER);

        buffer.Append(SystemLoggerService.LOG_ENTRY_TYPE_WRAPPERS[SystemLoggerService.LOG_ENTRY_WRAPPER_INDEX_START]);
        buffer.Append(logType ?? throw new ArgumentNullException($"{nameof(logType)} cannot be null."));
        buffer.Append(SystemLoggerService.LOG_ENTRY_TYPE_WRAPPERS[SystemLoggerService.LOG_ENTRY_WRAPPER_INDEX_FINISH]);

        buffer.Append(SystemLoggerService.LOG_ENTRY_DELIMITER);

        buffer.Append(logMessage ?? throw new ArgumentNullException($"{nameof(logMessage)} cannot be null."));

        buffer.Append(SystemLoggerService.LOG_ENTRY_TERMINATOR);

        ++this.logEntriesInBufferCount;

        if (this.logEntriesInBufferCount >= SystemLoggerService.AUTOMATIC_FLUSH_ENTRIES_COUNT)
            this.Flush();
    }

    internal void Flush()
    {
        Console.Write(this.buffer.ToString());
        this.logEntriesInBufferCount = 0;
        this.buffer.Clear();
    }
}
