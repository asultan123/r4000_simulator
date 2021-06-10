using System;
using System.Collections.Generic;

namespace r4000sim
{
    public class Simulator
    {

        public Assembler ass;

        //IF
        public ProgramCounter pc = new ProgramCounter();

        //IS
        public InstructionMemory imem;

        //RF
        public ControlUnit control = new ControlUnit();
        public RegisterFile reg = new RegisterFile(16);
        public Stack stack = new Stack();
        public BranchPredictor bp = new BranchPredictor();
        public HazardUnit hazardUnit = new HazardUnit();

        //EX
        public ALU alu = new ALU();
        public ForwardingUnit forwardingUnit = new ForwardingUnit();

        //DF
        public DataMemory dmem = new DataMemory(16);
        public ForwardingUnitLoadStore forwardingUnitLoadStore = new ForwardingUnitLoadStore();

        public Buffer IFIS = new Buffer("IFIS");
        public Buffer ISRF = new Buffer("ISRF");
        public Buffer RFEX = new Buffer("RFEX");
        public Buffer EXDF = new Buffer("EXDF");
        public Buffer DFDS = new Buffer("DFDS");
        public Buffer DSTC = new Buffer("DSTC");
        public Buffer TCWB = new Buffer("TCWB");

        public ControlBuffer RFEXcontrolSignals = new ControlBuffer("RFEXcontrol");
        public ControlBuffer EXDFcontrolSignals = new ControlBuffer("EXDFcontrol");
        public ControlBuffer DFDScontrolSignals = new ControlBuffer("DFDScontrol");
        public ControlBuffer DSTCcontrolSignals = new ControlBuffer("DSTCcontrol");
        public ControlBuffer TCWBcontrolSignals = new ControlBuffer("TCWBcontrol");

        IF ifControlSignals = new IF();
        int pcJumpTarget;
        public List<List<int>> pcHistory = new List<List<int>>();

        bool endOperation = false;

        public Simulator(string pathToassemblyFile)
        {
            ass = new Assembler(pathToassemblyFile);
            imem = new InstructionMemory(ass.instructions);
        }
         
