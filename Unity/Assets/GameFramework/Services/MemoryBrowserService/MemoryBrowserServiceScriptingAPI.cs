using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using Zenject;
using UniRx;

namespace Service.MemoryBrowserService {



    partial class MemoryBrowserServiceImpl : MemoryBrowserServiceBase
    {
        // TODO: Remove if not needed anymore
        public class TestEnvironment
        {
            public class Workplace
            {
                public string name;
                public List<Employee> employees = new List<Employee>();
            }

            public class Employee
            {
                public string name;
                public Workplace worplace;
                public List<Workplace> formerJobs = null;
                public int age;
                public float speed;
                public int DoubleAgeProperty {
                    get { return age * 2; }
                }

            }


            protected Workplace wp;
            public Employee emp;
            private Employee emp2;

            public TestEnvironment() {
                // TODO: REMOVE if not needed anymore
                // add some test values 

                wp = new Workplace() {
                    name = "Gentlymad"
                };

                emp = new Employee() {
                    name = "Tom",
                    worplace = wp,
                    age = 41,
                    speed = 0.95f
                };

                emp2 = new Employee() {
                    name = "Wolfgang",
                    worplace = wp,
                    age = 50,
                    speed = 0.5f
                };

                wp.employees.Add(emp);
                wp.employees.Add(emp2);
            }
        }

        protected override void InitAPI() {
            // use all methods of the service
            ActivateDefaultScripting("MemoryBrowserService");


            //
            //
            //API api = new API(this);
            //Kernel.Instance.Inject(api);
            //cmdGetScript.result.Globals["MTest"] = new TestEnvironment();
            //cmdGetScript.result.Globals["TestListInt"] = new List<int>() { 1,2,3,4,5,6,7};
            //cmdGetScript.result.Globals["TestDictStringInt"] = new Dictionary<string, int>() { {"tom", 1 }, { "FORTUNA", 95 }, { "GHF", 11111 } };
            //cmdGetScript.result.Globals["TestDictStringObject"] = new Dictionary<string, TestEnvironment.Employee>() { { "tom", new TestEnvironment.Employee() }, { "tom2", new TestEnvironment.Employee() }, { "tom3", new TestEnvironment.Employee() }};

            //cmdGetScript.result.Globals["TestEmp"] = new List<TestEnvironment.Employee>() { new TestEnvironment.Employee() { name="tom" }, new TestEnvironment.Employee() { name = "Stephan" }, new TestEnvironment.Employee() { name = "Daniel" }, new TestEnvironment.Employee() { name = "Matthias" }, new TestEnvironment.Employee() { name = "Wolfgang" } };
        }


        class API
        {
            MemoryBrowserServiceImpl instance;

            public API( MemoryBrowserServiceImpl instance) {
                this.instance = instance;
            }

            /* add here scripting for this service */
        }
    }

}
