using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System;

namespace EquationResolver.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {

        #region Private Members

        /// <summary>
        ///      The array of bytes
        /// </summary>
        private byte[]? ASCIIvalues;

        /// <summary>
        ///      The original string after removing all the spaces
        /// </summary>
        private string? trimmedAndDeSpacedOriginalString;

        /// <summary>
        ///      The counter for the number of Calculations that have been added to the Calculations Collection
        /// </summary>
        private int calculationCounter = 0;

        #endregion EndRegion-Private Members


        #region Observable Public Properties

        /// <summary>
        ///      The original string that is bound to the Equation TextBox
        /// </summary>
        #region Full Property : OriginalString
        private string originalString = "(1 + 6^2 /3) * 99 - 456";

        public string OriginalString
        {
            get => originalString;
            set
            {
                if (originalString == value) return;

                SetProperty(ref originalString, value);
            }
        }
        #endregion End Region: Full Property : OriginalString

        /// <summary>
        ///      Flag indicating if the equation has invalid chars in it
        /// </summary>
        ///         //[ObservableProperty] private bool invalidCharFound = true;
        #region Full Property : InvalidCharFound
        private bool invalidCharFound = false;

        public bool InvalidCharFound
        {
            get => invalidCharFound;
            set
            {
                SetProperty(ref invalidCharFound, value);
            }
        }
        #endregion End Region: Full Property : InvalidCharFound

        /// <summary>
        ///      The Error Message in the view when invalid chars are in the equation
        /// </summary>
        [ObservableProperty] private string invalidCharErrorMessage = "Invalid Characters in Equation";

        /// <summary>
        ///      Flag if we tried to divide by zero in any of our equations
        /// </summary>
        [ObservableProperty] private bool divideByZeroFound = false;

        /// <summary>
        ///      The Error Message in the view when a Divide by Zero is attempted in the equation
        /// </summary>
        [ObservableProperty] private string divideByZeroErrorMessage = "Can't divide by zero";

        /// <summary>
        ///      Flag indicating if there are not an equal number of opening and closing brackets
        /// </summary>
        [ObservableProperty] private bool mismatchedBracketsFound = false;

        /// <summary>
        ///      The Error Message in the view when the brackets don't match up
        ///           re:  Even number of opening and closing brackets
        /// </summary>
        [ObservableProperty] private string mismatchedBracketsErrorMessage = "Open/Close Brackets Mismatched";

        /// <summary>
        ///      The collection of all the equations performed to create the result in order of operation
        /// </summary>
        [ObservableProperty] private ObservableCollection<string> calculations = new ObservableCollection<string>();

        /// <summary>
        ///      The result of the equation that is bound to the view's Result
        /// </summary>
        [ObservableProperty] private double result = double.NaN;

        #endregion EndRegion:  Observable Public Properties


        #region Private helper methods

        /// <summary>
        ///      Helper method to check for mismatched brackets
        /// </summary>
        /// <param name="s">The string to check</param>
        private void CheckForMismatchedBrackets(string s)
        {
            // Declare a counter for opening and closing brackets
            int bracketsUnmatched = 0;

            // Iterate through the passed in string counting how many opening and closing brackets 
            //      we find in the string.  We need to match up the current opening bracket with
            //      its corresponding closing bracket
            foreach (var c in s)
            {
                // If the char being looked at is an opening bracket... increment the bracket counter
                if ((int)c == 40) bracketsUnmatched++;

                // If the char being looked at is a closing bracket... decrement the bracket counter
                if ((int)c == 41) bracketsUnmatched--;
            }

            //  If after traversing the entire string we have matching brackets
            if (bracketsUnmatched == 0)
            {
                // Set the Flag for Mismatched Brackets to false;
                MismatchedBracketsFound = false;
                // Quit the method
                return;
            }
            else
            {
                // If we don't have corresponding opening and closing brackets, Set the Error flag to true
                MismatchedBracketsFound = true;
            }
        }

        /// <summary>
        /// Returns an array of byte for the passed in string parameter
        /// </summary>
        /// <param name="str">The string to convert to bytes array</param>
        /// <returns></returns>
        private byte[] GetASCIIvalues(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }

