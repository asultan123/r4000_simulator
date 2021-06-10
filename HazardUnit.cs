using System;
namespace r4000sim
{
    public static class HazardUnit_UnitTest
    {
        //success
        public static void runUnitTest()
        {
            HazardUnit hazardUnit = new HazardUnit();
            DF dfControlSignals = new DF();
            dfControlSignals.memControl = 3;

            //bool shouldStall;

            //trigger stall
            //shouldStall = hazardUnit.checkHazard(Register.r0,Register.r1,Register.r0,dfControlSignals);

            hazardUnit.clock();

            //no stall trigger, should return true
            //shouldStall = hazardUnit.checkHazard(Register.r0, Register.r1, Register.r2, dfControlSignals);

            hazardUnit.clock();

            //no stall trigger, should return false bec 2 cycles have passed
            //shouldStall = hazardUnit.checkHazard(Register.r0, Register.r1, Register.r2, dfControlSignals);

            hazardUnit.clock();

            //should return false because > 2 clock cycles have passed
            //shouldStall = hazardUnit.checkHazard(Register.r0, Register.r1, Register.r2, dfControlSignals);

            //trigger stall
            //shouldStall = hazardUnit.checkHazard(Register.r0, Register.r1, Register.r0, dfControlSignals);

            hazardUnit.clock();

            //trigger stall
            //shouldStall = hazardUnit.checkHazard(Register.r0, Register.r1, Register.r0, dfControlSignals);

            //check hazard Unit, clock remaining should be 2



        }
    }

    public class HazardUnit
    {
        private int clockCyclesRemainingForStall;
        public HazardUnit()
        {
            clockCyclesRemainingForStall = 0;
        }
        public bool checkHazard(Register RFEX_RT, Register EXDF_RD, Register ISRF_RS, Register ISRF_RT, DF RFEXdf, DF EXDFdf, DF currentStageDFControlSignals)
        {
            if(RFEXdf.memControl == 3) //lw detecting load-immediate use
            {
                if(RFEX_RT == ISRF_RS || RFEX_RT == ISRF_RT)
                {
                    clockCyclesRemainingForStall = 2;
                }
            }

            //if(EXDFdf.memControl == 3)
            //{
            //    if (EXDF_RD == ISRF_RS || EXDF_RD == ISRF_RT)
            //    {
            //        clockCyclesRemainingForStall = 1;
            //    }
            //}

            if(RFEXdf.memControl == 3 && currentStageDFControlSignals.memControl == 2) //load then store in pipe, this is lw-sw hazard
            {
                if (RFEX_RT == ISRF_RS || RFEX_RT == ISRF_RT)
                {
                    clockCyclesRemainingForStall = 1; //only one stall cycle is needed for lw-sw hazard
                }
            }
            return clockCyclesRemainingForStall != 0;
        }
        public void clock()
        {
            clockCyclesRemainingForStall = (clockCyclesRemainingForStall > 0) ? clockCyclesRemainingForStall - 1 : clockCyclesRemainingForStall;
        }
    }
}
