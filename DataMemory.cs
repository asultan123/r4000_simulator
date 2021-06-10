using System;
using System.Collections.Generic;
using System.Linq;

namespace r4000sim
{
    public static class DataMemory_UnitTest
    {
        public static void runUnitTest(bool throwExceptionsAllowed)
        {
            DataMemory dmem = new DataMemory(size: 16);

            Console.WriteLine("Begin Datamemory unit test");
            Console.WriteLine("Making write request at sequential addresses");

            int simClock = 0;

            Random gen = new Random();
            //write
            for (int address = 0; address <= dmem.length - 4; address += 4)
            {
                dmem.requestWrite(address, gen.Next(0,int.MaxValue), simClock);
                dmem.clock(simClock);
                dmem.response(simClock); //no response excpected but must be called to finalize mem write
            }

            dmem.dumpDataMemory(simClock);

            //read
            for (int i = 0; i <= dmem.length - 4; i += 4)
            {
                int address = gen.Next(0, dmem.length - 4);

                dmem.requestRead(address, simClock);
                dmem.clock(simClock);
                int response = dmem.response(simClock);
                byte[] byteResponse = BitConverter.GetBytes(response);
                Console.WriteLine("Address: {0}, Data: {1}, as bytes: {2} {3} {4} {5}", address, response, byteResponse[3], byteResponse[2], byteResponse[1], byteResponse[0]);
            }

            if(throwExceptionsAllowed)
            {

                //no clock exception
                try
                {
                    dmem.clearRequest(simClock);
                    dmem.requestRead(0, simClock);
                    dmem.response(simClock);  
                }
                catch(Exception e)
                {
                    Console.WriteLine("Caught: {0}", e.Message);
                }

                //multiple requeast exceptiom
                try
                {
                    dmem.clearRequest(simClock);
                    dmem.requestRead(0, simClock);
                    dmem.requestRead(0, simClock);
                }

                catch (Exception e)
                {
                    Console.WriteLine("Caught: {0}", e.Message);
                }

                //not calling response exception
                try
                {
                    dmem.clearRequest(simClock);
                    dmem.requestRead(0, simClock);
                    dmem.clock(simClock);
                    dmem.requestWrite(0,0, simClock);                
                }

                catch (Exception e)
                {
                    Console.WriteLine("Caught: {0}", e.Message);
                }

                //out of bounds addresses requeasted
                try
                {
                    dmem.clearRequest(simClock);
                    dmem.requestRead(15, simClock);  
                }
                catch (Exception e)
                {
                    Console.WriteLine("Caught: {0}", e.Message);
                }

                //response without request
                try
                {
                    dmem.clearRequest(simClock);
                    dmem.response(simClock);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Caught: {0}", e.Message);
                }

                //clocked too many times exception
                try
                {
                    dmem.clearRequest(simClock);
                    dmem.requestRead(0,simClock);
                    dmem.clock(simClock);
                    dmem.clock(simClock);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Caught: {0}", e.Message);
                }

            }

        }
    }
    public class DataMemory
    {


        //Data memory is 2 cycle dependent, so nothing will happen even if a request is called unless clock is also called
        //.request has two variations readRequeast, and writeRequeast
        //.Clock initiates second stage of memory access, so the thing requested in the input function will be finalized
        //in the output function
        //.response finalizez whatever was requested, if called on a request that has not been clocked that will cause an exception
        //Data memory only has load word (4bytes) but it can load them from any starting position


        private bool clocked;
        private int numberOfTimesClocked;
        private List<byte> dataMemory;
        private int readData;
        public int length;
        public List<List<byte>> history;

        private enum RequestType
        {
            read, write, none
        };
        RequestType requestMade;

        public DataMemory(int size)
        {
            dataMemory = new List<byte>(new byte[size*4]);
            clocked = false;
            length = size * 4;
            readData = 0;
            requestMade = RequestType.none;
            numberOfTimesClocked = 0;
            history = new List<List<byte>>();
        }

