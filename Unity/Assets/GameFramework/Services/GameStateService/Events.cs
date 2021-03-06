///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 

using System.Collections.Generic;
using MoonSharp.Interpreter;

using UniRx;
using System.Runtime.Serialization;
using System;

namespace Service.GameStateService {
    
    public class Events {
        
        [DataContract(IsReference = true)]
        public class GameStateBeforeTick {
           [DataMember] public GameState gameState;

        }

        [DataContract(IsReference = true)]
        public class GameStateAfterTick {
           [DataMember] public GameState gameState;

        }

        [DataContract(IsReference = true)]
        public class GameStateBeforeEnter {
           [DataMember] public GameState gameState;

        }

        [DataContract(IsReference = true)]
        public class GameStateAfterEnter {
           [DataMember] public GameState gameState;

        }

        [DataContract(IsReference = true)]
        public class GameStateBeforeExit {
           [DataMember] public GameState gameState;

        }

        [DataContract(IsReference = true)]
        public class GameStateAfterExit {
           [DataMember] public GameState gameState;

        }
        
    }
}
///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 
