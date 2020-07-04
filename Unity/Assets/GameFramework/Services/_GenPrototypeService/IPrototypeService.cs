///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 

/*name:using*//*endname*/
using System;
using static /*name:namespace*/Service.GeneratorPrototype/*endname*/.Events;
using System.Runtime.Serialization;
using FlatBuffers;
using Service.Serializer;
using System.Linq;

namespace /*name:namespace*/Service.GeneratorPrototype/*endname*/ {
    public interface /*name:interfaceName*/IPrototypeService/*endname*/ : IFBSerializable2, IFBSerializable, IService {
/*block:property*/
    /// <summary>
    /// /*name:documentation*//*endname*/
    /// </summary>
/*name:type*/int/*endname*/ /*name:propName*/MaxSoundChannels/*endname*/{/*name:getter*/get;/*endname*//*name:setter*/set;/*endname*/}
/*endblock:property*/

        /*block:method*/
        /*block:documentation*/
        /// <summary>
        /// /*name:summary*/This method is about blabla/*endname*/ 
        /*block:param*/        /// <param name="/*name:name*/settings/*endname*/"></param>
        /*endblock:param*/ /// </summary>
        /*block:return*//// <returns>/*name:return*//*endname*/</returns>/*endblock:return*/
        /*endblock:documentation*/
        /*block:methodBody*/            /*name:returnType*/
        string/*endname*/ /*name:methodName*/DoPrototype/*endname*//*block:genericDefinition*//*name:genInput*//*endname*//*endblock:genericDefinition*/(/*block:parameter*//*name:comma*//*endname*//*name:type*/string/*endname*/ /*name:name*/settings/*endname*//*name:defaultValue*/= ""/*endname*//*endblock:parameter*/)/*block:genericRestriction*//*name:genericRestrictionInput*//*endname*//*endblock:genericRestriction*//*endblock:methodBody*/;
        /*name:usermethod*//*endname*/
                           /*endblock:method*/

    }

    /*block:modelEnum*/
    public enum /*name:enumName*/TheEnum/*endname*/ {
        /*block:rip*/
        A/*endblock:rip*//*block:elem*//*block:comma*/,/*endblock:comma*//*name:name*/B/*endname*//*endblock:elem*/
        /*block:rip*/, C, D/*endblock:rip*/
    }
    /*endblock:modelEnum*/
    /*block:modelClass*/
    [System.Serializable]
    public partial class /*name:className*/SomeModel/*endname*//*name:inheritance*/: DefaultSerializable2 /*endname*/
    {
        /*block:ser2_header*/
        [Newtonsoft.Json.JsonIgnore]

        private /*name:newkeyword*/new/*endname*/ ExtendedTable ser2table = ExtendedTable.NULL;

        [Newtonsoft.Json.JsonIgnore]
        public /*name:newkeyword*/new/*endname*/ ExtendedTable Ser2Table => ser2table;