        /// <summary>
        ///      Fixes the original string for all the minuses in the original string
        /// </summary>
        /// <param name="trimmedAndDeSpacedOriginalString">The string passed in</param>
        /// <returns></returns>
        private string FixForMinus(string trimmedAndDeSpacedOriginalString)
        {
            // Get an array of bytes for the passed in string
            var bytes = GetASCIIvalues(trimmedAndDeSpacedOriginalString);

            // Declare a property to return as a result
            string result = string.Empty;

            // Iterate through the bytes in the string
            for (var i = 0; i < bytes.Length; i++)
            {
                // If the current byte is a minus (45)
                if (bytes[i] == 45)
                {
                    // If the minus appears at the very beginning of the string don't change anything
                    //      just leave it as the first character
                    if (i == 0) result = result + trimmedAndDeSpacedOriginalString.Substring(i, 1);

                    // If the character before the minus is a number then change it to +- basically,
                    //      change it to adding a negative number
                    else if (IsANumber(bytes[i - 1]) || bytes[i - 1] == 41) result = result + "+-";

                    // If the character before the minus is an operator just leave it alone
                    else result = result + "-";
                }
                // The current byte is not a minus so just add the current character to the result
                else result = result + trimmedAndDeSpacedOriginalString.Substring(i, 1);
            }
            // Return the string with the minuses fixed
            return result;
        }

        /// <summary>
        ///      Check to see if any characters are not Numbers, Operators or Brackets 
        /// </summary>
        /// <param name="bytesToCheck"></param>
        /// <returns>True if all characters in the equation are valid, false otherwise</returns>
        private bool VerifyAllCharsInStringAreValidEquationCharacters(string equationString)
        {
            // Flag for if a Bad Char is found in the passed in string
            var foundBadChar = false;

            // Remove all the trim and empty spaces from the passed in string
            var trimmedAndDeSpacedstring = equationString.Replace(" ", "");

            // Convert the refactored string to an Array of Byte
            var bytesToCheck = GetASCIIvalues(trimmedAndDeSpacedstring);

            // Iterate through the bytes in the string to check to see if they belong
            foreach (var b in bytesToCheck)
            {
                // TODO:  THis is only here so I can pause and see the char value of the byte
                //          Remove this when we are done
                var x = (char)b;

                // If you find one that doesn't belong, quit and return false
                if (!((b >= 40 && b <= 43) || (b >= 45 && b <= 57) || (b == 94)))
                {
                    // If the character didn't match a legal one... set the fail flag to true
                    foundBadChar = true;
                }
            }
            // If we found a bad character in the equation...
            if (foundBadChar)
            {
                // Set the InvalidCharFound flag to true
                InvalidCharFound = true;

                // Return False for valid characters in equation
                return false;
            }
            // If all the characters in the equation are valid...
            else
            {
                // Set the InvalidCharFound flag to false
                InvalidCharFound = false;

                // Return True for valid characters in equation
                return true;
            }
        }

        /// <summary>
        ///      Check to see if a passed in character is a valid number character ( 0-9, .  , - )
        /// </summary>
        /// <param name="charAscii"></param>
        /// <returns></returns>
        private bool IsANumber(int charAscii)
        {

            // TODO: I changed this to include the negative (minus) character... this might break it
            if (((charAscii < 48) || (charAscii > 57)) && charAscii != 46 && charAscii != 45)
            {
                // If we didn't find a number character return false
                return false;
            }

            // If the passed in character is a valid number character, return a valid response
            else return true;
        }

