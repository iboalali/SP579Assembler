using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP579Assemblerv2 {
    public enum e_Error {
        NoError,
        DuplicateOpCode,
        DulicateLabel,
        InvalidLabel,
        InvalidOpCode,
        InvalidOperand,
        InvalidOpSize,
        InvalidORGOperand,
        InvalidAddressingMode,
        InvalidSourceRegister,
        InvalidDestinationRegister,
        LabelIsTooLong,
        LabelStartWithDigit,
        UndefinedLabel,
        OpCodeIsTooLong,
        OpCodeStartWithDigit,
        OpSizeIsTooLong,
        OperandIsTooLong,
        InstructionDoesntWorkWithOpSize,
        WrongOpSize,
        NotOpCodeNorDirective,
        MissingOpSize,
        MissingBrackets,
        MissingLabel,
        MissingOpCode,
        TooManyOpenBrackets,
        TooManyCloseBrackets,
        BracketsNotClosed,
        BracketsNotOpened,
        DirectiveOperandHasInvalidCharacter,
        DcConstantTooLong,
        TooManyOperands,
        TooFewOperands,
        IllegalAddressingMode,
        IllegalCharacter,
        AddressOutsideRange,
        OpCodeAsLabel,
        FirstLineORG,
        ReservedWord,
        MissingOpCodeOrDirective,
        OpCodeAndDirectiveOnSameLine,
        MissingOperands,
        //InvalidBranchAddress,

    }

    public struct TriErrorObject {
        public e_Error error;
        public string data;
        public int lineNumber;
    }

    public class TriError {
        public List<TriErrorObject> Error;
        public Dictionary<e_Error, string> ErrorDescription;
        TriMainWindow form;

        //public int lineNumber;
        TriErrorObject errorObject;

        public TriError (TriMainWindow form) {
            Error = new List<TriErrorObject>();
            ErrorDescription = new Dictionary<e_Error, string>();
            this.form = form;

            #region Error Description

            ErrorDescription.Add( e_Error.DuplicateOpCode, "Duplicate operation code" );
            ErrorDescription.Add( e_Error.InvalidLabel, "Invalid character found" );
            ErrorDescription.Add( e_Error.LabelIsTooLong, "Label is too long" );
            ErrorDescription.Add( e_Error.InvalidOpCode, "Invalid operation code" );
            ErrorDescription.Add( e_Error.BracketsNotClosed, "Brackets was opened but not closed" );
            ErrorDescription.Add( e_Error.BracketsNotOpened, "Brackets was closed but not opened" );
            ErrorDescription.Add( e_Error.InstructionDoesntWorkWithOpSize, "Instruction does not require an operation size" );
            ErrorDescription.Add( e_Error.InvalidOperand, "Invalid operand found" );
            ErrorDescription.Add( e_Error.InvalidOpSize, "Invalid operation size found" );
            ErrorDescription.Add( e_Error.InvalidORGOperand, "Invalid origin operand" );
            ErrorDescription.Add( e_Error.UndefinedLabel, "Label was not found in the address symbol table" );
            ErrorDescription.Add( e_Error.LabelStartWithDigit, "Label starts with a digit" );
            ErrorDescription.Add( e_Error.MissingBrackets, "Brackets are missing" );
            ErrorDescription.Add( e_Error.MissingLabel, "Label is missing" );
            ErrorDescription.Add( e_Error.MissingOpSize, "Operation size is missing" );
            ErrorDescription.Add( e_Error.NotOpCodeNorDirective, "Is not an operation code nor a directive" );
            ErrorDescription.Add( e_Error.OpCodeIsTooLong, "Operation code too long" );
            ErrorDescription.Add( e_Error.OpCodeStartWithDigit, "Operation code starts with digit" );
            ErrorDescription.Add( e_Error.OperandIsTooLong, "Operand is too long" );
            ErrorDescription.Add( e_Error.OpSizeIsTooLong, "Operation size is too long" );
            ErrorDescription.Add( e_Error.TooManyCloseBrackets, "Too many closing brackets" );
            ErrorDescription.Add( e_Error.TooManyOpenBrackets, "Too many opening brackets" );
            ErrorDescription.Add( e_Error.WrongOpSize, "Wrong operation size" );
            ErrorDescription.Add( e_Error.DirectiveOperandHasInvalidCharacter, "The operand for the directive has an invalid charecter" );
            ErrorDescription.Add( e_Error.AddressOutsideRange, "Address outside of the available range" );
            ErrorDescription.Add( e_Error.DcConstantTooLong, "Operand bigger than allowed" );
            ErrorDescription.Add( e_Error.DulicateLabel, "Label already in the Address symbol table" );
            ErrorDescription.Add( e_Error.FirstLineORG, "First line has to be ORG" );
            ErrorDescription.Add( e_Error.IllegalAddressingMode, "Illegal addressing mode" );
            ErrorDescription.Add( e_Error.IllegalCharacter, "Illegal character found" );
            ErrorDescription.Add( e_Error.InvalidAddressingMode, "Invalid addressing mode" );
            ErrorDescription.Add( e_Error.InvalidDestinationRegister, "Invalid destination register" );
            ErrorDescription.Add( e_Error.InvalidSourceRegister, "Invalid source register" );
            ErrorDescription.Add( e_Error.MissingOpCode, "Operation code is missing" );
            ErrorDescription.Add( e_Error.OpCodeAsLabel, "Operation code as label" );
            ErrorDescription.Add( e_Error.ReservedWord, "Reserved word" );
            ErrorDescription.Add( e_Error.TooFewOperands, "Too few operands" );
            ErrorDescription.Add( e_Error.TooManyOperands, "Too many Operands" );
            ErrorDescription.Add( e_Error.MissingOpCodeOrDirective, "Missing an Operation Code Or a Directive" );
            ErrorDescription.Add( e_Error.OpCodeAndDirectiveOnSameLine, "An operation code and a directive cannot be on same line" );
            ErrorDescription.Add( e_Error.MissingOperands, "Missing the operand(s)" );




            // and so on

            #endregion
        }


        public bool isThisErrorInCurrentLine ( e_Error error, int lineNumber ) {
            if ( Error[lineNumber].error == error ) {
                return true;
            }
            return false;
        }

        public void addError ( e_Error error, int lineNumber, string data ) {
            
            errorObject.error = error;
            errorObject.data = data;
            errorObject.lineNumber = lineNumber;

            //if ( Error.Contains( eo ) ) {
            //    Error[lineNumber] = eo;

            //} else {
                Error.Add( errorObject );

            //}

        }


        public void removeThisErrorFromCurrentLine ( e_Error error, int lineNumber ) {
            if ( Error[lineNumber].error == error ) {
                Error.RemoveAt( lineNumber );
            }

        }


        public void numberOfLines ( int numberOfLines ) {
            //Error = new List<TriErrorObject>( numberOfLines + 1 );
        }
    }

}
