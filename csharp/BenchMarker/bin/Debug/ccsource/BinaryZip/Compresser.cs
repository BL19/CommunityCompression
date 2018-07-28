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
            long sum = 0;
            int currentQueued = 0;
            int nextQueue = 0;
            bytesToProcess = bytes.Length;
            for (int i = 0; i < bytes.Length; i++){
                byte b = bytes[i];
                sum += b;
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
                        byte[] mbs = ToMultiByteStorage(sum);
                        while(queuepos != currentQueued) {
                            //CCConfig.debug(queuepos + " is awaiting to import data... (" + (queuepos - currentQueued) + " left)");
                            //Thread.Sleep(10);
                        }
                        ar = extend(ar, mbs.Length + 1);
                        for (int j = 0; j < mbs.Length; j++)
                        {
                            ar[(ar.Length - 2) - j] = byte.Parse(int.Parse(mbs[j] + "") + 1 + "" );
                        }
                        ar[ar.Length - 1] = 0;
                        currentQueued++;
                        runningDataHandlers--;
                    });
                    //Thread.Sleep();
                }
                processedBytes++;
            }

            while(nextQueue != currentQueued) { }
            return ar;
        }

        public byte[] DeCompress(byte[] arr) {
            byte[] result = new byte[0];

            return result;
        }

        public byte[] ToMultiByteStorage(long num) {
            CCConfig.debug("Converting " + num + " to MBS..");
            byte[] ar = new byte[0];
            bool[] br = new bool[0];
            long n = num;
            while (false)
            {

                long nb = FindBiggestNearBinary(n);
                long b = FindBiggestBinary(n);
                n -= b;
                if (nb - br.Length > 0)
                    br = extend(br, (nb) - br.Length);

                string resultby = "";
                foreach (var by in br)
                {
                    resultby += " " + (by ? "1" : "0") + " ";
                }
                CCConfig.debug("br: " + resultby);

                br = BinaryAdd(br, b);

                if(n == 0)
                    break;
            }
            long nb1 = FindBiggestNearBinary(n);
            br = new bool[nb1 + 1];
            char[] bytes = Convert.ToString(num,2).ToCharArray();

            for (int i = 0; i < bytes.Length; i++)
            {
                char c = bytes[i];
                if(c == '1') {
                    br[i] = true;
                } else {
                    br[i] = false;
                }
            }

            //Get the number of bytes needed for the number to fit
            double bytesNeeded = br.Length / 8;
            int bni = (int)bytesNeeded;
            if (bytesNeeded - bni > 0)
                bni += 1;

            ar = new byte[bni + 1];

            int bytecount = 0;
            int bc1 = 0;
            string bits = "";
            for (int i = 0; i < br.Length; i++)
            {
                int plc = br.Length - i;
                int bnum = (br[i] ? 1 : 0);
                bits += bnum + "";
                ar[bytecount] += (byte)(Math.Pow(2, (plc % 8)-1) * bnum);
                CCConfig.debug("Byte: " + Math.Pow(2, (plc % 8)-1) + ", " + ((plc % 8)-1) + ", " + bnum);
                if (bc1 == 8) {
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
            long result = 0;
            return result;
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