        /// <summary>
        ///      Finds the corresponding closing bracket for the open bracket 
        /// </summary>
        /// <param name="openBracketPosition">The position in the string where the open bracket was found</param>
        /// <param name="stringToCutBracketsOutOf">The string to cut out once the corresponding closing bracket is found</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private string CutOutTextInsideOfBrackets(int openBracketPosition, string stringToCutBracketsOutOf)
        {
            // If we got passed in empty opening and closing brackets then exit with a result of zero
            if (stringToCutBracketsOutOf.Length == 0) return "0";

            // Declare a property to keep track of the resultant string
            string result = null;

            // Counter property to keep track of the number of bracket openings and closings
            int numberOfOpenBrackets = 0;

            // Get an arrat of bytes to represent the passed in string
            var bytes = GetASCIIvalues(stringToCutBracketsOutOf);

            // Iterate through the characters after the found Open Bracket to the end of the string
            for (int i = openBracketPosition + 1; i < stringToCutBracketsOutOf.Length; i++)
            {
                // If we find another Open Bracket increment the number of open brackets found
                if (bytes[i] == 40) numberOfOpenBrackets++;

                // If we find a Closing Bracket...
                if (bytes[i] == 41)
                {
                    // If the Closing Bracket we found is the Corresponding Closing Bracket for the Open Bracket...
                    if (numberOfOpenBrackets == 0)
                    {
                        // Change the Result to be equal to the Equation's second number after the operator
                        result = stringToCutBracketsOutOf.Substring(openBracketPosition + 1, i - openBracketPosition - 1);
                    }

                    // Decrement the number of Open Brackets
                    numberOfOpenBrackets--;
                }

                // If we have a result then obviously we had matching brackets, therefore stop the loop
                if (result != null) break;
            }

            // If we have found a number after the operand, return it
            if (result != null) { return result; }

            // TODO: THis will crash, but it is the final check if result is nothing
            else
            {
                throw new Exception();
                return string.Empty;
            }
        }
        #endregion EndRegion: Private helper methods


        #region Command Methods

        /// <summary>
        ///      Button Command:  Runs when the Solve Equation button is pressed
        /// </summary>
        [RelayCommand]
        public void SolveEquationButtonPressed()
        {
            // Reset the Calculations when the Solve Equation button is pressed
            Calculations = new();

            // Reset the Calculations Counter property back to zero
            calculationCounter = 0;

            // Remove all the spaces from the OriginalString
            trimmedAndDeSpacedOriginalString = OriginalString.Replace(" ", "");

            // Temp property to store the original equation before the minuses are fixed
            var calculationString = trimmedAndDeSpacedOriginalString;

            // Fix the minuses in the string
            trimmedAndDeSpacedOriginalString = FixForMinus(trimmedAndDeSpacedOriginalString);

            // Create the list of ascii values for the string
            ASCIIvalues = GetASCIIvalues(trimmedAndDeSpacedOriginalString);

            // Check to see if any of the characters don't belong in an equation
            var allCharsAreValid = VerifyAllCharsInStringAreValidEquationCharacters(trimmedAndDeSpacedOriginalString);

            // If not all characters in the textbox are valid equation characters [  0-9, (, ), +, -, *, /, ^  ]
            //     then quit the equation resolver attempt
            if (!allCharsAreValid) return;

            // If there are opening brackets and closing brackets that don't match up
            //     then quit the equation resolver attempt
            if (MismatchedBracketsFound) return;

            // Increment the List of Calculations counter
            calculationCounter++;

            // Add the original equation to the list of Calculations
            Calculations.Add(string.Format("{0} :", calculationString));

            // Call the EquationStringResolver method
            var equationResult = EquationStringResolver(trimmedAndDeSpacedOriginalString);

            // If we discovered a divide by zero during the equation resolver, set the result to NaN
            if (DivideByZeroFound) Result = double.NaN;

            // Change the Equation Result back to a double and update the view with the result of the entire equation
            else Result = Convert.ToDouble(equationResult);
        }
        #endregion EndRegion: Command Methods


        #region Public Methods available to the view

        /// <summary>
        ///      Called from the View's code-behind for the Equation Entry Textbox's TextChanged event
        ///           Also it resets the previous Result back to NaN-Not a Number
        /// </summary>
        /// <param name="s"></param>
        public void TextboxTextChangedEventMethodInVM(string s)
        {
            // Method that changes the flag to display the error... TODO: doesn't work
            var _ = VerifyAllCharsInStringAreValidEquationCharacters(s);

            // Check to see if we have the correct number of opening and closing brackets or else
            //      change the error state to visible in the view
            CheckForMismatchedBrackets(s);

            // Reset the Divide by Zero Error Flag
            DivideByZeroFound = false;

            // If the text has changed then we should reset the Result displayed on the view
            Result = double.NaN;
        }

