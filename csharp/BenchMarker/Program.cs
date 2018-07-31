using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityComprimation;
using CommunityComprimation.BinaryZip;
using System.Diagnostics;

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
        private static int enqueuedCompressors;
        private static DateTime cstart;
        private static int finishedCompressors;
        private static int comp_files;
        private static int comp_folders;

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
            CCConfig.TOTAL_PEFORMANCE = true;
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
                    if (input.StartsWith("c "))
                    {
                        byte[] mbs = c.ToMultiByteStorage(long.Parse(input));
                        string result = "";
                        foreach (var b in mbs)
                        {
                            result += " " + b + " ";
                        }

                        write("MBS Succeded!");
                        write("[" + result + "]");
                    } else if(input.StartsWith("cd")) {
                        input = input.Remove(0, 3);
                        byte[] mbs = c.ToMultiByteStorage(long.Parse(input));
                        string result = "";
                        foreach (var b in mbs)
                        {
                            result += " " + b + " ";
                        }

                        write("MBS 1 Succeded!");
                        write("[" + result + "]");

                        long bmbs = c.FromMultiByteStorage(mbs);
                        write("MBS 2 Succeded!");
                        write(bmbs + "");
                    }
                } else if(input.StartsWith("c ")) {
                    input = input.Remove(0, 2);
                    if (input.StartsWith("c "))
                    {
                        input = input.Remove(0, 2);
                        byte[] arr = Encoding.ASCII.GetBytes(input);
                        byte[] comp = c.Compress(arr);
                        string result = "";
                        foreach (var b in comp)
                        {
                            result += " " + b + " ";
                        }
                        write(result + "  /  " + Convert.ToBase64String(comp) + "\n" + (((comp.Length * 100) / arr.Length) - 100) + "% (" + comp.Length + "/" + arr.Length + ")");
                    }
                    else if (input.StartsWith("f"))
                    {
                        input = input.Remove(0, 2);
                        string[] cargs = input.Split(' ');
                        if (Directory.Exists(cargs[0]))
                        {
                            enqueuedCompressors = 0;
                            finishedCompressors = 0;
                            cstart = DateTime.Now;
                            CompressFolder(cargs[0], (cargs.Length > 1 ? cargs[1] : "./" + cargs[0] + "-zipped"));
                        }
                        else
                        {
                            StreamReader r = new StreamReader(input);
                            byte[] arr = Encoding.ASCII.GetBytes(r.ReadToEnd());
                            r.Close();
                            Console.WriteLine("Press enter to start compression.. (size: " + arr.Length + ")");
                            Console.ReadLine();
                            cstart = DateTime.Now;
                            compress(arr, input, ".", true);
                        }


                    }
                    else if (input.StartsWith("cd")){
                        input = input.Remove(0, 3);

                        byte[] arr = Encoding.ASCII.GetBytes(input);
                        string aresult = "";
                        foreach (var b in arr)
                        {
                            aresult += " " + b + " ";
                        }
                        write(aresult);
                        byte[] comp = c.Compress(arr);
                        string result = "";
                        foreach (var b in comp)
                        {
                            result += " " + b + " ";
                        }
                        write(result + "  /  " + Convert.ToBase64String(comp) + "\n" + (((comp.Length * 100) / arr.Length) - 100) + "% (" + comp.Length + "/" + arr.Length + ")");
                        byte[] decomp = c.DeCompress(comp);
                        string dres = "";
                        foreach (var b in decomp)
                        {
                            dres += " " + b + " ";
                        }
                        write(dres);
                        write(Encoding.ASCII.GetString(decomp)); 
                    }
                    else
                    {
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

        private static void CompressFolder(string input, string basedir)
        {
            Directory.CreateDirectory(basedir);
            foreach (var dir in Directory.GetDirectories(input))
            {
                comp_folders++;
                string[] locarr = dir.Split("\\".ToCharArray()[0]);
                string loc = locarr[locarr.Length-1];
                CompressFolder(input + "/" + loc, basedir + "/" + loc);
            }

            foreach (var file in Directory.GetFiles(input))
            {
                try
                {
                    string[] locarr = file.Split("\\".ToCharArray()[0]);
                    string loc = locarr[locarr.Length - 1];
                    StreamReader r = new StreamReader(input + "/" + loc);
                    byte[] arr = Encoding.ASCII.GetBytes(r.ReadToEnd());
                    r.Close();
                    comp_files++;
                    compress(arr, loc, basedir, false);
                } catch (FileNotFoundException e) {
                    
                }
            }
        }

        public static void compress(byte[] arr, string fname, string location, bool printoutput) {
            Compresser cc = new Compresser();
            int current = enqueuedCompressors;
            enqueuedCompressors++;
            Task.Run(() =>
            {
                write("Compressing " + location + "/" + fname + " to " + location + "/" + fname + ".bz...");
                DateTime start = DateTime.Now;
                byte[] comp = c.Compress(arr);
                if (printoutput)
                {
                    string result = "";
                    foreach (var b in comp)
                    {
                        result += " " + b + " ";
                    }
                    write(result + "  /  " + Convert.ToBase64String(comp) + "\n" + (((comp.Length * 100) / arr.Length) - 100) + "% (" + comp.Length + "/" + arr.Length + ")");
                }
                StreamWriter w = new StreamWriter(location + "/" + fname + ".bz");
                w.WriteLine(Convert.ToBase64String(comp));
                w.Close();
                DateTime end = DateTime.Now;
                TimeSpan diff = end.Subtract(start);

                finishedCompressors++;
                write("Done! (" + diff.Seconds + " seconds " + (diff.Milliseconds) + " ms) \nSpeed: " + (diff.Seconds > 1 ?(arr.Length / diff.Seconds) : arr.Length) + " bytes/sec   Rate: " + (arr.Length > 1 ? (((comp.Length * 100) / arr.Length) - 100) : 0) + "%  File: " + fname + "  Progress: " + ((finishedCompressors * 100)/enqueuedCompressors) + "% (" + finishedCompressors + "/" + enqueuedCompressors + ")");
                Debug.WriteLine(finishedCompressors + "/" + enqueuedCompressors + "  " + fname);
                if(enqueuedCompressors == finishedCompressors) {
                    DateTime cend = DateTime.Now;
                    TimeSpan cdiff = cend.Subtract(cstart);
                    Console.WriteLine("\n\nDONE! (" + comp_files + " files & " + comp_folders + " folders) \nTime: " + cdiff.Hours + ":" + cdiff.Minutes + ":" + diff.Seconds + "." + cdiff.Milliseconds);
                }
            });
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
