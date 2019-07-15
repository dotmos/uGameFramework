
using ECS;
using FlatBuffers;
using Service.Serializer;
using System.Collections.Generic;
using System.Linq;
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

    /*block:modelClass*/
    [System.Serializable]
    public partial class /*name:className*/SomeModel/*endname*//*name:inheritance*//*endname*/ {
        /*block:field*/
            /// <summary>
            /// /*name:documentation*//*endname*/
            /// </summary>
        /*name:scope*/
        public/*endname*/ /*name:type*/string/*endname*/ /*name:name*/name/*endname*/ /*block:valueBlock*/= /*name:value*/"value"/*endname*//*endblock:valueBlock*/;
        /*endblock:field*/
        /*block:property*/
            /// <summary>
            /// /*name:documentation*//*endname*/
            /// </summary>
        /*name:scope*/public/*endname*/ /*name:type*/int/*endname*/ /*name:name*/MaxSoundChannels/*endname*/{/*name:getter*/get;/*endname*//*name:setter*/set;/*endname*/}
        /*endblock:property*/
        /*block:constructor*/
        /// <summary>
        /// /*name:documentation*//*endname*/
        /// </summary>
/*block:docParam*/        /// <param name="/*name:name*//*endname*/">/*name:documentation*//*endname*/</param>
/*endblock:docParam*/
        public /*name:className*/SomeModel/*endname*/(/*block:rip*/int maxChannels/*endblock:rip*//*block:parameter*//*name:comma*/,/*endname*//*name:type*/string/*endname*/ /*name:paramName*/name/*endname*//*endblock:parameter*/) {
/*block:constructorSet*/            this./*name:name*/name/*endname*/ = /*name:paramName*/name/*endname*/;
/*endblock:constructorSet*/
/*block:rip*/            this.MaxSoundChannels = maxChannels;/*endblock:rip*/
        }

        /*endblock:constructor*/
        /// <summary>
        /// Default constructor
        /// </summary>
        public /*name:className*/SomeModel/*endname*/() { }

        /*name:classSerialization*/
        public void Deserialize(int dataFormatNr,object incoming) {
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

    /*block:field*/
    /// <summary>
    /*name:comment*//*endname*/
                    /// </summary>
    /*name:attributes*//*endname*/
    /*name:accessor*/public/*endname*/ /*name:type*/State/*endname*/ /*name:name*/state/*endname*/ /*name:value*/= State.state1/*endname*/;
    /*endblock:field*/
    /*block:rip*/
    public string testName = "f95";
    public UID? testUID = new UID(1895);
    public float testNumber = 18.95f;

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

    public System.Collections.Generic.List<UID> testListUID = new System.Collections.Generic.List<UID>();
    public System.Collections.Generic.List<int> testListPrimitive = new System.Collections.Generic.List<int>();
    public System.Collections.Generic.Dictionary<int, int> testDict = new System.Collections.Generic.Dictionary<int, int>();
    public System.Collections.Generic.Dictionary<SerializableHelper, SerializableHelper> testDict2 = new System.Collections.Generic.Dictionary<SerializableHelper, SerializableHelper>();
    public System.Collections.Generic.List<string> testStringList = new System.Collections.Generic.List<string>();
    public List<State> enumList = new List<State>();
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

    public override IComponent Clone(bool cloneFromPrefab=false) {
        var clone = new  /*name:ComponentName*/GenTemplateComponent/*endname*/();
        clone.CopyValues(this,cloneFromPrefab);
        return clone;
    }
/*block:serialization*/
    #region serialization
    public /*name:override*/override/*endname*/ int Serialize(FlatBuffers.FlatBufferBuilder builder) {
/*block:inheritanceSer*/        var baseData = base.Serialize(builder);
/*endblock:inheritanceSer*/
/*block:s_enum*/        var /*name|fu,pre#s:name*/sState/*endname*/ = (int)/*name:name*/state/*endname*/;
/*endblock:s_enum*/
/*block:s_string*/      var /*name|fu,pre#s:name*/sTestName/*endname*/ = FlatBufferSerializer.GetOrCreateSerialize(builder,/*name:name*/testName/*endname*/) ;
/*endblock:s_string*/
/*block:s_nonprim*/        var /*name|fu,pre#s:name*/sTestUID/*endname*/ = new Offset<Serial./*name|pre#FB:type*/FBUID/*endname*/>((int)FlatBufferSerializer.GetOrCreateSerialize(builder,/*name:name*/testUID/*endname*/)) ;
        /*endblock:s_nonprim*/
        /*block:s_list_primitive*/        //var /*name|fu,pre#s:name*/sTestListPrimitive/*endname*/ = FlatbufferSerializer.CreateList(builder,/*name:name*/testListPrimitive/*endname*/, Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|fu,pre#Create,post#Vector:name*/CreateTestListPrimitiveVector/*endname*/) ;
        var /*name|fu,pre#s:name*/sTestListPrimitive/*endname*/ = FlatBufferSerializer.CreateManualList(builder,/*name:name*/testListPrimitive/*endname*/);
        /*endblock:s_list_primitive*/
        /*block:s_list_string*/
        var /*name|fu,pre#s:name*/sTestListString/*endname*/ = FlatBufferSerializer.CreateStringList(builder,/*name:name*/testStringList/*endname*/, Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|fu,pre#Create,post#Vector:name*/CreateTestStringListVector/*endname*/) ;
/*endblock:s_list_string*/
/*block:s_list_nonprim*/        //var /*name|fu,pre#s:name*/sTestListUID/*endname*/ = FlatbufferSerializer.CreateList</*name:innertype*/UID/*endname*/,Serial./*name:fbType*/FBUID/*endname*/>(builder,/*name:name*/testListUID/*endname*/, Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|fu,pre#Create,post#Vector:name*/CreateTestListUIDVector/*endname*/) ;
                var /*name|fu,pre#s:name*/sTestListUID/*endname*/ = FlatBufferSerializer.CreateManualList(builder,/*name:name*/testListUID/*endname*/);
        /*endblock:s_list_nonprim*/
        /*block:s_list_enum*/
        var /*name|fu,pre#s:name*/sEnumList/*endname*/ = FlatBufferSerializer.CreateManualList(builder,/*name:name*/enumList/*endname*/) ;
/*endblock:s_list_enum*/
/*block:s_dictold*/ 
        var /*name|fu,pre#s:name*/sIntIntDict/*endname*/ = FlatBufferSerializer.CreateDictionary</*name:keyType*/int/*endname*/, /*name:valueType*/int/*endname*/, Serial./*name:serialDictType*/DTEST_int_int/*endname*/>(builder, /*name:name*/testDict/*endname*/, Serial./*name:serialDictType*/DTEST_int_int/*endname*/./*name|fu,pre#Create:serialDictType*/CreateDTEST_int_int/*endname*/, Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|fu,pre#Create,post#Vector:name*/CreateTestDictVector/*endname*/);
/*endblock:s_dictold*/
/*block:s_dict*/                var /*name|fu,pre#s:name*/sIntIntDict2/*endname*/ = FlatBufferSerializer.CreateDictionary</*name:keyType*/int/*endname*/, /*name:valueType*/int/*endname*/,/*name:fbKeyType*/int/*endname*/, /*name:fbValueType*/int/*endname*/, Serial./*name:serialDictType*/DTEST_int_int/*endname*/>(builder, /*name:dictName*/testDict/*endname*/, Serial./*name:serialDictType*/DTEST_int_int/*endname*/./*name|fu,pre#Create:serialDictType*/CreateDTEST_int_int/*endname*/, Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|fu,pre#Create,post#Vector:name*/CreateTestDictVector/*endname*/);
/*endblock:s_dict*/
        Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|pre#StartFB:ComponentName*/StartFBGenTemplateComponent/*endname*/(builder);
/*block:s2_default*/        Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/./*name|fu,pre#Add:name*/AddState/*endname*/(builder,/*name|fu,pre#s:name*/sState/*endname*/);
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
        var input = (Serial./*name|pre#FB:ComponentName*/FBGenTemplateComponent/*endname*/)data;
/*block:inheritance_deser*/        base.Deserialize(input.BaseData);
/*endblock:inheritance_deser*/        /*name:useManual*/var manual = FlatBufferSerializer.GetManualObject(data);/*endname*/

/*block:d_default*/        /*name:name*/state/*endname*/ = (/*name:type*/State/*endname*/)input./*name|fu:name*/State/*endname*/; // string
/*endblock:d_default*/
/*block:d_nonprim*/     /*name:name*/testUID/*endname*/ = FlatBufferSerializer.GetOrCreateDeserialize</*name:type*/UID/*endname*/>(input./*name|fu:name*/TestUID/*endname*/);
        /*endblock:d_nonprim*/
        /*block:d_prim_list*/ //        /*name:name*/testListPrimitive/*endname*/ = FlatbufferSerializer.DeserializeList</*name:innertype:*/int/*endname*/>(input./*name|fu,post#BufferPosition:name*/TestListPrimitiveBufferPosition/*endname*/,input./*name|fu,pre#Get,post#Array:name*/GetTestListPrimitiveArray/*endname*/);
                                      /*name:name*/testListPrimitive/*endname*/ = (/*name:type*/List<int>/*endname*/)manual.GetPrimitiveList</*name:innertype:*/int/*endname*/>(input./*name|fu,post#BufferPosition:name*/TestListPrimitiveBufferPosition/*endname*/);
                              /*endblock:d_prim_list*/
                              /*block:d_nonprim_list*/
        {
            var tempList = new System.Collections.Generic.List<object>(); // first create List<object> of all results and then pass this to the Create-method. Didn't find a better way,yet Generics with T? do not work for interfaces
            for (int i=0;i<input./*name|fu,post#Length:name*/TestListUIDLength/*endname*/; i++) tempList.Add(input./*name|fu:name*/TestListUID/*endname*/(i));
            /*name:name*/testListUID/*endname*/ = (/*name:type*/ System.Collections.Generic.List<UID>/*endname*/)FlatBufferSerializer.DeserializeList</*name:innertype*/UID/*endname*/,Serial./*name:fbType*/FBUID/*endname*/>(input./*name|fu,post#BufferPosition:name*/TestListUIDBufferPosition/*endname*/, input./*name|fu,post#Length:name*/TestListUIDLength/*endname*/,tempList,/*name:isObservable*/false/*endname*/);
        }
/*endblock:d_nonprim_list*/
/*block:d_enum_list*/            /*name:name*/enumList/*endname*/ = input./*name|fu,pre#Get,post#Array:name*/GetTestListPrimitiveArray/*endname*/().Cast</*name:innertype*/State/*endname*/>().ToList();
/*endblock:d_enum_list*/  
/*block:d_string_list*/     {
            /*name:name*/testStringList/*endname*/ = new System.Collections.Generic.List<string>(); // first create List<object> of all results and then pass this to the Create-method. Didn't find a better way,yet Generics with T? do not work for interfaces
            for (int i=0;i<input./*name|fu,post#Length:name*/TestListUIDLength/*endname*/; i++) /*name:name*/testStringList/*endname*/.Add(input./*name|fu:name*/TestStringList/*endname*/(i));
        }
/*endblock:d_string_list*/
/*block:d_dict*/        
        {
            object result = FlatBufferSerializer.FindInDeserializeCache(input./*name|fu,post#BufferPosition:name*/TestDictNonPrimBufferPosition/*endname*/);
            if (result != null) {
                /*name:name*/testDict/*endname*/ = (/*name:dictType*/Dictionary/*endname*/</*name:keyType*/int/*endname*/, /*name:valueType*/int/*endname*/>)result;
            } else {
                /*name:name*/testDict/*endname*/ = new /*name:dictType*/Dictionary/*endname*/</*name:keyType*/int/*endname*/, /*name:valueType*/int/*endname*/>();
                for (int i = 0; i < input./*name|fu,post#Length:name*/TestDictLength/*endname*/; i++) {
                    var e = input./*name|fu:name*/TestDictNonPrim/*endname*/(i);
                    if (e.HasValue) {
                        var elem = e.Value;

                        /*block:nonprim_key*/
                        var key = FlatBufferSerializer.GetOrCreateDeserialize</*name:keyType*/SerializableHelper/*endname*/>((Serial./*name:fbKeyType*/FBComponent/*endname*/)elem.Key);
                        /*endblock:nonprim_key*/ 
                        /*block:nonprim_value*/
                        var value = FlatBufferSerializer.GetOrCreateDeserialize</*name:valueType*/SerializableHelper/*endname*/>((Serial./*name:fbValueType*/FBComponent/*endname*/)elem.Value);
                        /*endblock:nonprim_value*/ 
                        /*block:rip*/var elem2 = new Serial.DTEST_intlinst_intlist();/*endblock:rip*/
                        /*block:list_key*/var /*name:valueName*/key2/*endname*/ = manual.GetPrimitiveList</*name:listType*/int/*endname*/>(/*name:elemName*/elem2/*endname*/.KeyBufferPosition);
                        /*endblock:list_key*/
                        /*block:list_value*/var /*name:valueName*/value2/*endname*/ = manual.GetPrimitiveList</*name:listType*/int/*endname*/>(/*name:elemName*/elem2/*endname*/.ValueBufferPosition);
                        /*endblock:list_value*/
                        /*block:nonprim_list_key*/var /*name:keyName*/key3/*endname*/ = manual.GetNonPrimList<Serial./*name:fbKeyType*/FBComponent/*endname*/,/*name:keyType*/SerializableHelper/*endname*/>(/*name:elemName*/elem2/*endname*/.KeyBufferPosition);
                        /*endblock:nonprim_list_key*/
                        /*block:nonprim_list_value*/var /*name:valueName*/value3/*endname*/ = manual.GetNonPrimList<Serial./*name:fbValueType*/FBComponent/*endname*/,/*name:valueType*/SerializableHelper/*endname*/>(/*name:elemName*/elem2/*endname*/.ValueBufferPosition);
                        /*endblock:nonprim_list_value*/ 
                        /*name:name*/
                        testDict2/*endname*/[(/*name:keyType*/SerializableHelper/*endname*/)/*name:thekey*/key/*endname*/] = (/*name:valueType*/SerializableHelper/*endname*/)/*name:thevalue*/value/*endname*/;
                    }
                }
                FlatBufferSerializer.PutIntoDeserializeCache(input./*name|fu,post#BufferPosition:name*/TestDictNonPrimBufferPosition/*endname*/, /*name:name*/testDict/*endname*/);
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
}


/*block:rip*/
[System.Serializable]
public class GenTemplateComponent2 : ECS.Component
{
    public float time;

    public override IComponent Clone(bool cloneFromPrefab=false) {
        throw new System.NotImplementedException();
    }

    public override void CopyValues(IComponent target, bool initFromPrefab = false) {
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