        #endregion EndRegion: Command Methods


        #region Private methods to Solve the Equation

        /// <summary>
        ///      The main resolver method start
        /// </summary>
        /// <param name="stringToResolve">The equation to resolve for</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private string EquationStringResolver(string stringToResolve)
        {

            // Convert the string into an array of bytes
            var bytes = GetASCIIvalues(stringToResolve);

            // Counter for iterating through the characters
            var i = 0;

            // The length of the passed in string
            var l = bytes.Length;

            // The resultant string that includes the text before an opening bracket, the text of the solved
            //      equation, and the text after the corresponding closing bracket
            var resultantString = stringToResolve;

            // Iterate through the bytes
            while (i < l)
            {

                // If we find an open bracket...(
                if ((int)resultantString[i] == 40)
                {
                    // Find the corresponding closing bracket and get the string inside the brackets
                    var cutOutTextFromBrackets = CutOutTextInsideOfBrackets(i, resultantString);

                    // Remember the first part of the string before the opening bracket
                    var firstHalfOfString = resultantString.Substring(0, i);

                    // Remember the last part of the string after the corresponding closing bracket...)  
                    var secondHalfOfString = resultantString.Substring(i + (cutOutTextFromBrackets.Length + 2));

                    // Recursively throw the cut out string back into this method to solve further
                    var solution = EquationStringResolver(cutOutTextFromBrackets);

                    // If the user assumes a * before a bracket like 6(3) it will change it to 6 * (3)
                    if (firstHalfOfString.Length > 0 && IsANumber((int)Convert.ToChar(firstHalfOfString.Substring(firstHalfOfString.Length - 1, 1))))
                        firstHalfOfString = firstHalfOfString + "*";

                    // After the Recursive method has exited... Create a new Resultant sting
                    resultantString = firstHalfOfString + solution + secondHalfOfString;

                    // Adjust the counter for the change in the length of the string between
                    //     the passed in string and the new resultant string
                    i = firstHalfOfString.Length + solution.Length - 1;

                    // Adjust the length property of the while loop to adjust for the new resulant string
                    l = resultantString.Length;
                }
                // Increment the pointer counter
                i++;
            }

            // Solve the equation that occurs in between the brackets or after the brackets have been resolved
            var solvedEquation = SolveSimlifiedEquation(resultantString);

            // If we have found a divide by zero error then exit 
            if (DivideByZeroFound) return string.Empty;

            // Return the Solved Equation
            else return solvedEquation;
        }

