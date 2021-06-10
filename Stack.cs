using System;
using System.Collections.Generic;
using System.Collections;

namespace r4000sim
{
    public class Stack
    {
        int maxEntries;
        int currentEntries;
        Stack<int> pcStack;
        public List<List<int>> history;

        public Stack()
        {
            history = new List<List<int>>();
            currentEntries = 0;
            maxEntries = 4;
            pcStack = new Stack<int>(4);
        }

        public void pushPCToStack(int pc)
        {
            if(currentEntries < maxEntries)
            {
                pcStack.Push(pc);
                currentEntries++;
            }
            else
            {
                throw new Exception("Stack Overflow");
            }
        }
        public int popPCFromStack()
        {
            if(currentEntries>0)
            {
                return pcStack.Pop();
            }
            else
            {
                throw new Exception("Stack Empty");
            }
        }
        public void saveContentsToHistory(int clockCount)
        {
            history.Add(new List<int>(pcStack.ToArray()));
        }
    }
}
