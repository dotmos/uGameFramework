using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;

public class TestModel : MVC.Model{
    public ReactiveProperty<string> SomeStringProperty = new ReactiveProperty<string>();
    public string SomeString {
        get{
            return SomeStringProperty.Value;
        }
        set{
            SomeStringProperty.Value = value;
        }
    }

    public ReactiveProperty<int> SomeIntProperty = new ReactiveProperty<int>();
    public int SomeInt {
        get{
            return SomeIntProperty.Value;
        }
        set {
            SomeIntProperty.Value = value;
        }
    }

    public class SomeClass{
        public ReactiveProperty<string> SomeStringProperty = new ReactiveProperty<string>();
        public string aString {
            get{
                return SomeStringProperty.Value;
            }
            set{
                SomeStringProperty.Value = value;
            }
        }

        public int anInt;
    }

    public SomeClass someClass;// = new SomeClass(){anInt = 32, aString = "test string"};

    public List<string> stringList;// = new List<string>(){"one", "two", "three"};
    public List<SomeClass> someClassList;// = new List<SomeClass>(){ new SomeClass(){anInt = 42, aString = "omg"} };

    public string[] stringArray;// = {"a", "b", "c"};

    protected override void AfterBind()
    {
        base.AfterBind();
    }
}
