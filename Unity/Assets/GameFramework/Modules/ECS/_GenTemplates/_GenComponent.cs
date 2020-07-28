
using ECS;
using FlatBuffers;
using ModestTree;
using Service.Serializer;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/*name:using*/ /*endname*/

/// <summary>
/*name:componentComment*/ /*endname*/
/// </summary>
[System.Serializable]
public partial class /*name:ComponentName*/GenTemplateComponent/*endname*/ : ECS.Component {
    /*block:enum*/
    /// <summary>
    /*name:comment*//*endname*/
                    /// </summary>
    public enum /*name:enumName*/State/*endname*/ : int {
        /*block:entry*//*block:comment*/
                       /// <summary>
        /*name:comment*//*endname*/
                        /// </summary>/*endblock:comment*/
        /*name:entryName*/
        state1/*endname*/ /*name:entryNumber*//*endname*/,
        /*endblock:entry*/
        /*block:rip*/
        state2 = 1,
        state3 = 2,
        /*endblock:rip*/
    }
    /*endblock:enum*/

    /*block:modelEnum*/
    public enum /*name:enumName*/TheEnum/*endname*/
    {
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
        private /*name:newkeyword*/new/*endname*/ ExtendedTable ser2table = ExtendedTable.NULL;

        public /*name:newkeyword*/new/*endname*/ ExtendedTable Ser2Table { get => ser2table; set => ser2table = value; }

        [System.NonSerialized]
        private int ser2flags;

        [Newtonsoft.Json.JsonIgnore]
        public /*name:newkeyword*/new/*endname*/ int Ser2Flags { get => ser2flags; set => ser2flags = value; } // TODO. Is dirty should be some kind of virtual

        [Newtonsoft.Json.JsonIgnore]
        public /*name:newkeyword*/new/*endname*/ bool Ser2HasOffset => Ser2HasValidContext && !ser2table.IsNULL() && ser2table.bb != null;

        [Newtonsoft.Json.JsonIgnore]
        public /*name:newkeyword*/new/*endname*/ int Ser2Offset { get => ser2table.offset; set => ser2table.offset = value; }

        [Newtonsoft.Json.JsonIgnore]
        public  /*name:newkeyword*/new/*endname*/ IFB2Context Ser2Context { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public  /*name:newkeyword*/new/*endname*/ bool Ser2HasValidContext => Ser2Context != null && ((IFB2Context)Ser2Context).IsValid();

        //public /*name:override*/virtual/*endname*/ void Ser2Deserialize(DeserializationContext ctx) {
        //    int offset = ctx.bb.Length - ctx.bb.GetInt(ctx.bb.Position) + ctx.bb.Position;
        //    Ser2Deserialize(offset, ctx);
        //}

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
            if (incoming == null) {
                Debug.LogError("Trying to merge from null! Type: /*name:className*/SomeModel/*endname*/");
                return;
            }
            /*name:mergeDataInheritance*/ // /*endname*/ base.MergeDataFrom(incoming, onlyCopyPersistedData);

            /*block:MergeField*//*name:copyNonPersisted*/
            if (!onlyCopyPersistedData)/*endname*/ this./*name:name*/MaxSoundChannels/*endname*/ = incoming./*name:name*/MaxSoundChannels/*endname*/;
            /*endblock:MergeField*/
        }

        /*name:classSerialization*/
        public void Deserialize(int dataFormatNr, object incoming) {
            throw new System.NotImplementedException();
        }

        public virtual void Deserialize(ByteBuffer buf) {
            throw new System.NotImplementedException();
        }

        public virtual int Serialize(FlatBufferBuilder builder) {
            throw new System.NotImplementedException();
        }
        /*endname*/
    }

    public partial class /*name:className*/SomeModel/*endname*/ : IFBSerializeAsTypedObject, IMergeableData</*name:className*/SomeModel/*endname*/> { }

    /*endblock:modelClass*/

    /*block:field*/
    /// <summary>
    /*name:comment*//*endname*/
    /// </summary>
    /*name:attributes*//*endname*/
    /*name:accessor*/
    public/*endname*/ /*name:type*/State/*endname*//*name:nullable*//*endname*/ /*name:name*/state/*endname*/ /*name:value*/= State.state1/*endname*/;

    public void /*name|fu,pre#Set:name*/SetState/*endname*/(/*name:type*/State/*endname*/ value,bool addToLuaReplay=false, string luaValue=null) {
        if (addToLuaReplay && this./*name:name*/state/*endname*/ != value) {
            var scriptingService = Kernel.Instance.Container.Resolve<Service.Scripting.IScriptingService>();
            if (scriptingService.IsEntityRegistered(Entity)) {
                int uID = scriptingService.GetLUAEntityID(Entity);
                scriptingService.ReplayWrite_CustomLua($"component = script.GetComponent(uID[{uID}],'/*name:ComponentName*/GenTemplateComponent/*endname*/')");
                if (luaValue==null && value is bool) {
                    scriptingService.ReplayWrite_CustomLua($"component./*name:name*/state/*endname*/={luaValue ?? value.ToString().ToLower()}", false);
                } else {
                    scriptingService.ReplayWrite_CustomLua($"component./*name:name*/state/*endname*/={luaValue ?? value.ToString()}", false);
                }
            }
        }
        this./*name:name*/state/*endname*/ = value;
    }

    /*endblock:field*/
    /*block:rip*/
    public string testName = "f95";
    public UID? testUIDnullable = new UID(1895, 0);
    public UID testUID = new UID(1895, 0);
    public Vector2 vec2 = new Vector2(1, 2);
    public Vector3 vec3 = new Vector3(1, 2, 3);
    public Vector4 vec4 = new Vector4(1, 2, 3,4);

    public float testNumber = 18.95f;
    public State? nullState = null;
    public int intValue = 95;
    public DefaultSerializable2 typedObj1 = new SomeClazz1();
    public SomeClazz2 serObj = new SomeClazz2();
    public SomeStruct serStruct = new SomeStruct();

    public class SomeStruct : IFBSerializable2Struct
    {
        public int ByteSize => throw new System.NotImplementedException();
        #region nonimportant_default_implementation
        public void Get(ExtendedTable table, int fbPos) {
            throw new System.NotImplementedException();
        }


        public int Put(FlatBufferBuilder builder,bool prep=true) {
            throw new System.NotImplementedException();
        }
        #endregion
    }

    public class SomeClazz1 : DefaultSerializable2
{
    #region nonimportant_default_implementation
    public override void Ser2CreateTable(SerializationContext ctx, FlatBufferBuilder builder) {
        throw new System.NotImplementedException();
    }

    public override void Ser2UpdateTable(SerializationContext ctx, FlatBufferBuilder builder) {
        throw new System.NotImplementedException();
    }

