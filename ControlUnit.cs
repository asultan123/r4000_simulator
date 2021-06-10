using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace r4000sim
{
    public class ControlSignal : Object
    {
        //ControlSignal(ControlSignal rhs)
        //{
        //    if(rhs is EX)
        //    {
        //        //(this as EX).ALUControl = (rhs as EX).ALUControl;
        //    }
        //}
    };
    public class EX : ControlSignal
    {
        public bool useImmediateInAlu, registerSelect;
        public int ALUControl;
        public EX(){
            
        }
        public EX(EX copy)
        {
            this.registerSelect = copy.registerSelect;
            this.useImmediateInAlu = copy.useImmediateInAlu;
        }
    };

    public class DF : ControlSignal
    {
        public int memControl; 
        public DF(){
            
        }
        public DF(DF copy)
        {
            this.memControl = copy.memControl;
        }
    };

    public class WB : ControlSignal
    {
        public bool writeToRegisterFile, dataSelect;
        public WB(){
            
        }
        public WB(WB copy)
        {
            this.writeToRegisterFile = copy.writeToRegisterFile;
            this.dataSelect = copy.dataSelect;
        }
    };

    public class IF : ControlSignal
    {
        public bool jumpToProcedure, returnFromProcedure;
        public int IFjumpControl;
        public IF(){
            
        }
        public IF(IF copy)
        {
            this.IFjumpControl = copy.IFjumpControl;
            this.jumpToProcedure = copy.jumpToProcedure;
            this.returnFromProcedure = copy.returnFromProcedure;
        }
    };

    public class ControlUnit
    {
        public EX exControlSignals = new EX();
        public DF dfControlSignals = new DF();
        public WB wbControlSignals = new WB();
        public IF ifControlSignals = new IF();

        private bool EXdontUseImmediateInAlu, WBwriteToRegisterFile, EXregisterSelect, WBdataSelect, IFjumpToProcedure, IFreturnFromProcedure;
        private int IFjumpControl, DFmemControl, EXALUControl;

        public ControlUnit()
        {
            /*EXdontUseImmediateInAlu: S2 used in ALU->true, immediate used in ALU->false
            IFjumpControl: no jump->0, branch->1, j->2, jr->3
            WBwriteToRegisterFile = write_RF->true, nowrite_RF->false
            EXregisterSelect = writebackreg rt->false, rd->true
            WBdataSelect = writebackdata ALU->false, DM->true
            DFmemControl = noaction->0, sw->2, lw->3
            EXALUControl = add->0, xor->1, slt->2
            IFjumpToProcedure = procedure_jump->true
            IFreturnFromProcedure = return_procedure->true*/

        }
        public void decode(Operation X)
        {
            switch (X)
            {
                case Operation.add:
                    EXdontUseImmediateInAlu = true;
                    IFjumpControl = 0;
                    WBwriteToRegisterFile = true;
                    EXregisterSelect = true;
                    WBdataSelect = false;
                    DFmemControl = 0;
                    EXALUControl = 0;
                    IFjumpToProcedure = false;
                    IFreturnFromProcedure = false;
                    break;
                case Operation.addi:
                    EXdontUseImmediateInAlu = false;
                    IFjumpControl = 0;
                    WBwriteToRegisterFile = true;
                    EXregisterSelect = false;
                    WBdataSelect = false;
                    DFmemControl = 0;
                    EXALUControl = 0;
                    IFjumpToProcedure = false;
                    IFreturnFromProcedure = false;
                    break;
                case Operation.xor:
                    EXdontUseImmediateInAlu = true;
                    IFjumpControl = 0;
                    WBwriteToRegisterFile = true;
                    EXregisterSelect = true;
                    WBdataSelect = false;
                    DFmemControl = 0;
                    EXALUControl = 1;
                    IFjumpToProcedure = false;
                    IFreturnFromProcedure = false;
                    break;
                case Operation.lw:
                    EXdontUseImmediateInAlu = false;
                    IFjumpControl = 0;
                    WBwriteToRegisterFile = true;
                    EXregisterSelect = false;
                    WBdataSelect = true;
                    DFmemControl = 3;
                    EXALUControl = 0;
                    IFjumpToProcedure = false;
                    IFreturnFromProcedure = false;
                    break;
                case Operation.sw:
                    EXdontUseImmediateInAlu = false;
                    IFjumpControl = 0;
                    WBwriteToRegisterFile = false;
                    //EXregisterSelect = true;
                    //WBdataSelect = false;
                    DFmemControl = 2;
                    EXALUControl = 0;
                    IFjumpToProcedure = false;
                    IFreturnFromProcedure = false;
                    break;
                case Operation.ble:
                    EXdontUseImmediateInAlu = true;
                    IFjumpControl = 1;
                    WBwriteToRegisterFile = false;
                    //EXregisterSelect = false;
                    //WBdataSelect = true;
                    DFmemControl = 0;
                    //EXALUControl = false;               done in ID stage for branch prediction
                    IFjumpToProcedure = false;
                    IFreturnFromProcedure = false;
                    break;
                case Operation.j:  //EXdontUseImmediateInAlu = false;
                    IFjumpControl = 2;
                    WBwriteToRegisterFile = false;
                    //EXregisterSelect = true;
                    //WBdataSelect = false;
                    DFmemControl = 0;
                    //EXALUControl = 0;
                    IFjumpToProcedure = false;
                    IFreturnFromProcedure = false;
                    break;
                case Operation.slt:
                    EXdontUseImmediateInAlu = true;
                    IFjumpControl = 0;
                    WBwriteToRegisterFile = true;
                    EXregisterSelect = true;
                    WBdataSelect = false;
                    DFmemControl = 0;
                    EXALUControl = 2;
                    IFjumpToProcedure = false;
                    IFreturnFromProcedure = false;
                    break;
                case Operation.jr: //EXdontUseImmediateInAlu = false;
                    IFjumpControl = 3;
                    WBwriteToRegisterFile = false;
                    //EXregisterSelect = true;
                    //WBdataSelect = false;
                    DFmemControl = 0;
                    //EXALUControl = 0;
                    IFjumpToProcedure = false;
                    IFreturnFromProcedure = false;
                    break;
                case Operation.jp:  //EXdontUseImmediateInAlu = false;
                    IFjumpControl = 2;
                    WBwriteToRegisterFile = false;
                    //EXregisterSelect = true;
                    //WBdataSelect = false;
                    DFmemControl = 0;
                    EXALUControl = 0;
                    IFjumpToProcedure = true;
                    IFreturnFromProcedure = false;
                    break;
                case Operation.rp:  //EXdontUseImmediateInAlu = false;
                    IFjumpControl = 2;                          //similar to j istructions
                    WBwriteToRegisterFile = false;
                    //EXregisterSelect = true;
                    //WBdataSelect = false;
                    DFmemControl = 0;
                    EXALUControl = 0;
                    IFjumpToProcedure = false;
                    IFreturnFromProcedure = true;
                    break;
            }

            exControlSignals.useImmediateInAlu = !EXdontUseImmediateInAlu;
            exControlSignals.ALUControl = EXALUControl;
            exControlSignals.registerSelect = EXregisterSelect;

            ifControlSignals.IFjumpControl = IFjumpControl;
            ifControlSignals.jumpToProcedure = IFjumpToProcedure;
            ifControlSignals.returnFromProcedure = IFreturnFromProcedure;

            dfControlSignals.memControl = DFmemControl;

            wbControlSignals.dataSelect = WBdataSelect;
            wbControlSignals.writeToRegisterFile = WBwriteToRegisterFile;

        }
        public void zeroEXDFWBControlSignals()
        {
            dfControlSignals.memControl = 0;

            wbControlSignals.dataSelect = false;
            wbControlSignals.writeToRegisterFile = false;

            exControlSignals.ALUControl= 0;
            exControlSignals.useImmediateInAlu = false;
            exControlSignals.registerSelect = false;
        }
    }
}
