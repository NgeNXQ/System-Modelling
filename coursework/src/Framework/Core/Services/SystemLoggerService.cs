using System;
using System.Text;
using System.Collections.Generic;
using Coursework.Framework.Components.Common;

namespace Coursework.Framework.Core.Services;

internal sealed class SystemLoggerService : IDisposable
{
    private const char LOG_ENTRY_DELIMITER = ' ';
    private const char LOG_ENTRY_TERMINATOR = '\n';

    private const string LOG_ENTRY_TYPE_WRAPPERS = "()";
    private const string LOG_ENTRY_PREFIX_WRAPPERS = "||";

    private const int LOG_ENTRY_WRAPPER_INDEX_START = 0;
    private const int LOG_ENTRY_WRAPPER_INDEX_FINISH = 1;

    private static readonly SystemLoggerService instance;

    private readonly StringBuilder buffer;
    private readonly HashSet<Element> senders;

    private bool isInitialized;

    static SystemLoggerService()
    {
        SystemLoggerService.instance = new SystemLoggerService();
    }

    private SystemLoggerService()
    {
        this.buffer = new StringBuilder();
        this.senders = new HashSet<Element>();
    }

    internal static SystemLoggerService Instance => SystemLoggerService.instance;

    public void Dispose()
    {
        this.isInitialized = false;
    }

    internal void Initialize()
    {
        this.isInitialized = true;
    }

    internal void RegisterSender(Element element)
    {
        if (element == null)
            throw new ArgumentNullException($"{nameof(element)} cannot be null.");

        if (this.senders.Contains(element))
            throw new ArgumentException($"{base.GetType()} is already subscribed to notifications from the {nameof(element)}.");

        this.senders.Add(element);
    }

    internal void AppendLogEntry(Element sender, string logPrefix, string logType, string logMessage)
    {
        if (sender == null)
            throw new ArgumentNullException($"{nameof(sender)} cannot be null.");

        if (!this.isInitialized)
            return;

        if (!this.senders.Contains(sender))
            return;

        this.buffer.Append(SystemLoggerService.LOG_ENTRY_PREFIX_WRAPPERS[SystemLoggerService.LOG_ENTRY_WRAPPER_INDEX_START]);
        this.buffer.Append(logPrefix ?? throw new ArgumentNullException($"{nameof(logPrefix)} cannot be null."));
        this.buffer.Append(SystemLoggerService.LOG_ENTRY_PREFIX_WRAPPERS[SystemLoggerService.LOG_ENTRY_WRAPPER_INDEX_FINISH]);

        this.buffer.Append(SystemLoggerService.LOG_ENTRY_DELIMITER);

        this.buffer.Append(SystemLoggerService.LOG_ENTRY_TYPE_WRAPPERS[SystemLoggerService.LOG_ENTRY_WRAPPER_INDEX_START]);
        this.buffer.Append(logType ?? throw new ArgumentNullException($"{nameof(logType)} cannot be null."));
        this.buffer.Append(SystemLoggerService.LOG_ENTRY_TYPE_WRAPPERS[SystemLoggerService.LOG_ENTRY_WRAPPER_INDEX_FINISH]);

        this.buffer.Append(SystemLoggerService.LOG_ENTRY_DELIMITER);

        this.buffer.Append(logMessage ?? throw new ArgumentNullException($"{nameof(logMessage)} cannot be null."));

        this.buffer.Append(SystemLoggerService.LOG_ENTRY_TERMINATOR);

        Console.Write(this.buffer.ToString());
        this.buffer.Clear();
    }
}
