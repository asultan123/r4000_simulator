using System;
using System.Collections.Generic;

namespace r4000sim
{
    public static class Buffer_UnitTest
    {
        public static void runUnitTest()
        {
            Buffer[] testBuffers = {
                new Buffer("alpha"), new Buffer("beta"), new Buffer("gamma"), new Buffer("epsilon")
            };

            foreach (Buffer buffer in testBuffers)
            {
                //buffer.fillWithTempDataAtInput();
                buffer.input(Operation.add, 123123123);
                buffer.input(Register.r4, 312312312);
                buffer.input(Register.r2, 456456456);
                buffer.input(Type.psudo, 678678678);
            }

            Buffer.clockAll(0);

            foreach (Buffer buffer in testBuffers)
            {
                Enum[] keys = { Operation.add, Register.r4, Register.r2, Type.psudo };

                Console.WriteLine("\n Testing buffer {0} with valid data \n", buffer.id);
                //trying data that is supposedly in each buffer
                foreach (Enum key in keys)
                {
                    Console.WriteLine("Requested key: {0}, response: {1}", key, buffer.output(key.ToString()));
                }

                Enum[] badkeys = { Operation.ble, Register.r7, Register.r9, Type.rType };

                Console.WriteLine("\n Testing buffer {0} with invalid data \n", buffer.id);
                //trying data that is not in buffer
                foreach (Enum badkey in badkeys)
                {
                    Console.WriteLine("Requested badkey: {0}, response: {1}", badkey, buffer.output(badkey.ToString()));
                }
            }

            Console.WriteLine("\n Flushin and testing valid data \n");
            Buffer.flushAll(0);

            foreach (Buffer buffer in testBuffers)
            {
                Enum[] keys = { Operation.add, Register.r4, Register.r2, Type.psudo };

                Console.WriteLine("\n Testing buffer {0} with valid data \n", buffer.id);
                //trying data that is supposedly in each buffer
                foreach (Enum key in keys)
                {
                    Console.WriteLine("Requested key: {0}, response: {1}", key, buffer.output(key.ToString()));
                }
            }
        }
    }

    public class Buffer
    {
        private static List<WeakReference<Buffer>> references = new List<WeakReference<Buffer>>();
        private Dictionary<string, int> leftSide;
        private Dictionary<string, int> rightSide;
        private bool stalled;
        public string id;

        public Buffer(string id)
        {
            stalled = false;
            leftSide = new Dictionary<string, int>();
            rightSide = new Dictionary<string, int>();
            this.id = id;
            references.Add(new WeakReference<Buffer>(this));
        }

        public void input(Enum key, int value)
        {
            leftSide[key.ToString()] = value;
        }

        public void input(int key, int value)
        {
            leftSide[key.ToString()] = value;
        }

        public void input(string key, int value)
        {
            leftSide[key] = value;
        }

        public int output(string key)
        {
            int res;
            if (rightSide.TryGetValue(key, out res))
            {
                return res;
            }
            else
            {
                //Console.WriteLine("{0} buffer: requested data {1} at output unavailable, assumed data value is 0", this.id, key);
                return 0;
            }
        }

        public bool output(string key, out int res)
        {
            if (rightSide.TryGetValue(key, out res))
            {
                return true;
            }
            else
            {
                //Console.WriteLine("{0} buffer: requested data {1} at output unavailable, assumed data value is 0", this.id, key);
                return false;
            }
        }


        public List<int> getAllOutputs()
        {
            return new List<int>(leftSide.Values);
        }

        public void flush(int clockCount)
        {
            Console.WriteLine("{0} buffer: data flushed at clock number {1}", this.id, clockCount);
            leftSide.Clear();
            rightSide.Clear();
        }

        public static void clockAll(int clockCount)
        {
            foreach (WeakReference<Buffer> reference in references)
            {
                Buffer res;
                if (reference.TryGetTarget(out res))
                {
                    res.clock(clockCount);
                }
                else
                {
                    Console.WriteLine("Access to buffer instance failed at reference: {0}", reference);
                    references.Remove(reference);
                }
            }
        }

        public static void resetBuffers()
        {
            references.Clear();
        }

        public static List<int> dumpPCs(int clockCount)
        {
            List<int> pcInDifferentStages = new List<int>();
            foreach (WeakReference<Buffer> reference in references)
            {
                Buffer res;
                if (reference.TryGetTarget(out res))
                {
                    Console.WriteLine("{0}: CurrentPC: {1}", System.String.Concat(res.id[2], res.id[3]), res.output("pcPlusOne"));
                    pcInDifferentStages.Add(res.output("pcPlusOne"));
                }
                else
                {
                    Console.WriteLine("Access to buffer instance failed at reference: {0}", reference);
                    references.Remove(reference);
                }
            }
            return pcInDifferentStages;
        }

