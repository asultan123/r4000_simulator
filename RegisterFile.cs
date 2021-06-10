using System;
using System.Collections.Generic;

namespace r4000sim
{
    public static class RegisterFile_UnitTest
    {
        public static void runUnitTest()
        {
            RegisterFile regs = new RegisterFile(16);
            int r1Val, r2Val;

            Console.WriteLine("\n Writing to register 15 the value 32 \n");

            regs.write(Register.r15,32);

            Console.WriteLine("\n Writing to register 1 and 2 the values 1 and 2 \n");

            regs.write(Register.r1, Register.r2,1,2);

            Console.WriteLine("\n Writing to register 0 the value 4 \n");
            regs.write(Register.r0,4);

            regs.dumpRegisterValues();

            Console.WriteLine("Reading individual register");

            regs.read(Register.r1, Register.r2,out r1Val, out r2Val);
            Console.WriteLine("r1: {0}, r2: {1}",r1Val,r2Val);

            regs.read(Register.r0, out r1Val);
            Console.WriteLine("r0: {0}",r1Val);

            regs.read(Register.r15,out r1Val);
            Console.WriteLine("r15: {0}",r1Val);
        }
    }
    public class RegisterFile
    {
        private int[] registers;
        private int size;
        public List<List<int>> history;

        public RegisterFile(int size)
        {
            this.size = size;
            registers = new int[size];
            history = new List<List<int>>();
            Console.WriteLine("Instantiated Register file with size {0}", size);
        }

        private int getRegAtIndex(Register index)
        {
            throwOutOfBoundsExceptionAtIndexViolation(index);
            return (index == Register.r0) ? 0 : registers[(int)index];
        }

        private void setRegAtIndex(Register index, int value)
        {
            throwOutOfBoundsExceptionAtIndexViolation(index);
            if((int)index == 0)
            {
                Console.WriteLine("Attempt to modify 0 register made, no change will occur");
            }
            else
            {
                registers[(int)index] = value;
            }
        }

        public void read(Register r1, Register r2, out int r1Val,out int r2Val)
        {
            r1Val = getRegAtIndex(r1);
            r2Val = getRegAtIndex(r2);
        }

        public void read(Register r1, out int r1Val)
        {
            r1Val = getRegAtIndex(r1);
        }

        public void write(Register r1, Register r2, int r1Val, int r2Val)
        {
            setRegAtIndex(r1, r1Val);
            setRegAtIndex(r2, r2Val);
        }

        public void write(Register r1, int r1Val)
        {
            setRegAtIndex(r1, r1Val);
        }

        public void read(int r1, int r2, out int r1Val, out int r2Val)
        {
            r1Val = getRegAtIndex((Register)r1);
            r2Val = getRegAtIndex((Register)r2);
        }

        public void read(int r1, out int r1Val)
        {
            r1Val = getRegAtIndex((Register)r1);
        }

        public int read(Register r1)
        {
            return getRegAtIndex(r1);
        }

        public void write(int r1, int r2, int r1Val, int r2Val)
        {
            setRegAtIndex((Register)r1, r1Val);
            setRegAtIndex((Register)r2, r2Val);
        }

        public void write(int r1, int r1Val)
        {
            setRegAtIndex((Register)r1, r1Val);
        }

        private void throwOutOfBoundsExceptionAtIndexViolation(Register index)
        {
            if ((int)index < 0 || (int)index > size)
            {
                throw new Exception("Invalid register index:" + index.ToString() + "specified");
            }
        }

        public void dumpRegisterValues()
        {
            Console.WriteLine("\n Dumping register values \n");
            for (int i = 0; i < registers.Length; i++)
            {
                Console.WriteLine("Index: {0}, Value: {1}",i,registers[i]);
            }
        }

        public void saveContentsToHistory(int clockCount)
        {
            history.Add(new List<int>(registers));
        }
    }
}
