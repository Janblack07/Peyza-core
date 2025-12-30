using System;
using System.Collections.Generic;
using System.Text;

namespace Peyza.Core.NotificationManagement.Workers
{
    public class NotificationDispatcherOptions
    {
        public int PeriodSeconds { get; set; } = 10; // cada 10s
        public int BatchSize { get; set; } = 25;     // procesa 25 por iteración
    }
}
