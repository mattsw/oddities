namespace testapp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    
    public class Program
    {
        static ManualResetEvent resetEvent = new ManualResetEvent(false);
        public static void Main()
        {
            new DelegatesAndStucturalIdentities().Example();
        }

        public static void AsyncExample()
        {
            var stuffs = new TestAsyncStuff(resetEvent);
            var item = stuffs.PerformAsync();
            resetEvent.WaitOne();
            Console.WriteLine(item.IsCompleted);
        }
    }

    public class DelegatesAndStucturalIdentities
    {
        private delegate bool Function(int x);
        private delegate bool Predicate(int x);

        public void Example()
        {
            Function func = TestFunction;
            Predicate pred = TestFunction;

            Method(TestFunction);
            Method(func);
            //Method(pred);
        }

        private bool TestFunction(int x)
        {
            Console.WriteLine("Testing if {0} is greater than zero.", x);
            return x > 0;
        }

        private void Method(Function function)
        {
            function(1);
        }
    }

    public class TestAsyncStuff
    {
        private ManualResetEvent resetEvent;

        public TestAsyncStuff(ManualResetEvent resetEvent)
        {
            this.resetEvent = resetEvent;
        }

        public async Task PerformAsync()
        {
            Task[] tasks = new Task[3];
            tasks[0] = DoOperation0Async();
            tasks[1] = DoOperation1Async();
            tasks[2] = DoOperation2Async();

            // At this point, all three tasks are running at the same time.

            // Now, we await them all.
            await Task.WhenAll(tasks);
            Console.WriteLine("It's working and we have 42 and 100.5");
            resetEvent.Set();
        }

        private async Task<string> DoOperation2Async()
        {
            await Task.Delay(1000);
            return "It's working";
        }

        private async Task<int> DoOperation1Async()
        {
            await Task.Delay(1000);
            return 42;
        }

        private async Task<double> DoOperation0Async()
        {
            await Task.Delay(1000);
            return 100.5;
        }

    }

    public static class ClosureExampleTwo
    {
        public static void Example()
        {
            var action = new List<Action>();
            for (int num = 0; num < 10; num++)
            {
                action.Add(() => Console.WriteLine(num));
            }

            action.ForEach(a => a());//Show a broken yet promising approach

            var actionTakeTwo = new List<Action>();
            for (int num = 0; num < 10; num++)
            {
                int fix = num;
                actionTakeTwo.Add(() => Console.WriteLine(fix));
            }

            actionTakeTwo.ForEach(a => a());
        }
    }

    public static class ClosureExample
    {
        public static void Example()
        {
            int maxLength = 4;//show off the power of a closure
            Predicate<string> pred = item => item.Length > maxLength;
            List<string> items = new List<string>
                {
                    "LongEnough",
                    "No",
                    "Nope",
                    "!Yes",
                    "This should work",
                    "So should this",
                    "Total length 4"
                };
            Console.WriteLine(Filter(items, pred).Count == 4
                                  ? "Four items and the converter worked"
                                  : "Converter failed, I am bad.");

            maxLength = 3;//show off the power of a closure AGAIN!
            Console.WriteLine(Filter(items, pred).Count == 4
                                  ? "The converter failed"
                                  : "Converter has worked using my local variable.");
        }

        public static List<T> Filter<T>(List<T> anonList, Predicate<T> predicate)
        {
            var ret = new List<T>();
            anonList.ForEach(contained =>
                {
                    if (predicate(contained)) ret.Add(contained);
                });
            return ret;
        }
    }

    public class ItemFactory
    {
        private static IDictionary<int, Func<IItem>> items = new Dictionary<int, Func<IItem>>();

        static ItemFactory()
        {
            // thanks: http://stackoverflow.com/a/720171/54323
            foreach (var t in
                from a in AppDomain.CurrentDomain.GetAssemblies()
                from t in a.GetTypes()
                where t.IsDefined(typeof(DefiningAttribute), false)
                select t)
            {
                var att = (DefiningAttribute)t.GetCustomAttribute(typeof(DefiningAttribute), false);
                items.Add(att.DefiningInt, () => (IItem)Activator.CreateInstance(t));
            }
        }

        public static IItem GetItem(int itemNumber)
        {
            return items[itemNumber]();
        }
    }

    public class DefiningAttribute : Attribute
    {
        public int DefiningInt { get; private set; }

        public DefiningAttribute(int definingInt)
        {
            DefiningInt = definingInt;
        }
    }

    public interface IItem
    {
        string Contents { get; }
    }

    [DefiningAttribute(0)]
    public class ItemTest : IItem
    {
        public string Contents {get { return "TestItem"; }}
        public int val1 { get; set; }
        public int val2 { get; set; }
        public int val3 { get; set; }

        public int ClassCode
        {
            get { return val1 > val2 ? 199 : 198; }
        }

        public ItemTest() { }

        public ItemTest(int val1, int val2)
        {
            this.val1 = val1;
            this.val2 = val2;
        }

        public static ItemTest operator +(ItemTest item1, ItemTest item2)
        {
            return new ItemTest(item1.val1 + item2.val1, item1.val2 + item2.val2);
        }
    }

    [DefiningAttribute(1)]
    public class ItemTrial : IItem
    {
        public string Contents{get { return "Just A Trial"; }}
        public int val1 { get; set; }
        public decimal val2 { get; set; }

        public ItemTrial(){}

        public ItemTrial(int val1, decimal val2)
        {
            this.val1 = val1;
            this.val2 = val2;
        }
    }
}