using System;
using System.Collections.Generic;

namespace r4000sim
{
    public static class InstructionMemory_UnitTest
    {
        public static void runUnitTest(Assembler ass, bool throwException = false)
        {
            InstructionMemory imem = new InstructionMemory(ass.instructions);
            Random rand = new Random();

            for (int i = 0; i < 15; i++)
            {
                int pc = rand.Next(0, imem.getInstructionCount());
                Console.WriteLine("Instruction at pc: {0}", pc);
                imem.getInstruction(pc).description();
            }

            if(throwException)
            {
                try 
                {
                    int pc = imem.getInstructionCount() + 1;
                    imem.getInstruction(pc).description();
                }
                catch(Exception e)
                {
                    Console.WriteLine("Caught Exception: {0}", e.Message);
                }
            }

        }
    }

    public class InstructionMemory
    {
        private List<Instruction> instructions;
        public InstructionMemory(int size)
        {
            instructions = new List<Instruction>(size);
        }
        public InstructionMemory(List<Instruction> newInstructions)
        {
            this.instructions = newInstructions;
        }
        public Instruction getInstruction(int address)
        {
            throwOutOfRangeExceptionIfPCOutOfRange(address);
            return instructions[address];
        }
        private void throwOutOfRangeExceptionIfPCOutOfRange(int pc)
        {
            if(pc<0 || pc>instructions.Count)
            {
                throw new Exception("Invalid PC");
            }
        }
        public int getInstructionCount(){
            return instructions.Count;
        }
    }
}
