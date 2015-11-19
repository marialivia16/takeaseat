using System;
using System.Collections.Generic;

namespace TakeASeatApp.Utils
{

	public class SecurityHelper
	{
		public static char[] LowerLetters = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
		public static char[] UpperLetters = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
		public static char[] FigureChars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
		public static char[] SignChars = { '!', '"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/', ':', ';', '=', '?', '@', '[', '\\', ']', '^', '_', '`', '{', '|', '}', '~' };
		public static char[][] AllowedAscii = { LowerLetters, UpperLetters, FigureChars, SignChars };
		public static string GeneratePassword(byte passwordLength = 8, byte minimumLowercaseOccurences = 2, byte minimumUppercaseOccurences = 2, byte minimumFigureOccurences = 2, byte minimumSignOccurences = 2)
		{
			if (passwordLength < 8) passwordLength = 8;
			if (minimumLowercaseOccurences + minimumUppercaseOccurences + minimumFigureOccurences + minimumSignOccurences > passwordLength)
			{
				minimumLowercaseOccurences = 2;
				minimumUppercaseOccurences = 2;
				minimumFigureOccurences = 2;
				minimumSignOccurences = 2;
			}
			// the password is initially an array of empty slots to be filled with chars
			char[] passwordCharSlots = new char[passwordLength];
			// keep a list with indexes of empty char slots in password; this list decreases while populating password
			List<byte> indexesOfEmptySlotsInPassword = new List<byte>();
			// initially all password slots are empty; make a list of their indexes to know which one is empty
			for (byte i = 0; i < passwordLength; i++)
			{
				indexesOfEmptySlotsInPassword.Add(i);
			}
			Random random = new Random();
			byte emptySlotPicker;
			char passwordChar;
			#region fill the minimum number of char types: lowercase and uppercase letters, figures and signs
			for (byte i = 0; i < minimumLowercaseOccurences; i++)
			{
				emptySlotPicker = (byte)random.Next(indexesOfEmptySlotsInPassword.Count);
				passwordChar = LowerLetters[random.Next(LowerLetters.Length)];
				passwordCharSlots[indexesOfEmptySlotsInPassword[emptySlotPicker]] = passwordChar;
				indexesOfEmptySlotsInPassword.RemoveAt(emptySlotPicker);
			}
			for (byte i = 0; i < minimumUppercaseOccurences; i++)
			{
				emptySlotPicker = (byte)random.Next(indexesOfEmptySlotsInPassword.Count);
				passwordChar = UpperLetters[random.Next(UpperLetters.Length)];
				passwordCharSlots[indexesOfEmptySlotsInPassword[emptySlotPicker]] = passwordChar;
				indexesOfEmptySlotsInPassword.RemoveAt(emptySlotPicker);
			}
			for (byte i = 0; i < minimumFigureOccurences; i++)
			{
				emptySlotPicker = (byte)random.Next(indexesOfEmptySlotsInPassword.Count);
				passwordChar = FigureChars[random.Next(FigureChars.Length)];
				passwordCharSlots[indexesOfEmptySlotsInPassword[emptySlotPicker]] = passwordChar;
				indexesOfEmptySlotsInPassword.RemoveAt(emptySlotPicker);
			}
			for (byte i = 0; i < minimumSignOccurences; i++)
			{
				emptySlotPicker = (byte)random.Next(indexesOfEmptySlotsInPassword.Count);
				passwordChar = SignChars[random.Next(SignChars.Length)];
				passwordCharSlots[indexesOfEmptySlotsInPassword[emptySlotPicker]] = passwordChar;
				indexesOfEmptySlotsInPassword.RemoveAt(emptySlotPicker);
			}
			#endregion
			#region fill the rest of the empty slots in password
			byte charType;
			while (indexesOfEmptySlotsInPassword.Count > 0)
			{
				charType = (byte)random.Next(4);
				emptySlotPicker = (byte)random.Next(indexesOfEmptySlotsInPassword.Count);
				passwordChar = AllowedAscii[charType][random.Next(AllowedAscii[charType].Length)];
				passwordCharSlots[indexesOfEmptySlotsInPassword[emptySlotPicker]] = passwordChar;
				indexesOfEmptySlotsInPassword.RemoveAt(emptySlotPicker);
			}
			#endregion
			return new string(passwordCharSlots);
		}
	}
}