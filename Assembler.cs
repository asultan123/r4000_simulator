using System;
using System.Collections.Generic;
using System.Linq;

namespace r4000sim
{
    public struct Instruction
    {
        public Type type;
        public Operation op;
        public Register rs;
        public Register rt;
        public Register rd;
        public int target;
        public int immediate;

        public void description()
        {
            Console.WriteLine("Type: {0}, Op: {1}, rs: {2}, rt: {3}, rd: {4}, target: {5}, immediate: {6}",
                              this.type,this.op,this.rs,this.rt,this.rd,this.target,this.immediate);
        }
    }

    public class Assembler
    {
        private System.IO.StreamReader file;
        public List<Instruction> instructions;
        private Dictionary<string, int> labels;
        private int lineNumber;
        string filepath;

        public Assembler(string filepath)
        {

            this.filepath = filepath;
            instructions = new List<Instruction>();
            labels = new Dictionary<string, int>();
            parseLabels();
            parseInstructions();
        }

        private void parseInstructions()
        {

            lineNumber = 0;
            string line;
            file = new System.IO.StreamReader(@filepath);

            while ((line = file.ReadLine()) != null)
            {

                line = line.ToLower();

                String[] tokens = line.Split(new string[] { ",", "(", ")", " " }, StringSplitOptions.RemoveEmptyEntries);

                //ignore label, also taking care of rp instruction
                if (tokens.Length == 1 && !EnumHelper.Isoperation(tokens[0]))
                {
                    lineNumber++;
                    continue;
                }

                //not label, begin parsing
                Operation op = new Operation();
                if (!EnumHelper.Isoperation(tokens[0], ref op))
                {
                    throw new Exception(getCurrentLineNumber() + ", not operation"); ;
                }

                Type type = EnumHelper.getType(op);

                Instruction instruction = new Instruction();

                instruction.op = op;
                instruction.type = type;

                if (type == Type.rType)
                {
                    instruction = parseRTypeInstruction(tokens, instruction);
                }
                else if (type == Type.iType)
                {
                    instruction = parseITypeinstruction(tokens, instruction);
                }
                else if(type == Type.jType)
                {
                    instruction = parseJTypeinstruction(tokens, instruction);
                }
                else
                {
                    instruction = parsePsudoInstruction(tokens, instruction);
                }

                instructions.Add(instruction);

                //not label
                lineNumber++;
            }

        }

        private Instruction parsePsudoInstruction(string[] tokens, Instruction result)
        {
            if(result.op == Operation.jp)
            {
                if(tokens.Length == 2)
                {
                    return parseJTypeinstruction(tokens, result);
                }
                else
                {
                    throw new Exception("Invalide parameters for psudo Instruction at line " + getCurrentLineNumber());
                }
            }
            else
            {
                //Only op is set, so rp is basically pop stack and push to PC
            }
            return result;
        }

        private void checkNumberOfTokens(string[] tokens, int numberOFTokens)
        {
            if (tokens.Length != numberOFTokens)
            {
                throw new Exception(getCurrentLineNumber() + " Invalid number of parameters.");
            }
        }

        private Instruction parseJTypeinstruction(string[] tokens, Instruction result)
        {
            checkNumberOfTokens(tokens, 2);

            int targetLabelIndex = new int();
            if(getLabelIndex(tokens[1],ref targetLabelIndex))
            {
                result.target = targetLabelIndex - labels.Where((label) => label.Value < targetLabelIndex).Count();
                Console.WriteLine("Line number {0}, Operation: {1}, Target: {2}",getCurrentLineNumber(), tokens[0], tokens[1],result.target);
            }
            else
            {
                throw new Exception(getCurrentLineNumber() + " Invalid label."); ;
            }

            return result;
        }

