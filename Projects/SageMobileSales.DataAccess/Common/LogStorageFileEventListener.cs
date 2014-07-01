using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace SageMobileSales.DataAccess.Common
{
    public class LogStorageFileEventListener : EventListener
    {
        /// <summary>
        ///     Name of the current event listener
        /// </summary>
        private readonly string _mName;

        private readonly SemaphoreSlim _mSemaphoreSlim = new SemaphoreSlim(1);

        /// <summary>
        ///     The format to be used by logging.
        /// </summary>
        private string m_Format = "{0:yyyy-MM-dd HH\\:mm\\:ss\\:ffff}\tType: {1}\tId: {2}\tMessage: '{3}'";

        /// <summary>
        ///     Storage file to be used to write logs
        /// </summary>
        private StorageFile _mStorageFile;

        public LogStorageFileEventListener(string name)
        {
            _mName = name;

            Debug.WriteLine("StorageFileEventListener for {0} has name {1}", GetHashCode(), name);

            AssignLocalFile();
        }

        private async void AssignLocalFile()
        {
            _mStorageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(_mName.Replace(" ", "_") + ".log",
                CreationCollisionOption.OpenIfExists);
        }

        private async void WriteToFile(IEnumerable<string> lines)
        {
            await _mSemaphoreSlim.WaitAsync();

            await Task.Run(async () =>
            {
                try
                {
                    await FileIO.AppendLinesAsync(_mStorageFile, lines);
                }
                catch (Exception ex)
                {
                    // TODO:
                }
                finally
                {
                    _mSemaphoreSlim.Release();
                }
            });
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (_mStorageFile == null) return;

            var lines = new List<string>();

            string newFormatedLine = string.Format(m_Format, DateTime.Now, eventData.Level, eventData.EventId,
                eventData.Payload[0]);

            Debug.WriteLine(newFormatedLine);

            lines.Add(newFormatedLine);

            WriteToFile(lines);
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            Debug.WriteLine("OnEventSourceCreated for Listener {0} - {1} got eventSource {2}", GetHashCode(), _mName,
                eventSource.Name);
        }
    }
}