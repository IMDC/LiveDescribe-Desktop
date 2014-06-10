using System;

using LiveDescribe.Model;
namespace LiveDescribe.Events
{
    public class DescriptionEventArgs : EventArgs
    {
        private readonly Description _description;

        public DescriptionEventArgs(Description description)
        {
            _description = description;
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
