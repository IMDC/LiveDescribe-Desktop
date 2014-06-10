using System;
using LiveDescribe.Model;

namespace LiveDescribe.Events
{
    public class SpaceEventArgs : EventArgs
    {
        private readonly Space _space;

        public SpaceEventArgs(Space space)
        {
            _space = space;
        }

        public Space Space
        {
            get { return _space; }
        }
    }
}
