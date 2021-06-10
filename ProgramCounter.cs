using System;
namespace r4000sim
{
    //when input is called PC is updated and when clocked the set value is what is available
    //normally clocking increments pc, but if pc address was updated then clocking just allows the info 
    //to be presented
    public class ProgramCounter
    {
        private int currentAddress;
        private bool currentAddressSet;
        private bool stalled;

        public ProgramCounter()
        {
            currentAddressSet = false;
            currentAddress = 0;
            stalled = false;
        }
        public void clock()
        {
            if(!stalled && !currentAddressSet){
                currentAddress++;
                currentAddressSet = false;
            }
        }
        public void setCurrentAddress(int address)
        {
            currentAddressSet = true;
            currentAddress = address; 
        }
        public int getCurrentAddress()
        {
            currentAddressSet = false;
            return this.currentAddress;
        }
        public void setStall(bool stall)
        {
            this.stalled = stall;
        }
    }
}
