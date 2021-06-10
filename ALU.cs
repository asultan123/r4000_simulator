using System;
namespace r4000sim
{
    public class ALU
    {

        public ALU()
        {
        }

        public int calculate(int opA, int opB, int controlSignal)
        {
            switch (controlSignal)
            {
                case 0:
                    return opA + opB;
                case 1:
                    return opA ^ opB;
                case 2:
                    return (opA < opB)?1:0;
                default:
                    return 0;
            }
        }

    }
}
