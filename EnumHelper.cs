using System;
using System.Collections.Generic;

namespace r4000sim
{
    public enum Operation
    {
        add, addi, xor, lw, sw, ble, j, slt, jr, jp, rp
    };

    public enum Register
    {
        r0, r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15
    };

    public enum Type
    {
        iType, rType, jType, psudo
    };

    public enum MemOperation{
        read,write,nochange
    }

    public enum BranchPrediction{
        stronglyDontBranch,weaklyDontBranch,weaklyBranch,stronglyBranch
    }

    static class EnumHelper
    {
        public static bool Isoperation(string query, ref Operation res)
        {
            return Enum.TryParse<Operation>(query, out res);
        }

        public static bool Isoperation(string query)
        {
            Operation res = new Operation();
            return Isoperation(query, ref res);
        }

        public static bool Isregister(string query, ref Register res)
        {
            return Enum.TryParse<Register>(query, out res);
        }

        public static bool Isregister(string query)
        {

            Register res = new Register();
            return Isregister(query, ref res);
        }

        public static bool Ismemoperation(string query, ref MemOperation res)
        {
            return Enum.TryParse<MemOperation>(query, out res);
        }

        public static Type getType(Operation op)
        {

            if (op == Operation.add || op == Operation.jr || op == Operation.xor || op == Operation.slt)
            {
                return Type.rType;
            }
            else if (op == Operation.addi || op == Operation.lw || op == Operation.sw || op == Operation.ble)
            {   
                return Type.iType;
            }
            else if (op == Operation.j)
            {
                return Type.jType;
            }
            else if (op == Operation.jp || op == Operation.rp)
            {
                return Type.psudo;
            }
            else
            {
                throw new Exception("Invalid Operation Type");
            }

        }

    }
}
