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
    public interface /*name:interfaceName*/IPrototypeService/*endname*/ : IFBSerializable, IService {
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
		/*block:methodBody*/			/*name:returnType*/string/*endname*/ /*name:methodName*/DoPrototype/*endname*//*block:genericDefinition*//*name:genInput*//*endname*//*endblock:genericDefinition*/(/*block:parameter*//*name:comma*//*endname*//*name:type*/string/*endname*/ /*name:name*/settings/*endname*//*name:defaultValue*/= ""/*endname*//*endblock:parameter*/)/*block:genericRestriction*//*name:genericRestrictionInput*//*endname*//*endblock:genericRestriction*//*endblock:methodBody*/;
    /*name:usermethod*//*endname*/
/*endblock:method*/

	}

/*block:modelEnum*/
    public enum /*name:enumName*/TheEnum/*endname*/ {
        /*block:rip*/A/*endblock:rip*//*block:elem*//*block:comma*/,/*endblock:comma*//*name:name*/B/*endname*//*endblock:elem*/
        /*block:rip*/,C,D/*endblock:rip*/
    }
    /*endblock:modelEnum*/
    /*block:modelClass*/
    [System.Serializable]
    public /*name:partial*//*endname*/ class /*name:className*/SomeModel/*endname*//*name:inheritance*/: DefaultSerializable2 /*endname*/
    {

        private ExtendedTable ser2table = ExtendedTable.NULL;

        public new ExtendedTable Ser2Table => ser2table;

        public new bool Ser2IsDirty { get; set; } // TODO. Is dirty should be some kind of virtual

        public new bool Ser2HasOffset => !ser2table.IsNULL();

        public new int Ser2Offset => ser2table.offset;

        public /*name:override*/virtual/*endname*/ void Ser2Deserialize(DeserializationContext ctx) {
            int offset = ctx.bb.Length - ctx.bb.GetInt(ctx.bb.Position) + ctx.bb.Position;
            Ser2Deserialize(offset, ctx);
        }

        public /*name:override*/virtual/*endname*/ int Ser2Serialize(SerializationContext ctx) {
            if (!Ser2HasOffset) {
                Ser2CreateTable(ctx, ctx.builder);
            } else {
                Ser2UpdateTable(ctx, ctx.builder);
            }
            return base.ser2table.offset;
        }

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
        /// Default constructor
        /// </summary>
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

    /*endblock:modelClass*/
}
///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 
