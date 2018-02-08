using System.Collections.Generic;

namespace CUHKSelfCheckLauncher
{
    public class ProcessChain
    {
        List<List<int>> processChainList = new List<List<int>>();

        public void addProcess(int processId, int parentProcessId)
        {
            List<int> parentProcessChain = null;
            List<int> childProcessChain = null;

            foreach (List<int> processChain in processChainList)
            {
                if (processChain.Contains(processId) && !processChain.Contains(parentProcessId))
                {
                    processChain.Insert(processChain.IndexOf(processId), parentProcessId);
                    childProcessChain = processChain;
                }
                if (!processChain.Contains(processId) && processChain.Contains(parentProcessId))
                {
                    processChain.Insert(processChain.IndexOf(parentProcessId) + 1, processId);
                    parentProcessChain = processChain;
                }
                if (processChain.Contains(processId) && processChain.Contains(parentProcessId))
                {
                    return;
                }
            }
            if (parentProcessChain == null && childProcessChain == null)
            {
                List<int> newProcessChain = new List<int>();
                newProcessChain.Add(parentProcessId);
                newProcessChain.Add(processId);
                processChainList.Add(newProcessChain);
            }
            else if (parentProcessChain != null && childProcessChain != null)
            {
                foreach (int pprocessId in parentProcessChain)
                {
                    if (childProcessChain.IndexOf(pprocessId) >= 0)
                        childProcessChain.Remove(pprocessId);
                }
                parentProcessChain.AddRange(childProcessChain);
                processChainList.Remove(childProcessChain);
            }
        }

        public List<List<int>> GetProcessChains()
        {
            return processChainList;
        }

        public void Clear()
        {
            processChainList.Clear();
        }

        public override string ToString()
        {
            string result = "";
            foreach (List<int> processChain in processChainList)
            {
                if (result.Length > 0)
                    result += ", ";
                foreach (int processId in processChain)
                {
                    if (result.Length > 0)
                        result += " - ";
                    result += processId;
                }
            }
            return result;
        }
    }
}