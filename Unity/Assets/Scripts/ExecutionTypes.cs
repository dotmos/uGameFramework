﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx;
using Zenject;


public enum ExecutionDomain
{
    scripting,
    ui,
    pathfinding,
    unknown
}


