using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveDescribe.Model;

namespace LiveDescribe.Events
{
    public class SpaceEventArgs : EventArgs
    {
        private readonly Space _space;

        public SpaceEventArgs(Space space)
        {
            this._space = space;
        }

        public Space Space
        {
            get { return _space; }
        }
    }
}
