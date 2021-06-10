using System;
using System.Collections.Generic;

namespace r4000sim
{
    public struct BranchPredictorEntry
    {
        public int pc;
        public int target;
        public BranchPrediction state;
    }

    public static class BranchPredictor_UnitTest
    {
        public static void runTest()
        {
            //success

            //BranchPredictor br = new BranchPredictor();
            //int target;
            //bool willFlushAndBranch;
            //bool willBranch;
            //IF ifControlSignal = new IF();
            //ifControlSignal.IFjumpControl = 2;
            //Operation op = Operation.ble;

            //add strongly branch entry
            //willFlushAndBranch = br.verifyBranch(1, out target, ifControlSignal, op, 1, 2, 5);
            //willFlushAndBranch = br.verifyBranch(2, out target, ifControlSignal, op, 1, 2, 5);
            //willFlushAndBranch = br.verifyBranch(3, out target, ifControlSignal, op, 1, 2, 5);
            //willFlushAndBranch = br.verifyBranch(4, out target, ifControlSignal, op, 1, 2, 5);

            ////test result
            //willBranch = br.predict(0, out target);
            //willBranch = br.predict(1, out target);
            //willBranch = br.predict(2, out target);
            //willBranch = br.predict(3, out target);

            ////fail condition 1 once, should result in roll back, and going back to target at adter pc = 0 
            ////which is one, here I'm sending pc+1 because thats what the branch predictor has access to
            //willFlushAndBranch = br.verifyBranch(1, out target, ifControlSignal, op, 3, 2, 5);

            ////test result, should still branch
            //willBranch = br.predict(0, out target);

            ////fail condition 1 once, should result in roll back, and going back to target at 1
            //willFlushAndBranch = br.verifyBranch(1, out target, ifControlSignal, op, 3, 2, 5);

            ////test result, should not branch
            //willBranch = br.predict(0, out target);

            ////entry doesnt exist, condition will succeed so will flush and go to target
            //willFlushAndBranch = br.verifyBranch(7, out target, ifControlSignal, op, 1, 2, 5);

            ////entry now exists, condition will succeed so will not do anything
            //willFlushAndBranch = br.verifyBranch(7, out target, ifControlSignal, op, 1, 2, 5);

            ////test result, should still branch
            //willBranch = br.predict(6, out target);

            ////entry doesnt exists, condition will fail so will not do anything
            //willFlushAndBranch = br.verifyBranch(9, out target, ifControlSignal, op, 3, 2, 5);

            ////test result, should not branch because no entry was made
            //willBranch = br.predict(8, out target);

        }
    }

    public class BranchPredictor
    {


        Dictionary<int, BranchPredictorEntry> table;

        List<List<BranchPredictorEntry>> history;

        public BranchPredictor()
        {
            history = new List<List<BranchPredictorEntry>>();
            table = new Dictionary<int, BranchPredictorEntry>();
        }


        //called in IF stage
        private bool shouldBranch(int pc, out BranchPredictorEntry res )
        {
            table.TryGetValue(pc, out res);
            return res.state == BranchPrediction.stronglyBranch || res.state == BranchPrediction.weaklyBranch;
        }

        private int getBranchTarget(int pc)
        {
            BranchPredictorEntry res;
            table.TryGetValue(pc, out res);
            return res.target;
        }

        public void saveContentsToHistory(int clockCount)
        {
            history.Add(new List<BranchPredictorEntry>(table.Values));
        }


        //called in IF stage
        public bool predict(int pc, out int target)
        {
            BranchPredictorEntry result;
            bool entryExists = table.TryGetValue(pc, out result);
            if(entryExists && (result.state == BranchPrediction.stronglyBranch || result.state == BranchPrediction.weaklyBranch))
            {
                target = result.target;
                return true;
            }
            else
            {
                target = 0;
                return false;
            }
        }