        /// <summary>
        ///      Solves equations that don't have any brackets
        /// </summary>
        /// <param name="stringToResolve">The string to solve for</param>
        /// <returns></returns>
        private string SolveSimlifiedEquation(string stringToResolve)
        {
            // Working copy of the string to resolve
            var stringrecieved = stringToResolve;

            // Array of operators     ^   *   /   +   
            int[] operatorsAscii = { 94, 42, 47, 43 };

            // Iterate through each of the operators that the text may contain in operator order
            //     powers first, * and / next, then + and -
            foreach (var o in operatorsAscii)
            {
                // The length of the passed in string
                var l = stringrecieved.Length;

                // The string representing the number before the operand found
                var firstNumberString = string.Empty;

                // The start position of the number before the operator in relation to the string in whole
                int firstNumberPosition = 0;

                // The string representing the number after the operand found
                var secondNumberString = string.Empty;

                // The start position of the number after the operator in relation to the string in whole
                int secondintstrFinalPosition = 0;

                // Flag indicating if the currently being iterated through operator was found
                bool operatorFound = false;

                // counter for the while loop
                int i = 0;


                while (i <= l)
                {
                    // default byte value of the char in the string received
                    int a = 0;
                    // If counter is less than the length of the string received
                    if (i < l)
                    {
                        // The byte value of the char being looked at in the received string
                        a = (int)stringrecieved[i];
                    }

                    // If the char is part of a number and we haven't yet found an operator,
                    // then start remembering the number before the next operator is found
                    if (((a == 45) || (a == 46) || (a >= 48 && a <= 57)) && !operatorFound)
                    {
                        // If the first number string hasn't been changed yet, then remember the position
                        if (firstNumberString == string.Empty) firstNumberPosition = i;

                        // Add the char to the first number string
                        firstNumberString = firstNumberString + stringrecieved.Substring(i, 1);
                    }

                    // If the char is part of a number and we have already found an operator then
                    //    add the char to the second number string
                    else if ((a == 45) || (a == 46) || ((a >= 48 && a <= 57)) && operatorFound)
                    {

                        // Add the char to the second number string
                        secondNumberString = secondNumberString + stringrecieved.Substring(i, 1);

                        // Keep track of the final position of the Second Number String
                        secondintstrFinalPosition = i;
                    }

                    // If the char matches the operator being looked at and we haven't found an operator yet...
                    else if ((a == o) && !operatorFound)
                    {
                        // Set the Operator Found flag to true
                        operatorFound = true;
                    }

                    // If we have already found an operator and now we have found another... solve the equation
                    else if (operatorFound)
                    {
                        // Declare a variable to keep track of the math solution
                        double solution = 0.0;

                        // If the operator is a Power...
                        if (o == 94) solution = Math.Pow(Convert.ToDouble(firstNumberString), Convert.ToDouble(secondNumberString));

                        // If the operator is Multiplication...
                        if (o == 42) solution = Convert.ToDouble(firstNumberString) * Convert.ToDouble(secondNumberString);

                        // If the operator is Division we are trying to divide by zero
                        if (o == 47 && Convert.ToDouble(secondNumberString) == 0)
                        {
                            // Set the DivideByZero flag to true
                            DivideByZeroFound = true;

                            // This probably does nothing
                            solution = 0;
                        }
                        // If the operator is normal Division...
                        else if (o == 47) solution = Convert.ToDouble(firstNumberString) / Convert.ToDouble(secondNumberString);

                        // If the operator is Addition
                        if (o == 43) solution = Convert.ToDouble(firstNumberString) + Convert.ToDouble(secondNumberString);

                        // Temp property to hold the operator
                        var oper = o;

                        // Temp property to hold the second number string
                        string convertedSecondNumberString = secondNumberString;

                        // If we are dealing with addition of a second number that is negative... convert the operator to be
                        //      a minus and show it subtracting a positive instead of adding a negative
                        {
                            // Convert the second number string to a double
                            var secondNumberAsDouble = Convert.ToDouble(secondNumberString);

                            // If the second number is less than zero and we are doing addition...
                            if (o == 43) convertedSecondNumberString = (secondNumberAsDouble < 0) ? (secondNumberAsDouble * -1).ToString("#.##") : secondNumberString;

                            // If we changed the + - to just a -...
                            if (convertedSecondNumberString != secondNumberString) oper = 45;
                        }

                        // Increment the counter for the number of calculations that have already been added to the listview
                        calculationCounter++;

                        // Temp property to hold the solution string
                        var solutionString = string.Empty;

                        // If we have a Divide By Zero error, override the solution string
                        if (DivideByZeroFound) solutionString = "N/A";

                        // Else Set the solution string to the solution with formatting
                        else solutionString = solution.ToString("#.##");

                        // Check to see if the solution was zero then override it
                        if (solutionString == string.Empty) solutionString = "0";

                        // Add the equation string to the observable collection of equations
                        Calculations.Add(string.Format("EQ # {4}:    {0}  {1}  {2}  =  {3}",
                            firstNumberString, (char)oper, convertedSecondNumberString, solutionString, calculationCounter-1));

                        // If we found a divide by zero error in the calculation...
                        if (DivideByZeroFound)
                        {
                            // Increment the list of Calculations counter
                            calculationCounter++;

                            // Add the error to the list of Calculations
                            Calculations.Add("     Divide by zero error");
                        }

                        // Grab the first half of the string before the equation being solved
                        var firstHalfOfString = stringrecieved.Substring(0, firstNumberPosition);

                        // Grab the second half of the string after the equation being solved if there is nothing after the second
                        //      number then just grab the last part of the equation being resolved
                        var secondHalfOfString = (i < l) ?
                            stringrecieved.Substring(secondintstrFinalPosition + 1, stringrecieved.Length - (secondintstrFinalPosition + 1)) : "";

                        // The double solution as a string
                        solutionString = solution.ToString("#.##");

                        // Check to see if the solution was zero then override it
                        if (solutionString == string.Empty) solutionString = "0";

                        // Change the original string to first half, solution and send half
                        stringrecieved = firstHalfOfString + solutionString + secondHalfOfString;

                        // Reset the counter considering the string recieved has now been changed
                        i = firstNumberPosition + solutionString.Length - 1;

                        // Reset the length of the string recieved now that it has changed
                        l = l + solutionString.Length - firstNumberString.Length - secondNumberString.Length - 1;

                        // reset the Second Number string back to empty
                        secondNumberString = string.Empty;

                        // Set the first Number string to the solution of the equation
                        firstNumberString = solutionString;

                        // Reset the operator found flag back to false
                        operatorFound = false;
                    }
                    // If an operator was found but it wasn't the one being looked at, then change
                    //      the first number found to the second number string and second = empty
                    else
                    {
                        // Reset the First Number string
                        firstNumberString = secondNumberString;

                        // Set the Second Number string to empty
                        secondNumberString = string.Empty;
                    }

                    // Increment the counter
                    i++;

                    // If we found a divide by zero error break from the while loop
                    if (DivideByZeroFound) break;
                }

                // If we found a divide by zero error break from the foreach loop
                if (DivideByZeroFound) break;
            }
            // If we found a divide by zero error return an empty string
            if (DivideByZeroFound) return "";

            // Else return the result of the calculations
            else return stringrecieved;
        }

