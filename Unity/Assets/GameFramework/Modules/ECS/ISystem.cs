using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ECS {
    public interface ISystem {
        /// <summary>
        /// Fetch all needed components from the entity and return them.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        List<IComponent> GetNeededComponents(UID entity);
    }
}

