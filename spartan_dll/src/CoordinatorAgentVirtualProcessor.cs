using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Proteus;

namespace Spartan
{
    public class CoordinatorAgentVirtualProcessor
    {
        public int ProcessorId;
        private WipCommand _objWipCommand;
        public WipCommand WipCommand { get { return _objWipCommand; } private set { } }
        public ProcessorState ProcessorState;
        public int LastQueryStateStamp = System.Environment.TickCount;
        public int RequestIdentifier = -1; // Stored so we know who sent - when we are sending packets to the lcient.

        public List<WipCommand> CommandHistory = new List<WipCommand>();

        public void CreateWipCommand(WipCommand wc)
        {
            if (_objWipCommand != null)
                throw new Exception("Wip command was not null ** must be set to null in order to create new. Possible error.");
            _objWipCommand = wc;
        }
        public void ClearWipCommand()
        {
            if (_objWipCommand != null)
                CommandHistory.Add(_objWipCommand);
            _objWipCommand = null;
        }
        public CoordinatorAgentVirtualProcessor(int processorId)
        {
            ProcessorState = ProcessorState.Unknown;
            ProcessorId = processorId;
        }
        public void ClearState(bool wipCanBeNull = false)
        {
            if (WipCommand == null)
                if (wipCanBeNull == false)
                    // ** we were getting errors
                    // the wip can be null if there was no available processor.
                    Globals.Logger.LogError("Failed to remove wip... processor had no wip command.");

            RequestIdentifier = -1;
            ClearWipCommand();
            ProcessorState = ProcessorState.Unknown;
            LastQueryStateStamp = System.Environment.TickCount;
        }

    }
}
