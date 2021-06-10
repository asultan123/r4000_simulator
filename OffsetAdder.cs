using System;
namespace r4000sim
{
    public class OffsetAdder
    {
        ALU alu;
        public OffsetAdder()
        {
            alu = new ALU();
        }
        public int calculateOffset(int pc, int offset){
            pc = pc + 1;
            return alu.calculate(pc, offset, 0);
        }
    }
}