        [Newtonsoft.Json.JsonIgnore]
        public /*name:newkeyword*/new/*endname*/ int Ser2Flags { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public /*name:newkeyword*/new/*endname*/ bool Ser2HasOffset => Ser2HasValidContext && !ser2table.IsNULL() && ser2table.bb != null;

        [Newtonsoft.Json.JsonIgnore]
        public /*name:newkeyword*/new/*endname*/ int Ser2Offset { get => ser2table.offset; set => ser2table.offset = value; }

        public /*name:override*/virtual/*endname*/ void Ser2Deserialize(DeserializationContext ctx) {
            int offset = ctx.bb.Length - ctx.bb.GetInt(ctx.bb.Position) + ctx.bb.Position;
            Ser2Deserialize(offset, ctx);
        }
        [Newtonsoft.Json.JsonIgnore]
        public  /*name:newkeyword*/new/*endname*/ IFB2Context Ser2Context { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public  /*name:newkeyword*/new/*endname*/ bool Ser2HasValidContext => Ser2Context != null && ((IFB2Context)Ser2Context).IsValid();

        public /*name:override*/virtual/*endname*/ int Ser2Serialize(SerializationContext ctx) {
#if TESTING
            if (Ser2HasOffset && Ser2HasValidContext) {
                if (Ser2Context == this) {
                    UnityEngine.Debug.LogError($"Ser2Serialize called for {GetType()} but it was already serialized by this sctx");
                } else {
                    UnityEngine.Debug.LogError($"Ser2Serialize called for {GetType()} but it was already serialized by another context");
                }
            }
#endif
            Ser2CreateTable(ctx, ctx.builder);

            //if (!Ser2HasOffset) {
            //    Ser2CreateTable(ctx, ctx.builder);
            //} else {
            //    Ser2UpdateTable(ctx, ctx.builder);
            //}
            return Ser2Offset;
        }

        public /*name:newkeyword*/new/*endname*/ void Ser2Clear() {
            ser2table = ExtendedTable.NULL;
        }
        /*endblock:ser2_header*/

        public /*name:className*/SomeModel/*endname*/() { }
        /*block:field*/
        /// <summary>
        /// /*name:documentation*//*endname*/
        /// </summary>
        /*name:attributes*//*endname*/
        /*name:scope*/
        public/*endname*/ /*name:type*/string/*endname*//*name:nullable*//*endname*/ /*name:name*/name/*endname*/ /*block:valueBlock*/= /*name:value*/"value"/*endname*//*endblock:valueBlock*/;
        /*endblock:field*/
        /*block:property*/
        /// <summary>
        /// /*name:documentation*//*endname*/
        /// </summary>
        /*name:scope*/
        public/*endname*/ /*name:type*/int/*endname*/ /*name:name*/MaxSoundChannels/*endname*/{/*name:getter*/get;/*endname*//*name:setter*/set;/*endname*/}
        /*endblock:property*/
        /*block:constructor*/
        /// <summary>
        /// /*name:documentation*//*endname*/
        /// </summary>
/*block:docParam*/        /// <param name="/*name:name*//*endname*/">/*name:documentation*//*endname*/</param>
/*endblock:docParam*/
        public /*name:className*/SomeModel/*endname*/(/*block:rip*/int maxChannels/*endblock:rip*//*block:parameter*//*name:comma*/,/*endname*//*name:type*/string/*endname*/ /*name:paramName*/name/*endname*//*endblock:parameter*/) {
            /*block:constructorSet*/
            this./*name:name*/name/*endname*/ = /*name:paramName*/name/*endname*/;
            /*endblock:constructorSet*/
            /*block:rip*/
            this.MaxSoundChannels = maxChannels;/*endblock:rip*/
        }

        /*endblock:constructor*/

        /// <summary>
        /// Merges data into your object. (no deep copy)
        /// </summary>
        /// <param name="incoming"></param>
        /// <param name="onlyCopyPersistedData"></param>
        public void MergeDataFrom(/*name:className*/SomeModel/*endname*/ incoming, bool onlyCopyPersistedData = false) {
            /*name:mergeDataInheritance*/ // /*endname*/ base.MergeDataFrom(incoming, onlyCopyPersistedData);

            /*block:MergeField*//*name:copyNonPersisted*/if (!onlyCopyPersistedData)/*endname*/ this./*name:name*/MaxSoundChannels/*endname*/ = incoming./*name:name*/MaxSoundChannels/*endname*/;
            /*endblock:MergeField*/
        }

        /*name:classSerialization*/
        public void Deserialize(int dataFormatNr, object incoming) {
            throw new System.NotImplementedException();
        }

        public void Deserialize(ByteBuffer buf) {
            throw new System.NotImplementedException();
        }

        public int Serialize(FlatBufferBuilder builder) {
            throw new System.NotImplementedException();
        }
        /*endname*/
    }

    public partial class /*name:className*/SomeModel/*endname*/ : IFBSerializeAsTypedObject, IMergeableData</*name:className*/SomeModel/*endname*/> {
    }
        /*endblock:modelClass*/
}
///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 