        public static void flushAll(int clockCount)
        {
            foreach (WeakReference<Buffer> reference in references)
            {
                Buffer res;
                if (reference.TryGetTarget(out res))
                {
                    res.flush(clockCount);
                }
                else
                {
                    Console.WriteLine("Access to buffer instance failed at reference: {0}", reference);
                    references.Remove(reference);
                }
            }
        }

        //public void fillWithTempDataAtInput()
        //{
        //    this.input(Operation.add, 123123123);
        //    this.input(Register.r4, 312312312);
        //    this.input(Register.r2, 456456456);
        //    this.input(Type.psudo, 678678678);
        //}

        public void clock(int clockCount)
        {
            if(!stalled)
            {
                //Console.WriteLine("{0} buffer: clocked at clock number {1}", this.id, clockCount);
                rightSide.Clear();
                rightSide = new Dictionary<string, int>(leftSide);
            }
        }

        public void setStall(bool stalled)
        {
            this.stalled = stalled;
        }
    }
    public class ControlBuffer
    {
        private static List<WeakReference<ControlBuffer>> references = new List<WeakReference<ControlBuffer>>();
        private Dictionary<string, ControlSignal> leftSide;
        private Dictionary<string, ControlSignal> rightSide;
        private bool stalled;
        public string id;

        public static void resetBuffers()
        {
            references.Clear();
        }

        public ControlBuffer(string id)
        {
            stalled = false;
            leftSide = new Dictionary<string, ControlSignal>();
            rightSide = new Dictionary<string, ControlSignal>();
            this.id = id;
            references.Add(new WeakReference<ControlBuffer>(this));
        }

        public void input(Enum key, ControlSignal value)
        {
            leftSide[key.ToString()] = value;
        }

        public void input(int key, ControlSignal value)
        {
            leftSide[key.ToString()] = value;
        }

        public void input(string key, ControlSignal value)
        {
            leftSide[key] = value;
        }

        public ControlSignal output(string key)
        {
            ControlSignal res;
            if (rightSide.TryGetValue(key, out res))
            {
                return res;
            }
            else
            {
                
                Console.WriteLine("{0} buffer: requested data {1} at output unavailable, assumed default signal Values", this.id, key);

                if(key == "DF")
                {
                    DF defaultSignal = new DF();
                    defaultSignal.memControl = 0;
                    return defaultSignal as ControlSignal;
                }
                else if(key == "WB")
                {
                    WB defaultSignal = new WB();
                    defaultSignal.dataSelect = false;
                    defaultSignal.writeToRegisterFile = false;
                    return defaultSignal as ControlSignal;
                }
                else if(key == "EX")
                {
                    EX defaultSignal = new EX();
                    defaultSignal.ALUControl = 0;
                    defaultSignal.registerSelect = false;
                    defaultSignal.useImmediateInAlu = false;
                    return defaultSignal as ControlSignal;
                }
                else if(key == "IF")
                {
                    IF defaultSignals = new IF();
                    defaultSignals.IFjumpControl = 0;
                    defaultSignals.jumpToProcedure = false;
                    defaultSignals.returnFromProcedure = false;
                    return defaultSignals;
                }
                else
                {
                    throw new Exception("Invalid output requeasted from control signal buffer");
                    //return default(T);
                }
            }
        }


        public List<ControlSignal> getAllOutputs()
        {
            return new List<ControlSignal>(leftSide.Values);
        }

        public void flush(int clockCount)
        {
            Console.WriteLine("{0} buffer: data flushed at clock number {1}", this.id, clockCount);
            leftSide.Clear();
            rightSide.Clear();
        }

        public static void clockAll(int clockCount)
        {
            foreach (WeakReference<ControlBuffer>  reference in references)
            {
                ControlBuffer res;
                if (reference.TryGetTarget(out res))
                {
                    res.clock(clockCount);
                }
                else
                {
                    Console.WriteLine("Access to buffer instance failed at reference: {0}", reference);
                    references.Remove(reference);
                }
            }
        }

        public static void flushAll(int clockCount)
        {
            foreach (WeakReference<ControlBuffer> reference in references)
            {
                ControlBuffer res;
                if (reference.TryGetTarget(out res))
                {
                    res.flush(clockCount);
                }
                else
                {
                    Console.WriteLine("Access to buffer instance failed at reference: {0}", reference);
                    references.Remove(reference);
                }
            }
        }

        //public void fillWithTempDataAtInput()
        //{
        //    this.input(Operation.add, 123123123);
        //    this.input(Register.r4, 312312312);
        //    this.input(Register.r2, 456456456);
        //    this.input(Type.psudo, 678678678);
        //}

        public void clock(int clockCount)
        {
            if (!stalled)
            {
                Console.WriteLine("{0} buffer: clocked at clock number {1}", this.id, clockCount);
                rightSide.Clear();
                rightSide = new Dictionary<string, ControlSignal>(leftSide);
            }
        }

        public void setStall(bool stalled)
        {
            this.stalled = stalled;
        }
    }

}


