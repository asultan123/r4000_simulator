using System;
namespace r4000sim
{
    public enum ForwardingResult{
        ForwardRs,ForwardRt, both, noForward
    }
    public class ForwardingUnit
    {
        public ForwardingUnit()
        {
            
        }

        public ForwardingResult checkForForward(
            out int RS, out int RT, //output
            Register RFEX_RS,Register RFEX_RT, //Register inputs to EX stage
            Register EXDF_RD,Register DFDS_RD,Register DSTC_RD,Register TCWB_RD, //RD registers from units
            int EXDF_RDdata, int DFDS_RDdata, int DSTC_RDdata, int TCWB_RDdata, //data in those registers
            WB EXDF, WB DFDS, WB DSTC, WB TCWB
        )
        {
            ForwardingResult forwardOccured = ForwardingResult.noForward;
            //default values for RS, and RT, will be ignored if no forwarding occured
            RS = 0;
            RT = 0;
            bool forwardRs = false, forwardRt = false;

            //check and forward for RS
            //if RD register is register 0, this s meaningless, reg 0 is constatnt 0
            if(EXDF.writeToRegisterFile && EXDF_RD != Register.r0 && EXDF_RD == RFEX_RS)
            {
                RS = EXDF_RDdata;
                forwardRs = true;
            }
            else if(DFDS.writeToRegisterFile && DFDS_RD != Register.r0 && DFDS_RD == RFEX_RS)
            {
                RS = DFDS_RDdata;
                forwardRs = true;
            }
            else if (DSTC.writeToRegisterFile && DSTC_RD != Register.r0 && DSTC_RD == RFEX_RS)
            {
                RS = DSTC_RDdata;
                forwardRs = true;
            }
            else if (TCWB.writeToRegisterFile && TCWB_RD != Register.r0 && TCWB_RD == RFEX_RS)
            {
                RS = TCWB_RDdata;
                forwardRs = true;
            }

            //check and forward for RT

            if (EXDF.writeToRegisterFile && EXDF_RD != Register.r0 && EXDF_RD == RFEX_RT)
            {
                RT = EXDF_RDdata;
                forwardRt = true;
            }
            else if (DFDS.writeToRegisterFile && DFDS_RD != Register.r0 && DFDS_RD == RFEX_RT)
            {
                RT = DFDS_RDdata;
                forwardRt = true;
            }
            else if (DSTC.writeToRegisterFile && DSTC_RD != Register.r0 && DSTC_RD == RFEX_RT)
            {
                RT = DSTC_RDdata;
                forwardRt = true;
            }
            else if (TCWB.writeToRegisterFile && TCWB_RD != Register.r0 && TCWB_RD == RFEX_RT)
            {
                RT = TCWB_RDdata;
                forwardRt = true;
            }

            if(forwardRs && forwardRt)
            {
                forwardOccured = ForwardingResult.both;
            }
            else if(forwardRs)
            {
                forwardOccured = ForwardingResult.ForwardRs;
            }
            else if(forwardRt)
            {
                forwardOccured = ForwardingResult.ForwardRt;
            }

            return forwardOccured;
        }
    }
}
