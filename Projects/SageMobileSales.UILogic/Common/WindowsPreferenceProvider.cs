using SageSDK.Preference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SageMobileSales.UILogic.Common
{
    internal class WindowsPreferenceProvider : IPreferenceProvider
    {
        public void SavePreferenceValue<T>(string key, T value)
        {
            ApplicationData.Current.LocalSettings.Values[key] = value;
        }

        public T GetPreferenceValue<T>(string key, T defaultValue)
        {
            return (T)(ApplicationData.Current.LocalSettings.Values[key] ?? defaultValue);
        }
    }
}
