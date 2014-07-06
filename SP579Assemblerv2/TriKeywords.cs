using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP579Assemblerv2 {
    public enum e_Keywords {
        LD = 0,
        STR,
        LSP,
        EXG,
        ADD,
        SUB,
        MULS,
        DIVU,
        MIN,
        AND,
        NOT,
        OR,
        ASHR,
        LSHL,
        RCR,
        PUSH,
        POP,
        CMP,
        BTST,
        BSS,
        BCLR,
        BRA,
        BEQ,
        BNE,
        BCS,
        BLT,
        BSUB,
        RSUB,
        HLT,
        TRP,
        RTRP,
        NOP,
    }

    public enum e_Directives {
        ORG,
        EQU,
        DC,
        DS,
        EXT,
        ENT,
        END,
        NONE,
    }

    public enum e_AddressingMode {
        RegisterIndirectWithDisplacementAddressing = 1,
        RegisterAddressing = 2,
        ImmidiateAddressing = 4,
        AbsoluteAddressing = 8,
        None = 16
    }

    public enum e_OperationSize {
        Byte = 1,
        Word = 2,
        None = 4
    }

    public enum e_OperationMode {
        PCplusOffset = 1,
        Address = 2,
        None = 4
    }

    public struct TriInstruction {

        public string Name;
        public int OperationCode;
        public int MaxNumberOfOperands;
        public e_OperationSize OperationSize;
        public e_AddressingMode AddressingMode;
        public e_OperationMode OperationMode;
        public object OtherData; //maybe??


    }

    public class TriKeywords {
        /// <summary>
        /// Containts the location of all keyword in the code
        /// Updated on every change in the code
        /// int: the location in the text
        /// string: the keyword
        /// </summary>
        public List<TriInstruction> Instructions;
        //public Dictionary<char, List<string>> KeywordTree;

        //public int NumberOfKeywords;
        //public List<string> NameOfKeywords;

        public TriKeywords () {
            Instructions = new List<TriInstruction>();
            //KeywordTree = new Dictionary<char, List<string>>();

            #region Initialize Instruction Information
            int i = 0;
            TriInstruction temp = new TriInstruction();

            temp.Name = "LD";
            temp.OperationCode = i++; temp.MaxNumberOfOperands = 2;
            temp.OperationSize = e_OperationSize.Byte
                               | e_OperationSize.Word;
            temp.AddressingMode = e_AddressingMode.AbsoluteAddressing
                                | e_AddressingMode.ImmidiateAddressing
                                | e_AddressingMode.RegisterAddressing
                                | e_AddressingMode.RegisterIndirectWithDisplacementAddressing;
            temp.OperationMode = e_OperationMode.None;
            Instructions.Add( temp );

            temp.Name = "STR";
            temp.OperationCode = i++; temp.MaxNumberOfOperands = 2;
            temp.OperationSize = e_OperationSize.Byte
                               | e_OperationSize.Word;
            temp.AddressingMode = e_AddressingMode.AbsoluteAddressing
                                | e_AddressingMode.RegisterIndirectWithDisplacementAddressing;
            temp.OperationMode = e_OperationMode.None;
            Instructions.Add( temp );

            temp.Name = "LSP";
            temp.OperationCode = i++; temp.MaxNumberOfOperands = 1;
            temp.OperationSize = e_OperationSize.Word;
            temp.AddressingMode = e_AddressingMode.AbsoluteAddressing
                                | e_AddressingMode.ImmidiateAddressing
                                | e_AddressingMode.RegisterAddressing;
            temp.OperationMode = e_OperationMode.None;
            Instructions.Add( temp );

            temp.Name = "EXG";
            temp.OperationCode = i++; temp.MaxNumberOfOperands = 2;
            temp.OperationSize = e_OperationSize.Byte
                               | e_OperationSize.Word;
            temp.AddressingMode = e_AddressingMode.AbsoluteAddressing
                                | e_AddressingMode.RegisterIndirectWithDisplacementAddressing;
            temp.OperationMode = e_OperationMode.None;
            Instructions.Add( temp );

            temp.Name = "ADD";
            temp.OperationCode = i++; temp.MaxNumberOfOperands = 2;
            temp.OperationSize = e_OperationSize.Byte
                               | e_OperationSize.Word;
            temp.AddressingMode = e_AddressingMode.ImmidiateAddressing
                                | e_AddressingMode.RegisterAddressing;
            temp.OperationMode = e_OperationMode.None;
            Instructions.Add( temp );

            temp.Name = "SUB";
            temp.OperationCode = i++; temp.MaxNumberOfOperands = 2;
            temp.OperationSize = e_OperationSize.Byte
                               | e_OperationSize.Word;
            temp.AddressingMode = e_AddressingMode.ImmidiateAddressing
                                | e_AddressingMode.RegisterAddressing;
            temp.OperationMode = e_OperationMode.None;
            Instructions.Add( temp );

            temp.Name = "MULS";
            temp.OperationCode = i++; temp.MaxNumberOfOperands = 2;
            temp.OperationSize = e_OperationSize.Byte;
            temp.AddressingMode = e_AddressingMode.ImmidiateAddressing
                                | e_AddressingMode.RegisterAddressing;
            temp.OperationMode = e_OperationMode.None;
            Instructions.Add( temp );

            temp.Name = "DIVU";
            temp.OperationCode = i++; temp.MaxNumberOfOperands = 2;
            temp.OperationSize = e_OperationSize.Byte
                               | e_OperationSize.Word;
            temp.AddressingMode = e_AddressingMode.ImmidiateAddressing
                                | e_AddressingMode.RegisterAddressing;
            temp.OperationMode = e_OperationMode.None;
            Instructions.Add( temp );

            temp.Name = "MIN";
            temp.OperationCode = i++; temp.MaxNumberOfOperands = 2;
            temp.OperationSize = e_OperationSize.Byte
                               | e_OperationSize.Word;
            temp.AddressingMode = e_AddressingMode.ImmidiateAddressing
                                | e_AddressingMode.RegisterAddressing;
            temp.OperationMode = e_OperationMode.None;
            Instructions.Add( temp );

            temp.Name = "AND";
            temp.OperationCode = i++; temp.MaxNumberOfOperands = 2;
            temp.OperationSize = e_OperationSize.Byte
                               | e_OperationSize.Word;
            temp.AddressingMode = e_AddressingMode.AbsoluteAddressing
                                | e_AddressingMode.ImmidiateAddressing
                                | e_AddressingMode.RegisterAddressing;
            temp.OperationMode = e_OperationMode.None;
            Instructions.Add( temp );

            temp.Name = "NOT";
            temp.OperationCode = i++; temp.MaxNumberOfOperands = 2;
            temp.OperationSize = e_OperationSize.Byte
                               | e_OperationSize.Word;
            temp.AddressingMode = e_AddressingMode.ImmidiateAddressing
                                | e_AddressingMode.RegisterAddressing;
            temp.OperationMode = e_OperationMode.None;
            Instructions.Add( temp );

            temp.Name = "OR";
            temp.OperationCode = i++; temp.MaxNumberOfOperands = 2;
            temp.OperationSize = e_OperationSize.Byte
                               | e_OperationSize.Word;
            temp.AddressingMode = e_AddressingMode.ImmidiateAddressing
                                | e_AddressingMode.RegisterAddressing;
            temp.OperationMode = e_OperationMode.None;
            Instructions.Add( temp );

            temp.Name = "ASHR";
            temp.OperationCode = i++; temp.MaxNumberOfOperands = 2;
            temp.OperationSize = e_OperationSize.Byte
                               | e_OperationSize.Word;
            temp.AddressingMode = e_AddressingMode.ImmidiateAddressing
                                | e_AddressingMode.RegisterAddressing;
            temp.OperationMode = e_OperationMode.None;
            Instructions.Add( temp );

            temp.Name = "LSHL";
            temp.OperationCode = i++; temp.MaxNumberOfOperands = 2;
            temp.OperationSize = e_OperationSize.Byte
                               | e_OperationSize.Word;
            temp.AddressingMode = e_AddressingMode.ImmidiateAddressing
                                | e_AddressingMode.RegisterAddressing;
            temp.OperationMode = e_OperationMode.None;
            Instructions.Add( temp );

            temp.Name = "RCR";
            temp.OperationCode = i++; temp.MaxNumberOfOperands = 2;
            temp.OperationSize = e_OperationSize.Byte
                               | e_OperationSize.Word;
            temp.AddressingMode = e_AddressingMode.ImmidiateAddressing
                                | e_AddressingMode.RegisterAddressing;
            temp.OperationMode = e_OperationMode.None;
            Instructions.Add( temp );

            temp.Name = "PUSH";
            temp.OperationCode = i++; temp.MaxNumberOfOperands = 1;
            temp.OperationSize = e_OperationSize.Byte
                               | e_OperationSize.Word;
            temp.AddressingMode = e_AddressingMode.RegisterAddressing;
            temp.OperationMode = e_OperationMode.None;
            Instructions.Add( temp );

            temp.Name = "POP";
            temp.OperationCode = i++; temp.MaxNumberOfOperands = 1;
            temp.OperationSize = e_OperationSize.Byte
                               | e_OperationSize.Word;
            temp.AddressingMode = e_AddressingMode.RegisterAddressing;
            temp.OperationMode = e_OperationMode.None;
            Instructions.Add( temp );

            // Not gonna get used
            temp.Name = "CMP";
            temp.OperationCode = i++; temp.MaxNumberOfOperands = 2;
            temp.OperationSize = e_OperationSize.Byte
                               | e_OperationSize.Word;
            temp.AddressingMode = e_AddressingMode.AbsoluteAddressing
                                | e_AddressingMode.ImmidiateAddressing
                                | e_AddressingMode.RegisterAddressing
                                | e_AddressingMode.RegisterIndirectWithDisplacementAddressing;
            temp.OperationMode = e_OperationMode.None;
            Instructions.Add( temp );

            temp.Name = "BTST";
            temp.OperationCode = i++; temp.MaxNumberOfOperands = 2;
            temp.OperationSize = e_OperationSize.Byte
                               | e_OperationSize.Word;
            temp.AddressingMode = e_AddressingMode.ImmidiateAddressing
                                | e_AddressingMode.RegisterAddressing;
            temp.OperationMode = e_OperationMode.None;
            Instructions.Add( temp );

            temp.Name = "BSS";
            temp.OperationCode = i++; temp.MaxNumberOfOperands = 2;
            temp.OperationSize = e_OperationSize.Byte
                               | e_OperationSize.Word;
            temp.AddressingMode = e_AddressingMode.RegisterAddressing;
            temp.OperationMode = e_OperationMode.None;
            Instructions.Add( temp );

            temp.Name = "BCLR";
            temp.OperationCode = i++; temp.MaxNumberOfOperands = 2;
            temp.OperationSize = e_OperationSize.Byte
                               | e_OperationSize.Word;
            temp.AddressingMode = e_AddressingMode.ImmidiateAddressing
                                | e_AddressingMode.RegisterAddressing;
            temp.OperationMode = e_OperationMode.None;
            Instructions.Add( temp );

            temp.Name = "BRA";
            temp.OperationCode = i++; temp.MaxNumberOfOperands = 1;
            temp.OperationSize = e_OperationSize.None;
            temp.AddressingMode = e_AddressingMode.None;
            temp.OperationMode = e_OperationMode.Address
                               | e_OperationMode.PCplusOffset;
            Instructions.Add( temp );

            temp.Name = "BEQ";
            temp.OperationCode = i++; temp.MaxNumberOfOperands = 1;
            temp.OperationSize = e_OperationSize.None;
            temp.AddressingMode = e_AddressingMode.None;
            temp.OperationMode = e_OperationMode.Address
                               | e_OperationMode.PCplusOffset;
            Instructions.Add( temp );

            temp.Name = "BNE";
            temp.OperationCode = i++; temp.MaxNumberOfOperands = 1;
            temp.OperationSize = e_OperationSize.None;
            temp.AddressingMode = e_AddressingMode.None;
            temp.OperationMode = e_OperationMode.Address
                               | e_OperationMode.PCplusOffset;
            Instructions.Add( temp );

            temp.Name = "BCS";
            temp.OperationCode = i++; temp.MaxNumberOfOperands = 1;
            temp.OperationSize = e_OperationSize.None;
            temp.AddressingMode = e_AddressingMode.None;
            temp.OperationMode = e_OperationMode.Address
                               | e_OperationMode.PCplusOffset;
            Instructions.Add( temp );

            temp.Name = "BLT";
            temp.OperationCode = i++; temp.MaxNumberOfOperands = 1;
            temp.OperationSize = e_OperationSize.None;
            temp.AddressingMode = e_AddressingMode.None;
            temp.OperationMode = e_OperationMode.Address
                               | e_OperationMode.PCplusOffset;
            Instructions.Add( temp );

            temp.Name = "BSUB";
            temp.OperationCode = i++; temp.MaxNumberOfOperands = 1;
            temp.OperationSize = e_OperationSize.None;
            temp.AddressingMode = e_AddressingMode.None;
            temp.OperationMode = e_OperationMode.Address
                               | e_OperationMode.PCplusOffset;
            Instructions.Add( temp );

            temp.Name = "RSUB";
            temp.OperationCode = i++; temp.MaxNumberOfOperands = 0;
            temp.OperationSize = e_OperationSize.None;
            temp.AddressingMode = e_AddressingMode.None;
            temp.OperationMode = e_OperationMode.None;
            Instructions.Add( temp );

            temp.Name = "HLT";
            temp.OperationCode = i++; temp.MaxNumberOfOperands = 0;
            temp.OperationSize = e_OperationSize.None;
            temp.AddressingMode = e_AddressingMode.None;
            temp.OperationMode = e_OperationMode.None;
            Instructions.Add( temp );

            // Not gonna get used
            temp.Name = "TRP";
            temp.OperationCode = i++; temp.MaxNumberOfOperands = 1;
            temp.OperationSize = e_OperationSize.None;
            temp.AddressingMode = e_AddressingMode.None;
            temp.OperationMode = e_OperationMode.None;
            Instructions.Add( temp );

            // Not gonna get used
            temp.Name = "RTRP";
            temp.OperationCode = i++; temp.MaxNumberOfOperands = 0;
            temp.OperationSize = e_OperationSize.None;
            temp.AddressingMode = e_AddressingMode.None;
            temp.OperationMode = e_OperationMode.None;
            Instructions.Add( temp );

            #endregion



            //NameOfKeywords = new List<string>();

            //foreach ( e_Keywords k in Enum.GetValues( typeof( e_Keywords ) ) ) {
            //    NameOfKeywords.Add( k.ToString() );
            //}

            //for ( int i = 0; i < NameOfKeywords.Count; i++ ) {
            //    if ( !KeywordTree.ContainsKey( NameOfKeywords[i][0] ) ) {
            //        KeywordTree.Add( NameOfKeywords[i][0], new List<string>() );
            //    }
            //
            //    KeywordTree[NameOfKeywords[i][0]].Add( NameOfKeywords[i] );
            //}

            /*
            KeywordTree.Add( 'A', new List<string>() ); //3
            KeywordTree['A'].Add( "ADD" );
            KeywordTree['A'].Add( "AND" );
            KeywordTree['A'].Add( "ASHR" );

            KeywordTree.Add( 'B', new List<string>() ); //9
            KeywordTree['B'].Add( "BCLR" );
            KeywordTree['B'].Add( "BCS" );
            KeywordTree['B'].Add( "BEQ" );
            KeywordTree['B'].Add( "BLT" );
            KeywordTree['B'].Add( "BNE" );
            KeywordTree['B'].Add( "BRA" );
            KeywordTree['B'].Add( "BSS" );
            KeywordTree['B'].Add( "BSUB" );
            KeywordTree['B'].Add( "BTST" );

            //KeywordTree.Add( 'C', new List<string>() ); //0
            //KeywordTree['C'].Add( "CMP" );

            KeywordTree.Add( 'D', new List<string>() ); //1
            //KeywordTree['D'].Add( "DC" );
            KeywordTree['D'].Add( "DIVU" );
            //KeywordTree['D'].Add( "DS" );

            KeywordTree.Add( 'E', new List<string>() ); //1
            //KeywordTree['E'].Add( "END" );
            //KeywordTree['E'].Add( "ENT" );
            //KeywordTree['E'].Add( "EQU" );
            KeywordTree['E'].Add( "EXG" );
            //KeywordTree['E'].Add( "EXT" );

            KeywordTree.Add( 'H', new List<string>() ); //1
            KeywordTree['H'].Add( "HLT" );

            KeywordTree.Add( 'L', new List<string>() ); //3
            KeywordTree['L'].Add( "LD" );
            KeywordTree['L'].Add( "LSHL" );
            KeywordTree['L'].Add( "LSP" );

            KeywordTree.Add( 'M', new List<string>() ); //2
            KeywordTree['M'].Add( "MIN" );
            KeywordTree['M'].Add( "MULS" );

            KeywordTree.Add( 'N', new List<string>() ); //1
            KeywordTree['N'].Add( "NOT" );

            KeywordTree.Add( 'O', new List<string>() ); //1
            KeywordTree['O'].Add( "OR" );
            //KeywordTree['O'].Add( "ORG" );

            KeywordTree.Add( 'P', new List<string>() ); //2
            KeywordTree['P'].Add( "POP" );
            KeywordTree['P'].Add( "PUSH" );

            KeywordTree.Add( 'R', new List<string>() ); //2
            KeywordTree['R'].Add( "RCR" );
            KeywordTree['R'].Add( "RSUB" );
            //KeywordTree['R'].Add( "RTRP" );

            KeywordTree.Add( 'S', new List<string>() ); //2
            KeywordTree['S'].Add( "STR" );
            KeywordTree['S'].Add( "SUB" );

            //KeywordTree.Add( 'T', new List<string>() ); //0
            //KeywordTree['T'].Add( "TRP" );

            */

            //NumberOfKeywords = NameOfKeywords.Count;
        }

    }
}
