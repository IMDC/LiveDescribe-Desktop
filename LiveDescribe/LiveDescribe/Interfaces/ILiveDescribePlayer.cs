﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveDescribe.Interfaces
{
    public interface ILiveDescribePlayer
    {
        TimeSpan CurrentPosition { get; }

        double DurationSeconds { get; }

        LiveDescribeStates CurrentState { set; get; }
        
        string Path { set; get; }
    }
}
