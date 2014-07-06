using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP579Assemblerv2 {
    public struct TriLineOfCode {
        public int LocationCounter;
        public string Label;
        public int LabelValue;
        public e_Keywords OpCode;
        public e_Directives Directive;
        public e_OperationSize OpSize;
        public e_OperationMode OpMode;
        public e_AddressingMode AddrMode;
        public int SourceOperandDisplacement;
        public string SourceOperand;
        public int DestinationOperandDisplacement;
        public string DestinationOperand;
        public string[] DirectivesOperands;
        public int lineNumber;
        public string LabelOperand;

    }

    public enum e_LabelType {
        XTR,
        REL,
        STC,
    }

    public struct TriAddressSymbolTableValues {
        public int Value;
        public e_LabelType LabelType;

        public override string ToString () {
            return Value.ToString() + ", " + LabelType.ToString();
        }

    }

    public class TriTableOfContens {

        /// <summary>
        /// Stores the Symbole (Labels) and their values
        /// </summary>
        public Dictionary<string, TriAddressSymbolTableValues> AddressSymbolTable;
        public int InvalidValue = 0x10000;
        public List<TriLineOfCode> LinesOfCodes;
        public List<string> ENTlabels;
        public TriTableOfContens () {
            initialize();
        }

        public void initialize () {
            LinesOfCodes = new List<TriLineOfCode>();
            AddressSymbolTable = new Dictionary<string, TriAddressSymbolTableValues>();
            ENTlabels = new List<string>();
        }


    }
}
