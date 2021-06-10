using System;
namespace r4000sim
{
    public class ForwardingUnitLoadStore
    {
        public ForwardingUnitLoadStore()
        {
        }
        public bool checkForForward(
            out int SWdata, //output
            Register EXDF_RD, //Register inputs to DF stage
            Register DSTC_RD, Register TCWB_RD, //RD registers from stages ahead
            int DSTC_RDdata, int TCWB_RDdata, //data in those registers
            DF EXDF, WB DSTC, WB TCWB  //relevant control signals
        )
        {
            bool forwardOccured = false;

            //default values for SW, will be ignored if no forwarding occured
            SWdata = 0;

            //we are allowed to store 0 here
            //memcontrol = 2 for sw

            if (EXDF.memControl == 2 && DSTC.writeToRegisterFile && DSTC_RD == EXDF_RD)
            {
                SWdata = DSTC_RDdata;
                forwardOccured = true;
            }
            else if (EXDF.memControl == 2 && TCWB.writeToRegisterFile && TCWB_RD == EXDF_RD)
            {
                SWdata = TCWB_RDdata;
                forwardOccured = true;
            }

            return forwardOccured;
        }
    }
}