    #endregion

}
public class SomeClazz2 : DefaultSerializable2
{
    #region nonimportant_default_implementation
    public override void Ser2CreateTable(SerializationContext ctx, FlatBufferBuilder builder) {
        throw new System.NotImplementedException();
    }

    public override void Ser2UpdateTable(SerializationContext ctx, FlatBufferBuilder builder) {
        throw new System.NotImplementedException();
    }

    #endregion

}



    public class FreeTypeTestObject : IPocoBase<FreeTypeTestObject> {
        private string data;
        public FreeTypeTestObject() {
            data = "bla";
        }
        public void ShallowCopy(FreeTypeTestObject from) {
            this.data = from.data;
        }
    }



    public FreeTypeTestObject freeTypeTest = null;

    public List<UID> structList = new List<UID>();
    public ObservableList<UID> obsStructList = new ObservableList<UID>();

    public List<SomeClazz2> objectList = new List<SomeClazz2>();
    public ObservableList<SomeClazz2> obsObjectList = new ObservableList<SomeClazz2>();
    public System.Collections.Generic.List<UID> testListUID = new System.Collections.Generic.List<UID>();
    public System.Collections.Generic.List<int> testListPrimitive = new System.Collections.Generic.List<int>();
    public ObservableList<int> testObsListPrimitive = new ObservableList<int>();
    public System.Collections.Generic.Dictionary<int, int> testDict = new System.Collections.Generic.Dictionary<int, int>();
    public System.Collections.Generic.Dictionary<SerializableHelper, SerializableHelper> testDict2 = new System.Collections.Generic.Dictionary<SerializableHelper, SerializableHelper>();
    public System.Collections.Generic.List<string> testStringList = new System.Collections.Generic.List<string>();
    public ObservableList<string> obsTestStringList = new ObservableList<string>();
    public System.Collections.Generic.List<State> enumList = new System.Collections.Generic.List<State>();
    public ObservableList<State> enumObsList = new ObservableList<State>();
    public List<List<int>> testNestedList = new List<List<int>>();
    public StringOffset? nullableStringOffset = null;

    /*endblock:rip*/

    protected override void OnConstruct() {
        base.OnConstruct();

/*block:newInstance*/        this./*name:name*/state/*endname*/ = new /*name:type*/State()/*endname*/;
/*endblock:newInstance*/

    }

    /// <summary>
    /// Copy values from other component. Shallow copy.
    /// </summary>
    /// <param name="target"></param>
    public override void CopyValues(IComponent target, bool initFromPrefab=false) {
        /*name:ComponentName*/GenTemplateComponent/*endname*/ component = (/*name:ComponentName*/GenTemplateComponent/*endname*/)target;
/*block:copyField*/        this./*name:name*/state/*endname*/ = component./*name:name*/state/*endname*/;
/*endblock:copyField*/
/*block:shallowCopyList*/        if (this./*name:name*/testListUID/*endname*/== null) {
                this./*name:name*/testListUID/*endname*/ = new /*name:type*/System.Collections.Generic.List<UID>/*endname*/(component./*name:name*/testListUID/*endname*/);
            } else {
                this./*name:name*/testListUID/*endname*/.Clear();
                this./*name:name*/testListUID/*endname*/.AddRange(component./*name:name*/testListUID/*endname*/);
            }
/*endblock:shallowCopy*/
/*block:shallowCopyDict*/       if (this./*name:name*/testDict/*endname*/== null) {
                this./*name:name*/testDict/*endname*/ = new /*name:type*/System.Collections.Generic.Dictionary<int, int>/*endname*/(component./*name:name*/testDict/*endname*/);
            } else {
                this./*name:name*/testDict/*endname*/.Clear();
                foreach (var pair in component./*name:name*/testDict/*endname*/) {
                    this./*name:name*/testDict/*endname*/[pair.Key] = pair.Value;
                }
            }
/*endblock:shallowCopyDict*/
/*block:shallowCopyFreeType*/       if (this./*name:name*/freeTypeTest/*endname*/== null) {
                this./*name:name*/freeTypeTest/*endname*/ = new /*name:type*/FreeTypeTestObject/*endname*/();
            }
            this./*name:name*/freeTypeTest/*endname*/.ShallowCopy(component./*name:name*/freeTypeTest/*endname*/);
/*endblock:shallowCopyFreeType*/
/*block:newInstanceCopy*/       if (this./*name:name*/testDict/*endname*/!= null && initFromPrefab) {
            this./*name:name*/testDict/*endname*/.Clear();
        } else {
            this./*name:name*/testDict/*endname*/ = component./*name:name*/testDict/*endname*/;
        }   
/*endblock:newInstanceCopy*/
    }

#if TESTING
    //public override void AssertSimpleFieldsEqual(IComponent _comp2) {
    //    UnityEngine.Assertions.Assert.IsTrue(_comp2 is  /*name:ComponentName*/GenTemplateComponent/*endname*/,$"_comp2.type!=/*name:ComponentName*/GenTemplateComponent/*endname*/:{_comp2.GetType()}");
    //    var comp2 = (/*name:ComponentName*/GenTemplateComponent/*endname*/)_comp2;
    //    /*block:simpleAssert*/UnityEngine.Assertions.Assert.AreEqual(/*name:name*/intValue/*endname*/,comp2./*name:name*/intValue/*endname*/);
    //    /*endblock:simpleAssert*/
    //}
#endif

    public override IComponent Clone(bool cloneFromPrefab=false) {
        var clone = new  /*name:ComponentName*/GenTemplateComponent/*endname*/();
        clone.CopyValues(this,cloneFromPrefab);
        return clone;
    }

