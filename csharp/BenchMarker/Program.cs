using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityComprimation;
using CommunityComprimation.BinaryZip;

namespace BenchMarker
{
    class Program
    {
        static string testfile = "test.png";
        static bool runStringTest = false;
        static string stringTestString = "";
        static bool saveFiles = false;
        static string[] cached = new string[0];
        static int currentcached = 0;
        public static Compresser c;

        static void Main(string[] args)
        {
            
            if(args.Length > 0) {
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {

                        case "-st":
                        case "-stringtest":
                        case "--stringtest":
                            runStringTest = true;
                            stringTestString = args[i + 1];
                            break;

                        case "-sf":
                        case "-save":
                        case "--savefiles":
                            saveFiles = true;
                            break;
                        
                        default:
                            break;
                    }
                }
            }
            c = new Compresser();
            //c.PercentChanged += C_PercentChanged;
            Console.WriteLine(c.FindBiggestBinary(65));
            while (true)
            {
                string input = Console.ReadLine();
                clear();
                write(input, false);
                if (input.StartsWith("mbs"))
                {
                    input = input.Remove(0, 4);
                    byte[] mbs = c.ToMultiByteStorage(long.Parse(input));
                    string result = "";
                    foreach (var b in mbs)
                    {
                        result += " " + b + " ";
                    }

                    write("MBS Succeded!");
                    write("[" + result + "]");
                } else if(input.StartsWith("c")) {
                    input = input.Remove(0, 2);
                    if (input.StartsWith("c")) {
                        input = input.Remove(0, 2);
                        byte[] arr = Encoding.ASCII.GetBytes(input);
                        byte[] comp = c.Compress(arr);
                        string result = "";
                        foreach (var b in comp)
                        {
                            result += " " + b + " ";
                        }
                        write(result + "  /  " + Convert.ToBase64String(comp) + "\n" + (((comp.Length * 100) / arr.Length) - 100) + "% (" + comp.Length + "/" + arr.Length + ")");
                    } else if (input.StartsWith("f")) {
                        input = input.Remove(0, 2);
                        StreamReader r = new StreamReader(input);
                        byte[] arr = Encoding.ASCII.GetBytes(r.ReadToEnd());
                        r.Close();
                        Console.WriteLine("Press enter to start compression.. (size: " + arr.Length + ")");
                        Console.ReadLine();
                        DateTime start = DateTime.Now;
                        Thread t = new Thread(update);
                        //t.Start();
                        byte[] comp = c.Compress(arr);
                        //t.Suspend();
                        string result = "";
                        foreach (var b in comp)
                        {
                            result += " " + b + " ";
                        }
                        write(result + "  /  " + Convert.ToBase64String(comp) + "\n" + (((comp.Length * 100) / arr.Length) - 100) + "% (" + comp.Length + "/" + arr.Length + ")");
                        StreamWriter w = new StreamWriter(input + ".bz");
                        w.WriteLine(Convert.ToBase64String(comp));
                        w.Close();
                        DateTime end = DateTime.Now;
                        TimeSpan diff = end.Subtract(start);
                        
                        write("Done! (" + diff.Seconds + " seconds " + (diff.Milliseconds % 10) + " ms) \nSpeed: " + (arr.Length / diff.Seconds) + " bytes/sec");
                    } else {
                        string[] bytes = input.Split(' ');
                        byte[] arr = new byte[bytes.Length];
                        for (int i = 0; i < bytes.Length; i++)
                        {
                            arr[i] = byte.Parse(bytes[i]);
                        }
                        byte[] comp = c.Compress(arr);
                        string result = "";
                        foreach (var b in comp)
                        {
                            result += " " + b + " ";
                        }
                        write(result + "  /  " + Convert.ToBase64String(comp) + "\n" + (((comp.Length * 100) / arr.Length) - 100) + "% (" + comp.Length + "/" + arr.Length + ")");
                    }
                }
            }

        }

        public static void update() {
            long lastbytes = 0;
            while(c.bytesToProcess != c.processedBytes) {
                Console.Clear();
                long speed = (c.processedBytes - lastbytes);
                long estimatedRemainingTime = (c.bytesToProcess - c.processedBytes) / speed;
                DateTime dt = DateTime.Now.AddSeconds(estimatedRemainingTime);
                Console.WriteLine(((c.processedBytes * 100) / c.bytesToProcess) + "% (" + c.processedBytes + "/" + c.bytesToProcess + ") \n Speed: " + speed + "bytes/sec   ETR: " + dt.ToLongTimeString());
                Thread.Sleep(1000);
                lastbytes = c.processedBytes;
            }
        }

        public static void write(string ln) {
            write(ln, true);
        }

        public static void write(string ln, bool console) {
            cached = extend(cached, 1);
            cached[cached.Length - 1] = ln;
            if(console)
                Console.WriteLine(ln);
        }

        public static void clear() {
            cached = new string[0];
        }

        public static void writeCached() {
            foreach (var s in cached)
            {
                Console.WriteLine(s);
            }
        }
        private static void C_PercentChanged(object sender, PercentChangedEventArgs e)
        {
            Console.Clear();
            //writeCached();
            foreach (var p in e.changes)
            {
                if (p != null)
                {
                    string outp = p.where + ": " + p.percent + "% (" + p.currentiterations + "/" + p.totaliterations + ")";
                    Console.WriteLine(outp);
                }
            }
            
        }

        private static string[] extend(string[] ar, int v)
        {
            string[] c = ar;
            ar = new string[c.Length + v];
            for (int i = 0; i < c.Length; i++)
            {
                ar[i] = c[i];
            }
            return ar;
        }
    }
}