        #endregion EndRegion:  Private methods to Solve the Equation 

        ///// <summary>
        /////      When an operator is found, find the number before the operator
        ///// </summary>
        ///// <param name="stringRecieved">The equation passed in</param>
        ///// <param name="index">The index point of the operator</param>
        ///// <returns></returns>
        //private string FindNumberBeforeOperatorAsString(string stringRecieved, int index)
        //{
        //    // The number before the operator
        //    var firstHalfOfString = stringRecieved.Substring(0, index);

        //    // Set a counter equal to the index of the operator
        //    var i = index;

        //    // Iterate backwards through the passed in string
        //    foreach (var c in stringRecieved.Substring(0, index).Reverse())
        //    {
        //        // If a character is not a number then...
        //        if (!IsANumber(c))
        //        {
        //            // Grab the number before the operand as a string
        //            var numberBeforeOperand = stringRecieved.Substring(i, index - i + 1);

        //            // Return the value
        //            return numberBeforeOperand;
        //        }
        //        // If the character is a number then decrement the counter and look at the character 
        //        //      before this one
        //        else i--;
        //    }

        //    var result = stringRecieved.Substring(0, i);
        //    // TODO:  ONe of these is correct... figure it out

        //    //var result = stringRecieved.Substring(0, index);

        //    // Return the number as a string that is before the operand
        //    return result;

        //}

        /// <summary>
        ///      When an operator is found, find the number after the operator
        /// </summary>
        /// <param name="stringRecieved">The equation passed in</param>
        /// <param name="index">The index point of the operator</param>
        /// <returns></returns>
        //private string FindNumberAfterOperatorAsString(string stringRecieved, int index)
        //{
        //    // Start off by creating a secondHalfOfString that is the entire rest of the string received
        //    var secondHalfOfString = stringRecieved.Substring(index);

        //    // Set the counter at the point where the operator was found
        //    var i = index;

        //    // Iterate through all the characters that were found after the operator
        //    foreach (var c in secondHalfOfString)
        //    {
        //        // If the character is not a number...
        //        if (!IsANumber(c))
        //        {
        //            // Grab the number after the operator
        //            var numberAfterTheOperator = stringRecieved.Substring(i, index - i + 1);

        //            // Return the Number after the Operator
        //            return numberAfterTheOperator;
        //        }
        //        // If we haven't found a character that isn't a number... decrement the counter
        //        i--;
        //    }

        //    // Set the result for the Second Half of String to the found number
        //    var result = secondHalfOfString;

        //    // Return the result
        //    return result;
        //}
    }
}