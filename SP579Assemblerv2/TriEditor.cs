using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SP579Assemblerv2 {
    public class TriEditor {

        TriKeywords keyword;
        TriMainWindow form;
        TriError errors;
        public TriTableOfContens toc;
        CultureInfo provider;
        public Dictionary<string, string> forwardReference;

        /// <summary>
        /// The latest copy of the code in the editor will be here.
        /// Updated on every change in the code
        /// </summary>
        public string[] Content;
        public string result { get; set; }
        public List<string> NameOfKeywords;
        public int locationCounter;
        int maxLabelLength = 6;
        public bool isThereAnError;
        public bool isAtTheEnd;
        private  int indexInLine;
        string[] registers = { "X0", "X1", "X2", "X3", "X4", "X5", "X6", "X7" };
        private  TriAddressSymbolTableValues ast;

        public TriEditor ( TriKeywords keyword, TriMainWindow form, TriError errors, TriTableOfContens toc ) {
            this.form = form;
            this.keyword = keyword;
            this.errors = errors;
            NameOfKeywords = new List<string>();
            provider = new CultureInfo( "en-US" );
            forwardReference = new Dictionary<string, string>();
            this.toc = toc;
        }

        private bool getOpCodeOrLabel ( string rawWord, ref int position, int lineNumber, out string word ) {
            word = string.Empty;

            for ( int i = 0; i < rawWord.Length; i++, position++ ) {
                if ( char.IsLetterOrDigit( rawWord[i] ) || rawWord[i] == '.' ) {
                    word += rawWord[i];
                } else {
                    errors.addError( e_Error.IllegalCharacter, lineNumber, rawWord );
                    return false;
                }

            }


            return true;
        }
        private bool getOpCodeOrOperand ( string rawWord, ref int position, int lineNumber, out string word ) {
            word = string.Empty;

            for ( int i = 0; i < rawWord.Length; i++, position++ ) {
                if ( char.IsLetterOrDigit( rawWord[i] ) || rawWord[i] == '.'
                    || rawWord[i] == '&' || rawWord[i] == '#'
                    || rawWord[i] == '$' || rawWord[i] == ','
                    || rawWord[i] == '[' || rawWord[i] == ']'
                    || rawWord[i] == '-' ) {

                    word += rawWord[i];
                } else {
                    errors.addError( e_Error.IllegalCharacter, lineNumber, rawWord );
                    return false;
                }

            }

            return true;
        }
        private bool setOpSize ( string opSize, ref TriLineOfCode loc, int lineNumber ) {
            if ( keyword.Instructions[( int ) loc.OpCode].OperationSize == e_OperationSize.Byte ) {
                loc.OpSize = e_OperationSize.Byte;
                loc.OpMode = e_OperationMode.None;
                return true;

            } else if ( keyword.Instructions[( int ) loc.OpCode].OperationSize == e_OperationSize.Word ) {
                loc.OpSize = e_OperationSize.Word;
                loc.OpMode = e_OperationMode.None;
                return true;

            } else {

                e_OperationSize os;
                if ( opSize.ToUpper() == "B" ) {
                    os = e_OperationSize.Byte;

                } else if ( opSize.ToUpper() == "W" ) {
                    os = e_OperationSize.Word;

                } else if ( opSize == string.Empty ) {
                    // set the default
                    os = e_OperationSize.Byte;

                } else {
                    errors.addError( e_Error.InvalidOpSize, lineNumber, loc.OpCode.ToString() );
                    return false;

                }

                if ( keyword.Instructions[( int ) loc.OpCode].OperationSize  == e_OperationSize.None ) {
                    //errors.addError( e_Error.InstructionDoesntWorkWithOpSize, lineNumber, loc.OpCode.ToString() );
                    loc.OpSize = e_OperationSize.None;
                    return true;
                }

                if ( ( keyword.Instructions[( int ) loc.OpCode].OperationSize & os ) == os ) {
                    loc.OpSize = os;
                    loc.OpMode = e_OperationMode.None;
                    return true;

                } else {
                    errors.addError( e_Error.WrongOpSize, lineNumber, loc.OpCode.ToString() );
                    return false;

                }

            }
        }

        // Add label to the Address Symbol Table
        private void addToAddressSymbolTable ( string label, int labelValue = 0x10000, e_LabelType labelType = e_LabelType.REL ) {
            TriAddressSymbolTableValues ast;
            List<string> labelsToBeRemoved = new List<string>();
            bool t = true;
            string originalLabel = label;
            while ( ( forwardReference.Count > 0 ) && t ) {
                t = false;
                if ( forwardReference.ContainsValue( label ) ) {
                    foreach ( var item in forwardReference ) {
                        if ( item.Value == label ) {
                            ast = new TriAddressSymbolTableValues();
                            ast.LabelType = e_LabelType.STC;
                            ast.Value = labelValue;
                            toc.AddressSymbolTable[item.Key] = ast;
                            labelsToBeRemoved.Add( item.Key );
                            t = true;
                        }
                    }
                }

                foreach ( var item in labelsToBeRemoved ) {
                    if ( forwardReference.ContainsValue( item ) ) {
                        label = item;
                    }

                    forwardReference.Remove( item );
                    t = true;

                }

                labelsToBeRemoved = new List<string>();
            }
            
            ast = new TriAddressSymbolTableValues();
            ast.LabelType = labelType;
            ast.Value = labelValue;
            toc.AddressSymbolTable[originalLabel] = ast;

        }

        // Directive's Operands
        private bool analyzeOperandsDirectives ( string rawOperands, int lineNumber, ref TriLineOfCode loc ) {
            TriAddressSymbolTableValues ast;
            string[] temp;
            switch ( loc.Directive ) {
                #region e_Directives.ORG
                case e_Directives.ORG:

                    temp = rawOperands.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
                    if ( temp.Length > 1 ) {
                        errors.addError( e_Error.TooManyOperands, lineNumber, rawOperands );
                        return false;
                    }
                    if ( rawOperands[0] == '$' ) {
                        // cheacks if the operand is a valid hex number and return true, if not then it will
                        // return false and goes into the if statement to check if the operand is a pre-defined
                        // label, in case of multiple origins
                        if ( !int.TryParse( rawOperands.Substring( 1 ), System.Globalization.NumberStyles.HexNumber, provider, out loc.LocationCounter ) ) {

                            // checks if the value of the label is the Address Symbol Table
                            if ( toc.AddressSymbolTable.ContainsKey( loc.Label ) ) {
                                loc.LocationCounter = toc.AddressSymbolTable[rawOperands].Value;
                            } else {
                                errors.addError( e_Error.UndefinedLabel, lineNumber, rawOperands );
                                return false;
                            }

                        }
                    } else {
                        if ( !int.TryParse( rawOperands, out loc.LocationCounter ) ) {
                            if ( toc.AddressSymbolTable.ContainsKey( rawOperands ) ) {
                                loc.LocationCounter = toc.AddressSymbolTable[rawOperands].Value;
                                //loc.LabelValue = loc.LocationCounter;

                            } else {
                                errors.addError( e_Error.UndefinedLabel, lineNumber, loc.Label );
                                return false;
                            }

                        } else {
                            if ( loc.LocationCounter > 0xFBFF ) {
                                errors.addError( e_Error.AddressOutsideRange, lineNumber, rawOperands );
                                return false;
                            }
                        }
                    }


                    if ( loc.LocationCounter > 0xFBFF ) {
                        errors.addError( e_Error.AddressOutsideRange, lineNumber, rawOperands );
                        return false;
                    }

                    if ( loc.Label != string.Empty ) {
                        loc.LabelValue = loc.LocationCounter;
                        // Set the label type as REL
                        ast = new TriAddressSymbolTableValues();
                        ast.LabelType = e_LabelType.REL;
                        if ( toc.AddressSymbolTable.ContainsKey( rawOperands ) ) {
                            ast.Value = toc.AddressSymbolTable[rawOperands].Value;

                        } else {
                            ast.Value = loc.LocationCounter;
                        }

                        toc.AddressSymbolTable[loc.Label] = ast;
                    }

                    break;
                #endregion
                #region e_Directives.EQU
                case e_Directives.EQU:

                    if ( loc.Label == string.Empty ) {
                        errors.addError( e_Error.MissingLabel, lineNumber, string.Empty );
                        return false;
                    }

                    temp = rawOperands.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
                    if ( temp.Length > 1 ) {
                        errors.addError( e_Error.TooManyOperands, lineNumber, rawOperands );
                        return false;
                    }

                    if ( rawOperands.StartsWith( "$" ) ) {
                        // Convert a valid hex number to decimal and assing it to the label
                        if ( !int.TryParse( rawOperands.Substring( 1 ),
                            System.Globalization.NumberStyles.HexNumber, provider, out loc.LabelValue ) ) {
                            errors.addError( e_Error.InvalidOperand, lineNumber, rawOperands );
                        }

                        // Set the label type as STC
                        addToAddressSymbolTable( loc.Label, loc.LabelValue, e_LabelType.STC );
                        //ast = new TriAddressSymbolTableValues();
                        //ast.LabelType = e_LabelType.STC;
                        //ast.Value = loc.LabelValue;
                        //toc.AddressSymbolTable[loc.Label] = ast;

                    } else {
                        // check if the operand has invalid charecters for a directive
                        if ( rawOperands.StartsWith( "#" ) || rawOperands.StartsWith( "&" ) ) {
                            errors.addError( e_Error.DirectiveOperandHasInvalidCharacter, lineNumber, loc.Directive.ToString() );
                            return false;
                        }

                        // Assigns a valid decimal value to the label
                        if ( !int.TryParse( rawOperands, out loc.LabelValue ) ) {

                            // if the oprand is not a hex number, then
                            // checks if the value of the label is the Address Symbol Table
                            if ( toc.AddressSymbolTable.ContainsKey( rawOperands ) ) {
                                if ( toc.AddressSymbolTable[rawOperands].Value == toc.InvalidValue ) {
                                    forwardReference.Add( loc.Label, rawOperands );
                                }

                                loc.LabelValue = toc.AddressSymbolTable[rawOperands].Value;

                            } else {
                                loc.LabelValue = toc.InvalidValue;
                                ast = new TriAddressSymbolTableValues();
                                ast.LabelType = e_LabelType.STC;
                                ast.Value = toc.InvalidValue;
                                toc.AddressSymbolTable[loc.Label] = ast;
                                //loc.LabelOperand = rawOperands;
                                addToAddressSymbolTable( rawOperands );
                                forwardReference.Add( loc.Label, rawOperands );
                                break;
                                //addToTree( rawOperands );
                                //errors.addError( e_Error.UndefinedLabel, lineNumber, rawOperands );
                                //return false;
                            }

                        }

                        // Set the label type as STC
                        addToAddressSymbolTable( loc.Label, loc.LabelValue, e_LabelType.STC );
                        //ast = new TriAddressSymbolTableValues();
                        //ast.LabelType = e_LabelType.STC;
                        //ast.Value = loc.LabelValue;
                        //toc.AddressSymbolTable[loc.Label] = ast;

                    }



                    break;
                #endregion
                #region e_Directives.DC
                case e_Directives.DC:
                    int result = 0;
                    string[] constants = rawOperands.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );

                    if ( constants.Length > 8 ) {
                        errors.addError( e_Error.TooManyOperands, lineNumber, rawOperands );
                        return false;
                    }

                    loc.DirectivesOperands = new string[constants.Length]; // remove this??

                    // Go through all constants
                    for ( int i = 0; i < constants.Length; i++ ) {
                        // check if the constants is a hex number
                        if ( constants[i].StartsWith( "$" ) ) {

                            if ( int.TryParse( constants[i].Substring( 1 ),
                            System.Globalization.NumberStyles.HexNumber, provider, out result ) ) {

                                if ( loc.OpSize == e_OperationSize.Byte ) {
                                    if ( result > 0xFF ) {
                                        errors.addError( e_Error.DcConstantTooLong, lineNumber, constants[i] );
                                        return false;
                                    }
                                } else if ( loc.OpSize == e_OperationSize.Word ) {
                                    if ( result > 0xFFFF ) {
                                        errors.addError( e_Error.DcConstantTooLong, lineNumber, constants[i] );
                                        return false;
                                    }
                                } else {
                                    errors.addError( e_Error.InvalidOperand, lineNumber, constants[i] );
                                    return false;
                                }


                                loc.DirectivesOperands[i] = constants[i];


                            } else {
                                errors.addError( e_Error.InvalidOperand, lineNumber, constants[i] );
                                return false;
                            }

                            // else it's a decimal number or a label
                        } else {
                            if ( constants[i].StartsWith( "#" ) || constants[i].StartsWith( "&" ) ) {
                                errors.addError( e_Error.DirectiveOperandHasInvalidCharacter, lineNumber, loc.Directive.ToString() );
                                return false;
                            }

                            // check if the constant is a decimal value
                            if ( int.TryParse( constants[i], out result ) ) {

                                if ( loc.OpSize == e_OperationSize.Byte ) {
                                    if ( result > 0xFF ) {
                                        errors.addError( e_Error.DcConstantTooLong, lineNumber, constants[i] );
                                        return false;
                                    }
                                } else if ( loc.OpSize == e_OperationSize.Word ) {
                                    if ( result > 0xFFFF ) {
                                        errors.addError( e_Error.DcConstantTooLong, lineNumber, constants[i] );
                                        return false;
                                    }
                                } else {
                                    errors.addError( e_Error.InvalidOperand, lineNumber, constants[i] );
                                    return false;
                                }


                                loc.DirectivesOperands[i] = constants[i];

                                // if not then it's a label
                            } else {
                                // check if the label is in the address symbol table
                                if ( toc.AddressSymbolTable.ContainsKey( constants[i] ) ) {
                                    if ( loc.OpSize == e_OperationSize.Byte ) {
                                        loc.DirectivesOperands[i] = "$" + toc.AddressSymbolTable[constants[i]].Value.ToString( "X2" );

                                    } else if ( loc.OpSize == e_OperationSize.Word ) {
                                        loc.DirectivesOperands[i] = "$" + toc.AddressSymbolTable[constants[i]].Value.ToString( "X4" );

                                    }

                                } else {
                                    addToAddressSymbolTable( constants[i] );
                                    loc.DirectivesOperands[i] = constants[i];
                                    //errors.addError( e_Error.UndefinedLabel, lineNumber, constants[i] );
                                    //return false;
                                }

                            }
                        }
                    }

                    if ( loc.Label != string.Empty ) {
                        // Set the label type as REL
                        addToAddressSymbolTable( loc.Label, loc.LocationCounter, e_LabelType.REL );
                        //ast = new TriAddressSymbolTableValues();
                        //ast.LabelType = e_LabelType.REL;
                        //ast.Value = loc.LocationCounter;
                        //toc.AddressSymbolTable[loc.Label] = ast;
                    }

                    break;
                #endregion
                #region e_Directives.DS
                case e_Directives.DS:

                    temp = rawOperands.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
                    if ( temp.Length > 1 ) {
                        errors.addError( e_Error.TooManyOperands, lineNumber, rawOperands );
                        return false;
                    }

                    if ( temp.Length == 0 ) {
                        errors.addError( e_Error.MissingOperands, lineNumber, rawOperands );
                        return false;
                    }

                    if ( rawOperands[0] == '$' ) {
                        if ( int.TryParse( rawOperands.Substring( 1 ),
                            System.Globalization.NumberStyles.HexNumber, provider, out result ) ) {
                            if ( result > 0xFBFF ) {
                                errors.addError( e_Error.AddressOutsideRange, lineNumber, rawOperands );
                                return false;
                            }

                        } else {
                            errors.addError( e_Error.InvalidOperand, lineNumber, rawOperands );
                            return false;
                        }
                    } else {
                        if ( int.TryParse( rawOperands, out result ) ) {
                            if ( result > 0xFBFF ) {
                                // error
                                errors.addError( e_Error.AddressOutsideRange, lineNumber, rawOperands );
                                return false;
                            }

                        } else {
                            errors.addError( e_Error.InvalidOperand, lineNumber, rawOperands );
                            return false;
                        }

                    }


                    loc.SourceOperand = rawOperands;
                    loc.LabelValue = loc.LocationCounter;
                    //loc.LocationCounter = loc.LabelValue;
                    // Set the label type as REL
                    //ast = new TriAddressSymbolTableValues();
                    //ast.LabelType = e_LabelType.REL;
                    //ast.Value = toc.AddressSymbolTable[loc.Label].Value;
                    //toc.AddressSymbolTable[loc.Label] = ast;
                    break;
                #endregion
                #region e_Directives.EXT
                case e_Directives.EXT:

                    temp = rawOperands.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
                    if ( temp.Length > 4 ) {
                        errors.addError( e_Error.TooManyOperands, lineNumber, rawOperands );
                        return false;
                    }
                    loc.DirectivesOperands = temp;

                    for ( int i = 0; i < temp.Length; i++ ) {
                        // Set the label type as EXT
                        ast = new TriAddressSymbolTableValues();
                        ast.LabelType = e_LabelType.XTR;
                        toc.AddressSymbolTable[temp[i]] = ast;
                    }



                    break;
                // not finished
                #endregion
                #region e_Directives.ENT
                case e_Directives.ENT:

                    temp = rawOperands.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
                    if ( temp.Length > 5 ) {
                        errors.addError( e_Error.TooManyOperands, lineNumber, rawOperands );
                        return false;
                    }
                    loc.DirectivesOperands = temp;

                    for ( int i = 0; i < temp.Length; i++ ) {
                        toc.ENTlabels.Add( temp[i] );
                    }

                    break;
                #endregion
                #region e_Directives.END
                case e_Directives.END:
                    // here check if the start address (start ORG) has a label
                    temp = rawOperands.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
                    if ( temp.Length > 1 ) {
                        errors.addError( e_Error.TooManyOperands, lineNumber, rawOperands );
                        return false;
                    }

                    if ( !toc.AddressSymbolTable.ContainsKey( temp[0] ) ) {
                        errors.addError( e_Error.UndefinedLabel, lineNumber, temp[0] );
                        return false;
                    }

                    loc.LabelValue = toc.AddressSymbolTable[temp[0]].Value;
                    loc.LabelOperand = temp[0];
                    isAtTheEnd = true;
                    break;

                #endregion
                default:
                    break;
            }

            return true;

        }

        // Register Indirect with Displacement Addressing
        private bool analyzeAddressingMode00 ( string rawOperands, int lineNumber, ref TriLineOfCode loc ) {
            // for converting HEX to decimal in TryParse
            CultureInfo provider = new CultureInfo( "en-US" );
            int openBrackets = 0, closeBrackets = 0;
            int displacement = 0;
            string[] temp;

            // Split the operands string by comma
            temp = rawOperands.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );

            // check if there are too many operands
            if ( temp.Length > 2 ) {
                errors.addError( e_Error.TooManyOperands, lineNumber, rawOperands );
                return false;
            }

            // check if there are too few operands
            if ( temp.Length < 2 ) {
                errors.addError( e_Error.TooFewOperands, lineNumber, rawOperands );
                return false;
            }

            // check if there is a close and open brackets
            if ( !rawOperands.Contains( '[' ) || !rawOperands.Contains( ']' ) ) {
                errors.addError( e_Error.MissingBrackets, lineNumber, rawOperands );
                return false;
            }

            // check how many open/close brackets there are in the operands
            for ( int i = 0; i < temp.Length; i++ ) {
                openBrackets = 0;
                closeBrackets = 0;
                for ( int b = 0; b < temp[i].Length; b++ ) {
                    if ( temp[i][b] == '[' ) {
                        openBrackets++;
                    } else if ( temp[i][b] == ']' ) {
                        closeBrackets++;
                    }
                }

                if ( openBrackets > 1 ) {
                    errors.addError( e_Error.TooManyOpenBrackets, lineNumber, temp[i] );
                    return false;
                }

                if ( closeBrackets > 1 ) {
                    errors.addError( e_Error.TooManyCloseBrackets, lineNumber, temp[i] );
                    return false;
                }

                // check if there are more open brackets than close brackets
                if ( ( openBrackets - closeBrackets ) >= 1 ) {
                    errors.addError( e_Error.BracketsNotClosed, lineNumber, temp[i] );
                    return false;

                    // check if there are more close brackets than open brackets
                } else if ( ( openBrackets - closeBrackets ) <= -1 ) {
                    errors.addError( e_Error.BracketsNotOpened, lineNumber, temp[i] );
                    return false;
                }
            }

            // exchange the operand order for the STR instruction
            if ( loc.OpCode == e_Keywords.STR || loc.OpCode == e_Keywords.EXG ) {
                string t = temp[0];
                temp[0] = temp[1];
                temp[1] = t;
            }

            // Destination Register ( Source Register for STR )
            // check if the register format is valid
            // only X0 - X7 are allowed
            if ( temp[0].Length != 2 ) {
                if ( loc.OpCode == e_Keywords.STR || loc.OpCode == e_Keywords.EXG )
                    errors.addError( e_Error.InvalidSourceRegister, lineNumber, temp[0] );
                else
                    errors.addError( e_Error.InvalidDestinationRegister, lineNumber, temp[0] );

                return false;
            }

            // check if the first charecter is an 'X'
            if ( temp[0][0] != 'X' ) {
                // change the error type
                if ( loc.OpCode == e_Keywords.STR || loc.OpCode == e_Keywords.EXG )
                    errors.addError( e_Error.InvalidSourceRegister, lineNumber, temp[0] );
                else
                    errors.addError( e_Error.InvalidDestinationRegister, lineNumber, temp[0] );
                return false;
            }

            // check if the second charecter is a digit
            if ( !char.IsDigit( temp[0][1] ) ) {
                // change the error type
                if ( loc.OpCode == e_Keywords.STR || loc.OpCode == e_Keywords.EXG )
                    errors.addError( e_Error.InvalidSourceRegister, lineNumber, temp[0] );
                else
                    errors.addError( e_Error.InvalidDestinationRegister, lineNumber, temp[0] );
                return false;
            }

            if ( loc.OpCode == e_Keywords.STR || loc.OpCode == e_Keywords.EXG ) {
                loc.SourceOperand = temp[0];
            } else {
                loc.DestinationOperand = temp[0];
            }



            // Displacement for the Source Register ( Destination Register for STR )
            if ( temp[1][0] == '$' ) {
                if ( !int.TryParse( temp[1].Substring( 1, temp[1].IndexOf( '[' ) - 1 ),
                    System.Globalization.NumberStyles.HexNumber, provider, out displacement ) ) {
                    errors.addError( e_Error.InvalidOperand, lineNumber, temp[1] );
                    return false;
                }

                if ( loc.OpCode == e_Keywords.STR || loc.OpCode == e_Keywords.EXG ) {
                    loc.DestinationOperandDisplacement = displacement;
                } else {
                    loc.SourceOperandDisplacement = displacement;
                }

            } else {
                if ( !int.TryParse( temp[1].Substring( 0, temp[1].IndexOf( '[' ) ), out displacement ) ) {
                    errors.addError( e_Error.InvalidOperand, lineNumber, temp[1] );
                    return false;
                }

                if ( loc.OpCode == e_Keywords.STR || loc.OpCode == e_Keywords.EXG ) {
                    loc.DestinationOperandDisplacement = displacement;
                } else {
                    loc.SourceOperandDisplacement = displacement;
                }
            }

            string register = temp[1].Substring( temp[1].IndexOf( '[' ) + 1, 2 );
            if ( register[0] != 'X' ) {
                // change the error type
                if ( loc.OpCode == e_Keywords.STR || loc.OpCode == e_Keywords.EXG )
                    errors.addError( e_Error.InvalidDestinationRegister, lineNumber, temp[1] );
                else
                    errors.addError( e_Error.InvalidSourceRegister, lineNumber, temp[1] );

                return false;
            }

            if ( !char.IsDigit( register[1] ) ) {
                // change the error type
                if ( loc.OpCode == e_Keywords.STR || loc.OpCode == e_Keywords.EXG )
                    errors.addError( e_Error.InvalidDestinationRegister, lineNumber, register );
                else
                    errors.addError( e_Error.InvalidSourceRegister, lineNumber, register );

                return false;
            }

            if ( loc.OpCode == e_Keywords.STR || loc.OpCode == e_Keywords.EXG ) {
                loc.DestinationOperand = register;
            } else {
                loc.SourceOperand = register;
            }

            return true;

        }

        // Register Addressing
        private bool analyzeAddressingMode01 ( string rawOperands, int lineNumber, ref TriLineOfCode loc ) {

            string[] temp;

            if ( loc.OpCode == e_Keywords.POP || loc.OpCode == e_Keywords.PUSH || loc.OpCode == e_Keywords.LSP ) {
                if ( rawOperands.Length != 2 ) {
                    errors.addError( e_Error.InvalidOperand, lineNumber, rawOperands );
                    return false;
                }

                if ( rawOperands.Contains( ',' ) ) {
                    errors.addError( e_Error.InvalidOperand, lineNumber, rawOperands );
                    return false;
                }

                if ( loc.OpCode == e_Keywords.POP ) {
                    if ( !registers.Contains( rawOperands ) ) {
                        errors.addError( e_Error.InvalidDestinationRegister, lineNumber, rawOperands );
                        return false;

                    } else {
                        loc.DestinationOperand = rawOperands;

                    }
                } else if ( loc.OpCode == e_Keywords.PUSH || loc.OpCode == e_Keywords.LSP ) {
                    if ( !registers.Contains( rawOperands ) ) {
                        errors.addError( e_Error.InvalidSourceRegister, lineNumber, rawOperands );
                        return false;

                    } else {
                        loc.SourceOperand = rawOperands;

                    }
                }


            } else {
                if ( rawOperands.Length != 5 ) {
                    errors.addError( e_Error.InvalidOperand, lineNumber, rawOperands );
                    return false;
                }

                if ( !rawOperands.Contains( ',' ) ) {
                    errors.addError( e_Error.InvalidOperand, lineNumber, rawOperands );
                    return false;
                }

                rawOperands = rawOperands.ToUpper();
                temp = rawOperands.Split( new char[] { ',' }, StringSplitOptions.None );

                if ( temp.Length > 2 ) {
                    errors.addError( e_Error.OperandIsTooLong, lineNumber, rawOperands );
                    return false;
                }

                if ( !registers.Contains( temp[0] ) ) {
                    errors.addError( e_Error.InvalidDestinationRegister, lineNumber, rawOperands );
                    return false;

                } else {
                    loc.DestinationOperand = temp[0];

                }

                if ( !registers.Contains( temp[1] ) ) {
                    errors.addError( e_Error.InvalidSourceRegister, lineNumber, rawOperands );
                    return false;

                } else {
                    loc.SourceOperand = temp[1];

                }
            }

            return true;
        }

        // Immidiate Addressing
        private bool analyzeAddressingMode10 ( string rawOperands, int lineNumber, ref TriLineOfCode loc ) {

            string[] temp = rawOperands.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );

            if ( loc.OpCode == e_Keywords.LSP ) {
                if ( temp.Length > 1 ) {
                    errors.addError( e_Error.TooManyOperands, lineNumber, rawOperands );
                    return false;
                }

                if ( temp[0][1] == '$' ) {
                    if ( !int.TryParse( temp[0].Substring( 2 ), System.Globalization.NumberStyles.HexNumber,
                                    provider, out loc.SourceOperandDisplacement ) ) {
                        errors.addError( e_Error.InvalidOperand, lineNumber, temp[0] );
                        return false;

                    }

                } else {
                    if ( !int.TryParse( temp[0].Substring( 1 ), out loc.SourceOperandDisplacement ) ) {
                        errors.addError( e_Error.InvalidOperand, lineNumber, temp[0] );
                        return false;

                    } else {
                        if ( toc.AddressSymbolTable.ContainsKey( temp[0].Substring( 1 ) ) ) {
                            loc.SourceOperandDisplacement = toc.AddressSymbolTable[temp[0].Substring( 1 )].Value;

                        } else {
                            errors.addError( e_Error.UndefinedLabel, lineNumber, temp[0] );
                            return false;
                        }
                    }


                }//end if


                if ( loc.SourceOperandDisplacement < 0xFBFF || loc.SourceOperandDisplacement > 0xFFFF ) {
                    errors.addError( e_Error.AddressOutsideRange, lineNumber, loc.SourceOperandDisplacement.ToString() );
                    return false;
                }

            } else {
                // Destination Register
                if ( !registers.Contains( temp[0] ) ) {
                    errors.addError( e_Error.InvalidDestinationRegister, lineNumber, rawOperands );
                    return false;
                }

                loc.DestinationOperand = temp[0];

                // Source Register
                if ( temp[1][1] == '$' ) {
                    if ( !int.TryParse( temp[1].Substring( 2 ), System.Globalization.NumberStyles.HexNumber,
                                    provider, out loc.SourceOperandDisplacement ) ) {
                        errors.addError( e_Error.InvalidOperand, lineNumber, temp[1] );
                        return false;


                    }

                } else {
                    // Check if it's a decimal number
                    if ( !int.TryParse( temp[1].Substring( 1 ), out loc.SourceOperandDisplacement ) ) {
                        // Then it's a label
                        if ( toc.AddressSymbolTable.ContainsKey( temp[1].Substring( 1 ) ) ) {
                            if ( toc.AddressSymbolTable[temp[1].Substring( 1 )].Value != toc.InvalidValue ) {
                                loc.SourceOperandDisplacement = toc.AddressSymbolTable[temp[1].Substring( 1 )].Value;
                                loc.LabelOperand = temp[1].Substring( 1 );

                            }

                        } else {
                            loc.LabelOperand = temp[1].Substring( 1 );
                            addToAddressSymbolTable( temp[1].Substring( 1 ) );

                            if ( temp[1].Substring( 1 ).Length > 6 ) {
                                errors.addError( e_Error.LabelIsTooLong, lineNumber, temp[1] );
                                return false;
                            }

                        }
                    }


                }//end if

                //DO We need to check here if the index is out of range/within stack??
                // yes we do :P

                if ( loc.SourceOperandDisplacement > 0xFBFF ) {
                    errors.addError( e_Error.AddressOutsideRange, lineNumber, loc.SourceOperandDisplacement.ToString() );
                    return false;
                }

            }

            return true;

        }

        // Absolute Addressing
        private bool analyzeAddressingMode11 ( string rawOperands, int lineNumber, ref TriLineOfCode loc ) {
            CultureInfo provider = new CultureInfo( "en-US" );
            int reg = 9;
            string[] temp;

            temp = rawOperands.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );

            if ( temp.Length > 2 ) {
                errors.addError( e_Error.TooManyOperands, lineNumber, rawOperands );
                return false;
            }

            if ( temp.Length < 2 ) {
                errors.addError( e_Error.TooFewOperands, lineNumber, rawOperands );
                return false;
            }

            // exchange the operand order for the STR/EXG instruction
            if ( loc.OpCode == e_Keywords.STR || loc.OpCode == e_Keywords.EXG ) {
                string t = temp[0];
                temp[0] = temp[1];
                temp[1] = t;
            }

            // Destination Register ( Source Register for STR/EXG )
            if ( temp[0].Length != 2 ) {
                if ( loc.OpCode == e_Keywords.STR || loc.OpCode == e_Keywords.EXG )
                    errors.addError( e_Error.InvalidSourceRegister, lineNumber, temp[0] );
                else
                    errors.addError( e_Error.InvalidDestinationRegister, lineNumber, temp[0] );

                return false;
            }

            if ( temp[0][0] != 'X' ) {
                // change the error type
                if ( loc.OpCode == e_Keywords.STR || loc.OpCode == e_Keywords.EXG )
                    errors.addError( e_Error.InvalidSourceRegister, lineNumber, temp[0] );
                else
                    errors.addError( e_Error.InvalidDestinationRegister, lineNumber, temp[0] );
                return false;
            }

            if ( !char.IsDigit( temp[0][1] ) || temp[0][1] == '8' || temp[0][1] == '9' ) {
                // change the error type
                if ( loc.OpCode == e_Keywords.STR || loc.OpCode == e_Keywords.EXG )
                    errors.addError( e_Error.InvalidSourceRegister, lineNumber, temp[0] );
                else
                    errors.addError( e_Error.InvalidDestinationRegister, lineNumber, temp[0] );
                return false;
            }

            if ( loc.OpCode == e_Keywords.STR || loc.OpCode == e_Keywords.EXG ) {
                loc.SourceOperand = temp[0];
            } else {
                loc.DestinationOperand = temp[0];
            }


            // The Absolute Address
            // Check if it's a HEX number
            if ( temp[1][0] == '$' ) {

                if ( temp[1].Length > 5 ) {
                    if ( loc.OpCode == e_Keywords.STR || loc.OpCode == e_Keywords.EXG )
                        errors.addError( e_Error.InvalidOperand, lineNumber, temp[1] );
                    else
                        errors.addError( e_Error.InvalidOperand, lineNumber, temp[1] );

                    return false;
                }

                if ( !int.TryParse( temp[1].Substring( 1 ), System.Globalization.NumberStyles.HexNumber, provider, out reg ) ) {
                    errors.addError( e_Error.InvalidOperand, lineNumber, temp[1] );
                    return false;
                }

                if ( reg > 0xFBFF ) {
                    errors.addError( e_Error.AddressOutsideRange, lineNumber, reg.ToString() );
                    return false;
                }

                if ( loc.OpCode == e_Keywords.STR || loc.OpCode == e_Keywords.EXG ) {
                    loc.DestinationOperandDisplacement = reg;

                } else {
                    loc.SourceOperandDisplacement = reg;

                }

            } else {
                // Check if it's a decimal number
                if ( !int.TryParse( temp[1], out reg ) ) {
                    // Then it's a label
                    if ( toc.AddressSymbolTable.ContainsKey( temp[1] ) ) {
                        if ( toc.AddressSymbolTable[temp[1]].Value != toc.InvalidValue ) {
                            if ( loc.OpCode == e_Keywords.STR || loc.OpCode == e_Keywords.EXG ) {
                                loc.DestinationOperandDisplacement = toc.AddressSymbolTable[temp[1]].Value;
                                loc.LabelOperand = temp[1];

                            } else {
                                loc.SourceOperandDisplacement = toc.AddressSymbolTable[temp[1]].Value;
                                return true;
                            }
                        }

                    } else {
                        loc.LabelOperand = temp[1];
                        addToAddressSymbolTable( temp[1] );

                        //if ( loc.Label != string.Empty ) {
                        //    forwardReference.Add( loc.Label, rawOperands );
                        //
                        //}

                        if ( temp[1].Length > 6 ) {
                            errors.addError( e_Error.LabelIsTooLong, lineNumber, temp[1] );
                            return false;
                        }

                        return true;
                    }
                }

                if ( temp[1].Length > 4 ) {
                    errors.addError( e_Error.InvalidOperand, lineNumber, temp[1] );
                    return false;
                }

                

                if ( loc.OpCode == e_Keywords.STR || loc.OpCode == e_Keywords.EXG ) {
                    loc.DestinationOperandDisplacement = reg;

                } else {
                    loc.SourceOperandDisplacement = reg;

                }
            }

            return true;
        }

        public void Scan () {
            #region Variables
            string word1;
            string word2;
            string word3;
            string wordWithoutDot1;
            string wordWithoutDot2;
            string wordWithoutDot3;
            bool isOpCode1;
            bool isOpCode2;
            bool isOpCode3;
            bool isDirective1;
            bool isDirective2;
            bool isDirective3;
            e_Directives directive1;
            e_Directives directive2;
            e_Directives directive3;
            e_Keywords opCode1;
            e_Keywords opCode2;
            e_Keywords opCode3;
            string[] OpCodeAndOpSize;
            string[] lineParts;
            bool b0 = false, b1 = false;
            bool lineNotEmpty = false;
            int lastLine = 0;
            int cLineNumber = -1;
            locationCounter = 0;
            result = string.Empty;
            isThereAnError = false;
            isAtTheEnd = false;
            TriAddressSymbolTableValues ast = new TriAddressSymbolTableValues();
            TriLineOfCode loc = new TriLineOfCode();
            #endregion

            toc.initialize();
            errors.Error.Clear();
            forwardReference.Clear();

            for ( int lineNumber = 0; lineNumber < Content.Length && !isAtTheEnd; lineNumber++ ) {
                #region Skip Empty Lines
                lineNotEmpty = false;
                // skip the empty lines
                for ( int k = 0; k < Content[lineNumber].Length; k++ ) {
                    if ( !char.IsWhiteSpace( Content[lineNumber][k] ) ) {
                        lineNotEmpty = true;
                        break;
                    }

                }
                #endregion

                #region Some initialization
                indexInLine = 0;
                b0 = false; b1 = false;
                isOpCode1 = false;
                isOpCode2 = false;
                isOpCode3 = false;
                isDirective1 = false;
                isDirective2 = false;
                isDirective3 = false;
                opCode1 = e_Keywords.NOP;
                opCode2 = e_Keywords.NOP;
                opCode3 = e_Keywords.NOP;
                directive1 = e_Directives.NONE;
                directive2 = e_Directives.NONE;
                directive3 = e_Directives.NONE;
                #endregion

                if ( lineNotEmpty ) {
                    loc.lineNumber = lineNumber;
                    cLineNumber++;
                    #region Location Counter
                    // Updating the Location Counter according to the last line
                    // skip the first time
                    //currentLine = lineNumber;
                    if ( cLineNumber != 0 && !isThereAnError ) {
                        lastLine = cLineNumber - 1;
                        if ( toc.LinesOfCodes[lastLine].OpCode == e_Keywords.NOP ) {
                            if ( toc.LinesOfCodes[lastLine].Directive == e_Directives.ORG ) {
                                locationCounter = toc.LinesOfCodes[lastLine].LocationCounter;

                                // for the DC Directive
                            } else if ( toc.LinesOfCodes[lastLine].Directive == e_Directives.DC ) {
                                if ( toc.LinesOfCodes[lastLine].OpSize == e_OperationSize.Byte ) {
                                    locationCounter += toc.LinesOfCodes[lastLine].DirectivesOperands.Length;
                                    loc.LocationCounter = locationCounter;

                                } else if ( toc.LinesOfCodes[lastLine].OpSize == e_OperationSize.Word ) {
                                    locationCounter += loc.DirectivesOperands.Length * 2;
                                    loc.LocationCounter = locationCounter;

                                } else {
                                    errors.addError( e_Error.InvalidOpSize, lineNumber, toc.LinesOfCodes[lastLine].Directive.ToString() );
                                    isThereAnError = true;
                                    continue;
                                }
                                // for the DS Directive
                            } else if ( toc.LinesOfCodes[lastLine].Directive == e_Directives.DS ) {
                                if ( toc.LinesOfCodes[lastLine].OpSize == e_OperationSize.Byte ) {
                                    locationCounter += int.Parse( toc.LinesOfCodes[lastLine].SourceOperand.Substring( 1 ), System.Globalization.NumberStyles.HexNumber );
                                    loc.LocationCounter = locationCounter;
                                } else if ( toc.LinesOfCodes[lastLine].OpSize == e_OperationSize.Word ) {
                                    locationCounter += int.Parse( toc.LinesOfCodes[lastLine].SourceOperand.Substring( 1 ), System.Globalization.NumberStyles.HexNumber ) * 2;
                                    loc.LocationCounter = locationCounter;

                                } else {
                                    errors.addError( e_Error.InvalidOpSize, lineNumber, toc.LinesOfCodes[lastLine].Directive.ToString() );
                                    isThereAnError = true;
                                    continue;
                                }

                            } 
                            /*
                            else if ( toc.LinesOfCodes[lastLine].Directive == e_Directives.EXT
                                || toc.LinesOfCodes[lastLine].Directive == e_Directives.ENT ) {
                                    cLineNumber--;

                            } */
                            
                            else {
                                loc.LocationCounter = locationCounter;

                            }
                        } else {
                            if ( toc.LinesOfCodes[lastLine].AddrMode != e_AddressingMode.None ) {
                                if ( toc.LinesOfCodes[lastLine].AddrMode != e_AddressingMode.RegisterAddressing ) {
                                    locationCounter += 4;
                                    loc.LocationCounter = locationCounter;
                                } else {
                                    locationCounter += 2;
                                    loc.LocationCounter = locationCounter;
                                }

                            } else {
                                if ( loc.OpCode == e_Keywords.BRA || loc.OpCode == e_Keywords.BNE
                                 || loc.OpCode == e_Keywords.BEQ || loc.OpCode == e_Keywords.BSUB
                                 || loc.OpCode == e_Keywords.BCS || loc.OpCode == e_Keywords.BLT
                                    ) {
                                    if ( loc.OpMode == e_OperationMode.Address ) {
                                        locationCounter += 4;
                                    } else if ( loc.OpMode == e_OperationMode.PCplusOffset ) {
                                        locationCounter += 2;
                                    }
                                } else {
                                    errors.addError( e_Error.InvalidAddressingMode, lineNumber,
                                        toc.LinesOfCodes[lastLine].OpCode.ToString() + " " + toc.LinesOfCodes[lastLine].AddrMode.ToString() );
                                    isThereAnError = true;
                                    continue;
                                }
                            }

                        }
                    }

                    //lineNumber = currentLine;

                    #endregion

                    #region Line of Code Initialization
                    loc.SourceOperand = loc.DestinationOperand = loc.Label = string.Empty;
                    loc.DestinationOperandDisplacement = loc.SourceOperandDisplacement = loc.LabelValue = loc.LocationCounter = 0;
                    loc.Directive = e_Directives.NONE; loc.OpCode = e_Keywords.NOP;
                    loc.AddrMode = e_AddressingMode.None; loc.DirectivesOperands = null;
                    loc.OpSize = e_OperationSize.None; loc.OpMode = e_OperationMode.None;
                    loc.Label = string.Empty; loc.LabelOperand = string.Empty;
                    #endregion

                    lineParts = Content[lineNumber].Split( new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries );

                        #region No Token
                    if ( lineParts.Length == 0 ) {
                        //error 
                        isThereAnError = true;
                        continue;
                        #endregion
                        #region One Token
                    } else if ( lineParts.Length == 1 ) {
                        if ( !getOpCodeOrLabel( lineParts[0], ref indexInLine, lineNumber, out word1 ) ) {
                            errors.addError( e_Error.InvalidOpCode, lineNumber, word1 );
                            isThereAnError = true;
                            continue;
                        }
                        // Check if it's an OpCode
                        if ( word1.Contains( '.' ) ) {
                            wordWithoutDot1 = word1.Substring( 0, word1.IndexOf( '.' ) );
                        } else {
                            wordWithoutDot1 = word1;
                        }

                        foreach ( e_Keywords k in Enum.GetValues( typeof( e_Keywords ) ) ) {
                            if ( k.ToString() == wordWithoutDot1.ToUpper() ) {
                                b0 = true;
                                loc.OpCode = k;
                                break;
                            }
                        }


                        // Check if the only token has a dot in it
                        // if so it is probably an OpCode, but an OpCode
                        // with no operands doesn't have an OpSize
                        if ( word1.Contains( '.' ) && ( word1.Length - 1 > word1.IndexOf( '.' ) ) ) { // make sure of this
                            errors.addError( e_Error.InstructionDoesntWorkWithOpSize, lineNumber, wordWithoutDot1 );
                            isThereAnError = true;
                            continue;
                        }

                        if ( loc.OpCode != e_Keywords.HLT || loc.OpCode != e_Keywords.RSUB || loc.OpCode != e_Keywords.RTRP ) {
                            errors.addError( e_Error.MissingOperands, lineNumber, word1 );
                            isThereAnError = true;
                            continue;
                        }

                        #endregion
                        #region Two Tokens
                    } else if ( lineParts.Length == 2 ) {
                        if ( !getOpCodeOrLabel( lineParts[0], ref indexInLine, lineNumber, out word1 ) ) {
                            //error
                            isThereAnError = true;
                            continue;
                        }
                        if ( !getOpCodeOrOperand( lineParts[1], ref indexInLine, lineNumber, out word2 ) ) {
                            //error
                            isThereAnError = true;
                            continue;
                        }

                        if ( word1.Contains( '.' ) ) {
                            wordWithoutDot1 = word1.Substring( 0, word1.IndexOf( '.' ) );
                        } else {
                            wordWithoutDot1 = word1;
                        }
                        if ( word2.Contains( '.' ) ) {
                            wordWithoutDot2 = word2.Substring( 0, word2.IndexOf( '.' ) );
                        } else {
                            wordWithoutDot2 = word2;
                        }

                        foreach ( e_Keywords k in Enum.GetValues( typeof( e_Keywords ) ) ) {
                            if ( !isOpCode1 ) {
                                if ( k.ToString() == wordWithoutDot1.ToUpper() ) {
                                    opCode1 = k;
                                    isOpCode1 = true;
                                    if ( isOpCode2 )
                                        continue;

                                }
                            }

                            if ( !isOpCode2 ) {
                                if ( k.ToString() == wordWithoutDot2.ToUpper() ) {
                                    opCode2 = k;
                                    isOpCode2 = true;
                                    if ( isOpCode1 )
                                        continue;

                                }
                            }

                        }

                        foreach ( e_Directives d in Enum.GetValues( typeof( e_Directives ) ) ) {
                            if ( !isOpCode1 ) {
                                if ( d.ToString() == wordWithoutDot1.ToUpper() ) {
                                    directive1 = d;
                                    isDirective1 = true;
                                    if ( isDirective2 )
                                        continue;

                                }
                            }

                            if ( !isOpCode2 ) {
                                if ( d.ToString() == wordWithoutDot2.ToUpper() ) {
                                    directive2 = d;
                                    isDirective2 = true;
                                    if ( isDirective1 )
                                        continue;

                                }
                            }
                        }

                        // token 1 and 2 are OpCodes
                        if ( isOpCode1 && isOpCode2 ) {
                            errors.addError( e_Error.OpCodeAsLabel, lineNumber, wordWithoutDot1 + " " + wordWithoutDot2 );
                            isThereAnError = true;
                            continue;
                        }

                        if ( isOpCode1 && isDirective2 ) {
                            // error: 1. token OpCode, 2. token Directive
                            errors.addError( e_Error.OpCodeAndDirectiveOnSameLine, lineNumber, string.Empty );
                            isThereAnError = true;
                            continue;
                        } else if ( isOpCode2 && isDirective1 ) {
                            // error: 1. token Directive, 2. token OpCode
                            errors.addError( e_Error.OpCodeAndDirectiveOnSameLine, lineNumber, string.Empty );
                            isThereAnError = true;
                            continue;
                        } else if ( !isOpCode2 && !isDirective2 && !isDirective1 && !isOpCode1 ) {
                            errors.addError( e_Error.MissingOpCodeOrDirective, lineNumber, string.Empty );
                            isThereAnError = true;
                            continue;

                            // Label Directive
                        } else if ( !isOpCode1 && !isDirective1 && isDirective2 ) {
                            if ( word1.Length > maxLabelLength ) {
                                errors.addError( e_Error.LabelIsTooLong, lineNumber, word1 );
                                isThereAnError = true;
                                continue;
                            }

                            loc.Label = word1;
                            loc.Directive = directive2;
                            loc.OpCode = e_Keywords.NOP;
                            // Directive Operands
                        } else if ( isDirective1 && !isDirective2 && !isOpCode2 ) {
                            loc.Directive = directive1;
                            loc.OpCode = e_Keywords.NOP;

                            loc.DirectivesOperands = new string[lineParts.Length - 1];
                            for ( int i = 0; i < loc.DirectivesOperands.Length; i++ ) {
                                loc.DirectivesOperands[i] = lineParts[i + 1];
                            }

                            // OpCode Operands
                        } else if ( isOpCode1 && !isOpCode2 && !isDirective2 ) {
                            loc.OpCode = opCode1;
                            loc.Directive = e_Directives.NONE;

                            loc.DirectivesOperands = new string[lineParts.Length - 1];
                            for ( int i = 0; i < loc.DirectivesOperands.Length; i++ ) {
                                loc.DirectivesOperands[i] = lineParts[i + 1];
                            }

                            // Label OpCode
                        } else if ( !isOpCode1 && !isDirective1 && isOpCode2 ) {
                            if ( word1.Length > maxLabelLength ) {
                                errors.addError( e_Error.LabelIsTooLong, lineNumber, word1 );
                                isThereAnError = true;
                                continue;
                            }

                            loc.Label = word1;
                            loc.OpCode = opCode2;
                            loc.Directive = e_Directives.NONE;
                        } else {
                            // error: something went wrong
                            isThereAnError = true;
                            continue;
                        }



                        #endregion
                        #region Three Tokens or more
                    } else if ( lineParts.Length >= 3 ) {
                        if ( !getOpCodeOrLabel( lineParts[0], ref indexInLine, lineNumber, out word1 ) ) {
                            //error
                            isThereAnError = true;
                            continue;
                        }
                        if ( !getOpCodeOrOperand( lineParts[1], ref indexInLine, lineNumber, out word2 ) ) {
                            //error
                            isThereAnError = true;
                            continue;
                        }
                        if ( !getOpCodeOrOperand( lineParts[2], ref indexInLine, lineNumber, out word3 ) ) {
                            //error
                            isThereAnError = true;
                            continue;
                        }
                        if ( word1.Contains( '.' ) ) {
                            wordWithoutDot1 = word1.Substring( 0, word1.IndexOf( '.' ) );
                        } else {
                            wordWithoutDot1 = word1;
                        }
                        if ( word2.Contains( '.' ) ) {
                            wordWithoutDot2 = word2.Substring( 0, word2.IndexOf( '.' ) );
                        } else {
                            wordWithoutDot2 = word2;
                        }
                        if ( word3.Contains( '.' ) ) {
                            wordWithoutDot3 = word3.Substring( 0, word3.IndexOf( '.' ) );
                        } else {
                            wordWithoutDot3 = word3;
                        }

                        foreach ( e_Keywords k in Enum.GetValues( typeof( e_Keywords ) ) ) {
                            if ( !isOpCode1 ) {
                                if ( k.ToString() == wordWithoutDot1.ToUpper() ) {
                                    opCode1 = k;
                                    isOpCode1 = true;
                                    if ( isOpCode2 && isOpCode3 )
                                        continue;

                                }
                            }

                            if ( !isOpCode2 ) {
                                if ( k.ToString() == wordWithoutDot2.ToUpper() ) {
                                    opCode2 = k;
                                    isOpCode2 = true;
                                    if ( isOpCode1 && isOpCode2 )
                                        continue;

                                }
                            }

                            if ( !isOpCode3 ) {
                                if ( k.ToString() == wordWithoutDot3.ToUpper() ) {
                                    opCode3 = k;
                                    isOpCode3 = true;
                                    if ( isOpCode1 && isOpCode2 )
                                        continue;

                                }
                            }
                        }

                        foreach ( e_Directives d in Enum.GetValues( typeof( e_Directives ) ) ) {
                            if ( !isOpCode1 && !isDirective1 ) {
                                if ( d.ToString() == wordWithoutDot1.ToUpper() ) {
                                    directive1 = d;
                                    isDirective1 = true;
                                    if ( isDirective2 && isDirective3 )
                                        continue;

                                }
                            }

                            if ( !isOpCode2 && !isDirective2 ) {
                                if ( d.ToString() == wordWithoutDot2.ToUpper() ) {
                                    directive2 = d;
                                    isDirective2 = true;
                                    if ( isDirective1 && isDirective3 )
                                        continue;

                                }
                            }

                            if ( !isOpCode3 && !isDirective3 ) {
                                if ( d.ToString() == wordWithoutDot3.ToUpper() ) {
                                    directive3 = d;
                                    isDirective3 = true;
                                    if ( isDirective1 && isDirective2 )
                                        continue;

                                }
                            }

                        }

                        if ( isOpCode1 ) {
                            //error
                            isThereAnError = true;
                            continue;
                        } else if ( isDirective1 ) {
                            //error
                            isThereAnError = true;
                            continue;
                        }

                        if ( !isOpCode2 && !isDirective2 ) {
                            errors.addError( e_Error.MissingOpCodeOrDirective, lineNumber, string.Empty );
                            isThereAnError = true;
                            continue;
                        }

                        if ( isOpCode3 ) {
                            //error
                            isThereAnError = true;
                            continue;
                        } else if ( isDirective3 ) {
                            //error
                            isThereAnError = true;
                            continue;
                        }

                        // Label OpCode Operands
                        if ( !isOpCode1 && !isDirective1 && isOpCode2 && !isOpCode3 && !isDirective3 ) {
                            if ( word1.Length > maxLabelLength ) {
                                errors.addError( e_Error.LabelIsTooLong, lineNumber, word1 );
                                isThereAnError = true;
                                continue;
                            }

                            loc.DirectivesOperands = new string[lineParts.Length - 2];
                            for ( int i = 0; i < loc.DirectivesOperands.Length; i++ ) {
                                loc.DirectivesOperands[i] = lineParts[i + 2];
                            }

                            loc.Label = word1;
                            loc.OpCode = opCode2;
                            loc.Directive = e_Directives.NONE;

                            // Label Directive Operands
                        } else if ( !isOpCode1 && !isDirective1 && isDirective2 && !isOpCode3 && !isDirective3 ) {
                            if ( word1.Length > maxLabelLength ) {
                                errors.addError( e_Error.LabelIsTooLong, lineNumber, word1 );
                                isThereAnError = true;
                                continue;
                            }

                            // here split the operands by the comma
                            loc.DirectivesOperands = new string[lineParts.Length - 2];
                            for ( int i = 0; i < loc.DirectivesOperands.Length; i++ ) {
                                loc.DirectivesOperands[i] = lineParts[i + 2];
                            }

                            loc.Label = word1;
                            loc.OpCode = e_Keywords.NOP;
                            loc.Directive = directive2;
                        } else {
                            //error
                            isThereAnError = true;
                            continue;
                        }



                    }
                        #endregion

                    // Check if the first line is ORG
                    if ( lineNumber == 0 && loc.Directive != e_Directives.ORG ) {
                        errors.addError( e_Error.FirstLineORG, lineNumber, Content[lineNumber] );
                        isThereAnError = true;
                        form.addToErrorInterface();
                        form.addToLocationCounter();
                        form.addToProgramLength();
                        return;
                    }

                    #region Address Symbol Table
                    // Check if the label is already in the Address Symbol Table
                    if ( toc.AddressSymbolTable.ContainsKey( loc.Label ) ) {
                        if ( toc.AddressSymbolTable[loc.Label].Value != toc.InvalidValue ) {
                            errors.addError( e_Error.DulicateLabel, lineNumber, loc.Label );
                            isThereAnError = true;
                            continue;
                        }

                    } else {
                        if ( loc.Label != string.Empty ) {

                            if ( registers.Contains( loc.Label ) ) {
                                errors.addError( e_Error.ReservedWord, lineNumber, loc.Label );
                                isThereAnError = true;
                                continue;
                            }

                            // default the label type to REL then
                            // change it depending on the directive
                            ast.LabelType = e_LabelType.REL;
                            ast.Value = locationCounter;
                            toc.AddressSymbolTable.Add( loc.Label, ast );

                        }
                        loc.LocationCounter = locationCounter;
                    }
                    #endregion

                    string OpCodeOrDirective = string.Empty;
                    if ( loc.OpCode == e_Keywords.NOP ) {
                        if ( isDirective1 ) {
                            OpCodeOrDirective = lineParts[0];
                        } else if ( isDirective2 ) {
                            OpCodeOrDirective = lineParts[1];
                        }

                        OpCodeAndOpSize = OpCodeOrDirective.Split( new char[] { '.' }, StringSplitOptions.None );

                        #region Operation Size for DC and DS
                        if ( loc.Directive == e_Directives.DC || loc.Directive == e_Directives.DS ) {
                            // Check the validity of the Operation Size.
                            // If the Operation Size is not available, set the default (Word)
                            if ( OpCodeAndOpSize.Length == 1 ) {
                                loc.OpSize = e_OperationSize.Word;
                                loc.OpMode = e_OperationMode.None;

                                // if the Operation Size is available, then check if it's valid
                            } else if ( OpCodeAndOpSize.Length == 2 ) {

                                // Check if the Operation Size string is longer than 1
                                if ( OpCodeAndOpSize[1].Length > 1 ) {
                                    errors.addError( e_Error.OpSizeIsTooLong, lineNumber, OpCodeAndOpSize[1] );
                                    isThereAnError = true;
                                    continue;
                                }

                                if ( OpCodeAndOpSize[1].Length == 0 ) {
                                    errors.addError( e_Error.MissingOpSize, lineNumber, string.Empty );
                                    isThereAnError = true;
                                    continue;

                                    //------------------------------------\\
                                    // loc.OpSize = e_OperationSize.Word; \\
                                    // loc.OpMode = e_OperationMode.None; \\
                                    //------------------------------------\\

                                }

                                if ( OpCodeAndOpSize[1].ToUpper() == "W" ) {
                                    loc.OpSize = e_OperationSize.Word;
                                    loc.OpMode = e_OperationMode.None;
                                } else if ( OpCodeAndOpSize[1].ToUpper() == "B" ) {
                                    loc.OpSize = e_OperationSize.Byte;
                                    loc.OpMode = e_OperationMode.None;
                                } else {
                                    errors.addError( e_Error.InvalidOpSize, lineNumber, OpCodeAndOpSize[1] );
                                    isThereAnError = true;
                                    continue;
                                }

                            } else {
                                errors.addError( e_Error.InvalidOpSize, lineNumber, OpCodeAndOpSize[1] );
                                isThereAnError = true;
                                continue;
                            }
                        } else {

                            // if the Operation Size is available, then check if it's valid
                            if ( OpCodeAndOpSize.Length == 2 ) {
                                if ( OpCodeAndOpSize[1].Length != 0 ) {
                                    errors.addError( e_Error.InstructionDoesntWorkWithOpSize, lineNumber, OpCodeOrDirective );
                                    isThereAnError = true;
                                    continue;
                                }

                            } else if ( OpCodeAndOpSize.Length > 2 ) {
                                errors.addError( e_Error.InvalidOpSize, lineNumber, OpCodeAndOpSize[1] );
                                isThereAnError = true;
                                continue;
                            }
                        }

                        #endregion


                    } else {
                        if ( isOpCode1 ) {
                            OpCodeOrDirective = lineParts[0];
                        } else if ( isOpCode2 ) {
                            OpCodeOrDirective = lineParts[1];
                        }

                        OpCodeAndOpSize = OpCodeOrDirective.Split( new char[] { '.' }, StringSplitOptions.None );

                        #region Operation Size for Operation Codes
                        // Check the validity of the Operation Size.
                        // If the Operation Size is not available, set the default (Byte)
                        if ( OpCodeAndOpSize.Length == 1 ) {

                            if ( !setOpSize( string.Empty, ref loc, lineNumber ) ) {
                                // The Errors are handled in the method
                                isThereAnError = true;
                                continue;
                            }

                            // if the Operation Size is available, then check if it's valid
                        } else if ( OpCodeAndOpSize.Length == 2 ) {

                            // Check if the Operation Size string is longer than 1
                            if ( OpCodeAndOpSize[1].Length > 1 ) {
                                errors.addError( e_Error.OpSizeIsTooLong, lineNumber, OpCodeAndOpSize[1] );
                                isThereAnError = true;
                                continue;
                            }

                            if ( OpCodeAndOpSize[1].Length == 0 ) {

                                if ( loc.OpCode != e_Keywords.LSP || loc.OpCode != e_Keywords.MULS
                                    || loc.OpCode != e_Keywords.TRP || loc.OpCode == e_Keywords.HLT
                                    || loc.OpCode == e_Keywords.BRA || loc.OpCode == e_Keywords.BNE
                                    || loc.OpCode == e_Keywords.BEQ || loc.OpCode == e_Keywords.BSUB
                                    || loc.OpCode == e_Keywords.BCS || loc.OpCode == e_Keywords.BLT
                                    || loc.OpCode == e_Keywords.RSUB ) {
                                    errors.addError( e_Error.MissingOpSize, lineNumber, string.Empty );
                                    continue;
                                }


                            }

                            if ( (OpCodeAndOpSize[1].Length == 1) && (
                                       loc.OpCode == e_Keywords.LSP || loc.OpCode == e_Keywords.MULS
                                    || loc.OpCode == e_Keywords.TRP || loc.OpCode == e_Keywords.HLT
                                    || loc.OpCode == e_Keywords.BRA || loc.OpCode == e_Keywords.BNE
                                    || loc.OpCode == e_Keywords.BEQ || loc.OpCode == e_Keywords.BSUB
                                    || loc.OpCode == e_Keywords.BCS || loc.OpCode == e_Keywords.BLT
                                    || loc.OpCode == e_Keywords.RSUB                                
                                )) {
                                    errors.addError( e_Error.InstructionDoesntWorkWithOpSize, lineNumber, OpCodeAndOpSize[0] + "." + OpCodeAndOpSize[1] );
                                    isThereAnError = true;
                                    continue;
                                
                            }



                            if ( !setOpSize( OpCodeAndOpSize[1], ref loc, lineNumber ) ) {
                                // The Errors are handled in the method
                                isThereAnError = true;
                                continue;
                            }

                        }
                        #endregion

                    }

                    string RO = string.Empty;
                    for ( int i = 0; i < loc.DirectivesOperands.Length; i++ ) {
                        RO += loc.DirectivesOperands[i];
                    }

                    if ( loc.OpCode == e_Keywords.NOP ) {
                        analyzeOperandsDirectives( RO, lineNumber, ref loc );

                    } else {
                        e_AddressingMode am = e_AddressingMode.None;
                        string[] op = RO.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );

                            #region OpCode: LSP
                        if ( loc.OpCode == e_Keywords.LSP ) {
                            if ( loc.DirectivesOperands[0][0] == 'X'
                                && (
                                    char.IsDigit( loc.DirectivesOperands[0][1] )
                                    && loc.DirectivesOperands[0][1] != '8'
                                    && loc.DirectivesOperands[0][1] != '9'
                                    )
                                    && loc.DirectivesOperands[0].Length == 2
                                ) {

                                am = e_AddressingMode.RegisterAddressing;

                            } else if ( loc.DirectivesOperands[0][0] == '#' ) {
                                am = e_AddressingMode.ImmidiateAddressing;

                            } else if ( loc.DirectivesOperands[0][0] == '$' ) {
                                am = e_AddressingMode.AbsoluteAddressing;

                            } else {
                                errors.addError( e_Error.InvalidAddressingMode, lineNumber, loc.DirectivesOperands[0] );
                                continue;
                            }
                            #endregion
                            #region OpCode: STR, EXG
                        } else if ( loc.OpCode == e_Keywords.STR || loc.OpCode == e_Keywords.EXG ) {
                            if ( loc.DirectivesOperands[0].Contains( '[' ) || loc.DirectivesOperands[0].Contains( ']' ) ) {
                                am = e_AddressingMode.RegisterIndirectWithDisplacementAddressing;

                            } else if ( loc.DirectivesOperands[0][0] == '$' ) {
                                am = e_AddressingMode.AbsoluteAddressing;

                            } else {
                                errors.addError( e_Error.InvalidAddressingMode, lineNumber, loc.DirectivesOperands[0] );
                                continue;
                            }
                            #endregion
                            #region OpCode: POP, PUSH
                        } else if ( loc.OpCode == e_Keywords.POP || loc.OpCode == e_Keywords.PUSH ) {
                            if ( loc.DirectivesOperands[0][0] == 'X'
                                && (
                                    char.IsDigit( loc.DirectivesOperands[0][1] )
                                    && loc.DirectivesOperands[0][1] != '8'
                                    && loc.DirectivesOperands[0][1] != '9'
                                    ) && loc.DirectivesOperands[0].Length == 2
                                ) {

                                am = e_AddressingMode.RegisterAddressing;

                            } else {
                                errors.addError( e_Error.InvalidAddressingMode, lineNumber, loc.DirectivesOperands[0] );
                                continue;
                            }
                            #endregion
                            #region OpCode: RSUB, HLT, RTRP
                        } else if ( loc.OpCode == e_Keywords.RSUB || loc.OpCode == e_Keywords.HLT
                                        || loc.OpCode == e_Keywords.RTRP ) {
                            if ( loc.DirectivesOperands[0] != string.Empty
                                || loc.DirectivesOperands != null ) { // make sure of that
                                errors.addError( e_Error.IllegalAddressingMode, lineNumber, loc.DirectivesOperands[0] );
                                continue;
                            }
                            #endregion
                            #region OpCode: TRP
                        } else if ( loc.OpCode == e_Keywords.TRP ) { // make sure of that
                            loc.SourceOperand = loc.DirectivesOperands[0];
                            #endregion
                            #region OpCode: BRA, BEQ, BNE, BSUB, BCS, BLT
                        } else if ( loc.OpCode == e_Keywords.BRA || loc.OpCode == e_Keywords.BNE
                                 || loc.OpCode == e_Keywords.BEQ || loc.OpCode == e_Keywords.BSUB
                                 || loc.OpCode == e_Keywords.BCS || loc.OpCode == e_Keywords.BLT
                            ) {

                                if ( loc.DirectivesOperands[0][0] == '&' ) {
                                    loc.OpMode = e_OperationMode.Address;
                                    if ( loc.DirectivesOperands[0][1] == '$' ) {
                                        if ( !int.TryParse( loc.DirectivesOperands[0].Substring( 2 ),
                                            System.Globalization.NumberStyles.HexNumber, provider,
                                            out loc.SourceOperandDisplacement ) ) {

                                        }
                                    } else {
                                        if ( !int.TryParse( loc.DirectivesOperands[0].Substring( 1 ),
                                            out loc.SourceOperandDisplacement ) ) {
                                            if ( toc.AddressSymbolTable.ContainsKey( loc.DirectivesOperands[0].Substring( 1 ) ) ) {
                                                if ( toc.AddressSymbolTable[loc.DirectivesOperands[0].Substring( 1 )].Value != toc.InvalidValue ) {
                                                    loc.SourceOperandDisplacement = toc.AddressSymbolTable[loc.DirectivesOperands[0].Substring( 1 )].Value;
                                                    loc.LabelOperand = loc.DirectivesOperands[0].Substring( 1 );

                                                }


                                            } else {
                                                loc.LabelOperand = loc.DirectivesOperands[0];
                                                addToAddressSymbolTable( loc.DirectivesOperands[0] );

                                                if ( loc.DirectivesOperands[0].Substring( 1 ).Length > 6 ) {
                                                    errors.addError( e_Error.LabelIsTooLong, lineNumber, loc.DirectivesOperands[0] );
                                                    isThereAnError = true;
                                                    continue;
                                                }

                                            }
                                        }
                                    }


                                } else {
                                    loc.OpMode = e_OperationMode.PCplusOffset;

                                    if ( loc.DirectivesOperands[0][0] == '$' ) {
                                        if ( !int.TryParse( loc.DirectivesOperands[0].Substring( 1 ),
                                            System.Globalization.NumberStyles.HexNumber, provider,
                                            out loc.SourceOperandDisplacement ) ) {

                                                errors.addError( e_Error.InvalidOperand, lineNumber, loc.DirectivesOperands[0] );
                                                isThereAnError = true;
                                                continue;                                            

                                        }
                                    } else {
                                        if ( !int.TryParse( loc.DirectivesOperands[0], out loc.SourceOperandDisplacement ) ) {
                                            if ( toc.AddressSymbolTable.ContainsKey( loc.DirectivesOperands[0] ) ) {
                                                if ( toc.AddressSymbolTable[loc.DirectivesOperands[0]].Value != toc.InvalidValue ) {
                                                    loc.SourceOperandDisplacement = toc.AddressSymbolTable[loc.DirectivesOperands[0]].Value;
                                                    loc.LabelOperand = loc.DirectivesOperands[0];

                                                }

                                            } else {
                                                loc.LabelOperand = loc.DirectivesOperands[0];
                                                addToAddressSymbolTable( loc.DirectivesOperands[0] );

                                                if ( loc.DirectivesOperands[0].Length > 6 ) {
                                                    errors.addError( e_Error.LabelIsTooLong, lineNumber, loc.DirectivesOperands[0] );
                                                    isThereAnError = true;
                                                    continue;
                                                }
                                            }
                                        }
                                    }

                                }
                            #endregion
                            #region OpCode: The Rest
                        } else {
                            if ( op.Length < 2 ) {
                                errors.addError( e_Error.TooFewOperands, lineNumber, op[0] );
                                isThereAnError = true;
                                continue;
                            }

                            if ( op[1][0] == '#' ) {
                                am = e_AddressingMode.ImmidiateAddressing;

                            } else if ( op[1][0] == '$' ) {
                                am = e_AddressingMode.AbsoluteAddressing;

                            } else if ( op[1][0] == 'X'
                                && ( char.IsDigit( op[1][1] ) && op[1][1] != '8' && op[1][1] != '9' )
                                && op[0].Length == 2 ) {

                                am = e_AddressingMode.RegisterAddressing;

                            } else if ( op[1].Contains( '[' ) || op[1].Contains( ']' ) ) {
                                am = e_AddressingMode.RegisterIndirectWithDisplacementAddressing;

                            } else {
                                am = e_AddressingMode.AbsoluteAddressing;

                            }

                        }
                            #endregion

                            #region Register Indirect With Displacement Addressing
                        if ( ( keyword.Instructions[( int ) loc.OpCode].AddressingMode
                            & e_AddressingMode.RegisterIndirectWithDisplacementAddressing ) == am ) {

                            loc.AddrMode = e_AddressingMode.RegisterIndirectWithDisplacementAddressing;
                            analyzeAddressingMode00( RO, lineNumber, ref loc );

                            #endregion
                            #region Register Addressing
                        } else if ( ( keyword.Instructions[( int ) loc.OpCode].AddressingMode
                             & e_AddressingMode.RegisterAddressing ) == am ) {
                            loc.AddrMode = e_AddressingMode.RegisterAddressing;

                            analyzeAddressingMode01( RO, lineNumber, ref loc );
                            #endregion
                            #region Immidiate Addressing
                        } else if ( ( keyword.Instructions[( int ) loc.OpCode].AddressingMode
                             & e_AddressingMode.ImmidiateAddressing ) == am ) {
                            loc.AddrMode = e_AddressingMode.ImmidiateAddressing;

                            analyzeAddressingMode10( RO, lineNumber, ref loc );
                            #endregion
                            #region Absolute Addressing
                        } else if ( ( keyword.Instructions[( int ) loc.OpCode].AddressingMode
                             & e_AddressingMode.AbsoluteAddressing ) == am ) {
                            loc.AddrMode = e_AddressingMode.AbsoluteAddressing;

                            analyzeAddressingMode11( RO, lineNumber, ref loc );
                            #endregion
                            #region Invalid AddressingMode
                        } else {

                            if (       loc.OpCode != e_Keywords.LSP && loc.OpCode != e_Keywords.MULS
                                    && loc.OpCode != e_Keywords.TRP && loc.OpCode != e_Keywords.HLT
                                    && loc.OpCode != e_Keywords.BRA && loc.OpCode != e_Keywords.BNE
                                    && loc.OpCode != e_Keywords.BEQ && loc.OpCode != e_Keywords.BSUB
                                    && loc.OpCode != e_Keywords.BCS && loc.OpCode != e_Keywords.BLT
                                    && loc.OpCode != e_Keywords.RSUB ) {

                                errors.addError( e_Error.InvalidAddressingMode, lineNumber, RO );
                                continue;
                            }

                            loc.AddrMode = e_AddressingMode.None;

                            
                        }
                            #endregion

                    }

                    toc.LinesOfCodes.Add( loc );

                }


            }

            foreach ( var item in toc.AddressSymbolTable ) {
                if ( item.Value.Value == toc.InvalidValue ) {
                    errors.addError( e_Error.UndefinedLabel, -1, item.Key );
                }
            }

            form.addToErrorInterface();
            form.addToAddressSymbolTableInterface();
            form.addToLocationCounter();
            form.addToProgramLength();

        }

    }


}
