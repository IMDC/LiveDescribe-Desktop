using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LiveDescribe.Model;
namespace LiveDescribe.Events
{
    public class DescriptionEventArgs : EventArgs
    {
        private Description _description;

        public DescriptionEventArgs(Description description)
        {
            this._description = description;
        }

        public Description Description
        {
            get
            {
                return _description;
            }
        }
    }
}