        //called in RF stage
        public bool verifyBranch(int pcPlusOne, out int pcTarget,IF ifControlSignal,Operation op, int RSdata, int RTdata, int offset) 
        {
            bool branchWillOccur = false; //determines whether a flush to pc and an update are necessary

            if(ifControlSignal.IFjumpControl == 1 && op == Operation.ble) //branch (ble is the only one)
            {
                //verify whether condition is successful or not
                if(RSdata <= RTdata)
                {
                    //successful
                    int pc = pcPlusOne - 1;

                    //does an entry already exist?
                    if (getEntry(pc, out BranchPredictorEntry result))
                    {
                        //entry exists
                        if(result.state == BranchPrediction.stronglyBranch || result.state == BranchPrediction.weaklyBranch)
                        {
                            //its last state is branch
                            //it must have been taken in a previous if stage
                            //since condition is true, there's nothing to be done except
                            //increase state level and set target to dummy 0
                            branchWillOccur = false; //do nothing
                            updateEntry(pc, increaseState: true);
                            pcTarget = 0;
                        }
                        else
                        {
                            //its last state is dont branch
                            //it could not have been take in a previous if stage
                            //condition is true, branch was not taken so it will be taken now
                            //increase state level and set target to target in table
                            branchWillOccur = true; //branch
                            updateEntry(pc,increaseState: true);
                            pcTarget = result.target;
                        }
                    }
                    else
                    {
                        //entry does not exist
                        //it could not have been take in a previous if stage
                        //condition is true, branch was never taken so it will be taken now
                        //create entry for the first time in table with target pc+1+offset
                        //set target to pc+1+offset
                        branchWillOccur = true;
                        addEntry(pc,target: pcPlusOne + offset);
                        pcTarget = pcPlusOne + offset;
                    }
                }
                else
                {
                    //condition failed
                    int pc = pcPlusOne - 1;
                    if (getEntry(pc, out BranchPredictorEntry result))
                    {
                        //entry exists
                        if (result.state == BranchPrediction.stronglyBranch || result.state == BranchPrediction.weaklyBranch)
                        {
                            //its last state is branch
                            //it must have been taken in a previous if stage
                            //since condition is false, we have to roll back the prediction
                            //decrease state level and set target to pcPlusOne to undo effect
                            branchWillOccur = true;
                            updateEntry(pc, increaseState: false);
                            pcTarget = pcPlusOne;
                        }
                        else
                        {
                            //its last state is dont branch
                            //it could not have been take in a previous if stage
                            //since condition is false, we dont have to roll back anything
                            //decrease state level and set target to dummy 0
                            branchWillOccur = false;
                            updateEntry(pc, increaseState: false);
                            pcTarget = 0;
                        }
                    }
                    else
                    {
                        //entry does not exist
                        //it could not have been take in a previous if stage
                        //since condition is false, we dont have to roll back anything or add an entry
                        //set target to dummy 0
                        branchWillOccur = false;
                        pcTarget = 0;
                    }
                }

            }
            else
            {
                //this is not a branch instruction
                //nothing will happen
                branchWillOccur = false;
                pcTarget = 0;
            }

            return branchWillOccur;
        }

        private bool getEntry(int pc,out BranchPredictorEntry res)
        {
            return table.TryGetValue(pc, out res);
        }

        private void addEntry(int pc, int target)
        {
            BranchPredictorEntry entry;
            entry.pc = pc;
            entry.target = target;
            entry.state = BranchPrediction.stronglyBranch;
            table.Add(pc, entry);
        }

        private void updateEntry(int pc, bool increaseState)
        {
            BranchPredictorEntry entry;
            table.TryGetValue(pc, out entry);

            if(increaseState == true)
            {
                //increase confidence
                int stateAsInteger = (int)entry.state;
                stateAsInteger = (stateAsInteger < 3) ? stateAsInteger + 1 : stateAsInteger;
                entry.state = (BranchPrediction)stateAsInteger;
            }
            else
            {
                //decrease confidence
                int stateAsInteger = (int)entry.state;
                stateAsInteger = (stateAsInteger > 0) ? stateAsInteger - 1 : stateAsInteger;
                entry.state = (BranchPrediction)stateAsInteger;

            }
            //update entry
            table[pc] = entry;
        }
    }
}
