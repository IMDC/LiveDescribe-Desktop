using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveDescribe.Interfaces
{
    /// <summary>
    /// An interface for specifying specific method signatures for view models that alter values in
    /// Settings.Default.
    /// </summary>
    interface ISettingsViewModel
    {
        void RetrieveApplicationSettings();
        void SetApplicationSettings();
    }
}
