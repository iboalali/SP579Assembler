using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SP579Assemblerv2 {
    public class TriObjectFile {
        CultureInfo provider;

        TriMainWindow form; TriEditor editor; TriTableOfContens toc;
        public TriObjectFile ( TriMainWindow form, TriEditor editor, TriTableOfContens toc ) {
            this.form = form;
            this.editor = editor;
            this.toc = toc;
            provider = new CultureInfo( "en-US" );

        }

        private string HEXfrom4BitBinary ( string binary ) {
            switch ( binary ) {
                case "0000":
                    return "0";
                case "0001":
                    return "1";
                case "0010":
                    return "2";
                case "0011":
                    return "3";
                case "0100":
                    return "4";
                case "0101":
                    return "5";
                case "0110":
                    return "6";
                case "0111":
                    return "7";
                case "1000":
                    return "8";
                case "1001":
                    return "9";
                case "1010":
                    return "A";
                case "1011":
                    return "B";
                case "1100":
                    return "C";
                case "1101":
                    return "D";
                case "1110":
                    return "E";
                case "1111":
                    return "F";
                default:
                    return "0";
            }
        }

        public string objectCode () {
            int num = 0;
            string instruction = string.Empty;
            string temp = string.Empty;
            string objectFileString = string.Empty;

            #region T Record
            objectFileString += "T ";
            objectFileString += ( toc.LinesOfCodes.Last().LocationCounter - toc.LinesOfCodes.First().LocationCounter ).ToString( "X4" );
            objectFileString += ",";
            objectFileString += toc.LinesOfCodes.First().Label;
            objectFileString += "_";
            objectFileString += toc.LinesOfCodes.First().LocationCounter.ToString( "X4" );
            for ( int i = 0; i < toc.ENTlabels.Count; i++ ) {
                objectFileString += "/" + toc.ENTlabels[i] + "_" + toc.AddressSymbolTable[toc.ENTlabels[i]].Value.ToString( "X4" );

            }
            objectFileString += Environment.NewLine;
            #endregion
            #region N Record
            for ( int i = 0; i < toc.LinesOfCodes.Count; i++ ) {

                if ( toc.LinesOfCodes[i].Directive == e_Directives.EXT
                    || toc.LinesOfCodes[i].Directive == e_Directives.ENT
                    || toc.LinesOfCodes[i].Directive == e_Directives.EQU
                    || toc.LinesOfCodes[i].Directive == e_Directives.DS
                    || toc.LinesOfCodes[i].Directive == e_Directives.ORG
                    || toc.LinesOfCodes[i].Directive == e_Directives.END ) {
                        objectFileString += "Skipped" + Environment.NewLine;
                        continue;

                }

                objectFileString += "N " + toc.LinesOfCodes[i].LocationCounter.ToString( "X4" ) + "-";

                #region Directives
                if ( toc.LinesOfCodes[i].OpCode == e_Keywords.NOP ) {
                    #region DC
                    if ( toc.LinesOfCodes[i].Directive == e_Directives.DC ) {
                        int counter = 0;
                        for ( int j = 0; j < toc.LinesOfCodes[i].DirectivesOperands.Length; j++ ) {
                            if ( toc.LinesOfCodes[i].OpSize == e_OperationSize.Word ) {
                                if ( toc.LinesOfCodes[i].DirectivesOperands[j][0] == '$' ) {
                                    temp = toc.LinesOfCodes[i].DirectivesOperands[j].Substring( 1 );
                                    


                                } else {
                                    if ( !int.TryParse( toc.LinesOfCodes[i].DirectivesOperands[j], out num ) ) {
                                        if ( toc.AddressSymbolTable.ContainsKey( toc.LinesOfCodes[i].DirectivesOperands[j] ) ) {
                                            temp = toc.AddressSymbolTable[toc.LinesOfCodes[i].DirectivesOperands[j]].Value.ToString( "X4" ) + "* ";
                                            temp = temp.Substring( temp.Length - 4 );
                                        }

                                    } else {
                                        temp = num.ToString( "X4" );
                                        temp = temp.Substring( temp.Length - 4 );
                                    }
                                }

                                if ( temp[0] > '8' )
                                    temp = temp.PadLeft( 4, 'F' );
                                else
                                    temp = temp.PadLeft( 4, '0' );

                                objectFileString += temp + "* ";


                            } else if ( toc.LinesOfCodes[i].OpSize == e_OperationSize.Byte ) {
                                if ( toc.LinesOfCodes[i].DirectivesOperands[j][0] == '$' ) {
                                    temp = toc.LinesOfCodes[i].DirectivesOperands[j].Substring( 1 );
                                    


                                } else {
                                    if ( !int.TryParse( toc.LinesOfCodes[i].DirectivesOperands[j], out num ) ) {
                                        if ( toc.AddressSymbolTable.ContainsKey( toc.LinesOfCodes[i].DirectivesOperands[j] ) ) {
                                            temp = toc.AddressSymbolTable[toc.LinesOfCodes[i].DirectivesOperands[j]].Value.ToString( "X2" ) + "* ";
                                            temp = temp.Substring( temp.Length - 2 );
                                        }

                                    } else {
                                        temp = num.ToString( "X2" );
                                        temp = temp.Substring( temp.Length - 2 );
                                    }

                                }
                                if ( temp[0] > '8' )
                                    temp = temp.PadLeft( 2, 'F' );
                                else
                                    temp = temp.PadLeft( 2, '0' );

                                if ( counter < 1 ) {
                                    objectFileString += temp;
                                    counter++;

                                } else {
                                    objectFileString += temp + "* ";
                                    counter = 0;

                                }// end if (counter < 2)
                            }// end if (Byte)
                        }// end for loop j
                    }// end if DC
                    #endregion

                } else {
                #endregion
                #region Operation Code
                    temp = Convert.ToString( ( int ) toc.LinesOfCodes[i].OpCode, 2 ).PadLeft( 5, '0' );
                    instruction = temp.Substring( temp.Length - 5 );

                    #region With Operation Size
                    if ( toc.LinesOfCodes[i].OpSize != e_OperationSize.None ) {
                     
                        #region Operation Size
                        switch ( toc.LinesOfCodes[i].OpSize ) {
                            case e_OperationSize.Byte:
                                instruction += "0";
                                break;
                            case e_OperationSize.Word:
                                instruction += "1";
                                break;
                            default:
                                break;

                        }
                        #endregion
                        #region Addressing Mode
                        switch ( toc.LinesOfCodes[i].AddrMode ) {
                            case e_AddressingMode.RegisterIndirectWithDisplacementAddressing:
                                instruction += "00";
                                break;
                            case e_AddressingMode.RegisterAddressing:
                                instruction += "01";
                                break;
                            case e_AddressingMode.ImmidiateAddressing:
                                instruction += "10";
                                break;
                            case e_AddressingMode.AbsoluteAddressing:
                                instruction += "11";
                                break;
                            default:
                                break;
                        }
                        #endregion
                        #region Source Operand
                        if ( toc.LinesOfCodes[i].SourceOperand == string.Empty ) {
                            instruction += "000";

                        } else {
                            switch ( toc.LinesOfCodes[i].SourceOperand[1] ) {
                                case '0':
                                    instruction += "000";
                                    break;
                                case '1':
                                    instruction += "001";
                                    break;
                                case '2':
                                    instruction += "010";
                                    break;
                                case '3':
                                    instruction += "011";
                                    break;
                                case '4':
                                    instruction += "100";
                                    break;
                                case '5':
                                    instruction += "101";
                                    break;
                                case '6':
                                    instruction += "110";
                                    break;
                                case '7':
                                    instruction += "111";
                                    break;
                                default:
                                    instruction += "000";
                                    break;
                            }
                        }
                        #endregion
                        #region Destination Operand
                        if ( toc.LinesOfCodes[i].DestinationOperand == string.Empty ) {
                            instruction += "000";

                        } else {
                            switch ( toc.LinesOfCodes[i].DestinationOperand[1] ) {
                                case '0':
                                    instruction += "000";
                                    break;
                                case '1':
                                    instruction += "001";
                                    break;
                                case '2':
                                    instruction += "010";
                                    break;
                                case '3':
                                    instruction += "011";
                                    break;
                                case '4':
                                    instruction += "100";
                                    break;
                                case '5':
                                    instruction += "101";
                                    break;
                                case '6':
                                    instruction += "110";
                                    break;
                                case '7':
                                    instruction += "111";
                                    break;
                                default:
                                    instruction += "000";
                                    break;
                            }
                        }
                        #endregion
                        #region Two more '0' Bits
                        instruction += "00";
                        #endregion
                        #region Convert 4-Bit Binray to HEX
                        for ( int j = 0; j < instruction.Length; j += 4 ) {
                            objectFileString += HEXfrom4BitBinary( instruction.Substring( j, 4 ) );
                        }
                        objectFileString += "* ";
                        #endregion
                        #region Some more data
                        if ( toc.LinesOfCodes[i].AddrMode != e_AddressingMode.RegisterAddressing ) {
                            if ( toc.LinesOfCodes[i].OpCode == e_Keywords.STR
                                || toc.LinesOfCodes[i].OpCode == e_Keywords.EXG ) {
                                    objectFileString += toc.LinesOfCodes[i].DestinationOperandDisplacement.ToString( "X4" ) + "* ";

                            } else {
                                objectFileString += toc.LinesOfCodes[i].SourceOperandDisplacement.ToString( "X4" ) + "* ";

                            }

                        } else {




                        }
                        #endregion

                    }
                    #endregion
                    #region Without Operation Size
                    else {

                        if ( toc.LinesOfCodes[i].OpMode == e_OperationMode.None ) {
                            if ( toc.LinesOfCodes[i].OpCode == e_Keywords.RSUB ) {
                                objectFileString += "D800* ";

                            } else if ( toc.LinesOfCodes[i].OpCode == e_Keywords.HLT ) {
                                objectFileString += "E000* ";

                            } else if ( toc.LinesOfCodes[i].OpCode == e_Keywords.RTRP ) {
                                objectFileString += "F000* ";

                            }
                        } else {

                            #region Address or Offset
                            temp = Convert.ToString( toc.LinesOfCodes[i].SourceOperandDisplacement, 2 ).PadLeft( 10, '0' );
                            if ( toc.LinesOfCodes[i].OpMode == e_OperationMode.PCplusOffset ) {
                                instruction += "0";
                                instruction += temp.Substring( temp.Length - 10 );

                            } else if ( toc.LinesOfCodes[i].OpMode == e_OperationMode.Address ) {
                                instruction += "10000000000";

                            }
                            #endregion
                            #region Convert 4-Bit Binray to HEX
                            for ( int j = 0; j < instruction.Length; j += 4 ) {
                                objectFileString += HEXfrom4BitBinary( instruction.Substring( j, 4 ) );
                            }
                            objectFileString += "* ";
                            #endregion
                            #region Some more data
                            if ( toc.LinesOfCodes[i].OpMode == e_OperationMode.Address ) {
                                objectFileString += toc.LinesOfCodes[i].SourceOperandDisplacement.ToString( "X4" );

                            } else if ( toc.LinesOfCodes[i].OpMode == e_OperationMode.PCplusOffset ) {


                            }
                            #endregion

                        }
                    }
                    #endregion
                #endregion

                }

                objectFileString += Environment.NewLine;

            }
            #endregion
            #region Y Record
            objectFileString += "Y " + toc.AddressSymbolTable[toc.LinesOfCodes.Last().LabelOperand].Value.ToString( "X4" );
            #endregion

            return objectFileString;

        }
    }
}
