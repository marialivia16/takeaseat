using System.Linq;
using System.Text;

namespace TakeASeatApp.Utils
{

	public class TextHelper
	{
		public static string CleanupDiacriticsFromUsername(string input)
		{
			StringBuilder stbRetVal = new StringBuilder(input);

			stbRetVal.Replace('ă', 'a'); stbRetVal.Replace('Ă', 'A');
			stbRetVal.Replace('î', 'i'); stbRetVal.Replace('Î', 'I');
			stbRetVal.Replace('â', 'a'); stbRetVal.Replace('Â', 'A');
			stbRetVal.Replace('ș', 's'); stbRetVal.Replace('Ș', 'S');
			stbRetVal.Replace('ț', 't'); stbRetVal.Replace('Ț', 'T');

			stbRetVal.Replace('ã', 'a'); stbRetVal.Replace('Ã', 'A');
			stbRetVal.Replace('ţ', 't'); stbRetVal.Replace('Ţ', 'T');
			stbRetVal.Replace('ş', 's'); stbRetVal.Replace('Ş', 'S');
			stbRetVal.Replace('ȃ', 'a'); stbRetVal.Replace('Ȃ', 'A');

			stbRetVal.Replace('á', 'a'); stbRetVal.Replace('Á', 'A');
			stbRetVal.Replace('à', 'a'); stbRetVal.Replace('À', 'A');
			stbRetVal.Replace('ä', 'a'); stbRetVal.Replace('Ä', 'A');
			stbRetVal.Replace('è', 'a'); stbRetVal.Replace('È', 'A');
			stbRetVal.Replace('é', 'a'); stbRetVal.Replace('É', 'A');
			stbRetVal.Replace('ë', 'a'); stbRetVal.Replace('Ë', 'A');
			stbRetVal.Replace('ö', 'a'); stbRetVal.Replace('Ö', 'A');
			stbRetVal.Replace('ò', 'a'); stbRetVal.Replace('Ò', 'A');
			stbRetVal.Replace('ó', 'a'); stbRetVal.Replace('Ó', 'A');
			stbRetVal.Replace('ù', 'a'); stbRetVal.Replace('Ù', 'A');
			stbRetVal.Replace('ú', 'a'); stbRetVal.Replace('Ú', 'A');
			stbRetVal.Replace('ü', 'a'); stbRetVal.Replace('Ü', 'A');
			//stbRetVal.Replace('', 'a'); stbRetVal.Replace('', 'A');

			char[] allowedChars = { ' ', '.', '-', '?' };
			char[] letters = stbRetVal.ToString().ToCharArray();
			int i = 0;
			while (i < stbRetVal.Length)
			{
				if (allowedChars.Contains(stbRetVal[i]))
				{
					i++;
					continue;
				}
				if ((int)stbRetVal[i] > 127 || !char.IsLetter(stbRetVal[i]))
				{
					stbRetVal[i] = '?';
					i++;
					continue;
				}
				i++;
			}
			return stbRetVal.ToString();
		}
		public static string RemoveDiacritics(string input)
		{
			StringBuilder stbRetVal = new StringBuilder(input);

			stbRetVal.Replace('ă', 'a'); stbRetVal.Replace('Ă', 'A');
			stbRetVal.Replace('î', 'i'); stbRetVal.Replace('Î', 'I');
			stbRetVal.Replace('â', 'a'); stbRetVal.Replace('Â', 'A');
			stbRetVal.Replace('ș', 's'); stbRetVal.Replace('Ș', 'S');
			stbRetVal.Replace('ț', 't'); stbRetVal.Replace('Ț', 'T');

			stbRetVal.Replace('ã', 'a'); stbRetVal.Replace('Ã', 'A');
			stbRetVal.Replace('ţ', 't'); stbRetVal.Replace('Ţ', 'T');
			stbRetVal.Replace('ş', 's'); stbRetVal.Replace('Ş', 'S');
			stbRetVal.Replace('ȃ', 'a'); stbRetVal.Replace('Ȃ', 'A');

			stbRetVal.Replace('á', 'a'); stbRetVal.Replace('Á', 'A');
			stbRetVal.Replace('à', 'a'); stbRetVal.Replace('À', 'A');
			stbRetVal.Replace('ä', 'a'); stbRetVal.Replace('Ä', 'A');
			stbRetVal.Replace('è', 'a'); stbRetVal.Replace('È', 'A');
			stbRetVal.Replace('é', 'a'); stbRetVal.Replace('É', 'A');
			stbRetVal.Replace('ë', 'a'); stbRetVal.Replace('Ë', 'A');
			stbRetVal.Replace('ö', 'a'); stbRetVal.Replace('Ö', 'A');
			stbRetVal.Replace('ò', 'a'); stbRetVal.Replace('Ò', 'A');
			stbRetVal.Replace('ó', 'a'); stbRetVal.Replace('Ó', 'A');
			stbRetVal.Replace('ù', 'a'); stbRetVal.Replace('Ù', 'A');
			stbRetVal.Replace('ú', 'a'); stbRetVal.Replace('Ú', 'A');
			stbRetVal.Replace('ü', 'a'); stbRetVal.Replace('Ü', 'A');
			//stbRetVal.Replace('', 'a'); stbRetVal.Replace('', 'A');

			return stbRetVal.ToString();
		}
		public static string CleanupFileName(string fileName)
		{
			if (string.IsNullOrEmpty(fileName)) return fileName;
			StringBuilder stbRetVal = new StringBuilder(fileName);
			for (int i = 0; i < stbRetVal.Length; i++)
			{
				if (stbRetVal[i] == '.' || stbRetVal[i] == ',') continue;
				if (stbRetVal[i] == '_' || stbRetVal[i] == '-') continue;
				if (stbRetVal[i] == '(' || stbRetVal[i] == ')') continue;
				if (char.IsLetter(stbRetVal[i])) continue;
				if (char.IsNumber(stbRetVal[i])) continue;
				stbRetVal[i] = '_';
			}
			return stbRetVal.ToString();
		}
	}
}