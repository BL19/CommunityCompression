using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommunityComprimation.BinaryZip
{
    public class Compresser
    {

        PercentChangedEventArgs pargs = new PercentChangedEventArgs();
        public int runningDataHandlers = 0;
        public int totalDataHandlers = 0;
        public int bytesToProcess = 0;
        public int processedBytes = 0;

        public byte[] Compress(byte[] bytes) {
            byte[] ar = new byte[0];
            byte currentMax = 0;
            long psum = 0;
            int currentQueued = 0;
            int nextQueue = 0;
            bytes = extend(bytes, 1);
            bytes[bytes.Length - 1] = 0;
            bytesToProcess = bytes.Length;
            int pmin = 0;
            for (int i = 0; i < bytes.Length; i++){
                byte b = bytes[i];

                if(pmin == 0) {
                    pmin = b;
                }

                if(b > currentMax) {
                    currentMax = b;
                } else {
                    currentMax = 0;
                    //Compress
                    Task.Run(() =>
                    {
                        int queuepos = nextQueue;
                        nextQueue++;
                        runningDataHandlers++;
                        totalDataHandlers++;
                        //CCConfig.debug("Enqueued at " + queuepos);
                        long sum = psum;
                        int min = pmin;
                        pmin = 0;
                        psum = 0;
                        byte[] mbs = ToMultiByteStorage(sum);
                        while(queuepos != currentQueued) {
                            //CCConfig.debug(queuepos + " is awaiting to import data... (" + (queuepos - currentQueued) + " left)");
                            //Thread.Sleep(10);
                        }
                        int pkgstart = ar.Length;
                        ar = extend(ar, mbs.Length + 2);
                        ar[pkgstart] = byte.Parse((min) + "");
                        for (int j = 0; j < mbs.Length; j++)
                        {
                            ar[(ar.Length - 1) - j] = mbs[j];
                        }
                        ar[ar.Length - 1] = 0;
                        currentQueued++;
                        runningDataHandlers--;
                    });
                    //Thread.Sleep();
                }
                psum += (long) Math.Pow(2, int.Parse(b + "") - (pmin - 1));
                processedBytes++;
            }
            Thread.Sleep(20);
            while (nextQueue != currentQueued) { }
            return ar;
        }

        public byte[] DeCompress(byte[] arr) {
            byte[] result = new byte[0];
            byte[] buf = new byte[0];
            for (int i = 0; i < arr.Length; i++)
            {
                byte b = arr[i];
                if(b == 0) {
                    int min = buf[0];
                    byte[] bbuf = buf;
                    buf = new byte[buf.Length - 1];
                    for (int j = 1; j < bbuf.Length; j++)
                    {
                        buf[j - 1] = byte.Parse(int.Parse(bbuf[j] + "") + min + "");
                    }
                    long mbs = FromMultiByteStorage(buf);
                    buf = new byte[0];
                    byte[] mbsres = new byte[0];
                    while(true) {
                        int next = FindBiggestNearBinary(mbs);
                        mbs -= (long) Math.Pow(2, next);
                        mbsres = extend(mbsres, 1);
                        mbsres[mbsres.Length - 1] = (byte) next;
                        if (next == 1 || next == 0)
                            break;
                    } 
                    result = extend(result, mbsres.Length);
                    for (int j = 0; j < mbsres.Length; j++)
                    {
                        result[(result.Length - mbsres.Length) + j] = mbsres[j];
                    }
                }else{
                    buf = extend(buf, 1);
                    buf[buf.Length - 1] = byte.Parse((int.Parse(b + "")-1) + "");
                }
            }
            return result;
        }

        public byte[] ToMultiByteStorage(long num) {
            CCConfig.debug("Converting " + num + " to MBS..");
            byte[] ar = new byte[0];
            bool[] br = new bool[0];
            long n = num;
            //while (false)
            //{

            //    long nb = FindBiggestNearBinary(n);
            //    long b = FindBiggestBinary(n);
            //    n -= b;
            //    if (nb - br.Length > 0)
            //        br = extend(br, (nb) - br.Length);

            //    string resultby = "";
            //    foreach (var by in br)
            //    {
            //        resultby += " " + (by ? "1" : "0") + " ";
            //    }
            //    CCConfig.debug("br: " + resultby);

            //    br = BinaryAdd(br, b);

            //    if(n == 0)
            //        break;
            //}
            char[] bytes = Convert.ToString(num,2).ToCharArray(); // To binary bytes
            br = new bool[bytes.Length];
            string bits = "";

            for (int i = 0; i < bytes.Length; i++)
            {
                char c = bytes[i];
                if(c == '1') {
                    br[i] = true;
                } else {
                    br[i] = false;
                }
                bits += c;
            }
            CCConfig.debug(bits);

            //Get the number of bytes needed for the number to fit
            double bytesNeeded = br.Length / 8;
            int bni = (int)bytesNeeded;
            if (br.Length % 8 != 0)
            {
                bni += 1;
                bytesNeeded += 1;
            }

            ar = new byte[bni + (bytesNeeded - bni != 0 ? 1:0)];

            int bytecount = 0;
            int bc1 = 0;
            string bbuf = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                bbuf += bytes[i] + "";
                if (bc1 == 7 || i == bytes.Length - 1) {
                    long b = BinaryToDec(bbuf);
                    if (ar.Length == bytecount)
                        ar = extend(ar, 1);
                    ar[bytecount] = byte.Parse(b + "");
                    bbuf = "";
                    bytecount++;
                    bc1 = 0;
                }
                bc1++;
                if (!CCConfig.TOTAL_PEFORMANCE)
                {
                    PercentageChange args = new PercentageChange();
                    args.currentiterations = i;
                    args.totaliterations = br.Length - 1;
                    args.where = "Bits to Bytes";
                    args.percent = (int)((i * 100) / br.Length - 1);
                    pargs.changes[1] = args;
                    OnPercentChange(pargs);
                }
            }
            string result = "";
            foreach (var b in ar) {
                result += " " + b + " ";
            }
            CCConfig.debug(num + " has been converted to MBS! (Result: [" + result + "] / " + bits + ")");
            return ar;
        }

        public long FromMultiByteStorage(byte[] arr) {
            string binary = "";
            foreach (var b in arr)
            {
                char[] bytes = Convert.ToString(int.Parse(b + ""), 2).ToCharArray(); // To binary bytes
                for (int i = 0; i < bytes.Length; i++)
                {
                    char c = bytes[i];
                    binary += c;
                }
            }
            return BinaryToDec(binary);
        }

        private bool[] extend(bool[] br, long v)
        {
            CCConfig.debug("Extending bool array by " + v + " spaces");
            bool[] c = br;
            br = new bool[c.Length + v];
            for (int i = 0; i < c.Length; i++)
            {
                br[i] = c[i];
            }
            CCConfig.debug("Extended!");
            return br;
        }

        private byte[] extend(byte[] ar, int v)
        {
            CCConfig.debug("Extending byte array by " + v + " spaces");
            byte[] c = ar;
            ar = new byte[c.Length + v];
            for (int i = 0; i < c.Length; i++)
            {
                ar[i] = c[i];
            }
            return ar;
        }

        public long FindBiggestBinary(long num) {
            return (num != 1 ? (long) Math.Pow(2, FindBiggestNearBinary(num)) : 1);
        }

        public int FindBiggestNearBinary(long num) {
            CCConfig.debug("Finding biggest \"near\" binary to " + num);
            int res = 0;
            for (int i = 0; Math.Pow(2, i) <= num; i++)
            {
                res = i;
            }
            if (Math.Pow(2, res) == num) //When the same number the number wont be right
                num++;
            CCConfig.debug("Found " + res);
            return res;
        }

        private bool[] BinaryAdd(bool[] originalbits, long valuetoadd)
        {
            long totaliterations = valuetoadd - 1 * originalbits.Length - 1;
            long iterations = 0;
            int lastPercent = -1;
            CCConfig.debug("Iterating binaryadding " + totaliterations + " times..");
            bool[] returnbits = new bool[originalbits.Length];

            for (long i = 0; i <= valuetoadd - 1; i++)
            {
                bool r = false; //r=0
                for (long j = originalbits.Length - 1; j <= originalbits.Length; j--)
                {
                    bool breakcond = false;
                    bool o1 = originalbits[j];
                    if (r == false)
                    {
                        if (o1 == false) { o1 = true; breakcond = true; }//break
                        else if (o1 == true) { o1 = false; r = true; }
                    }
                    else
                    {
                        if (o1 == false) { o1 = true; breakcond = true; }//break
                        else if (o1 == true) { o1 = false; r = true; }
                    }

                    originalbits[j] = o1;
                    if (CCConfig.totalDEBUG)
                    {
                        string bits = "";
                        foreach (var bit in originalbits)
                        {
                            bits += (bit ? 1 : 0);
                        }
                        CCConfig.debug(bits);
                    }
                    if (breakcond == true)
                    {
                        break;
                    }
                    iterations++;
                    long percent = ((iterations * 100) / totaliterations);
                    if (!CCConfig.TOTAL_PEFORMANCE)
                    {
                        if (lastPercent < percent)
                        {
                            CCConfig.debug("Binary Adding: " + percent + "% (" + iterations + "/" + totaliterations + ")");
                            lastPercent = (int)percent;
                            PercentageChange args = new PercentageChange();
                            args.percent = (int)percent;
                            args.where = "Binary Adder";
                            args.totaliterations = totaliterations;
                            args.currentiterations = iterations;
                            pargs.changes[0] = args;
                            OnPercentChange(pargs);
                        }
                    }
                }

            }
            returnbits = originalbits;

            return returnbits;
        }

        static long BinaryToDec(string input)
        {
            char[] array = input.ToCharArray();
            // Reverse since 16-8-4-2-1 not 1-2-4-8-16. 
            Array.Reverse(array);
            /*
             * [0] = 1
             * [1] = 2
             * [2] = 4
             * etc
             */
            long sum = 0;

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == '1')
                {
                    // Method uses raising 2 to the power of the index. 
                    if (i == 0)
                    {
                        sum += 1;
                    }
                    else
                    {
                        sum += (int)Math.Pow(2, i);
                    }
                }

            }

            return sum;
        }

        private void UpdatePercentage(PercentageChange args)
        {
            if(pargs.changes == null) {
                pargs.changes = new PercentageChange[2];
            }
            
            bool found = true;
            for (int i = 0; i < pargs.changes.Length; i++)
            {
                if (pargs.changes[i] != null)
                {
                    if (args.where == pargs.changes[i].where)
                    {
                        pargs.changes[i] = args;
                        found = true;
                    }
                }
            }
            if(!found) {
                CCConfig.debug("Not found!");
                for (int i = 0; i < pargs.changes.Length; i++)
                {
                    if(pargs.changes[i] == null) {
                        pargs.changes[i] = args;
                        break;
                    }
                }
            }
            OnPercentChange(pargs);
        }

        protected virtual void OnPercentChange(PercentChangedEventArgs e)
        {
            PercentChanged?.Invoke(this, e);
        }

        public event PercentChangedEventHandler PercentChanged;
    }

    public class PercentChangedEventArgs : EventArgs
    {
        public PercentageChange[] changes = new PercentageChange[2];
    }

    public class PercentageChange {
        public string where;
        public int percent;

        public long currentiterations;
        public long totaliterations;
    }

    public delegate void PercentChangedEventHandler(Object sender, PercentChangedEventArgs e);
}