        public int run()
        {
            int clockCount = 0;

            List<List<int>> stagesHistory = new List<List<int>>();
            while(endOperation == false)
            {
                Console.WriteLine("IF: CurrentPC: {0}", pc.getCurrentAddress() + 1);
                int numberOfStagesThatHaveReachedLastInst = 0;
                List<int> pcInStages = new List<int>();

                pcInStages.Add(pc.getCurrentAddress() + 1);
                pcInStages.AddRange(Buffer.dumpPCs(clockCount));

                int prevStage = pcInStages[1];
                int stage;

                for (int i = 2; i < pcInStages.Count; i++)
                {
                    stage = pcInStages[i];
                    if(stage == prevStage && stage == imem.getInstructionCount())
                    {
                        numberOfStagesThatHaveReachedLastInst++;
                        if (numberOfStagesThatHaveReachedLastInst == 6)
                        {
                            endOperation = true;
                        }
                    }
                    prevStage = stage;
                }
            
                //log all data
                pcHistory.Add(pcInStages);

                dmem.saveContentsToHistory(clockCount);
                reg.saveContentsToHistory(clockCount);
                bp.saveContentsToHistory(clockCount);
                stack.saveContentsToHistory(clockCount);

                //run
                WBStage(clockCount);
                TCStage(clockCount);
                DSStage(clockCount);
                DFStage(clockCount);
                EXStage(clockCount);
                RFStage(clockCount);
                ISStage(clockCount);
                IFStage(clockCount);

                //clock all elements
                Buffer.clockAll(clockCount);
                ControlBuffer.clockAll(clockCount);
                dmem.clock(clockCount);
                hazardUnit.clock();
                pc.clock();
                clockCount++;
            }

            dmem.saveContentsToHistory(clockCount);
            reg.saveContentsToHistory(clockCount);
            bp.saveContentsToHistory(clockCount);
            stack.saveContentsToHistory(clockCount);

            Buffer.resetBuffers();
            ControlBuffer.resetBuffers();
            return clockCount;
            //&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&??

            //Here is the part where al the outps are available
            //you have BP table history
            //reg history
            //pcHistory
            //dmem history
            //each is there own data structure, they are ready after the simulation ends

            //while (true)
            //{


            //    console.write("enter clock no: ");
            //    int clock = convert.toint32(console.readline());
            //    if (clock < reg.history.count)
            //    {
            //        console.writeline("\n");

            //        console.writeline("register file");
            //        for (int i = 0; i < 16; i++)
            //        {
            //            console.writeline("reg {0}: {1}", i, reg.history[clock][i]);
            //        }

            //        console.writeline("\n data memory \n");
            //        for (int i = 0; i < dmem.length; i++)
            //        {
            //            console.writeline("dmem add {0}: {1}", i, dmem.history[clock][i]);
            //        }

            //        console.writeline("\n contents of stages \n");
            //        console.writeline("if {0} is {1} rf {2} ex {3} df {4} ds {5} tc {6} wb {7}",
            //                          pchistory[clock][0], pchistory[clock][1], pchistory[clock][2],
            //                          pchistory[clock][3], pchistory[clock][4], pchistory[clock][5],
            //                          pchistory[clock][6], pchistory[clock][7]);
            //    }
            //}

            //for (int i = 0; i < pcHistory.Count; i++)
            //{
            //    Console.WriteLine("{0} clock" , i);
            //    Console.Write(pcHistory[i]);
            //}
        }
        void WBStage(int clockCount)
        {
            //Get control signals and data
            WB wbControlSignals = (WB)TCWBcontrolSignals.output("WB");
            int dataMemoryResult = TCWB.output("dataMem");
            int aluResult = TCWB.output("aluResult");
            Register rd = (Register)TCWB.output("rd");

            //do stuff
            if(wbControlSignals.writeToRegisterFile)
            {
                //WBdataSelect = writebackdata ALU->false, DM->true
                int writeBackValue = (wbControlSignals.dataSelect) ? dataMemoryResult : aluResult;
                reg.write(rd,writeBackValue);
            }

            //if(TCWB.output("pcPlusOne") == imem.getInstructionCount())
            //{
            //    endOperation = true;
            //}
        }
        void TCStage(int clockCount)
        {
            //just forwarding the inputs

            //get inputs
            int dataMem = DSTC.output("dataMem");
            int aluResult = DSTC.output("aluResult");
            int pcPlusOne = DSTC.output("pcPlusOne");
            Register rd = (Register)DSTC.output("rd");
            WB wbControlSignals = (WB)DSTCcontrolSignals.output("WB");

            //forward
            TCWBcontrolSignals.input("WB",new WB(wbControlSignals));
            TCWB.input("dataMem",dataMem);
            TCWB.input("aluResult", aluResult);
            TCWB.input("pcPlusOne", pcPlusOne);
            TCWB.input("rd", (int)rd);

        }
        void DSStage(int clockCount)
        {
            //generally just forwarding, DMEM response aqquired is forwarded here

            //get inputs to be forwarded
            WB wbControlSignals = (WB)DFDScontrolSignals.output("WB");
            Register rd = (Register)DFDS.output("rd");
            int pcPlusOne = DFDS.output("pcPlusOne");
            int aluResult = DFDS.output("aluResult");

            //try to forward dmem result
            if(dmem.requestWasMade())
            {
                int dataMem = dmem.response(clockCount);
                DSTC.input("dataMem",dataMem);
            }

            //forward  rest of inputs
            DSTC.input("rd",(int)rd);
            DSTC.input("aluResult",aluResult);
            DSTC.input("pcPlusOne", pcPlusOne);
            DSTCcontrolSignals.input("WB",new WB(wbControlSignals));

        }
        void DFStage(int clockCount)
        {
            //get inputs to be forwarded
            WB wbControlSignals = (WB)EXDFcontrolSignals.output("WB");
            int pcPlusOne = EXDF.output("pcPlusOne");
            int aluResult = EXDF.output("aluResult");

            //forwarding irrelevant sinals
            DFDScontrolSignals.input("WB", new WB(wbControlSignals));
            DFDS.input("pcPlusOne", pcPlusOne);
            DFDS.input("aluResult", aluResult);


            //get inputs relevant to current stage
            Register EXDF_RD = (Register)EXDF.output("rd");
            int EXDF_RDdata = EXDF.output("writeMemoryData");

            Register DSTC_RD = (Register)DSTC.output("rd");
            int DSTC_RDdata = DSTC.output("dataMem");
            WB DSTC_WB = (WB)DSTCcontrolSignals.output("WB"); 

            Register TCWB_RD = (Register)TCWB.output("rd");
            int TCWB_RDdata = TCWB.output("dataMem");
            WB TCWB_WB = (WB)TCWBcontrolSignals.output("WB"); 

            DF dfControlSignals = (DF)EXDFcontrolSignals.output("DF");

            int address = aluResult;
            int storeDataResult;
            int fowardedData;

            bool forward = forwardingUnitLoadStore.checkForForward(
                out fowardedData, EXDF_RD,
                DSTC_RD, TCWB_RD,
                DSTC_RDdata,TCWB_RDdata,
                dfControlSignals,DSTC_WB,TCWB_WB
            );

            if(forward)
            {
                storeDataResult = fowardedData;
            }
            else
            {
                storeDataResult = EXDF_RDdata;
            }

            if(dfControlSignals.memControl == 2)
            {
                dmem.requestWrite(address,storeDataResult,clockCount);
            }
            else if(dfControlSignals.memControl == 3)
            {
                dmem.requestRead(address,clockCount);
            }

            DFDS.input("rd", (int)EXDF_RD);

        }
        void EXStage(int clockCount)
        {
            EX exControlSignals = (EX)RFEXcontrolSignals.output("EX");

            WB wbControlSignals = (WB)RFEXcontrolSignals.output("WB");
            EXDFcontrolSignals.input("WB",new WB(wbControlSignals));

            DF dfControlSignals = (DF)RFEXcontrolSignals.output("DF");
            EXDFcontrolSignals.input("DF", new DF(dfControlSignals));

            int pcPlusOne = RFEX.output("pcPlusOne");

            int rsData = RFEX.output("rsData");
            int rtData = RFEX.output("rtData");
            int immediate = RFEX.output("immediate");
            int target = RFEX.output("target");

            Register rs = (Register)RFEX.output("rs");
            Register rt = (Register)RFEX.output("rt");
            Register rd = (Register)RFEX.output("rd");

            int forwardedRs;
            int forwardedRt;

            WB DSTCwb = (WB)DSTCcontrolSignals.output("WB");
            WB TCWBwb = (WB)TCWBcontrolSignals.output("WB");
            WB EXDFwb = (WB)EXDFcontrolSignals.output("WB");
            WB DFDSwb = (WB)DFDScontrolSignals.output("WB");

            int DSTC_RDdata;
            int TCWB_RDdata;

            if(DSTCwb.dataSelect)
            {
                DSTC_RDdata = DSTC.output("dataMem");
            }
            else 
            {
                DSTC_RDdata = DSTC.output("aluResult");
            }

            if(TCWBwb.dataSelect)
            {
                TCWB_RDdata = TCWB.output("dataMem");
            }
            else
            {
                TCWB_RDdata = TCWB.output("aluResult");
            }

            ForwardingResult fResult = forwardingUnit.checkForForward(
                out forwardedRs, out forwardedRt,
                rs, rt,
                (Register)EXDF.output("rd"), (Register)DFDS.output("rd"), (Register)DSTC.output("rd"), (Register)TCWB.output("rd"),
                EXDF.output("aluResult"),DFDS.output("aluResult"),DSTC_RDdata,TCWB_RDdata,
                EXDFwb,DFDSwb,DSTCwb,TCWBwb            
            );

            if(fResult == ForwardingResult.ForwardRs)
            {
                rsData = forwardedRs;
            }
            else if(fResult == ForwardingResult.ForwardRt)
            {
                rtData = forwardedRt;
            }
            else if(fResult == ForwardingResult.both)
            {
                rtData = forwardedRt;
                rsData = forwardedRs;
            }

            EXDF.input("writeMemoryData",rtData);

            ifControlSignals = (IF)RFEXcontrolSignals.output("IF"); //define if controls here

            if (ifControlSignals.jumpToProcedure)
            {
                stack.pushPCToStack(pcPlusOne);
                pcJumpTarget = target;
                IFIS.flush(clockCount);
                ISRF.flush(clockCount);
            }
            else if (ifControlSignals.returnFromProcedure)
            {
                int oldPC = stack.popPCFromStack();
                pcJumpTarget = oldPC;
                IFIS.flush(clockCount);
                ISRF.flush(clockCount);
            }

            // IFjumpControl: no jump->0, branch->1, j->2, jr->3
            else if (ifControlSignals.IFjumpControl == 2)
            {
                IFIS.flush(clockCount);
                ISRF.flush(clockCount);
                pcJumpTarget = target;
            }

            // IFjumpControl: no jump->0, branch->1, j->2, jr->3
            else if (ifControlSignals.IFjumpControl == 3)
            {
                IFIS.flush(clockCount);
                ISRF.flush(clockCount);
                pcJumpTarget = reg.read(rs);
            }

            int pcTarget;
            bool rollback = bp.verifyBranch(RFEX.output("pcPlusOne"), out pcTarget, ifControlSignals, Operation.ble, rsData, rtData, immediate);
            if (rollback)
            {
                IFIS.flush(clockCount);
                ISRF.flush(clockCount);
                pcJumpTarget = pcTarget;
            }

            if(exControlSignals.useImmediateInAlu)
            {
                rtData = immediate;
            }

            rd = (exControlSignals.registerSelect) ? rd : rt;

            int aluResult = alu.calculate(rsData,rtData,exControlSignals.ALUControl);

            EXDF.input("aluResult", aluResult);
            EXDF.input("pcPlusOne", RFEX.output("pcPlusOne"));
            EXDF.input("rd",(int)rd);
        }
        void RFStage(int clockCount)
        {
            Operation op = (Operation)ISRF.output("op");
            Register rs = (Register)ISRF.output("rs");
            Register rt = (Register)ISRF.output("rt");
            Register rd = (Register)ISRF.output("rd");
            int target = ISRF.output("target");
            int immediate = ISRF.output("immediate");

            int pcPlusOne = ISRF.output("pcPlusOne");

            control.decode(op);

            int rsData = reg.read(rs);
            int rtData = reg.read(rt);

            bool stall = hazardUnit.checkHazard(
                (Register)RFEX.output("rt"), (Register)EXDF.output("rd"),
                (Register)ISRF.output("rs"), (Register)ISRF.output("rt"),
                (DF)RFEXcontrolSignals.output("DF"), (DF)EXDFcontrolSignals.output("DF"),
                control.dfControlSignals
            );

            IFIS.setStall(stall);
            ISRF.setStall(stall);
            pc.setStall(stall);

            RFEX.input("immediate",immediate);
            RFEX.input("rsData", rsData);
            RFEX.input("rtData", rtData);
            RFEX.input("rs",(int)rs);
            RFEX.input("rt",(int)rt);
            RFEX.input("rd", (int)rd);
            RFEX.input("target", target);

            if (stall)
            {
                control.zeroEXDFWBControlSignals();
                RFEX.input("pcPlusOne", 0);
            }
            else
            {
                RFEX.input("pcPlusOne", pcPlusOne);
            }

            if(op == Operation.add && rs == Register.r0 && rd == Register.r0 && rt == Register.r0)
            {
                //nop
                control.zeroEXDFWBControlSignals();
            }

            RFEXcontrolSignals.input("IF", new IF(control.ifControlSignals));
            RFEXcontrolSignals.input("EX", new EX(control.exControlSignals));
            RFEXcontrolSignals.input("DF", new DF(control.dfControlSignals));
            RFEXcontrolSignals.input("WB", new WB(control.wbControlSignals));

        }
        void ISStage(int clockCount)
        {
            int instructionAddress;
            int pcPlusOne;

            bool tryGetInst = IFIS.output("instructionAddress", out instructionAddress);
            bool tryGetpcPlusOne = IFIS.output("pcPlusOne", out pcPlusOne);

            if(tryGetInst && tryGetpcPlusOne)
            {
                Instruction inst = imem.getInstruction(instructionAddress);
                ISRF.input("op", (int)inst.op);
                ISRF.input("rs", (int)inst.rs);
                ISRF.input("rt", (int)inst.rt);
                ISRF.input("rd", (int)inst.rd);
                ISRF.input("target", inst.target);
                ISRF.input("immediate", inst.immediate);
                ISRF.input("pcPlusOne", pcPlusOne);
            }
        }
        void IFStage(int clockCount)
        {

            if(bp.predict(pc.getCurrentAddress(), out int target))
            {
                //check if there's an ovveride, if there is dont send anything down pipe, just correct
                //pc and it will be sent down pipe in next cycle
                if (ifControlSignals.jumpToProcedure || ifControlSignals.returnFromProcedure
                || ifControlSignals.IFjumpControl == 2 || ifControlSignals.IFjumpControl == 3
                || ifControlSignals.IFjumpControl == 1)
                {
                    //if a correction occurs, or and dont send, 
                    pc.setCurrentAddress(pcJumpTarget);

                }
                else
                {
                    //if no override, forward current inst for verification of prediction
                    //in next cycle, prediction target will be forwarded
                    if (pc.getCurrentAddress() < imem.getInstructionCount())
                    {
                        IFIS.input("instructionAddress", pc.getCurrentAddress());
                        IFIS.input("pcPlusOne", pc.getCurrentAddress() + 1);
                    }
                    pc.setCurrentAddress(target);
                }
            }
            //else if there's an ovverride do it
            else if (ifControlSignals.jumpToProcedure || ifControlSignals.returnFromProcedure
                || ifControlSignals.IFjumpControl == 2 || ifControlSignals.IFjumpControl == 3
                || ifControlSignals.IFjumpControl == 1)
            {
                //if a correction occurs, or and dont send, 
                pc.setCurrentAddress(pcJumpTarget);

            }
            //if there is no prediction, and no override, send current pc down pipe and next clock is pc+1
            else{
                if (pc.getCurrentAddress() < imem.getInstructionCount())
                {
                    IFIS.input("instructionAddress", pc.getCurrentAddress());
                    IFIS.input("pcPlusOne", pc.getCurrentAddress() + 1);
                }
            }
        }
    }
}
