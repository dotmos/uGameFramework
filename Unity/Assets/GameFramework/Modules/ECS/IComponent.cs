using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ECS {
    public interface IComponent{
        UID ID { get; set; }
        UID Entity { get; set; }
    }
}