        private Instruction parseITypeinstruction(string[] tokens, Instruction result)
        {

            checkNumberOfTokens(tokens, 4);

            //parse Itype Inst that contain labels
            if (result.op == Operation.ble)
            {
                if (EnumHelper.Isregister(tokens[1], ref result.rt) && EnumHelper.Isregister(tokens[2], ref result.rs))
                {
                    int labelIndex = new int();
                    if (getLabelIndex(tokens[3], ref labelIndex))
                    {
                        result.immediate = labelIndex - lineNumber;
                        Console.WriteLine("Line number {0}, Operation: {1}, Registers: {2} {3}, Offset: {4}",getCurrentLineNumber(), tokens[0], tokens[1], tokens[2], result.immediate);
                    }
                    else
                    {
                        throw new Exception(getCurrentLineNumber() + " unknown label.");
                    }
                }
                else
                {
                    throw new Exception(getCurrentLineNumber() + "invalide register paremeters for itype inst.");
                }

            }
            //parse Itype Inst that contain immediate as second paremeter
            else if(result.op == Operation.lw || result.op == Operation.sw)
            {
                if (EnumHelper.Isregister(tokens[1], ref result.rt) && EnumHelper.Isregister(tokens[3], ref result.rs))
                {
                    if (int.TryParse(tokens[2], out result.immediate))
                    {
                        Console.WriteLine("Line number {0}, Operation: {1}, Registers: {2} {3}, Imm: {4}",getCurrentLineNumber(), tokens[0], tokens[1], tokens[2], result.immediate);
                    }
                    else
                    {
                        throw new Exception(getCurrentLineNumber() + " invalid immediate specified.");
                    }

                }
                else
                {
                    throw new Exception(getCurrentLineNumber() + " invalide register paremeters for itype inst.");
                }
            }

            //parse IType Inst that contain immediate as last paremeter
            else
            {
                if (EnumHelper.Isregister(tokens[1], ref result.rt) && EnumHelper.Isregister(tokens[2], ref result.rs))
                {
                    if (int.TryParse(tokens[3], out result.immediate))
                    {
                        Console.WriteLine("Line number {0}, Operation: {1}, Registers: {2} {3}, immediate: {4}",getCurrentLineNumber(), tokens[0], tokens[1], tokens[2], result.immediate);
                    }
                    else
                    {
                        throw new Exception(getCurrentLineNumber() + " invalid immediate specified.");
                    }
                }
                else
                {
                    throw new Exception(getCurrentLineNumber() + "invalide register paremeters for itype inst.");
                }
            }


            return result;
        }

        private Instruction parseRTypeInstruction(string[] tokens, Instruction result)
        {

            if (result.op == Operation.jr)
            {
                checkNumberOfTokens(tokens, 2);
                if (EnumHelper.Isregister(tokens[1], ref result.rs))
                {
                    Console.WriteLine("Line number {0}, Operation: {1}, Registers: {2}",getCurrentLineNumber(), tokens[0], tokens[1]);
                }
                else
                {
                    throw new Exception(getCurrentLineNumber() + "Invalid registers specified for RType Inst."); ;
                }
            }
            else
            {
                checkNumberOfTokens(tokens, 4);

                if (EnumHelper.Isregister(tokens[1], ref result.rs) && EnumHelper.Isregister(tokens[2], ref result.rt) && EnumHelper.Isregister(tokens[3], ref result.rd))
                {
                    Console.WriteLine("Line number {0}, Operation: {1}, Registers: {2} {3} {4}",getCurrentLineNumber(), tokens[0], tokens[1], tokens[2], tokens[3]);
                }
                else
                {
                    throw new Exception(getCurrentLineNumber() + " Invalid registers specified for RType Inst."); ;
                }
            }

            return result;
        }

        private bool getLabelIndex(string label, ref int index)
        {
            return labels.TryGetValue(label, out index);
        }

        private string getCurrentLineNumber(){
            return (lineNumber + 1).ToString();
        }

        public List<Instruction> getInstructions()
        {
            return this.instructions;
        }

        private void parseLabels()
        {
            lineNumber = 0;
            string line;
            file = new System.IO.StreamReader(@filepath);

            while ((line = file.ReadLine()) != null)
            {
                line = line.ToLower();
                String[] tokens = line.Split(',', '(', ')', ' ', ';');

                if (tokens.Length == 1 && !EnumHelper.Isoperation(tokens[0]))
                {
                    if(labels.ContainsKey(tokens[0])){
                        throw new Exception("Invalid label at line " + lineNumber.ToString() + " duplicate label");
                    }else{
                        labels.Add(tokens[0], lineNumber);
                    }
                }
                lineNumber++;
            }
        }
    }
}