        public bool requestRead(int address, int clockCount)
        {
            throwResponseAlreadyMadeExceptionIfResponseAlreadyMade(clockCount);

            throwOutOfBoundsExceptionIfAddressOutOfBounds(address, clockCount);
            byte[] requestedData = dataMemory.GetRange(address, 4).ToArray();
            readData = BitConverter.ToInt32(requestedData, 0);

            requestMade = RequestType.read;

            return true;
        }

        public bool requestWrite(int address, int data, int clockCount)
        {

            throwResponseAlreadyMadeExceptionIfResponseAlreadyMade(clockCount);

            throwOutOfBoundsExceptionIfAddressOutOfBounds(address, clockCount);

            byte[] writeData = BitConverter.GetBytes(data);
            dataMemory[address] = writeData[0];
            dataMemory[address + 1] = writeData[1];
            dataMemory[address + 2] = writeData[2];
            dataMemory[address + 3] = writeData[3];

            requestMade = RequestType.write;

            return true;
        }

        public bool clock(int clockCount)
        {
            if(numberOfTimesClocked<1 && requestMade != RequestType.none)
            {
                numberOfTimesClocked++;
                clocked = !clocked;
            }
            return clocked;
        }

        public int response(int clockCount)
        {
            throwMemoryNotClockedExceptionIfNotClocked(clockCount);
            numberOfTimesClocked = 0;

            if (requestMade == RequestType.read)
            {
                requestMade = RequestType.none;
                return readData;
            }
            else if (requestMade == RequestType.write)
            {
                requestMade = RequestType.none;
                return 1; //nothing to return if write operation was requeasted
            }
            else
            {
                throwNoRequeastMadeException(clockCount);
            }
            return 0;
        }

        private void throwOutOfBoundsExceptionIfAddressOutOfBounds(int address, int clockCount)
        {
            if (address < 0 || address > length - 4)
            {
                throw new Exception("Data memory access violation at clock " + clockCount + ", address " + address + " out of range");
            }
        }

        private void throwClockedTooManyTimesException(int clockCount)
        {
            throw new Exception("Data memory access violation at clock " + clockCount + ", can't clock multiple times without finalizing with call to response");
        }

        private void throwNoRequeastMadeException(int clockCount)
        {
            throw new Exception("Data memory access violation at clock cycle " + clockCount + ", no request made");
        }

        private void throwResponseAlreadyMadeExceptionIfResponseAlreadyMade(int clockCount)
        {
            if (requestMade != RequestType.none)
            {
                throw new Exception("Data memory access violation at clock cycle " + clockCount + ", requeast already made");
            }
        }

        public void dumpDataMemory(int clockCount)
        {
            Console.WriteLine("Begin Data memory dump at clock {0}", clockCount);

            for (int i = 0; i <= length - 4; i += 4)
            {
                byte[] portion = dataMemory.GetRange(i, 4).ToArray();
                Console.WriteLine("Address {0}: {1} {2} {3} {4} , in decimal: {5}", i, portion[3], portion[2], portion[1], portion[0], BitConverter.ToInt32(portion,0));
            }

            Console.WriteLine("End Data memory dump");
        }

        private void throwMemoryNotClockedExceptionIfNotClocked(int clockCount)
        {
            if (!clocked)
            {
                throw new Exception("Data memory access violation at clock cycle " + clockCount + ", memory has not been clocked");
            }
            else
            {
                clocked = false;
            }
        }

        public void clearRequest(int clockCount)
        {
            requestMade = RequestType.none;
            Console.WriteLine("Data memory request clear at clock: {0}", clockCount);
        }

        public void saveContentsToHistory(int clockCount)
        {
            history.Add(new List<byte>(dataMemory));
        }

        public bool requestWasMade()
        {
            return requestMade != RequestType.none;
        }
        public bool requestWasNotMade()
        {
            return !requestWasMade();
        }
    }
}