    public override T Clone<T>(bool cloneFromPrefab = false) {
        return (T)Clone(cloneFromPrefab);
    }
/*block:serialization*/
    #region serialization
    public /*name:override*/override/*endname*/ int Serialize(FlatBuffers.FlatBufferBuilder builder) {
/*block:inheritanceSer*/        var baseData = base.Serialize(builder);
/*endblock:inheritanceSer*/
/*block:s_enum*/        var /*name|fu,pre#s:name*/sState/*endname*/ = (int/*name:nullable*//*endname*/)/*name:name*/state/*endname*/;
/*endblock:s_enum*/
/*block:s_string*/      var /*name|fu,pre#s:name*/sTestName/*endname*/ = FlatBufferSerializer.GetOrCreateSerialize(builder,/*name:name*/testName/*endname*/) ;
/*endblock:s_string*/
/*block:s_nonprim*/        var /*name|fu,pre#s:name*/sTestUID/*endname*/ = new Offset<Serial./*name|pre#FB:type*/FBUID/*endname*/>((int)FlatBufferSerializer.GetOrCreateSerialize(builder,/*name:name*/testUID/*endname*/)) ;
        /*endblock:s_nonprim*/
/*block:s_nonprim_typed*/        var /*name|fu,pre#s:name*/sTestTypedObject/*endname*/ = new Offset<Serial.FBRef>((int)FlatBufferSerializer.SerializeTypedObject(builder,/*name:name*/testUID/*endname*/)) ;
        /*endblock:s_nonprim_typed*/
        /*block:s_list_primitive*/        //var /*name|fu,pre#s:name*/sTestListPrimitive/*endname*/ = FlatbufferSerializer.CreateList(builder,/*name:name*/testListPrimitive/*endname*/, Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|fu,pre#Create,post#Vector:name*/CreateTestListPrimitiveVector/*endname*/) ;
        var /*name|fu,pre#s:name*/sTestListPrimitive/*endname*/ = FlatBufferSerializer.CreateManualList(builder,/*name:name*/testListPrimitive/*endname*/);
        /*endblock:s_list_primitive*/
        /*block:s_list_string*/
        var /*name|fu,pre#s:name*/sTestListString/*endname*/ = FlatBufferSerializer.CreateStringList(builder,/*name:name*/testStringList/*endname*/, Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|fu,pre#Create,post#Vector:name*/CreateTestStringListVector/*endname*/) ;
/*endblock:s_list_string*/
/*block:s_list_nonprim*/        //var /*name|fu,pre#s:name*/sTestListUID/*endname*/ = FlatbufferSerializer.CreateList</*name:innertype*/UID/*endname*/,Serial./*name:fbType*/FBUID/*endname*/>(builder,/*name:name*/testListUID/*endname*/, Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|fu,pre#Create,post#Vector:name*/CreateTestListUIDVector/*endname*/) ;
                var /*name|fu,pre#s:name*/sTestListUID/*endname*/ = FlatBufferSerializer.CreateManualList(builder,/*name:name*/testListUID/*endname*/);
        /*endblock:s_list_nonprim*/
/*block:s_list_nonprim_typed*/        //var /*name|fu,pre#s:name*/sTestListUID/*endname*/ = FlatbufferSerializer.CreateList</*name:innertype*/UID/*endname*/,Serial./*name:fbType*/FBUID/*endname*/>(builder,/*name:name*/testListUID/*endname*/, Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|fu,pre#Create,post#Vector:name*/CreateTestListUIDVector/*endname*/) ;
                var /*name|fu,pre#s:name*/sTestTypedListUID/*endname*/ = FlatBufferSerializer.CreateTypedList(builder,/*name:name*/testListUID/*endname*/);
        /*endblock:s_list_nonprim_typed*/
        /*block:s_list_enum*/
        var /*name|fu,pre#s:name*/sEnumList/*endname*/ = FlatBufferSerializer.CreateManualList(builder,/*name:name*/enumList/*endname*/) ;
/*endblock:s_list_enum*/
/*block:s_dictold*/ 
        var /*name|fu,pre#s:name*/sIntIntDict/*endname*/ = FlatBufferSerializer.CreateDictionary</*name:keyType*/int/*endname*/, /*name:valueType*/int/*endname*/, Serial./*name:serialDictType*/DTEST_int_int/*endname*/>(builder, /*name:name*/testDict/*endname*/, Serial./*name:serialDictType*/DTEST_int_int/*endname*/./*name|fu,pre#Create:serialDictType*/CreateDTEST_int_int/*endname*/, Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|fu,pre#Create,post#Vector:name*/CreateTestDictVector/*endname*/);
/*endblock:s_dictold*/
/*block:s_dict*/                var /*name|fu,pre#s:name*/sIntIntDict2/*endname*/ = FlatBufferSerializer.CreateDictionary</*name:keyType*/int/*endname*/, /*name:valueType*/int/*endname*/,/*name:fbKeyType*/int/*endname*/, /*name:fbValueType*/int/*endname*/, Serial./*name:serialDictType*/DTEST_int_int/*endname*/>(builder, /*name:dictName*/testDict/*endname*/, Serial./*name:serialDictType*/DTEST_int_int/*endname*/./*name|fu,pre#Create:serialDictType*/CreateDTEST_int_int/*endname*/, Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|fu,pre#Create,post#Vector:name*/CreateTestDictVector/*endname*/,true,/*name:keyTyped*/false/*endname*/,/*name:valueTyped*/false/*endname*/);
/*endblock:s_dict*/
        Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|pre#StartFB:ComponentName*/StartFBGenTemplateComponent/*endname*/(builder);
/*block:s2_default*/        /*block:nullcheck*/if (/*name|fu,pre#s:name*/sState/*endname*/!=null)/*endblock:nullcheck*/ Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|fu,pre#Add:name*/AddState/*endname*/(builder,/*name|fu,pre#s:name*/sState/*endname*/);
/*endblock:s2_default*/        
/*block:s2_nullable*/        if (/*name|fu,pre#s:name*/nullableStringOffset/*endname*/.HasValue) Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|fu,pre#Add:name*/AddTestName/*endname*/(builder,/*name|fu,pre#s:name*/nullableStringOffset/*endname*/.Value);
/*endblock:s2_nullable*/        
/*block:s2_primitive*/        Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|fu,pre#Add:name*/AddState/*endname*/(builder,/*name:name*/sState/*endname*/);
/*endblock:s2_primitive*/
/*block:s2_list*/        if (/*name:name*/testListPrimitive/*endname*/!= null) Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|fu,pre#Add:name*/AddTestListPrimitive/*endname*/(builder,(VectorOffset)/*name|fu,pre#s:name*/sTestListPrimitive/*endname*/);
/*endblock:s2_list*/
/*block:inheritanceSer2*/        Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/.AddBaseData(builder,new Offset<Serial./*name:basetype*/FBComponent/*endname*/>(baseData) );
/*endblock:inheritanceSer2*/

        return Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|pre#EndFB:ComponentName*/EndFBGenTemplateComponent/*endname*/(builder).Value;
    }

    public /*name:override*/override/*endname*/ void Deserialize(object data) {
        if (data is Serial.FBRef) {
            data = FlatBufferSerializer.CastSerialObject<Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/>(data);
        }
        var input = (Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/)data;
/*block:inheritance_deser*/        base.Deserialize(input.BaseData);
/*endblock:inheritance_deser*/        /*name:useManual*/var manual = FlatBufferSerializer.GetManualObject(data);/*endname*/

/*block:d_default*/        /*block:nullcheck*/if (input./*name|fu,post#BufferPosition:name*/StateBufferPosition/*endname*/!=0) /*endblock:nullcheck*//*name:name*/state/*endname*/ = (/*name:type*/State/*endname*/)input./*name|fu:name*/State/*endname*/; // string
/*endblock:d_default*/
/*block:d_nonprim*/         if (input./*name|fu:name*/TestUID/*endname*/!=null) /*name:name*/testUID/*endname*/ = FlatBufferSerializer.GetOrCreateDeserialize</*name:type*/UID/*endname*/>(input./*name|fu:name*/TestUID/*endname*/);
        /*endblock:d_nonprim*/
/*block:d_nonprim_typed*/         if (input./*name|fu:name*/TestUID/*endname*/!=null) /*name:name*/testUID/*endname*/ = FlatBufferSerializer.DeserializeTypedObject</*name:type*/UID/*endname*/>(input./*name|fu:name*/TestUID/*endname*/);
/*endblock:d_nonprim_typed*/
        /*block:d_prim_list*/ //        /*name:name*/testListPrimitive/*endname*/ = FlatbufferSerializer.DeserializeList</*name:innertype:*/int/*endname*/>(input./*name|fu,post#BufferPosition:name*/TestListPrimitiveBufferPosition/*endname*/,input./*name|fu,pre#Get,post#Array:name*/GetTestListPrimitiveArray/*endname*/);
                                      /*name:name*/testListPrimitive/*endname*/ = (/*name:type*/System.Collections.Generic.List<int>/*endname*/)manual.GetPrimitiveList</*name:innertype:*/int/*endname*/>(input./*name|fu,post#TableOffset:name*/TestListPrimitiveTableOffset/*endname*/,/*name:isObservable*/false/*endname*/);
        /*endblock:d_prim_list*/
        /*block:d_nonprim_list_typed*/
        if (input./*name|fu,post#BufferPosition:name*/TestListUIDBufferPosition/*endname*/!= 0) {
            if (/*name:name*/testListUID/*endname*/== null) {
                /*name:name*/testListUID/*endname*/ = new /*name:type*/System.Collections.Generic.List<UID>/*endname*/();
            } else {
                /*name:name*/testListUID/*endname*/.Clear();
            }
            /*name:name*/testListUID/*endname*/ = (/*name:type*/System.Collections.Generic.List<UID>/*endname*/)manual.GetTypedList</*name:innertype*/UID/*endname*/>(input./*name|fu,post#TableOffset:name*/TestListUIDTableOffset/*endname*/,/*name:name*/testListUID/*endname*/);
        }
/*endblock:d_nonprim_list_typed*/
/*block:d_nonprim_list*/
        {
            int size = input./*name|fu,post#Length:name*/TestListUIDLength/*endname*/;
            var tempList = FlatBufferSerializer.poolListObject.GetList(size); // first create List<object> of all results and then pass this to the Create-method. Didn't find a better way,yet Generics with T? do not work for interfaces
            for (int i=0;i< size; i++) tempList.Add(input./*name|fu:name*/TestListUID/*endname*/(i));
            /*name:name*/testListUID/*endname*/ = (/*name:type*/ System.Collections.Generic.List<UID>/*endname*/)FlatBufferSerializer.DeserializeList</*name:innertype*/UID/*endname*/,Serial./*name:fbType*/FBUID/*endname*/>(input./*name|fu,post#BufferPosition:name*/TestListUIDBufferPosition/*endname*/, input./*name|fu,post#Length:name*/TestListUIDLength/*endname*/,tempList,null,/*name:isObservable*/false/*endname*/);
            FlatBufferSerializer.poolListObject.Release(tempList);
        }
        /*endblock:d_nonprim_list*/
        /*block:d_enum_list*/
        {
            var arrayData = input./*name|fu,pre#Get,post#Array:name*/GetTestListPrimitiveArray/*endname*/();
            if (arrayData != null) /*name:name*/enumList/*endname*/ = arrayData.Cast</*name:innertype*/State/*endname*/>().ToList();
        }
        /*endblock:d_enum_list*/
        /*block:d_enum_obs_list*/
        {
            var arrayData = input./*name|fu,pre#Get,post#Array:name*/GetTestListPrimitiveArray/*endname*/();
            
            if (arrayData != null) /*name:name*/enumObsList/*endname*/ = new ObservableList</*name:innertype*/State/*endname*/>(arrayData.Cast</*name:innertype*/State/*endname*/>().ToList());
        } 
/*endblock:d_enum_obs_list*/  
/*block:d_string_list*/     {
            /*name:name*/testStringList/*endname*/ = new System.Collections.Generic.List<string>(); // first create List<object> of all results and then pass this to the Create-method. Didn't find a better way,yet Generics with T? do not work for interfaces
            for (int i=0;i<input./*name|fu,post#Length:name*/TestListUIDLength/*endname*/; i++) /*name:name*/testStringList/*endname*/.Add(input./*name|fu:name*/TestStringList/*endname*/(i));
        }
/*endblock:d_string_list*/
/*block:d_dict*/        
        {
            //            object result = FlatBufferSerializer.FindInDeserializeCache</*name:dictType*/Dictionary/*endname*/</*name:keyType*/int/*endname*/, /*name:valueType*/int/*endname*/>>(input./*name|fu,post#BufferPosition:name*/TestDictNonPrimBufferPosition/*endname*/);
            object result = null;
            if (result != null) {
                /*name:name*/testDict/*endname*/ = (/*name:dictType*/System.Collections.Generic.Dictionary/*endname*/</*name:keyType*/int/*endname*/, /*name:valueType*/int/*endname*/>)result;
            } else {
                /*name:name*/testDict/*endname*/ = new /*name:dictType*/System.Collections.Generic.Dictionary/*endname*/</*name:keyType*/int/*endname*/, /*name:valueType*/int/*endname*/>();
                var innerManual = FlatBufferSerializer.GetManualObject(data);
                for (int i = 0; i < input./*name|fu,post#Length:name*/TestDictLength/*endname*/; i++) {
                    var e = input./*name|fu:name*/TestDictNonPrim/*endname*/(i);
                    if (e.HasValue) {
                        var elem = e.Value;
                        innerManual.__initFromRef(elem);
                        /*block:nonprim_key*/
                        var key = FlatBufferSerializer.GetOrCreateDeserialize</*name:keyType*/SerializableHelper/*endname*/>((Serial./*name:fbKeyType*/FBComponent/*endname*/)elem.Key);
                        /*endblock:nonprim_key*/ 
                        /*block:nonprim_value*/
                        var value = FlatBufferSerializer.GetOrCreateDeserialize</*name:valueType*/SerializableHelper/*endname*/>((Serial./*name:fbValueType*/FBComponent/*endname*/)elem.Value);
                        /*endblock:nonprim_value*/ 
                        /*block:rip*/var elem2 = new Serial.DTEST_intlinst_intlist();/*endblock:rip*/
                        /*block:list_key*/var /*name:valueName*/key2/*endname*/ = innerManual.GetPrimitiveList</*name:listType*/int/*endname*/>(0);
                        /*endblock:list_key*/
                        /*block:list_value*/var /*name:valueName*/value2/*endname*/ = innerManual.GetPrimitiveList</*name:listType*/int/*endname*/>(1);
                        /*endblock:list_value*/
                        /*block:nonprim_list_key*/var /*name:keyName*/key3/*endname*/ = innerManual.GetNonPrimList<Serial./*name:fbKeyType*/FBComponent/*endname*/,/*name:keyType*/SerializableHelper/*endname*/>(0);
                        /*endblock:nonprim_list_key*/
                        /*block:nonprim_list_value*/        
                        var /*name:valueName*/value3/*endname*/ = innerManual.GetNonPrimList<Serial./*name:fbValueType*/FBComponent/*endname*/,/*name:valueType*/SerializableHelper/*endname*/>(1);
                        /*endblock:nonprim_list_value*/
                        /*block:nonprim_obs_list_key*/var /*name:keyName*/key4/*endname*/ = new ObservableList</*name:keyType*/SerializableHelper/*endname*/>((System.Collections.Generic.List</*name:keyType*/SerializableHelper/*endname*/>)innerManual.GetNonPrimList<Serial./*name:fbKeyType*/FBComponent/*endname*/,/*name:keyType*/SerializableHelper/*endname*/>(0));
                        /*endblock:nonprim_obs_list_key*/
                        /*block:nonprim_obs_list_value*/var /*name:valueName*/value4/*endname*/ = new ObservableList</*name:valueType*/SerializableHelper/*endname*/>((System.Collections.Generic.List</*name:valueType*/SerializableHelper/*endname*/>)innerManual.GetNonPrimList<Serial./*name:fbValueType*/FBComponent/*endname*/,/*name:valueType*/SerializableHelper/*endname*/>(1));
                        /*endblock:nonprim_obs_list_value*/
                        /*block:nonprim_key_typed_object*/
                        var /*name:keyName*/key5/*endname*/ = innerManual.GetTypedObject</*name:keyType*/SerializableHelper/*endname*/>(0);
                        /*endblock:nonprim_key_typed_object*/
                        /*block:nonprim_value_typed_object*/
                        var /*name:valueName*/value5/*endname*/ = innerManual.GetTypedObject</*name:valueType*/SerializableHelper/*endname*/>(1);
                        /*endblock:nonprim_value_typed_object*/
                        /*name:name*/
                        testDict2/*endname*/[(/*name:keyType*/SerializableHelper/*endname*/)/*name:thekey*/key/*endname*/] = (/*name:valueType*/SerializableHelper/*endname*/)/*name:thevalue*/value/*endname*/;
                    }
                }
                //FlatBufferSerializer.PutIntoDeserializeCache(input./*name|fu,post#BufferPosition:name*/TestDictNonPrimBufferPosition/*endname*/, /*name:name*/testDict/*endname*/);
            }
        } 
/*endblock:d_dict*/
    }

    public /*name:override*/override/*endname*/ void Deserialize(FlatBuffers.ByteBuffer buf) {
        var fbSettlerDataComponent = Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|pre#GetRootAsFB:ComponentName*/GetRootAsFBGenTemplateComponent/*endname*/(buf);
        Deserialize(fbSettlerDataComponent);
    }
    #endregion
    /*endblock:serialization*/


    /*block:serialization2*/
    #region serialization2
    public  override void Ser2CreateTable(SerializationContext ctx, FlatBuffers.FlatBufferBuilder builder) {
        /*block:s_inheritance_offset*/
        base.Ser2CreateTable(ctx, builder);
        int offsetBase = Ser2Offset;
        /*endblock:s_inheritance_offset*/
        /*block:s_create_string*/
        int /*name|fu,pre#offset:name*/offsetString/*endname*/ = /*name:name*/testName/*endname*/==null ? 0 : builder.CreateString(/*name:name*/testName/*endname*/).Value;
        /*endblock:s_create_string*/
        /*block:s_list_primitive*/int /*name|fu,pre#offset:name*/offsetTestListPrimitive/*endname*/ = /*name:name*/testListPrimitive/*endname*/==null ? 0 : ctx.builder.CreatePrimitiveList(/*name:name*/testListPrimitive/*endname*/);
        /*endblock:s_list_primitive*/
        /*block:s_list_struct*/int /*name|fu,pre#offset:name*/offsetStructList/*endname*/ = /*name:name*/structList/*endname*/==null ? 0 : ctx.builder.CreateStructList(/*name:name*/testListPrimitive/*endname*/);
        /*endblock:s_list_struct*/
        /*block:s_obs_list_struct*/int /*name|fu,pre#offset:name*/offsetObsStructList/*endname*/ = /*name:name*/obsStructList/*endname*/==null ? 0 : ctx.builder.CreateStructList(/*name:name*/obsStructList/*endname*/.__innerList);
        /*endblock:s_obs_list_struct*/
        /*block:s_obs_list_primitive*/int /*name|fu,pre#offset:name*/offsetTestObsListPrimitive/*endname*/ = /*name:name*/testObsListPrimitive/*endname*/==null ? 0 : ctx.builder.CreatePrimitiveList(/*name:name*/testObsListPrimitive/*endname*/.__innerList);
        /*endblock:s_list_primitive*/
        /*block:s_list_non_primitive*/int /*name|fu,pre#offset:name*/offsetTestListNonPrimitive/*endname*/ =/*name:name*/objectList/*endname*/== null ? 0 : ctx.builder.CreateNonPrimitiveList(/*name:name*/objectList/*endname*/, ctx);
        /*endblock:s_list_non_primitive*/
        /*block:s_obs_list_non_primitive*/int /*name|fu,pre#offset:name*/offsetTestObsListNonPrimitive/*endname*/ = /*name:name*/obsObjectList/*endname*/== null ? 0 : ctx.builder.CreateNonPrimitiveList(/*name:name*/obsObjectList/*endname*/.__innerList,ctx);
        /*endblock:s_list_non_primitive*/
        /*block:s_list_string*/int /*name|fu,pre#offset:name*/offsetStringList/*endname*/ =/*name:name*/testStringList/*endname*/== null ? 0 : ctx.builder.CreateStringList(/*name:name*/testStringList/*endname*/);
        /*endblock:s_list_string*/
        /*block:s_obs_list_string*/int /*name|fu,pre#offset:name*/offsetObsStringList/*endname*/ = /*name:name*/obsTestStringList/*endname*/== null ? 0 : ctx.builder.CreateStringList(/*name:name*/obsTestStringList/*endname*/.__innerList);
        /*endblock:s_obs_list_string*/
        /*block:s_dict*/int /*name|fu,pre#offset:name*/offsetTestDict/*endname*/ =/*name:name*/testDict2/*endname*/== null ? 0 : ctx.builder.CreateDictionary(/*name:name*/testDict2/*endname*/, ctx);
        /*endblock:s_dict*/

        builder.StartTable(/*name:fieldamount*/10/*endname*/);
        /*block:s_component_header*/builder.AddStruct(0, builder.PutUID(Entity), 0);
        builder.AddStruct(1, builder.PutUID(ID), 0);
        /*endblock:s_component_header*/
        /*block:s_add_offset*/
        builder.AddOffset(/*name:fieldid*/1/*endname*/,/*name|fu,pre#offset:name*/offsetString/*endname*/,0);
        /*endblock:s_add_offset*/
        /*block:s_enum*/builder.AddInt(/*name:fieldid*/2/*endname*/, (int)/*name:name*/state/*endname*/, 0); 
        /*endblock:s_enum*/
        /*block:s_enumnullable*/if (/*name:name*/nullState/*endname*/.HasValue) builder.AddInt(/*name:fieldid*/3/*endname*/, (int)/*name:name*/nullState/*endname*/.Value, 0);
        /*endblock:s_enumnullable*/
        /*block:s2_primitive*/builder./*name:addPrimitive*/AddInt/*endname*/(/*name:fieldid*/4/*endname*/,/*name:name*/intValue/*endname*/, 0);
        /*endblock:s2_primitive*/
        /*block:s_typed_object*/ctx.AddReferenceOffset(/*name:fieldid*/5/*endname*/,/*name:name*/typedObj1/*endname*/);
        /*endblock:s_typed_object*/
        /*block:s_object*/ctx.AddReferenceOffset(/*name:fieldid*/6/*endname*/,/*name:name*/serObj/*endname*/);
        /*endblock:s_object*/
        /*block:s_special_object*/builder.AddStruct(/*name:fieldid*/8/*endname*/,builder./*name|pre#Put:type*/PutUID/*endname*/(ref /*name:name*/testUID/*endname*/),0);
        /*endblock:s_special_object*/
        /*block:s_special_object_nullable*/if (/*name:name*/testUIDnullable/*endname*/.HasValue) {
            builder.AddStruct(/*name:fieldid*/9/*endname*/,builder./*name|pre#Put:type*/PutUID/*endname*/(/*name:name*/testUIDnullable/*endname*/.Value),0);
        } else {
            builder.AddStruct(/*name:fieldid*/9/*endname*/, 0, 0);
        }
        /*endblock:s_special_object_nullable*/
        /*block:s_struct*/builder.AddStruct(/*name:fieldid*/10/*endname*/, /*name:name*/serStruct/*endname*/.Put(builder),0);
        /*endblock:s_struct*/
        /*block:s_ref_offset*/ctx.AddReferenceOffset(/*name:fieldid*/6/*endname*/,/*name:name*/serObj/*endname*/);
        /*endblock:s_ref_offset*/

        int tblPos = builder.EndTable();
        ser2table = new ExtendedTable(tblPos, builder);
        //int sState = 0;

        ///*block:inheritanceSer*/
        //var baseData = base.Ser2Serialize(ctx);
        ///*endblock:inheritanceSer*/
        ///*block:s_string*/
        //var /*name|fu,pre#s:name*/sTestName/*endname*/ = FlatBufferSerializer.GetOrCreateSerialize(builder,/*name:name*/testName/*endname*/);
        ///*endblock:s_string*/
        ///*block:s_nonprim*/
        //var /*name|fu,pre#s:name*/sTestUID/*endname*/ = new Offset<Serial./*name|pre#FB:type*/FBUID/*endname*/>((int)FlatBufferSerializer.GetOrCreateSerialize(builder,/*name:name*/testUID/*endname*/));
        ///*endblock:s_nonprim*/
        ///*block:s_nonprim_typed*/
        //var /*name|fu,pre#s:name*/sTestTypedObject/*endname*/ = new Offset<Serial.FBRef>((int)FlatBufferSerializer.SerializeTypedObject(builder,/*name:name*/testUID/*endname*/));
        ///*endblock:s_nonprim_typed*/
        ///*block:s_list_nonprim*/        //var /*name|fu,pre#s:name*/sTestListUID/*endname*/ = FlatbufferSerializer.CreateList</*name:innertype*/UID/*endname*/,Serial./*name:fbType*/FBUID/*endname*/>(builder,/*name:name*/testListUID/*endname*/, Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|fu,pre#Create,post#Vector:name*/CreateTestListUIDVector/*endname*/) ;
        //var /*name|fu,pre#s:name*/sTestListUID/*endname*/ = FlatBufferSerializer.CreateManualList(builder,/*name:name*/testListUID/*endname*/);
        ///*endblock:s_list_nonprim*/
        ///*block:s_list_nonprim_typed*/        //var /*name|fu,pre#s:name*/sTestListUID/*endname*/ = FlatbufferSerializer.CreateList</*name:innertype*/UID/*endname*/,Serial./*name:fbType*/FBUID/*endname*/>(builder,/*name:name*/testListUID/*endname*/, Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|fu,pre#Create,post#Vector:name*/CreateTestListUIDVector/*endname*/) ;
        //var /*name|fu,pre#s:name*/sTestTypedListUID/*endname*/ = FlatBufferSerializer.CreateTypedList(builder,/*name:name*/testListUID/*endname*/);
        ///*endblock:s_list_nonprim_typed*/
        ///*block:s_list_enum*/
        //var /*name|fu,pre#s:name*/sEnumList/*endname*/ = FlatBufferSerializer.CreateManualList(builder,/*name:name*/enumList/*endname*/);
        ///*endblock:s_list_enum*/
        ///*block:s_dictold*/
        //var /*name|fu,pre#s:name*/sIntIntDict/*endname*/ = FlatBufferSerializer.CreateDictionary</*name:keyType*/int/*endname*/, /*name:valueType*/int/*endname*/, Serial./*name:serialDictType*/DTEST_int_int/*endname*/>(builder, /*name:name*/testDict/*endname*/, Serial./*name:serialDictType*/DTEST_int_int/*endname*/./*name|fu,pre#Create:serialDictType*/CreateDTEST_int_int/*endname*/, Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|fu,pre#Create,post#Vector:name*/CreateTestDictVector/*endname*/);
        ///*endblock:s_dictold*/
        //Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|pre#StartFB:ComponentName*/StartFBGenTemplateComponent/*endname*/(builder);
        ///*block:s2_default*/        /*block:nullcheck*/
        //if (/*name|fu,pre#s:name*/sState/*endname*/!= null)/*endblock:nullcheck*/ Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|fu,pre#Add:name*/AddState/*endname*/(builder,/*name|fu,pre#s:name*/sState/*endname*/);
        ///*endblock:s2_default*/
        ///*block:s2_nullable*/
        //if (/*name|fu,pre#s:name*/nullableStringOffset/*endname*/.HasValue) Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|fu,pre#Add:name*/AddTestName/*endname*/(builder,/*name|fu,pre#s:name*/nullableStringOffset/*endname*/.Value);
        ///*endblock:s2_nullable*/

        ///*block:s2_list*/
        ////if (/*name:name*/testListPrimitive/*endname*/!= null) Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|fu,pre#Add:name*/AddTestListPrimitive/*endname*/(builder, (VectorOffset)/*name|fu,pre#s:name*/sTestListPrimitive/*endname*/);
        ///*endblock:s2_list*/
        ///*block:inheritanceSer2*/
        //Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/.AddBaseData(builder, new Offset<Serial./*name:basetype*/FBComponent/*endname*/>(baseData));
        ///*endblock:inheritanceSer2*/


    }


    public new void Ser2UpdateTable(SerializationContext ctx, FlatBufferBuilder builder) {
        // not implemented,yet
    }
    public override void Ser2Deserialize(int tblOffset, DeserializationContext dctx) {
        /*block:d_deserialize_clazz_header*/ser2table = new ExtendedTable(tblOffset, dctx.bb);
        /*endblock:d_deserialize_clazz_header*/
        /*block:d_component_header*/
        base.Ser2Deserialize(tblOffset, dctx); // init extendend table;
        Entity = ser2table.GetUID(0);
        ID = ser2table.GetUID(1);
        /*endblock:d_component_header*/

        /*block:d_deserialize_base*/
        base.Ser2Deserialize(ser2table.GetOffset(0), dctx);
        ser2table = new ExtendedTable(tblOffset, dctx.bb);
        /*endblock:d_deserialize_base*/
        /*block:dser_master*/
        /*name:pps*/ dctx.AddToPostDeserializeAction(() => { /*endname*/
        /*block:d_enum*//*name:name*/state/*endname*/ = (/*name:type*/State/*endname*/)ser2table.GetInt(/*name:fieldid*/0/*endname*/);
        /*endblock:d_enum*/
        /*block:d_enum_nullable*//*name:name*/nullState/*endname*/ = (/*name:type*/State/*endname*/?)ser2table.GetNullableInt(/*name:fieldid*/0/*endname*/);
        /*endblock:d_enum_nullable*/
        /*block:d_primitive*//*name:name*/intValue/*endname*/ = ser2table./*name:getPrimitive*/GetInt/*endname*/(/*name:fieldid*/0/*endname*/);
        /*endblock:d_primitive*/
        /*block:d_string*//*name:name*/testName/*endname*/ = ser2table.GetString(/*name:fieldid*/0/*endname*/);
        /*endblock:d_string*/
        /*block:d_typed_object*//*name:name*/typedObj1/*endname*/ = ser2table.GetReference</*name:type*/DefaultSerializable2/*endname*/> (/*name:fieldid*/0/*endname*/,dctx);
        /*endblock:d_typed_object*/
        /*block:d_object*//*name:name*/serObj/*endname*/ = ser2table.GetReference</*name:type*/SomeClazz2/*endname*/>(/*name:fieldid*/0/*endname*/,dctx);
        /*endblock:d_object*/
        /*block:d_special_object*/ser2table./*name|pre#Get:type*/GetUID/*endname*/(/*name:fieldid*/8/*endname*/,ref /*name:name*/testUID/*endname*/);
        /*endblock:d_special_object*/
        /*block:d_special_object_nullable*/ if (ser2table.GetOffset(/*name:fieldid*/9/*endname*/)==0) {
            /*name:name*/testUIDnullable/*endname*/=null;
            } else {
                /*name:name*/testUIDnullable/*endname*/ = ser2table./*name|pre#Get:type*/GetUID/*endname*/(/*name:fieldid*/9/*endname*/);
            }
        /*endblock:d_special_object_nullable*/
        /*block:d_struct*/ser2table.GetStruct(/*name:fieldid*/8/*endname*/, ref /*name:name*/serStruct/*endname*/);
        /*endblock:d_struct*/
        /*block:d_list_primitive*//*name:name*/testListPrimitive/*endname*/=null;ser2table.GetPrimitiveList</*name:innertype*/int/*endname*/>(/*name:fieldid*/9/*endname*/, ref /*name:name*/testListPrimitive/*endname*/);
        /*endblock:d_list_primitive*/
        /*block:d_list_object*//*name:name*/objectList/*endname*/=null;ser2table.GetReference(/*name:fieldid*/9/*endname*/, ref /*name:name*/objectList/*endname*/,dctx);
        /*endblock:d_list_object*/
        /*block:d_list_struct*//*name:name*/structList/*endname*/=null;ser2table.GetStructList(/*name:fieldid*/9/*endname*/, ref /*name:name*/structList/*endname*/);
        /*endblock:d_list_struct*/
        /*block:d_list_string*//*name:name*/testStringList/*endname*/=null;ser2table.GetStringList(/*name:fieldid*/9/*endname*/, ref /*name:name*/testStringList/*endname*/);
        /*endblock:d_list_string*/
        /*block:d_list_nested*//*name:name*/testNestedList/*endname*/=null;ser2table.GetList</*name:innertype*/List<int>/*endname*/>(/*name:fieldid*/9/*endname*/, ref /*name:name*/testNestedList/*endname*/, dctx);
        /*endblock:d_list_nested*/
        /*block:d_dict*//*name:name*/testDict/*endname*/=null;ser2table.GetDictionary(/*name:fieldid*/9/*endname*/, ref /*name:name*/testDict/*endname*/, dctx);
        /*endblock:d_dict*/
        /*block:d_ref_offset*/ser2table.GetReference(/*name:fieldid*/9/*endname*/, ref /*name:name*/objectList/*endname*/,dctx);
        /*endblock:d_ref_offset*/
        /*name:ppe*/ }); /*endname*/
        /*endblock:dser_master*/
        //if (this is IFBPostDeserialization) {
        //    dctx.AddOnPostDeserializationObject((IFBPostDeserialization)this);
        //}

        //object data = null;
        //if (data is Serial.FBRef) {
        //    data = FlatBufferSerializer.CastSerialObject<Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/>(data);
        //}
        //var input = (Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/)data;
        ///*block:inheritance_deser*/
        //base.Deserialize(input.BaseData);
        ///*endblock:inheritance_deser*/        /*name:useManual*/
        //var manual = FlatBufferSerializer.GetManualObject(data);/*endname*/

        ///*block:d_default*/        /*block:nullcheck*/
        //if (input./*name|fu,post#BufferPosition:name*/StateBufferPosition/*endname*/!= 0) /*endblock:nullcheck*//*name:name*/state/*endname*/ = (/*name:type*/State/*endname*/)input./*name|fu:name*/State/*endname*/; // string
        ///*endblock:d_default*/
        ///*block:d_nonprim*/
        //if (input./*name|fu:name*/TestUID/*endname*/!= null) /*name:name*/testUID/*endname*/ = FlatBufferSerializer.GetOrCreateDeserialize</*name:type*/UID/*endname*/>(input./*name|fu:name*/TestUID/*endname*/);
        ///*endblock:d_nonprim*/
        ///*block:d_nonprim_typed*/
        //if (input./*name|fu:name*/TestUID/*endname*/!= null) /*name:name*/testUID/*endname*/ = FlatBufferSerializer.DeserializeTypedObject</*name:type*/UID/*endname*/>(input./*name|fu:name*/TestUID/*endname*/);
        ///*endblock:d_nonprim_typed*/
        ///*block:d_prim_list*/ //        /*name:name*/testListPrimitive/*endname*/ = FlatbufferSerializer.DeserializeList</*name:innertype:*/int/*endname*/>(input./*name|fu,post#BufferPosition:name*/TestListPrimitiveBufferPosition/*endname*/,input./*name|fu,pre#Get,post#Array:name*/GetTestListPrimitiveArray/*endname*/);
        ///*name:name*/
        //testListPrimitive/*endname*/ = (/*name:type*/System.Collections.Generic.List<int>/*endname*/)manual.GetPrimitiveList</*name:innertype:*/int/*endname*/>(input./*name|fu,post#TableOffset:name*/TestListPrimitiveTableOffset/*endname*/,/*name:isObservable*/false/*endname*/);
        ///*endblock:d_prim_list*/
        ///*block:d_nonprim_list_typed*/
        //if (input./*name|fu,post#BufferPosition:name*/TestListUIDBufferPosition/*endname*/!= 0) {
        //    if (/*name:name*/testListUID/*endname*/== null) {
        //        /*name:name*/
        //        testListUID/*endname*/ = new /*name:type*/System.Collections.Generic.List<UID>/*endname*/();
        //    } else {
        //        /*name:name*/
        //        testListUID/*endname*/.Clear();
        //    }
        //    /*name:name*/
        //    testListUID/*endname*/ = (/*name:type*/System.Collections.Generic.List<UID>/*endname*/)manual.GetTypedList</*name:innertype*/UID/*endname*/>(input./*name|fu,post#TableOffset:name*/TestListUIDTableOffset/*endname*/,/*name:name*/testListUID/*endname*/);
        //}
        ///*endblock:d_nonprim_list_typed*/
        ///*block:d_nonprim_list*/
        //{
        //    int size = input./*name|fu,post#Length:name*/TestListUIDLength/*endname*/;
        //    var tempList = FlatBufferSerializer.poolListObject.GetList(size); // first create List<object> of all results and then pass this to the Create-method. Didn't find a better way,yet Generics with T? do not work for interfaces
        //    for (int i = 0; i < size; i++) tempList.Add(input./*name|fu:name*/TestListUID/*endname*/(i));
        //    /*name:name*/
        //    testListUID/*endname*/ = (/*name:type*/ System.Collections.Generic.List<UID>/*endname*/)FlatBufferSerializer.DeserializeList</*name:innertype*/UID/*endname*/, Serial./*name:fbType*/FBUID/*endname*/>(input./*name|fu,post#BufferPosition:name*/TestListUIDBufferPosition/*endname*/, input./*name|fu,post#Length:name*/TestListUIDLength/*endname*/, tempList, null,/*name:isObservable*/false/*endname*/);
        //    FlatBufferSerializer.poolListObject.Release(tempList);
        //}
        ///*endblock:d_nonprim_list*/
        ///*block:d_enum_list*/
        //{
        //    var arrayData = input./*name|fu,pre#Get,post#Array:name*/GetTestListPrimitiveArray/*endname*/();
        //    if (arrayData != null) /*name:name*/enumList/*endname*/ = arrayData.Cast</*name:innertype*/State/*endname*/>().ToList();
        //}
        ///*endblock:d_enum_list*/
        ///*block:d_enum_obs_list*/
        //{
        //    var arrayData = input./*name|fu,pre#Get,post#Array:name*/GetTestListPrimitiveArray/*endname*/();

        //    if (arrayData != null) /*name:name*/enumObsList/*endname*/ = new ObservableList</*name:innertype*/State/*endname*/>(arrayData.Cast</*name:innertype*/State/*endname*/>().ToList());
        //}
        ///*endblock:d_enum_obs_list*/
        ///*block:d_string_list*/
        //{
        //    /*name:name*/
        //    testStringList/*endname*/ = new System.Collections.Generic.List<string>(); // first create List<object> of all results and then pass this to the Create-method. Didn't find a better way,yet Generics with T? do not work for interfaces
        //    for (int i = 0; i < input./*name|fu,post#Length:name*/TestListUIDLength/*endname*/; i++) /*name:name*/testStringList/*endname*/.Add(input./*name|fu:name*/TestStringList/*endname*/(i));
        //}
        ///*endblock:d_string_list*/
    }



    #endregion
    /*endblock:serialization2*/

}


/*block:rip*/
[System.Serializable]
public class GenTemplateComponent2 : ECS.Component
{
    public float time;

//#if TESTING
//    public override void AssertSimpleFieldsEqual(IComponent _comp2) {
//        throw new System.NotImplementedException();
//    }
//#endif
    public override IComponent Clone(bool cloneFromPrefab=false) {
        throw new System.NotImplementedException();
    }

    public override T Clone<T>(bool cloneFromPrefab = false) {
        throw new System.NotImplementedException();
    }

    public override void CopyValues(IComponent target, bool initFromPrefab = false) {
        throw new System.NotImplementedException();
    }

    public override void Ser2CreateTable(SerializationContext ctx, FlatBufferBuilder builder) {
        throw new System.NotImplementedException();
    }

    public override void Ser2UpdateTable(SerializationContext ctx, FlatBufferBuilder builder) {
        throw new System.NotImplementedException();
    }
}

[System.Serializable]
public partial class SerializableHelper : IFBSerializable {

    public float getBackResourceFactor = 0.5f;

    public float destructionSpeedUp = 2.0f;

    public void Deserialize(object incoming) {
        throw new System.NotImplementedException();
    }

    public void Deserialize(ByteBuffer buf) {
        throw new System.NotImplementedException();
    }

    public int Serialize(FlatBufferBuilder builder) {
        throw new System.NotImplementedException();
    }
}
/*endblock:rip*/
