using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.Win32;
using System.IO;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Configuration;
using System.Web;
using System.Xml;
using System.Reflection;
using System.Linq;
using System.Text;


public static class UtilReflection
{
	/// <summary>
	/// this method is much faster than the method below, since reflection is intensive
	/// </summary>
	/// <param name="o1"></param>
	/// <param name="o2"></param>
	/// <param name="arrProp"></param>
	/// <param name="strField"></param>
	/// <returns></returns>
	public static bool Equals(object o1, object o2, PropertyInfo[] arrProp, string strField)
	{
		if (o1.GetType() != o2.GetType())
		{
			return false;
		}

		PropertyInfo prop = arrProp.Where(p => p.Name.ToUpper() == strField.ToUpper()).FirstOrDefault();
		if (prop == null)
			return false;

		return prop.GetValue(o1, null).Equals(prop.GetValue(o2, null));
	}

	public static bool Equals(object o1, object o2, string strField)
	{
		PropertyInfo[] arrProp = o1.GetType().GetProperties();
		return Equals(o1, o2, arrProp, strField);
	}

	public static void CopyTo(object from, object to)
	{
		if (from.GetType() != to.GetType())
		{
			return;
		}

		PropertyInfo[] arrProp = from.GetType().GetProperties();
		foreach (PropertyInfo p in arrProp)
		{
			p.SetValue(to, p.GetValue(from, null), null);
		}
	}

	public static object GetTypedObject(SqlConnection cn, string strSQL, Type type)
	{
		object ret = Activator.CreateInstance(type);
		DataTable dTbl = UtilDB.ExecuteQueryDataTable(cn, strSQL);

		if (dTbl.Rows.Count > 0)
		{
			//ret = Activator.CreateInstance(type);
			UtilReflection.GetTypedObject(dTbl.Rows[0], ret);
		}
		else
			ret = null;

		return ret;
	}

	public static void GetTypedObject(DataRow dr, object ret)
	{
		PropertyInfo[] arrProp = ret.GetType().GetProperties();
		GetTypedObject(dr, ret, arrProp);
	}

	public static void GetTypedObject(DataRow dr, object ret, PropertyInfo[] arrProp)
	{
		foreach (DataColumn dc in dr.Table.Columns)
		{
			PropertyInfo prop = arrProp.Where(p => p.Name.ToUpper() == dc.ColumnName.ToUpper()).FirstOrDefault();
			if (prop != null)
			{
				if (dr[dc.Ordinal] == DBNull.Value)
				{
					if (prop.GetType() == typeof(string))
					{
						prop.SetValue(ret, string.Empty, null);
					}
					else if (prop.GetType() == typeof(int))
					{
						prop.SetValue(ret, -1, null);
					}
					else if (prop.GetType() == typeof(DateTime))
					{
						prop.SetValue(ret, UtilDate.EmptyDate, null);
					}
					else if (prop.GetType() == typeof(bool))
					{
						prop.SetValue(ret, false, null);
					}
					else
					{
						prop.SetValue(ret, null, null);
					}
				}
				else
				{
					prop.SetValue(ret, dr[dc.Ordinal], null);
				}
			}
		}
	}

	public static void SetTypedObject(DataRow dr, object ret)
	{
		PropertyInfo[] arrProp = ret.GetType().GetProperties();

		foreach (DataColumn dc in dr.Table.Columns)
		{
			PropertyInfo prop = arrProp.Where(p => p.Name.ToUpper() == dc.ColumnName.ToUpper()).FirstOrDefault();
			if (prop != null)
			{
				object tmp = prop.GetValue(ret, null);

				if (tmp == null)
				{
					dr[dc.Ordinal] = DBNull.Value;
				}
				else
				{
					dr[dc.Ordinal] = tmp;
				}
			}
		}
	}
}


public static class UtilXML
{
	/// <summary>
	/// get a matching xml node whose name matches the given regular expression pattern.  search node, and all sub-children. 
	///   return null if none found
	/// </summary>
	public static XmlNode GetMatchingNode(XmlNode node, string strRegexMatch)
	{
		if (Regex.IsMatch(node.Name, strRegexMatch, RegexOptions.IgnoreCase))
		{
			return node;
		}

		foreach (XmlNode childNode in node.ChildNodes)
		{
			XmlNode subChild = GetMatchingNode(childNode, strRegexMatch);
			if (subChild != null) return subChild;
		}

		return null;
	}

	/// <summary>
	/// return all elements in an xmlnode matching a specific tag (including sub-elements).
	/// </summary>
	public static XmlNodeList GetXMLNodes(XmlNode xNode, string strTag)
	{
		XmlDocument xDoc = new XmlDocument();
		xDoc.LoadXml(xNode.OuterXml);
		return xDoc.GetElementsByTagName(strTag);
	}

	/// <summary>
	/// strAttribValue is case insensitive
	/// </summary>
	public static List<XmlNode> GetXMLNodesByAttrib(XmlNodeList arrNode, string strAttribName, string strAttribValue)
	{
		List<XmlNode> arrRet = new List<XmlNode>();

		foreach (XmlNode xn in arrNode)
		{
			if (string.Compare(xn.Attributes[strAttribName].Value, strAttribName, true) == 0)
			{
				arrRet.Add(xn);
			}
		}
		return arrRet;
	}
}

public class UtilFile
{
	public UtilFile()
	{
		// 
		// TODO: Add constructor logic here
		//
	}

	public static void CopyDirectory(string Src, string Dst)
	{
		String[] Files;

		if (Dst[Dst.Length - 1] != Path.DirectorySeparatorChar)
			Dst += Path.DirectorySeparatorChar;
		if (!Directory.Exists(Dst)) Directory.CreateDirectory(Dst);
		Files = Directory.GetFileSystemEntries(Src);
		foreach (string Element in Files)
		{
			// Sub directories

			if (Directory.Exists(Element))
				CopyDirectory(Element, Dst + Path.GetFileName(Element));
			// Files in directory

			else
				File.Copy(Element, Dst + Path.GetFileName(Element), true);

		}
	}

	public static String AppPath()
	{
		return DirNoSlash(System.AppDomain.CurrentDomain.BaseDirectory);
	}

	public static bool CompFiles(string strFile1, string strFile2)
	{
		if (!System.IO.File.Exists(strFile1) || !System.IO.File.Exists(strFile2)) return false;

		clsFile cFile1 = new clsFile(strFile1, true);
		clsFile cFile2 = new clsFile(strFile2, true);

		if (cFile1.arrLine.Count != cFile2.arrLine.Count)
		{
			return false;
		}
		else if (cFile1.arrLine.Count == 0)
		{
			return false;
		}

		for (int i = 0; i < cFile1.arrLine.Count; i++)
		{
			if ((string)cFile1.arrLine[i] != (string)cFile2.arrLine[i])
			{
				return false;
			}
		}

		return true;
	}

	public static String ContainingDirectory(String strFile)
	{
		String strPath;
		int intSlash;

		strPath = strFile;
		if (strFile == "" || strFile == "\\")
		{
			return strFile;
		}
		intSlash = strFile.LastIndexOf("\\");

		if (strFile.IndexOf(".") < 0)
		{
			//it's a directory
			if (intSlash == strFile.Length)
			{
				strPath = strFile.Substring(0, strFile.Length - 1);
			}
		}

		intSlash = strPath.LastIndexOf("\\");

		if (intSlash <= 0)
		{
			return strFile;
		}
		strPath = strFile.Substring(0, intSlash);
		return strPath;
	}

	public static String DirWithSlash(String strDir)
	{
		String strTemp;
		if (strDir == "")
		{
			return "";
		}
		strTemp = strDir.Substring(strDir.Length - 1, 1);
		if (strTemp != "\\" && strTemp != "/")
		{
			if (strDir.Contains("/"))
			{
				return strDir + "/"; //http path or something
			}
			else
			{
				return strDir + "\\";
			}
		}
		else
		{
			return strDir;
		}
	}

	public static string DirNoSlash(string strDir)
	{
		if ((strDir.EndsWith("\\") || strDir.EndsWith("/")) && strDir != "\\" && strDir != "/")
		{
			return strDir.Substring(0, strDir.Length - 1);
		}

		return strDir;
	}

	/// <summary>
	/// the size of all files within and (optionally) files within subdirectories
	/// </summary>
	/// <param name="strDir"></param>
	/// <param name="boolSubDirs"></param>
	/// <returns></returns>
	public static long DirLen(string strDir, bool boolSubDirs)
	{
		clsDirectory cDir = new clsDirectory(strDir);
		cDir.Populate(boolSubDirs);

		long intTotLength = 0;

		foreach (clsFile cFile in cDir.arrFile)
		{
			intTotLength += FileLen(cFile.strFile);

		}

		if (boolSubDirs)
		{
			foreach (clsDirectory cSubDir in cDir.arrDirectory)
			{
				intTotLength += DirLen(cSubDir.strDirectory, true);
			}
		}

		return intTotLength;
	}

	public static long FileLen(String strFile)
	{
		if (!FileIsThere(strFile))
			return -1;

		FileInfo fi = new FileInfo(strFile);
		return fi.Length;
	}

	public static void FindReplace(string strTextFile, string strFind, string strReplace)
	{
		string strTemp;
		strFind = Regex.Escape(strFind);
		try
		{
			clsFile clsText = new clsFile(strTextFile, true);

			UtilFile.KillFile(clsText.strFile);
			foreach (string str in clsText.arrLine)
			{
				strTemp = Regex.Replace(str, strFind, strReplace, RegexOptions.IgnoreCase);
				UtilFile.WriteToFile(strTemp, clsText.strFile);
			}
		}
		catch
		{

		}
	}

	public static long SplitFile(String strFilePath, ArrayList arrFile)
	{
		arrFile.Clear();

		if (!FileIsThere(strFilePath))
		{
			return -1;
		}
		System.IO.StreamReader sr = new System.IO.StreamReader(strFilePath);

		String strTemp;
		strTemp = sr.ReadLine();

		while (strTemp != null)
		{
			//if(strTemp!="")
			//changed 100906 - we want to preserve blank line info(preserve crlfs)
			arrFile.Add(strTemp);

			strTemp = sr.ReadLine();
		}
		sr.Close();
		sr = null;
		return arrFile.Count;
	}

	public static List<string> SplitFile(String strFilePath)
	{
		List<string> arrFile = new List<string>();

		if (!FileIsThere(strFilePath))
		{
			return arrFile;
		}

		System.IO.StreamReader sr = new System.IO.StreamReader(strFilePath);

		String strTemp;
		strTemp = sr.ReadLine();

		while (strTemp != null)
		{
			//if(strTemp!="")
			//changed 100906 - we want to preserve blank line info(preserve crlfs)
			arrFile.Add(strTemp);

			strTemp = sr.ReadLine();
		}
		sr.Close();
		sr = null;
		return arrFile;
	}

	public static void WriteToFileBinary(byte[] arrData, string strFile)
	{
		if (UtilFile.FileIsThere(strFile))
			throw new Exception("File already exists: " + strFile);

		using (System.IO.BinaryWriter bw = new BinaryWriter(File.Open(strFile, FileMode.Create)))
		{
			bw.Write(arrData);
		}
	}

	public static void WriteToFile(String strText, String strFile)
	{
		try
		{
			StreamWriter sr = new StreamWriter(strFile, true);
			sr.WriteLine(strText);
			sr.Close();
			sr = null;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
	}

	public static void WriteToFile(String strText, String strFile, System.Text.Encoding enc)
	{
		try
		{
			StreamWriter sr = new StreamWriter(strFile, true, enc);
			sr.WriteLine(strText);
			sr.Close();
			sr = null;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
	}

	public static bool FileIsThere(String strFile)
	{
		return File.Exists(strFile);
	}

	public static string FileFromDir(string strFile)
	{
		if (strFile.IndexOf("\\") < 0) return strFile;
		if (strFile.EndsWith("\\")) return "";
		int intTemp = strFile.LastIndexOf("\\");
		string strRet = strFile.Substring(intTemp + 1);
		return strRet;
	}

	public static String NameMinusExtension(String strFile)
	{
		int intPos;
		String retVal;
		retVal = "";
		intPos = UtilString.InStrRev(strFile, ".");
		if (intPos <= 1)
		{
			return retVal;
		}

		retVal = UtilString.Mid(strFile, 1, intPos - 1);
		return retVal;
	}

	public static bool DeleteDir(String strdirectory)
	{
		Directory.Delete(strdirectory, true);
		return !Directory.Exists(strdirectory) ? true : false;
	}

	public static void KillFile(String strFile)
	{
		try
		{
			//need to be able to delete read only files
			if ((File.GetAttributes(strFile) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
			{
				File.SetAttributes(strFile, File.GetAttributes(strFile) ^ FileAttributes.ReadOnly);
			}

			File.Delete(strFile);
		}
		catch
		{

		}
	}

	public static bool DirIsThere(String strdirectory)
	{
		return Directory.Exists(strdirectory);
	}

	public static bool MakePath(String strdirectory)
	{
		if (!DirIsThere(strdirectory))
		{
			Directory.CreateDirectory(strdirectory);
			return DirIsThere(strdirectory) ? true : false;
		}
		return true;
	}

	public static bool IsDirectory(String strFile)
	{
		if (UtilFile.FileIsThere(strFile) || UtilFile.DirIsThere(strFile))
		{
			return ((File.GetAttributes(strFile) & FileAttributes.Directory) == FileAttributes.Directory);
		}
		else
		{
			return false;
		}
	}

	public static bool OpenFileCopy(String strFileFrom, String strFileTo)
	{
		return OpenFileCopy(strFileFrom, strFileTo, true);
	}

	public static bool OpenFileCopy(String strFileFrom, String strFileTo, bool boolOverWrite)
	{
		if (UtilFile.IsDirectory(strFileTo))
		{
			strFileTo = UtilFile.DirWithSlash(strFileTo) + UtilFile.FileFromDir(strFileFrom);
		}

		try
		{
			File.Copy(strFileFrom, strFileTo, boolOverWrite);
			return true;
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	/// Return a string containing the contents of a whole file
	/// </summary>
	public static string FileData(string strFile)
	{
		if (!UtilFile.FileIsThere(strFile)) return "";

		StreamReader strm = new StreamReader(strFile, System.Text.Encoding.ASCII);
		string strRet = strm.ReadToEnd();
		strm.Dispose();
		return strRet;
	}

	public static byte[] FileDataBinary(string strFile)
	{
		System.IO.BinaryReader br = new System.IO.BinaryReader(new System.IO.FileStream(strFile, System.IO.FileMode.Open, FileAccess.Read));
		byte[] arr = br.ReadBytes(Convert.ToInt32(FileLen(strFile)));
		br.Close();
		return arr;
	}

}

/// <summary>
/// Summary description for UtilString.
/// </summary>
public static class UtilString
{

	public static string ConcatStringFromListArr(List<string> items, string delimConcat = ", ")
	{
		string ret = "";

		foreach (string item in items)
		{
			ret += item + delimConcat;
		}

		if (ret != "")
			ret = ret.Substring(0, ret.Length - delimConcat.Length);

		return ret;
	}

	public static string DataTableToCSV(DataTable dTbl, bool shortenDateTimes = true)
	{
		StringBuilder sb = new StringBuilder();

		IEnumerable<string> columnNames = dTbl.Columns.Cast<DataColumn>().
										  Select(column => column.ColumnName);
		sb.AppendLine(string.Join(",", columnNames));

		foreach (DataRow row in dTbl.Rows)
		{
			List<string> fields = row.ItemArray.Select(field => field.ToString()).ToList();

			if (shortenDateTimes)
			{
				for (int i = 0; i < fields.Count(); i++)
				{
					string val = fields[i];
					if (!UtilDate.DateIsEmpty(UtilDate.DateGetDateTime(val)))
					{
						val = UtilDate.DateGetDateTime(val).ToShortDateString();
						fields[i] = val;
					}
				}
			}

			//put quotes around all fields
			fields = fields.Select(field => "\"" + field.ToString().Replace("\"", "") + "\"").ToList();

			sb.AppendLine(string.Join(",", fields));
		}

		return sb.ToString();
	}



	public static string HTMLTableToCSV(string strTable, string initialTableTagReplace = "<table>", string replaceCommas = ";")
	{

		return strTable.Replace(",", replaceCommas).Replace("<tr>", "").Replace(initialTableTagReplace, "").Replace("</tr>", "\r\n")
			.Replace("<td>", "").Replace("</td>", ", ").Replace("</table>", "");
	}


	public static int ToIntDefault(object num, int intdefault = 0)
	{
		if (num == null)
			return 0;

		if (!UtilString.IsInt(num.ToSafeString()))
			return intdefault;

		return Convert.ToInt32(num.ToString());
	}

	public static string ToSafeString(this object str)
	{
		if (str == null) return string.Empty;
		return str.ToString();
	}

	public static string ToSafeString(this string str)
	{
		if (str == null) return string.Empty;
		return str;
	}

	public static int ToInt32(this string toInt)
	{
		if (string.IsNullOrEmpty(toInt.ToSafeString()))
			return 0;

		return Convert.ToInt32(toInt);
	}

	public static string ToTitleCase(string mText)
	{
		string rText = "";
		mText = mText.ToLower();
		try
		{
			System.Globalization.CultureInfo cultureInfo =
			System.Threading.Thread.CurrentThread.CurrentCulture;
			System.Globalization.TextInfo TextInfo = cultureInfo.TextInfo;
			rText = TextInfo.ToTitleCase(mText);
		}
		catch
		{
			rText = mText;
		}
		return rText;
	}

	public static decimal ToNumberDefaultDecimal(string strNumCheck, decimal dcDefault)
	{
		decimal dcRet = dcDefault;
		if (IsNumber(strNumCheck))
		{
			dcRet = Convert.ToDecimal(strNumCheck);
		}
		return dcRet;
	}

	public static int IndexOf(String str, String strSearchFor, int intOccurrence)
	{
		int intCurr = 1;
		int intCurrPos = str.IndexOf(strSearchFor);

		while (intCurrPos > 0)
		{
			if (intOccurrence == intCurr)
			{
				return intCurrPos;
			}

			if (intCurrPos == str.Length - 1)
				return -1;

			intCurrPos = str.IndexOf(strSearchFor, intCurrPos + 1);

			intCurr++;
		}
		return -1;
	}

	public static String DelimField(String str, String strDelim, int intField, bool boolIncludeEverythingAfter = false)
	{
		String strTemp = string.Empty;
		int i;
		int intTemp1 = -1;

		if (str.Length == 0 || strDelim.Length == 0 || intField <= 0)
		{
			return string.Empty;
		}

		int intCount = Count(str, strDelim);
		if (intCount == 0 && intField == 1)
		{
			return str;
		}

		if (intField > intCount + 1)
		{
			return string.Empty;
		}

		int intCurrPos = 1;

		i = 0;
		while (intCurrPos <= intField)
		{
			intTemp1 = str.IndexOf(strDelim, i);
			if (intTemp1 < 0)
			{
				if (intCurrPos == intField)
				{
					strTemp = str.Substring(i, str.Length - i);
				}
				return strTemp;
			}
			else
			{
				if (intCurrPos == intField)
				{
					if (boolIncludeEverythingAfter)
					{
						strTemp = str.Substring(i);
					}
					else
					{
						strTemp = str.Substring(i, intTemp1 - i);
					}
					return strTemp;
				}

				i = intTemp1 + strDelim.Length;
				intCurrPos = intCurrPos + 1;
			}
		}

		return string.Empty;
	}

	/// <summary>
	/// remove substrings, ie 'hello<start>123434</start>567 would result in hello567
	/// if you passed <start>, </start>
	/// </summary>
	/// <param name="strFull"></param>
	/// <param name="strStart"></param>
	/// <param name="strEnd"></param>
	/// <returns></returns>
	public static string BeforeAfterRemove(string strFull, string strStart, string strEnd)
	{
		List<int> arrStart = new List<int>();
		int i = strFull.IndexOf(strStart, StringComparison.CurrentCultureIgnoreCase);

		while (i >= 0 && i != strFull.Length - 1)
		{
			arrStart.Add(i);
			i = strFull.IndexOf(strStart, i + 1, StringComparison.CurrentCultureIgnoreCase);
		}

		i = strFull.IndexOf(strEnd, StringComparison.CurrentCultureIgnoreCase);
		List<int> arrEnd = new List<int>();
		while (i >= 0 && i != strFull.Length - 1)
		{
			arrEnd.Add(i + strEnd.Length);
			i = strFull.IndexOf(strEnd, i + 1, StringComparison.CurrentCultureIgnoreCase);
		}

		string strRet = strFull;
		if (arrStart.Count > 0 && arrEnd.Count > 0)
		{
			strRet = string.Empty;
			bool bOn = true;

			for (int c = 0; c < strFull.Length; c++)
			{
				if (arrStart.Contains(c))
				{
					bOn = false;
				}
				else if (arrEnd.Contains(c))
				{
					bOn = true;
				}

				if (bOn)
				{
					strRet += strFull[c];
				}
			}
		}

		return strRet;
	}

	public static string FormatPhone(string strPhone)
	{
		if (strPhone == null)
			return string.Empty;

		string strRet = strPhone;

		if (UtilString.ReplaceNonInt(strRet, "", true, true).Length != 10)
			return strRet;

		strRet = UtilString.ReplaceNonInt(strRet, string.Empty, true, true);

		strRet = "(" + strRet.Substring(0, 3) + ") " + strRet.Substring(3, 3) + "-" + strRet.Substring(6);
		return strRet;
	}

	public static string SafeStr(object obj)
	{
		if (obj == null || obj == DBNull.Value)
		{
			return string.Empty;
		}

		return obj.ToString();
	}

	//convert a number to words anywhere from 0 - 999
	public static string NumberToWords(int num)
	{
		string[] one_to_nineteen = {"zero", "one",
			"two", "three", "four", "five", "six", "seven",
			"eight", "nine", "ten", "eleven", "twelve",
			"thirteen", "fourteen", "fifteen", "sixteen",
			"seventeen", "eightteen", "nineteen"};

		string[] multiples_of_ten = {"twenty",
			"thirty", "forty", "fifty", "sixty", "seventy",
			"eighty", "ninety"};

		if (num == 0) return "zero";

		//hundreds digit
		int digit;
		string result = "";
		if (num > 99)
		{
			digit = num / 100;
			num = num % 100;
			result = one_to_nineteen[digit] + " hundred";
		}

		if (num == 0) return result.Trim();

		if (num < 20)
		{
			result += " " + one_to_nineteen[num];
		}
		else
		{
			digit = num / 10;
			num = num % 10;
			result += " " + multiples_of_ten[digit - 2];

			if (num > 0)
				result += " " + one_to_nineteen[num];
		}
		return result.Trim();
	}

	public static String CharListPunct()
	{
		return ".,;'\"!_-@#$%^&*()+=\\/|?><`~[]{}: \r\n—";
	}

	public static String CharListInt(bool IncludeDec, bool IncludeNeg)
	{
		string strRet = "01232456789";
		if (IncludeDec)
		{
			strRet += ".";
		}
		if (IncludeNeg)
		{
			strRet += "-";
		}

		return strRet;
	}

	public static String CharListAlpha()
	{
		return "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	}

	public static int Count(String strSource, String strCount)
	{
		return Count(strSource, strCount, false);
	}

	public static int Count(String strSource, String strCount, bool boolIgnoreCase)
	{
		int intCount = 0;
		int intCurrPos = 0;

		if (strSource == string.Empty || strCount == string.Empty) return 0;

		StringComparison compType = boolIgnoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture;

		while (intCurrPos >= 0)
		{
			intCurrPos = strSource.IndexOf(strCount, intCurrPos, compType);

			if (intCurrPos < 0) break;
			else intCount++;

			intCurrPos = intCurrPos + strCount.Length;
		}

		return intCount;
	}

	/// <summary>
	/// ie replace (abcdefg, bc, ef, zz, true) would yield 'azzg'
	/// </summary>
	/// <param name="strSource"></param>
	/// <param name="strAfter"></param>
	/// <param name="strBefore"></param>
	/// <param name="strReplaceWith"></param>
	/// <param name="boolAllNOTSingle"></param>
	/// <returns></returns>
	public static string ReplaceAfterBefore(string strSource, string strAfter, string strBefore, string strReplaceWith, bool boolAllNOTSingle)
	{
		int intIndex1 = strSource.IndexOf(strAfter);
		int intIndex2 = strSource.IndexOf(strBefore, intIndex1 + 3);

		if (intIndex1 > 0 && intIndex2 > intIndex1 + strAfter.Length)
		{
			strSource = strSource.Substring(0, intIndex1) + strReplaceWith + strSource.Substring(intIndex2 + strBefore.Length);
		}
		int intNext = intIndex1 + strReplaceWith.Length;

		while (boolAllNOTSingle && intNext > 0)
		{
			intIndex1 = strSource.IndexOf(strAfter, intNext);
			intIndex2 = strSource.IndexOf(strBefore, intIndex1 + 3);

			if (intIndex1 > 0 && intIndex2 > intIndex1 + strAfter.Length)
			{
				strSource = strSource.Substring(0, intIndex1) + strReplaceWith + strSource.Substring(intIndex2 + strBefore.Length);
			}
			else
			{
				break;
			}
			intNext = intIndex1 + strReplaceWith.Length;
		}

		return strSource;
	}

	public static String DelimFieldFromEnd(String str, char strDelim, int intField)
	{
		string[] arrTemp = str.Split(strDelim);
		String strRet;
		if (arrTemp.Length > 0)
		{
			if (intField >= 1 && intField <= arrTemp.Length)
			{
				strRet = arrTemp[arrTemp.Length - intField];
			}
			else
			{
				strRet = "";
			}
			return strRet;
		}
		else
		{
			return "";
		}
	}

	public static String FillChars(String strToAppendTo, String strChar, int intTotalLength, bool boolFront)
	{
		while (strToAppendTo.Length < intTotalLength)
		{
			if (boolFront)
			{
				strToAppendTo = strChar + strToAppendTo;
			}
			else
			{
				//'append to back
				strToAppendTo = strToAppendTo + strChar;
			}
		}

		return strToAppendTo;
	}

	public static bool IsNumber(String strCheck)
	{
		Decimal decTemp;
		try
		{
			decTemp = Convert.ToDecimal(strCheck);
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static bool IsInt(String strCheck)
	{
		int intTemp;
		try
		{
			intTemp = Convert.ToInt32(strCheck);
			return true;
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	/// Runs 'IsNumber' on each character.  strNum cannot have decimals or negatives
	/// </summary>
	public static bool IsNumberLong(String strNum)
	{
		int i;
		if (strNum == "")
		{
			return true;
		}
		for (i = 0; i < strNum.Length; i++)
		{
			if (!UtilString.IsNumber(strNum.Substring(i, 1)))
			{
				return false;
			}
		}
		return true;
	}

	public static int InStrRev(String strValue, String strFind)
	{
		return strValue.LastIndexOf(strFind) + 1;
	}

	public static int InStr(int intStart, String strValue, String strFind)
	{
		return strValue.IndexOf(strFind, intStart - 1) + 1;
	}

	public static String LikeStrToRegEx(String strLikeStr)
	{
		String strRet;
		strRet = Regex.Escape(strLikeStr); //replaces certain strings with their escape codes

		strRet = strRet.Replace("#", "[0-9]");
		strRet = strRet.Replace("\\#", "[0-9]");
		strRet = strRet.Replace("\\[", "[");
		strRet = "^" + strRet.Replace("\\*", ".*").Replace("\\?", ".") + "$";
		return strRet;
	}

	public static String Left(String str, int intLength)
	{
		if (intLength <= 0)
		{
			return "";
		}

		if (str.Length > intLength)
		{
			return str.Substring(0, intLength);
		}
		else
		{
			return str;
		}
	}

	public static bool LikeStr(String strToCheck, String strLike)
	{
		//regular expressions
		//isalpha ("[^a-zA-Z]");
		//isalphanumeric ("[^a-zA-Z0-9]");
		//isnumber ("[^0-9]");

		strLike = LikeStrToRegEx(strLike);
		Regex objPositivePattern = new Regex(strLike, RegexOptions.IgnoreCase);
		return objPositivePattern.IsMatch(strToCheck);
	}

	public static String Mid(String strValue, int intStart)
	{
		return Mid(strValue, intStart, -1);
	}

	public static String Mid(String strValue, int intStart, int intLen)
	{
		if (intStart < 1)
		{
			intStart = 1;
		}
		if (intStart > strValue.Length)
		{
			return "";
		}
		if (intLen == 0)
		{
			return "";
		}
		else if (intStart - 1 + intLen > strValue.Length)
		{
			return strValue.Substring(intStart - 1);
		}
		else if (intLen > 0)
		{
			return strValue.Substring(intStart - 1, intLen);
		}
		else
		{   //intLen < 0.. return the whole rest of the string
			return strValue.Substring(intStart - 1);
		}
	}

	/// <summary>
	/// strname must look like Jones, Tom or Jones Tom M
	/// </summary>
	/// <param name="strName"></param>
	/// <returns></returns>
	public static String NameGrabLastName(String strName)
	{
		//'strname must look like this:
		//'jones, tom j
		//'or jones tom m

		int intPos;
		String strRet = "";
		strName = strName.Trim();

		intPos = UtilString.InStr(1, strName, ",");
		if (intPos > 1)
		{
			strRet = UtilString.Mid(strName, 1, intPos - 1).Trim();
		}
		else if (intPos == 0)
		{
			intPos = UtilString.InStr(1, strName, " ");

			if (intPos > 1)
			{
				strRet = UtilString.Mid(strName, 1, intPos - 1).Trim();
			}
			else if (intPos == 0)
			{
				strRet = strName.Trim();
			}
			else if (intPos == 1)
			{
				strRet = "";
			}
		}
		else if (intPos == 1)
		{
			strRet = "";
		}
		return strRet;
	}

	/// <summary>
	/// strname must look like Jones, Tom or Jones Tom M (then it will return 'Tom M')
	/// </summary>
	/// <param name="strName"></param>
	/// <returns></returns>
	public static String NameGrabFirstName(String strName)
	{
		//'strname must look
		//'or jones tom m
		String strRet = "";
		int intPos;
		intPos = InStr(1, strName, ",");
		if (intPos == 0)
		{
			intPos = InStr(1, strName, " ");

			if (intPos == 0)
			{
				strRet = "";
			}
			else if (intPos < strName.Length)
			{
				strRet = UtilString.Mid(strName, intPos + 1).Trim();
			}
			else if (intPos == strName.Length)
			{
				strRet = "";
			}
		}
		else if (intPos < strName.Length)
		{
			strRet = UtilString.Mid(strName, intPos + 1).Trim();
		}
		else if (intPos == strName.Length)
		{
			strRet = "";
		}
		return strRet;
	}

	/// <summary>
	/// take spaces out of the name before the comma so last names dont get split up 
	/// ie: MC DONNELL, CAROL   or  DE ANGELO, RIVERA
	/// </summary>
	/// <param name="strName"></param>
	/// <returns></returns>
	public static String NameMergeBeforeComma(String strName)
	{
		//'take spaces out of the name before the comma so last names dont get split up
		//' ie: MC DONNELL, CAROL   or  DE ANGELO, RIVERA

		int intFirst;
		int intFirstSpace;
		String retVal;

		intFirst = UtilString.InStr(1, strName, ",");
		intFirstSpace = UtilString.InStr(1, strName, " ");

		//'dont want to merge anything if the comma is the last character
		if ((strName.Length != intFirst) && (intFirst > 1))
		{
			if (intFirstSpace < intFirst && intFirstSpace < 4)
			{
				strName = UtilString.Replace(UtilString.Mid(strName, 1, intFirst - 1), " ", "") + UtilString.Mid(strName, intFirst);
			}
		}

		retVal = strName;
		return retVal;
	}

	/// <summary>
	/// this function puts bars inbetween the last, first and middle names;
	/// name should be in this format: LAST, FIRST MIDDLE 
	/// or LAST FIRST MIDDLE;
	/// Output looks like this: 'SMITH|JOHN|R.'
	/// </summary>
	/// <param name="strName"></param>
	/// <param name="strDelim"></param>
	/// <returns></returns>
	public static String NameSplit(String strName, String strDelim)
	{
		int firstDelimPos;
		int secondDelimPos;
		int i;
		int length;
		String tempChar;
		String retVal;

		firstDelimPos = -1;
		secondDelimPos = -1;
		length = strName.Length;

		//'this is here because names often come in like ' MC CALLAN, DONNA' and
		//' if we didn't call it, this function would output MC|CALLAN|DONNA
		strName = NameMergeBeforeComma(strName);

		strName = UtilString.Replace(strName, ",", " ");
		strName = ReplaceExtraSpaces(strName);

		for (i = 1; i <= length; i++)
		{
			tempChar = UtilString.Mid(strName, i, 1);
			if (tempChar == " ")
			{
				firstDelimPos = i;
				strName = UtilString.Mid(strName, 1, i - 1) + strDelim + UtilString.Mid(strName, i + 1);
				break;
			}
		}

		//'now reverse from the end
		for (i = length; i >= 1; i--)
		{
			tempChar = UtilString.Mid(strName, i, 1);
			if (tempChar == " ")
			{
				secondDelimPos = i;

				if (i <= firstDelimPos)
				{
					break;
				}

				strName = UtilString.Mid(strName, 1, i - 1) + strDelim + UtilString.Mid(strName, i + 1);

				//'if there is another space following, take it out out
				if (UtilString.Mid(strName, i + 1, 1) == " ")
				{
					strName = UtilString.Mid(strName, 1, i) + UtilString.Mid(strName, i + 2);
				}
				break;
			}
		}

		//'there HAS to be 2 delimiters in this string at the end of this function
		while (Count(strName, strDelim) < 2)
		{
			strName = strName + strDelim;
		}

		retVal = strName;
		return retVal;
	}


	public static String ReplaceExtraSpaces(String str)
	{

		while (str.IndexOf("  ", 0) > -1)
		{
			str = str.Replace("  ", " ");
		}
		return str;
	}

	public static String ReplacePunct(String str, String strReplaceWith)
	{
		String strPunctList = CharListPunct();
		str = Replace(str, strPunctList, strReplaceWith);
		return str;
	}

	public static String ReplaceInt(String str, String strReplaceWith, bool RemoveDec, bool RemoveNeg)
	{
		String strList = CharListInt(RemoveDec, RemoveNeg);
		str = Replace(str, strList, strReplaceWith);
		return str;
	}

	public static String ReplaceNonInt(String str, String strReplaceWith, bool RemoveDec, bool RemoveNeg)
	{
		String strList = CharListInt(!RemoveDec, !RemoveNeg);
		str = Replace(str, strList, strReplaceWith, true);
		return str;
	}

	public static String ReplaceNonAlpha(String str, String strReplaceWith)
	{
		String strList = CharListAlpha();
		str = Replace(str, strList, strReplaceWith, true);
		return str;
	}

	public static String Replace(String str, String strCharList, String strReplaceWith)
	{
		return Replace(str, strCharList, strReplaceWith, false);
	}

	public static String Replace(String str, String strCharList, String strReplaceWith, bool boolInverse)
	{
		//for just a regular replace function (not a char list, but full strings - use str.Replace)

		//strReplaceChars is a list of characters to replace ie "1234567890" .. would emulate a "replaceint" function
		//                     (unless boolInverse was set to true, then it would emulate a "replacenonint" function
		//strReplaceWith : a string to replace each character in the character list with (this can be any length)
		//boolInverse: if set to false, all characters in the charlist will be replaced, otherwise, everything else
		// will be replaced

		int i, c;
		if (str.Length == 0)
		{
			return "";
		}

		c = str.Length;
		for (i = c - 1; i >= 0; i--)
		{
			if (boolInverse)
			{
				if (strCharList.IndexOf(str.Substring(i, 1)) < 0)
				{
					str = str.Remove(i, 1);
					str = str.Insert(i, strReplaceWith);
				}
			}
			else
			{
				if (strCharList.IndexOf(str.Substring(i, 1)) >= 0)
				{
					str = str.Remove(i, 1);
					str = str.Insert(i, strReplaceWith);
				}
			}
		}

		return str;
	}

	public static String Right(String str, int intLength)
	{
		if (intLength <= 0)
		{
			return "";
		}
		if (str.Length > intLength)
		{
			return str.Substring(str.Length - intLength);
		}
		else
		{
			return str;
		}
	}

	public static StringCollection ArrayListToStringColl(ArrayList arr)
	{
		StringCollection arrRet = new StringCollection();
		foreach (string str in arr)
		{
			arrRet.Add(str);
		}
		return arrRet;
	}

	public static List<string> Split(String str, String split)
	{
		//return an array of strings
		List<string> arrRet = new List<string>();
		arrRet.Clear();

		String strTemp;
		int i, intLen;
		int intTemp1;
		intLen = str.Length;
		int intLenSplit;
		intLenSplit = split.Length;

		if (intLen == 0 || intLenSplit == 0)
		{
			return arrRet;
		}

		int intCount = Count(str, split);
		if (intCount == 0)
		{
			arrRet.Add(str);
			return arrRet;
		}

		i = 0;
		while (i <= intLen)
		{
			intTemp1 = str.IndexOf(split, i);
			if (intTemp1 < 0)
			{
				strTemp = str.Substring(i, intLen - i);
				arrRet.Add(strTemp);
				return arrRet;
			}
			else
			{
				strTemp = str.Substring(i, intTemp1 - i);
				arrRet.Add(strTemp);
				i = intTemp1 + intLenSplit;
			}
		}
		return arrRet;
	}

	/// <summary>
	/// 1000 is a near perfect or perfect match, 300 and up is VERY close, above 100 is pretty good, below 50 not very good
	/// </summary>
	/// <param name="strOne"></param>
	/// <param name="strTwo"></param>
	/// <returns></returns>
	public static int StringCompare(String strOne, String strTwo)
	{
		ArrayList arrCompOne = new ArrayList();
		ArrayList arrCompTwo = new ArrayList();
		int countX;
		int countY;
		int countI;
		int countJ;
		int countMatch;
		int lngTotalMatch = 0;
		int countLetter;

		strOne = strOne.ToUpper();
		strTwo = strTwo.ToUpper();
		if (strOne == strTwo)
		{
			return 1000;
		}

		if (strOne.Length == 0 || strTwo.Length == 0)
		{ return 0; }

		for (countLetter = 0; countLetter < strOne.Length; countLetter++)
		{
			arrCompOne.Add(strOne.Substring(countLetter, 1));
		}
		for (countLetter = 0; countLetter < strTwo.Length; countLetter++)
		{
			arrCompTwo.Add(strTwo.Substring(countLetter, 1));
		}

		//'code for fuzzy logic string comparison found at:
		//'http://www.english.upenn.edu/~jlynch/Computing/compare.html
		for (countI = 0; countI < strOne.Length; countI++)
		{
			countX = countI;
			for (countJ = 0; countJ < strTwo.Length; countJ++)
			{
				countY = countJ;
				countMatch = 0;
				while ((countX < strOne.Length) && (countY < strTwo.Length)
					&& (arrCompOne[countX].ToString() == arrCompTwo[countY].ToString()))
				{
					countMatch++;
					lngTotalMatch += countMatch * countMatch;
					countX++;
					countY++;
				}
			}
		}

		lngTotalMatch = (lngTotalMatch * 25) / (strOne.Length * strTwo.Length);

		if (lngTotalMatch > 1000)
		{
			lngTotalMatch = 1000;
		}

		return lngTotalMatch;
	}

	/// <summary>
	/// Reformat any string.  strSource, strFormat must be the same length and strFormat must contain
	/// UNIQUE characters.  example: strformat:12345678 strsource:19001122 stroutformat:56/78/1234
	/// Output: 11/22/1900
	/// </summary>
	/// <param name="strSource"></param>
	/// <param name="strFormat"></param>
	/// <param name="strOutFormat"></param>
	/// <returns></returns>
	public static String StringFormat(String strSource, String strFormat, String strOutFormat)
	{
		String strRet = "";
		int intTemp;
		if (strFormat.Length != strSource.Length)
		{
			throw new Exception("String formatting parameters incorrect");
		}

		for (int i = 0; i < strOutFormat.Length; i++)
		{
			intTemp = strFormat.IndexOf(strOutFormat[i]);
			if (intTemp < 0)
			{
				strRet += strOutFormat.Substring(i, 1);
			}
			else
			{
				strRet += strSource.Substring(intTemp, 1);
			}
		}
		return strRet;
	}

	public static int ToNumberDefault(string strNumCheck, int intDefault)
	{
		int intRet = intDefault;
		if (IsNumber(strNumCheck))
		{
			intRet = Convert.ToInt32(strNumCheck);
		}
		return intRet;
	}

	public static String TrimLeft(String strValue, String strChar)
	{
		//trim beginning characters
		if (strValue.Length != 0)
		{
			while (strValue.Substring(0, 1) == strChar)
			{
				strValue = strValue.Substring(1);
				if (strValue.Length == 0)
				{
					break;
				}
			}
		}
		return strValue;
	}
}

/// <summary>
/// Summary description for UtilDate.
/// </summary>
public static class UtilDate
{

	public static string ToHoursDays(decimal dcHours, decimal dcHoursPerDay)
	{
		string strTemp = "";
		decimal dcDays = dcHours / dcHoursPerDay;

		if (dcDays != 0)
		{
			string strDateLabel = (dcDays == 1 || dcDays == -1) ? "Day" : "Days";

			if (dcDays < 0)
			{
				strTemp += Math.Ceiling(Convert.ToDouble(dcDays)).ToString("n0") + " " + strDateLabel;
			}
			else
			{
				strTemp += Math.Floor(Convert.ToDouble(dcDays)).ToString("n0") + " " + strDateLabel;
			}
		}

		if (strTemp == "0 Days")
			strTemp = string.Empty;

		dcHours = dcHours % dcHoursPerDay;
		if (dcHours != 0)
		{
			if (strTemp != string.Empty)
				strTemp += " ";

			string strHourLabel = (dcHours == 1 || dcHours == -1) ? "Hr" : "Hrs";

			string strTemp2 = dcHours.ToString("n2");
			if (strTemp2.Contains("."))
			{
				if (strTemp2.EndsWith(".00"))
					strTemp2 = strTemp2.Substring(0, strTemp2.Length - 3);
				else if (strTemp2.EndsWith("0"))
					strTemp2 = strTemp2.Substring(0, strTemp2.Length - 1);
			}

			strTemp += strTemp2 + strHourLabel;
		}

		if (dcDays == 0 && dcHours == 0)
		{
			return "0 Days";
		}

		return strTemp;
	}

	/// <summary>
	/// return last sept 1st
	/// </summary>
	public static DateTime LastSeptemberFirst(DateTime dtFrom)
	{
		if (dtFrom.Month >= 9)
		{
			return new DateTime(dtFrom.Year, 9, 1);
		}
		else
		{
			return new DateTime(dtFrom.Year - 1, 9, 1);
		}
	}

	public static bool DateIsBetween(DateTime dtCheck, DateTime dtFrom, DateTime dtTo, bool boolPartialDay = false)
	{
		if (!boolPartialDay)
		{
			dtFrom = new DateTime(dtFrom.Year, dtFrom.Month, dtFrom.Day);
			dtTo = new DateTime(dtTo.Year, dtTo.Month, dtTo.Day).AddDays(1);

			if (dtCheck.CompareTo(dtFrom) >= 0 && dtCheck.CompareTo(dtTo) < 0)
				return true;
		}
		else
		{
			if (dtCheck.CompareTo(dtFrom) > 0 && dtCheck.CompareTo(dtTo) < 0)
				return true;
		}

		return false;
	}


	public static bool DateEqualsDisregardTime(DateTime dt1, DateTime dt2)
	{
		if (dt1.Year == dt2.Year && dt1.Month == dt2.Month && dt1.Day == dt2.Day)
			return true;

		return false;
	}


	public static string GetShortDateTimeStringFromTo(DateTime dtFrom, DateTime dtTo)
	{
		string strRet = string.Empty;

		if (dtFrom.ToShortDateString().Equals(dtTo.ToShortDateString()))
		{
			if (dtFrom.ToShortTimeString().Equals(dtTo.ToShortTimeString()))
			{
				strRet += dtFrom.ToShortDateString();
			}
			else
			{
				strRet += dtFrom.ToShortDateString() + " " + dtFrom.ToShortTimeString() + " - " + dtTo.ToShortTimeString();
			}
		}
		else
		{
			strRet += dtFrom.ToShortDateString() + "-" + dtTo.ToShortDateString();
		}

		return strRet;
	}

	public static string DateTimeShort(DateTime dtDate)
	{
		string strTemp = dtDate.ToShortDateString();

		if (dtDate.Minute != 0 || dtDate.Hour != 0 || dtDate.Second != 0)
		{
			strTemp += " " + dtDate.ToShortTimeString();
		}
		return strTemp;
	}


	public static List<string> WeekdayNames(bool boolAbbreviated)
	{
		string strTemp = boolAbbreviated ? "Sun|Mon|Tues|Wed|Thurs|Fri|Sat" : "Sunday|Monday|Tuesday|Wednesday|Thursday|Friday|Saturday";

		string[] arr = strTemp.Split('|');

		return arr.ToList();
	}

	public static List<string> MonthNames()
	{
		string[] arrMonth = "January|February|March|April|May|June|July|August|September|October|November|December".Split('|');

		return arrMonth.ToList();
	}

	public static string MonthStringFromInt(int intMonth)
	{
		if (intMonth < 1 || intMonth > 12)
			throw new Exception("invalid month specified in monthstringfromint function: " + intMonth);

		return MonthNames()[intMonth - 1];
	}

	public static int MonthFromString(string strMonth)
	{
		switch (strMonth.ToUpper())
		{
			case "JANUARY":
				return 1;

			case "FEBRUARY":
				return 2;

			case "MARCH":
				return 3;

			case "APRIL":
				return 4;

			case "MAY":
				return 5;

			case "JUNE":
				return 6;

			case "JULY":
				return 7;

			case "AUGUST":
				return 8;

			case "SEPTEMBER":
				return 9;

			case "OCTOBER":
				return 10;

			case "NOVEMBER":
				return 11;

			case "DECEMBER":
				return 12;

		}
		return -1;
	}


	public static DateTime EmptyDate
	{
		get
		{
			return new DateTime(1900, 1, 1);
		}
	}

	public static int MonthFrom3Dig(string strMonth)
	{
		switch (strMonth.ToUpper())
		{
			case "JAN":
				return 1;

			case "FEB":
				return 2;

			case "MAR":
				return 3;

			case "APR":
				return 4;

			case "MAY":
				return 5;

			case "JUN":
				return 6;

			case "JUL":
				return 7;

			case "AUG":
				return 8;

			case "SEP":
				return 9;

			case "OCT":
				return 10;

			case "NOV":
				return 11;

			case "DEC":
				return 12;

		}
		return -1;
	}

	public static bool DateIsEmpty(DateTime dtDate)
	{
		if ((dtDate.Day == 1 && dtDate.Month == 1) && (dtDate.Year == 1900 || dtDate.Year == 1901)) return true;
		return false;
	}

	public static bool DateIsEmpty(string strDate)
	{
		DateTime dtDate = DateGetDateTime(strDate);
		return DateIsEmpty(dtDate);
	}

	public static bool DateIsPastDate(String strCurr, String strCheckIfPastOther)
	{
		DateTime dtCurr = DateGetDateTime(strCurr);
		DateTime dtCheckIfPastOther = DateGetDateTime(strCheckIfPastOther);
		return DateIsPastDate(dtCurr, dtCheckIfPastOther);
	}

	public static bool DateIsPastDate(DateTime dtCurr, DateTime dtCheckIfPastOther)
	{
		if (dtCurr.Equals(dtCheckIfPastOther))
		{
			return false;
		}

		TimeSpan dtTemp = dtCurr.Subtract(dtCheckIfPastOther);

		if (Convert.ToInt32(dtTemp.TotalDays) < 0)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	/// <summary>
	/// Get a date time object from a date stringstrDate must look like : 10/20/2006 9:30 PM OR just 9:30 PM (will be 1/1/1900 9:30 pm)
	///  or 10202006 or 102006 or 10/20/2006, etc
	/// </summary>
	/// <param name="strDate"></param>
	/// <returns>a datetime object (1/1/1900 if invalid strDate)</returns>
	public static DateTime DateGetDateTime(String strDate)
	{
		//initial cleanup
		strDate = strDate.Trim();
		while (strDate.IndexOf("  ") >= 0)
		{
			strDate = strDate.Replace("  ", " ");
		}

		if (UtilString.LikeStr(strDate, "## ## ####") || UtilString.LikeStr(strDate, "## ## ##") ||
			UtilString.LikeStr(strDate, "#### ## ##"))
		{
			strDate = strDate.Replace(" ", "/");
		}

		DateTime dtRet;
		string strTime = UtilString.DelimField(strDate, " ", 2);
		bool boolTime = UtilString.LikeStr(strTime, "#:##") || UtilString.LikeStr(strTime, "##:##");
		bool boolPM, boolMilitary = false;
		int intHours = 0, intMinutes = 0;


		string strDateOnly = UtilString.DelimField(strDate, " ", 1);
		strDateOnly = DateGetNewFormat(strDateOnly);
		try
		{
			dtRet = Convert.ToDateTime(strDateOnly);
			if (!boolTime)
			{
				return dtRet;
			}
			else
			{
				//now we have to get the time in here
				boolPM = strDate.ToUpper().EndsWith("PM");
				if (!boolPM)
				{
					boolMilitary = !strDate.ToUpper().EndsWith("AM");
				}

				intHours = Convert.ToInt32(UtilString.DelimField(strTime, ":", 1));
				intMinutes = Convert.ToInt32(UtilString.DelimField(strTime, ":", 2));

				if (boolPM && intHours < 12)
				{
					intHours += 12;
				}
				else if (!boolPM && !boolMilitary && intHours == 12)
				{
					intHours = 0; //12 AM is 00 military
				}

				dtRet = dtRet.AddHours(intHours);
				dtRet = dtRet.AddMinutes(intMinutes);
				return dtRet;
			}
		}
		catch
		{
			return new DateTime(1900, 1, 1);
		}
	}

	/// <summary>
	/// return a string date in the format: mm/dd/ccyy. 6 dig dates assume mmddyy
	/// </summary>
	/// <param name="strDate"></param>
	/// <returns></returns>
	public static String DateGetNewFormat(String strDate)
	{
		string strRet;
		if (UtilString.LikeStr(strDate, "######"))
		{
			strRet = DateGetNewFormat(strDate, "MMDDYY", "MM/DD/CCYY");
		}
		else if (UtilString.LikeStr(strDate, "########"))
		{
			if (strDate.StartsWith("19") || strDate.StartsWith("20"))
			{
				strRet = DateGetNewFormat(strDate, "CCYYMMDD", "MM/DD/CCYY");
			}
			else
			{
				strRet = DateGetNewFormat(strDate, "MMDDCCYY", "MM/DD/CCYY");
			}
		}
		else
		{
			strRet = DateGetNewFormat(strDate, "", "");
		}
		return strRet;
	}

	public static String DateGetNewFormat(String strDate, String strSourceFormat, String strFormatTo)
	{
		int intCentCutoff = DateTime.Today.Year + 3 - 2000;
		return DateGetNewFormat(strDate, strSourceFormat, strFormatTo, intCentCutoff);
	}

	/// <summary>
	/// Reformat any date any way you like.  If strDate is delimited by '/' then sourceformat, strformatto not required (just pass in blank)
	/// </summary>
	/// <param name="strDate">ie "121006"</param>
	/// <param name="strSourceFormat">ie "MMDDYY".  If strsourceformat.length != strDate.length then 1/1/1900 is returned</param>
	/// <param name="strFormatTo">ie "MMDDCCYY".. to force a century u can do: "MMDD20YY" </param>
	/// <returns></returns>
	public static String DateGetNewFormat(String strDate, String strSourceFormat, String strFormatTo,
		int intCenturyCutOffYear)
	{
		strSourceFormat = strSourceFormat.ToUpper();
		strFormatTo = strFormatTo.ToUpper();
		if (strFormatTo == "")
		{
			strFormatTo = "MM/DD/CCYY";
		}
		int intCent, intMonth, intYear, intDay;
		string strRet, strTemp;

		strDate = strDate.Trim();
		strDate = strDate.Replace("-", "/");
		strDate = strDate.Replace(" ", "/");

		if (UtilString.Count(strDate, "/") == 2)
		{
			if (UtilString.LikeStr(strDate, "##/##/####") || UtilString.LikeStr(strDate, "#/##/####") ||
				UtilString.LikeStr(strDate, "##/#/####") || UtilString.LikeStr(strDate, "#/#/####"))
			{
				//we have mm/dd/ccyy
				strTemp = UtilString.DelimField(strDate, "/", 3);
				intCent = Convert.ToInt32(strTemp.Substring(0, 2));
				intYear = Convert.ToInt32(strTemp.Substring(2, 2));
				intMonth = Convert.ToInt32(UtilString.DelimField(strDate, "/", 1));
				intDay = Convert.ToInt32(UtilString.DelimField(strDate, "/", 2));
			}
			else if (UtilString.LikeStr(strDate, "####/##/##") || UtilString.LikeStr(strDate, "####/#/##") ||
				UtilString.LikeStr(strDate, "####/##/#") || UtilString.LikeStr(strDate, "####/#/#"))
			{
				//we have ccyy/mm/dd
				strTemp = UtilString.DelimField(strDate, "/", 1);
				intCent = Convert.ToInt32(strTemp.Substring(0, 2));
				intYear = Convert.ToInt32(strTemp.Substring(2, 2));
				intMonth = Convert.ToInt32(UtilString.DelimField(strDate, "/", 2));
				intDay = Convert.ToInt32(UtilString.DelimField(strDate, "/", 3));
			}
			else if (UtilString.LikeStr(strDate, "##/##/##") || UtilString.LikeStr(strDate, "#/##/##") ||
				UtilString.LikeStr(strDate, "##/#/##") || UtilString.LikeStr(strDate, "#/#/##"))
			{
				//we have mm/dd/yy
				intYear = Convert.ToInt32(UtilString.DelimField(strDate, "/", 3));
				intMonth = Convert.ToInt32(UtilString.DelimField(strDate, "/", 1));
				intDay = Convert.ToInt32(UtilString.DelimField(strDate, "/", 2));

				if (intYear < intCenturyCutOffYear)
				{
					intCent = 20;
				}
				else
				{
					intCent = 19;
				}
			}
			else
			{
				//bad date!
				//BAD DATE
				goto BadDate;
			}
		}
		else if (UtilString.Count(strDate, "/") > 0 || strDate.Length != strSourceFormat.Length)
		{
			//BAD DATE
			goto BadDate;
		}
		else if (!UtilString.LikeStr(strDate, "######") && !UtilString.LikeStr(strDate, "########"))
		{
			goto BadDate;
		}
		else
		{
			strSourceFormat = strSourceFormat.Replace("MM", "ab");
			strSourceFormat = strSourceFormat.Replace("DD", "cd");
			strSourceFormat = strSourceFormat.Replace("YY", "ef");
			strSourceFormat = strSourceFormat.Replace("CC", "gh");

			if (strDate.Length == 8)
			{
				strDate = UtilString.StringFormat(strDate, strSourceFormat, "abcdghef");
				intMonth = Convert.ToInt32(strDate.Substring(0, 2));
				intDay = Convert.ToInt32(strDate.Substring(2, 2));
				intYear = Convert.ToInt32(strDate.Substring(6, 2));
				intCent = Convert.ToInt32(strDate.Substring(4, 2));
			}
			else
			{
				strDate = UtilString.StringFormat(strDate, strSourceFormat, "abcdef");

				intMonth = Convert.ToInt32(strDate.Substring(0, 2));
				intDay = Convert.ToInt32(strDate.Substring(2, 2));
				intYear = Convert.ToInt32(strDate.Substring(4, 2));

				if (intYear < intCenturyCutOffYear)
				{
					intCent = 20;
				}
				else
				{
					intCent = 19;
				}
			}
		}

		strRet = strFormatTo;
		strRet = strRet.Replace("MM", UtilString.FillChars(intMonth.ToString(), "0", 2, true));
		strRet = strRet.Replace("M", intMonth.ToString()); //for single dig months ie M/D/CCYY
		strRet = strRet.Replace("DD", UtilString.FillChars(intDay.ToString(), "0", 2, true));
		strRet = strRet.Replace("D", intDay.ToString()); //for single dig days ie M/D/CCYY
		strRet = strRet.Replace("CC", UtilString.FillChars(intCent.ToString(), "0", 2, true));
		strRet = strRet.Replace("YY", UtilString.FillChars(intYear.ToString(), "0", 2, true));

		return strRet;

	BadDate:
		return "01/01/1900";
	}

	/// <summary>
	/// Return the month of a specified string date
	/// </summary>
	/// <param name="strDate">Date must start with the month, ie 09012000</param>
	/// <returns>an integer representing the month</returns>
	public static int DateMonth(String strDate)
	{
		//date must be in format MM.... and at least 6 chars (no 4 char dates)
		while (strDate.Length < 6)
		{
			strDate = "0" + strDate;
		}

		return Convert.ToInt32(strDate.Substring(0, 2));
	}
	/// <summary>
	/// 
	/// </summary>
	/// <param name="strDate">must be in the format ###### or ##/##/####</param>
	/// <returns>returns a string in this format : MMDDYY</returns>
	public static String DateNextBusinessDayMMDDYY(string strDate)
	{
		//next business day after strDate.. strDate must eitehr be in format
		// ###### or ##/##/####
		// IMPORTANT: returns a string in this format : MMDDYY
		if (UtilString.LikeStr(strDate, "######"))
		{
			strDate = strDate.Substring(0, 2) + "/" + strDate.Substring(2, 2) + "/20" + strDate.Substring(4, 2);
		}
		DateTime dt = DateGetDateTime(strDate);
		if (dt.DayOfWeek.ToString().ToUpper() == "FRIDAY")
		{
			//add 3 days
			dt = dt.AddDays(3);
		}
		else if (dt.DayOfWeek.ToString().ToUpper() == "SATURDAY")
		{
			dt = dt.AddDays(2);
		}
		else
		{
			dt = dt.AddDays(1);
		}
		return DateFormat(dt.ToShortDateString(), "MMDDYY");
	}

	/// <summary>
	/// Return the year contained in the specified date. If the date is 6 digits
	/// and the year is lower than 50, it will assume Century is 2000, else 1900
	/// </summary>
	/// <param name="strDate">Any string date with a 4 digit year on the end</param>
	/// <returns>The year ie 1999 or 2007</returns>
	public static int DateYear(String strDate)
	{
		//year must be at the end of the string but the date can be
		// 6 or 8 ints (after punct strip)
		strDate = UtilString.ReplaceNonInt(strDate, "", true, true);
		//return something like 1999 or 2007
		while (strDate.Length < 6)
		{
			strDate = "0" + strDate;
		}
		int intTemp;
		if (strDate.Length == 8)
		{
			intTemp = Convert.ToInt32(strDate.Substring(4, 4));
		}
		else
		{
			intTemp = Convert.ToInt32(strDate.Substring(4, 2));
		}
		if (intTemp < 50)
		{
			intTemp += 2000;
		}
		else
		{
			intTemp += 1900;
		}
		return intTemp;
	}

	/// <summary>
	/// Get the difference in # of days between 2 dates
	/// </summary>
	/// <returns>difference between 2 dates in days</returns>
	public static int DateDiff(DateTime dtLater, DateTime dtEarlier)
	{
		TimeSpan dtTemp = dtLater.Subtract(dtEarlier);
		int intRet = Convert.ToInt32(dtTemp.TotalDays);
		return intRet;
	}

	/// <summary>
	/// Reformat a string date
	/// </summary>
	/// <param name="strDate">must look like MM/DD/CCYY or M/D/CCYY, etc</param>
	/// <param name="strFormat">ie MMDDCCYY OR YYMMDD OR CCYY/MM/DD, etc</param>
	/// <returns>Reformatted string date in the specified format</returns>
	public static String DateFormat(String strDate, String strFormat)
	{
		//strDate mustbe in format: MM/DD/YYYY
		// OR M/D/YYYY , etc
		//format looks something like this:
		//  MMDDCCYY OR YYMMDD OR CCYYMMDD or something like that
		String strCent = "", strMonth = "", strDay = "", strYear = "";


		strMonth = UtilString.DelimField(strDate, "/", 1);
		strDay = UtilString.DelimField(strDate, "/", 2);
		strYear = UtilString.DelimField(strDate, "/", 3);
		if (strMonth.Length == 1)
		{
			strMonth = "0" + strMonth;
		}
		if (strDay.Length == 1)
		{
			strDay = "0" + strDay;
		}
		if (strYear.Length == 2)
		{
			if (UtilString.LikeStr(strYear, "##"))
			{
				if (Convert.ToInt32(strYear) > 30)
				{
					strYear = "19" + strYear;
				}
				else
				{
					strYear = "20" + strYear;
				}
			}
		}

		strDate = strMonth + "/" + strDay + "/" + strYear;

		if (!UtilString.LikeStr(strDate, "??/??/????"))
		{
			return strDate;
		}


		strMonth = strDate.Substring(0, 2);
		strDay = strDate.Substring(3, 2);
		strCent = strDate.Substring(6, 2);
		strYear = strDate.Substring(8, 2);

		String strRet = strFormat.ToUpper();
		strRet = strRet.Replace("MM", strMonth);
		strRet = strRet.Replace("DD", strDay);
		strRet = strRet.Replace("YYYY", strCent + strYear);
		strRet = strRet.Replace("CC", strCent);
		strRet = strRet.Replace("YY", strYear);
		return strRet;
	}

	/// <summary>
	/// Convert a string julian date to a normal string date
	/// </summary>
	/// <param name="strJulian">5 digit julian date : CCjjj</param>
	/// <param name="strCent">First 2 characters of the returned century ie 20 or 19</param>
	/// <returns>String date in the format: mm/dd/ccyy </returns>		
	public static String DateFromJulian(String strJulian)
	{
		//use current century
		String strCent;
		strCent = DateTime.Today.Year.ToString().Substring(0, 2);
		return DateFromJulian(strJulian, strCent);
	}

	/// <summary>
	/// Convert a string julian date to a normal string date
	/// </summary>
	/// <param name="strJulian">5 digit julian date : CCjjj</param>
	/// <param name="strCent">First 2 characters of the returned century ie 20 or 19</param>
	/// <returns>String date in the format: mm/dd/ccyy </returns>
	public static String DateFromJulian(String strJulian, String strCent)
	{
		//5 digit date.. CCjjj
		// returns string date in the format: mm/dd/ccyy 
		// return blank on error
		DateTime bDate;
		String strTemp, strTemp2;
		int intYear, intDayOfYear;
		if (strJulian.Length != 5 || strCent.Length != 2)
		{
			return "";
		}

		strTemp = strJulian;
		strTemp2 = strTemp.Substring(0, 2);
		strTemp = strTemp.Substring(2, 3);
		intYear = Convert.ToInt32(strCent + strTemp2);
		bDate = new DateTime(intYear, 1, 1);
		intDayOfYear = Convert.ToInt32(strTemp) - 1;
		bDate = bDate.AddDays(intDayOfYear);

		strTemp = bDate.ToShortDateString();
		strTemp = DateFormat(strTemp, "MM/DD/CCYY");
		return strTemp;
	}

}

public delegate void LogFunction(string strMessage);
public delegate void GUITextFunction(string strTextBox, string strText);
public delegate int GUIProgBarFunction(string strAction, int intValue);
public delegate void GUIProgBarFunctionPercent(int intPercent);

public class UtilMisc
{
	public static void SendAppointment(string strToEmail, string strFromEmail, string strFromName, string strSubject, string strBody,
		DateTime dtFrom, DateTime dtTo, string strEmailServer, string strLocation = "", bool boolMeeting = false)
	{
		SmtpClient sc = new SmtpClient(strEmailServer);
		MailMessage msg = new MailMessage();
		msg.From = new MailAddress(strFromEmail, strFromName);
		msg.To.Add(new MailAddress(strToEmail));
		msg.Subject = strSubject;
		msg.Body = strBody;

		StringBuilder str = new StringBuilder();
		str.AppendLine("BEGIN:VCALENDAR");
		str.AppendLine("PRODID:-//VSL Time Leave");
		str.AppendLine("VERSION:2.0");
		str.AppendLine("METHOD:REQUEST");
		str.AppendLine("BEGIN:VEVENT");
		str.AppendLine(string.Format("DTSTART:{0:yyyyMMddTHHmmssZ}", dtFrom));
		str.AppendLine(string.Format("DTSTAMP:{0:yyyyMMddTHHmmssZ}", DateTime.UtcNow));
		str.AppendLine(string.Format("DTEND:{0:yyyyMMddTHHmmssZ}", dtTo));

		if (strLocation != string.Empty)
			str.AppendLine("LOCATION: " + strLocation);

		str.AppendLine(string.Format("UID:{0}", Guid.NewGuid()));
		str.AppendLine(string.Format("DESCRIPTION:{0}", msg.Body));
		str.AppendLine(string.Format("X-ALT-DESC;FMTTYPE=text/html:{0}", msg.Body));
		str.AppendLine(string.Format("SUMMARY:{0}", msg.Subject));

		str.AppendLine(string.Format("ORGANIZER:MAILTO:{0}", msg.From.Address));

		if (boolMeeting)
		{
			str.AppendLine(string.Format("ATTENDEE;CN=\"{0}\";RSVP=TRUE:mailto:{1}", msg.To[0].DisplayName, msg.To[0].Address));

			str.AppendLine("BEGIN:VALARM");
			str.AppendLine("TRIGGER:-PT15M");
			str.AppendLine("ACTION:DISPLAY");
			str.AppendLine("DESCRIPTION:Reminder");
			str.AppendLine("END:VALARM");
		}

		str.AppendLine("END:VEVENT");
		str.AppendLine("END:VCALENDAR");
		System.Net.Mime.ContentType ct = new System.Net.Mime.ContentType("text/calendar");
		ct.Parameters.Add("method", "REQUEST");
		AlternateView avCal = AlternateView.CreateAlternateViewFromString(str.ToString(), ct);
		msg.AlternateViews.Add(avCal);

		sc.Send(msg);
	}



	public static string SafeString(object value)
	{
		if (value != null && value != DBNull.Value)
		{
			return value.ToString();
		}
		return string.Empty;
	}

	public static string SafeString(string strtmp)
	{
		return strtmp == null ? string.Empty : strtmp.ToString();
	}

	public static bool SafeBool(bool? btmp)
	{
		return btmp == null ? false : (bool)btmp;
	}

	public static DateTime SafeDate(DateTime? dtTemp)
	{
		return dtTemp == null ? new DateTime(1900, 1, 1) : (DateTime)dtTemp; ;
	}

	public static string SafeDate(object dtTemp)
	{
		if (dtTemp == DBNull.Value || dtTemp == null)
			return string.Empty;

		return Convert.ToDateTime(dtTemp).ToShortDateString();
	}

	public static Guid SafeGuid(string strGuid)
	{
		if (!string.IsNullOrEmpty(strGuid) && IsGUID(strGuid))
		{
			return new Guid(strGuid);
		}
		return Guid.Empty;
	}

	public static bool IsGUID(string expression)
	{
		if (expression != null)
		{
			Regex guidRegEx = new Regex(@"^(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})$");

			return guidRegEx.IsMatch(expression);
		}
		return false;
	}

	public static bool Run(String strCommand, String strArguments)
	{
		try
		{
			System.Diagnostics.Process.Start(strCommand, strArguments);
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static Object CreateObject(string strObjectName)
	{
		//Get the excel object 
		//ie "Excel.Application"
		Type objType = Type.GetTypeFromProgID(strObjectName);
		//Create instance of excel 
		Object myObject = Activator.CreateInstance(objType);
		return myObject;
	}

	/// <summary>
	/// Case insensitive version of String.IndexOf
	/// </summary>
	public static bool ArrInArr(ArrayList arr, String str)
	{
		foreach (String strAr in arr)
		{
			if (strAr.ToUpper() == str.ToUpper())
			{
				return true;
			}
		}
		return false;
	}

	public static String GetSetting(String strProgram, String strSetting)
	{
		String strRet;
		try
		{
			RegistryKey MyReg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\NETProgs\\" + strProgram);

			strRet = (String)MyReg.GetValue(strSetting, "");

			MyReg.Close();
			return strRet;
		}
		catch
		{
			return "";
		}
	}

	public static bool InArr(ArrayList arr, String strCheck)
	{
		for (int i = 0; i < arr.Count; i++)
		{
			if (arr[i].ToString().ToUpper() == strCheck.ToUpper())
			{
				return true;
			}
		}
		return false;
	}

	public static void SaveSetting(String strProgram, String strSetting, String strValue)
	{
		RegistryKey MyReg;
		try
		{
			MyReg = Registry.CurrentUser.CreateSubKey
				("SOFTWARE\\NETProgs\\" + strProgram);

			MyReg.SetValue(strSetting, strValue);
			MyReg.Close();
		}
		catch
		{

		}
	}

	//below are the windows forms functions
	/*

/// <summary>
	/// 
	/// </summary>
	/// <param name="strStartPath"></param>
	/// <param name="strFilter">example: Microsoft Access Database (*.mdb)|*.mdb</param>
	/// <returns></returns>
	public static string BrowseFile(string strStartPath, string strFilter)
	{
		string strRet = "";
		OpenFileDialog fBrowse = new OpenFileDialog();
		if (UtilFile.IsDirectory(strStartPath))
			fBrowse.InitialDirectory = strStartPath;

		fBrowse.Filter = strFilter;
		fBrowse.ShowDialog();


		strRet = fBrowse.FileName;

		return strRet;
	}


	public static string BrowseFolder(string strStartPath)
	{
		string strRet = "";
		FolderBrowserDialog fBrowse = new FolderBrowserDialog();
		if (UtilFile.IsDirectory(strStartPath))
			fBrowse.SelectedPath = strStartPath;
		fBrowse.ShowDialog();

		strRet = fBrowse.SelectedPath;

		return strRet;
	}


public static void SaveAllGUISettings(System.Windows.Forms.Form frm, Type ctrlType, string strProgName)
	{
		foreach (System.Windows.Forms.Control c in frm.Controls)
		{
			if (c.GetType() == ctrlType)
				SaveSettingAndSubs(ctrlType, c, strProgName);
		}
	}

	private static void SaveSettingAndSubs(Type ctrlType, System.Windows.Forms.Control ctrl, string strProgName)
	{
		SaveSetting(strProgName, ctrl);

		foreach (System.Windows.Forms.Control c in ctrl.Controls)
		{
			if (c.GetType() == ctrlType)
				SaveSettingAndSubs(ctrlType, c, strProgName);
		}
	}

	public static void GetAllGUISettings(System.Windows.Forms.Form frm, Type ctrlType, string strProgName)
	{
		foreach (System.Windows.Forms.Control c in frm.Controls)
		{
			if (c.GetType() == ctrlType)
				GetSettingAndSubs(ctrlType, c, strProgName);
		}
	}

	private static void GetSettingAndSubs(Type ctrlType, System.Windows.Forms.Control ctrl, string strProgName)
	{
		GetSetting(strProgName, ctrl);

		foreach (System.Windows.Forms.Control c in ctrl.Controls)
		{
			if (c.GetType() == ctrlType)
				GetSettingAndSubs(ctrlType, c, strProgName);
		}
	}

	public static void GetSetting(String strProgram, System.Windows.Forms.Control cntrl)
	{
		switch (cntrl.GetType().ToString())
		{
			case "System.Windows.Forms.CheckBox":
				if (GetSetting(strProgram, cntrl.Name) == "1")
				{
					((System.Windows.Forms.CheckBox)cntrl).Checked = true;
				}
				else
				{
					((System.Windows.Forms.CheckBox)cntrl).Checked = false;
				}
				break;
			case "System.Windows.Forms.RadioButton":
				if (GetSetting(strProgram, cntrl.Name) == "1")
				{
					((System.Windows.Forms.RadioButton)cntrl).Checked = true;
				}
				else
				{
					((System.Windows.Forms.RadioButton)cntrl).Checked = false;
				}
				break;
			default:
				cntrl.Text = GetSetting(strProgram, cntrl.Name);
				break;
		}
	}

	/// <summary>
	/// Safely set the value of a progress bar (in percent)
	/// </summary>
	public static void GUIProgBar(System.Windows.Forms.ProgressBar ProgressBar1, int intPercent)
	{
		intPercent = (intPercent < 0 || intPercent > 100) ? 0 : intPercent;

		if(ProgressBar1.Minimum != 0 || ProgressBar1.Maximum != 100)
		{
			ProgressBar1.Minimum = 0;
			ProgressBar1.Maximum = 100;
		}
		ProgressBar1.Value = intPercent;
	}

	/// <summary>
	/// Safely set or get the value of a progress bar (no exceptions thrown)
	/// </summary>
	/// <param name="strAction">"MIN", "MAX" or "VALUE"</param>
	public static int GUIProgBar(System.Windows.Forms.ProgressBar ProgressBar1, String strAction, int intValue, bool boolGet)
	{
		//this function is for easy access to the prog bar.. uses a static variable..
		// now we can interact with the progress bar much easier (like it's not limited by 32000)
		try
		{
			switch(strAction.ToUpper())
			{
				case "MIN" :
					if(boolGet)
					{
						return ProgressBar1.Minimum;
					}
					ProgressBar1.Minimum = intValue;
					break;
				case "MAX" :
					if(boolGet)
					{
						return ProgressBar1.Maximum;
					}
					ProgressBar1.Maximum = intValue;
					break;
				case "VALUE" :
					if(boolGet)
					{
						return ProgressBar1.Value;
					}
					ProgressBar1.Value = intValue;
					break;
				default:
					break;
			}
		}
		catch
		{
			//error... just keep going
		}
		return -1;
	}

	public static void SaveSetting(String strProgram, System.Windows.Forms.Control cntrl)
	{
		switch(cntrl.GetType().ToString())
		{
			case "System.Windows.Forms.CheckBox" :
				if(((System.Windows.Forms.CheckBox)cntrl).Checked)
				{
					SaveSetting(strProgram, cntrl.Name, "1");
				}
				else
				{
					SaveSetting(strProgram, cntrl.Name, "0");
				}
				break;
			case "System.Windows.Forms.RadioButton" :
				if(((System.Windows.Forms.RadioButton)cntrl).Checked)
				{
					SaveSetting(strProgram, cntrl.Name, "1");
				}
				else
				{
					SaveSetting(strProgram, cntrl.Name, "0");
				}
				break;
			default :
				SaveSetting(strProgram, cntrl.Name, cntrl.Text);
				break;
		}
	}
	*/

}

/// <summary>
/// Summary description for UtilDB.
/// </summary>
public class UtilDB
{
	public UtilDB()
	{
		//
		// TODO: Add constructor logic here
		//
	}

	/// <summary>
	/// return an xml representation of database data.  This can be bound to a data grid using the datamember "results".  return an error
	///  or xml success.  max rows limits the # of rows that can be returned.  If result is larger than that, an error will be returned
	/// </summary>
	public static string GetDBXML(OleDbConnection cn, string strQuery, int intMaxRows)
	{
		string strret = string.Empty;

		int intNumRows = UtilDB.NumRowsOfQuery(cn, strQuery);
		if (intMaxRows > 0 && intNumRows > intMaxRows)
		{
			return "The search resulted in too many results.  Please narrow down your search.";
		}
		DataTable dTbl = UtilDB.ExecuteQueryDataTable(cn, strQuery);

		dTbl.TableName = "Results";
		DataSet dSet = new DataSet();
		dSet.Tables.Add(dTbl);
		strret = dSet.GetXml();
		dTbl.Dispose();
		dSet.Dispose();

		return strret;
	}



	/// <summary>
	/// quickly obtain the # of rows a query will return (without running through all rows).. Query must be a select from statment
	/// also note that it doesn't work for nested select statements
	/// </summary>
	/// <param name="strSQL"></param>
	/// <returns></returns>
	public static int NumRowsOfQuery(OleDbConnection cn, string strSQL)
	{
		strSQL = UtilString.DelimField(strSQL.ToUpper(), "FROM", 2);
		strSQL = "SELECT COUNT(*) FROM " + strSQL;

		OleDbCommand cmd = new OleDbCommand(strSQL, cn);
		OleDbDataReader rd = cmd.ExecuteReader();

		rd.Read();
		int intRet = Convert.ToInt32(rd[0]);

		rd.Close();
		cmd.Dispose();
		return intRet;
	}

	/// <summary>
	/// return whether two tables contain related records
	/// </summary>
	public static int RelatedRecordsCount(OleDbConnection cn, string strTable, string strRelatedTable, string strField, string strRelatedField, string strValue, bool boolQuotes, string strWhere)
	{
		strTable = "[" + strTable + "]";
		strRelatedTable = "[" + strRelatedTable + "]";
		strField = "[" + strField + "]";
		strRelatedField = "[" + strRelatedField + "]";
		if (boolQuotes)
		{
			strValue = "'" + strValue + "'";
		}

		strWhere = strWhere.Trim();

		if (strWhere != string.Empty)
		{
			if (!strWhere.ToUpper().StartsWith("AND "))
			{
				strWhere = " and " + strWhere;
			}
			else
			{
				strWhere = " " + strWhere;
			}
		}

		OleDbCommand cmd = new OleDbCommand(string.Format("select count(*) from {0} inner join {1} on {0}.{2}={1}.{3} where {0}.{2}={4}{5};",
											new object[] { strTable, strRelatedTable, strField, strRelatedField, strValue, strWhere }), cn);
		OleDbDataReader rd = cmd.ExecuteReader();

		rd.Read();

		int intRet = Convert.ToInt32(rd[0]);

		rd.Close();
		cmd.Dispose();
		rd.Dispose();
		return intRet;
	}

	/// <summary>
	/// Open a microsoft access database
	/// </summary>
	/// <param name="cn">An uninitialized connection variable</param>
	/// <param name="strDatabase">The file path</param>
	/// <returns></returns>
	public static bool DBOpen(out OleDbConnection cn, String strDatabase)
	{
		try
		{
			cn = new OleDbConnection(GetConnStringAccess(strDatabase));
			cn.Open();
			return true;
		}
		catch
		{
			cn = null;
			return false;
		}
	}

	public static bool DBOpen(out SqlConnection cn, String strConnString)
	{
		try
		{
			cn = new SqlConnection(strConnString);
			cn.Open();
			return true;
		}
		catch
		{
			cn = null;
			return false;
		}
	}

	public static string ExecuteNonQuery(SqlConnection cn, String strSQL)
	{
		String strRet = "";
		SqlCommand cmd = new SqlCommand(strSQL, cn);
		try
		{
			cmd.ExecuteNonQuery();
		}
		catch (Exception ex)
		{
			strRet = ex.Message;
		}
		cmd.Dispose();

		return strRet;
	}

	public static string ExecuteNonQuery(OleDbConnection cn, String strSQL)
	{
		String strRet = "";
		OleDbCommand cmd = new OleDbCommand(strSQL, cn);
		try
		{
			cmd.ExecuteNonQuery();
		}
		catch (Exception ex)
		{
			strRet = ex.Message;
		}
		cmd.Dispose();

		return strRet;
	}

	public static String ValueSafe(DataRow rd, string strField)
	{
		return ValueSafe(rd, strField, "");
	}
	public static String ValueSafe(DataRow rd, string strField, string strDefault)
	{
		if (rd[strField] == System.DBNull.Value)
		{
			return strDefault;
		}
		else
		{
			return rd[strField].ToString();
		}
	}

	public static String ValueSafe(OleDbDataReader rd, string strField)
	{
		return ValueSafe(rd, strField, "");
	}
	public static String ValueSafe(OleDbDataReader rd, string strField, string strDefault)
	{
		if (rd[strField] == System.DBNull.Value)
		{
			return strDefault;
		}
		else
		{
			return rd[strField].ToString();
		}
	}

	public static ArrayList TableNames(OleDbConnection cnDB)
	{
		ArrayList arrRet = new ArrayList();
		DataTable dTbl;
		Object[] arrRestrict = new Object[4] { null, null, null, "TABLE" };
		dTbl = cnDB.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, arrRestrict);

		foreach (DataRow dr in dTbl.Rows)
		{
			arrRet.Add(dr["TABLE_NAME"].ToString());
		}
		return arrRet;
	}

	public static ArrayList ColumnNames(OleDbConnection cnDB, String strTable)
	{
		ArrayList arrRet = new ArrayList();
		DataTable dTbl;
		Object[] arrRestrict = new Object[4] { null, null, strTable, null };
		dTbl = cnDB.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, arrRestrict);

		foreach (DataRow dr in dTbl.Rows)
		{
			arrRet.Add(dr["COLUMN_NAME"].ToString());
		}
		return arrRet;
	}

	public static String GetSQL(String strTable, String strCondition)
	{
		return GetSQL(strTable, strCondition, "", false);
	}
	public static String GetSQL(String strTable, String strCondition, String strOrderByColumn)
	{
		return GetSQL(strTable, strCondition, strOrderByColumn, false);
	}
	public static String GetSQL(String strTable)
	{
		return GetSQL(strTable, "", "", false);
	}
	public static String GetSQL(String strTable, String strCondition, String strOrderByColumn, bool boolDescending)
	{
		String SQL;
		SQL = "SELECT * FROM [" + strTable + "]";

		if (strCondition != "")
		{
			SQL += " WHERE " + strCondition;
		}

		if (strCondition != "")
		{
			if (strOrderByColumn != "")
			{
				SQL += " ORDER BY [" + strTable + "].[" + strOrderByColumn + "]";
				if (boolDescending)
				{
					SQL += " DESC";
				}
			}
		}

		SQL += ";";
		return SQL;
	}

	public static bool TableIsThere(OleDbConnection cnConn, String strTable)
	{
		OleDbCommand cmd = null;
		try
		{
			cmd = new OleDbCommand("SELECT * FROM [" + strTable + "];", cnConn);
			OleDbDataReader rd = cmd.ExecuteReader();
			rd.Close();
			cmd.Dispose();
			return true;
		}
		catch
		{
			if (cmd != null)
			{
				cmd.Dispose();
			}
			return false;
		}
	}

	public static bool FieldIsThere(OleDbDataReader rd, String strCol)
	{
		try
		{
			int intColOrd = rd.GetOrdinal(strCol);
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static bool ColumnIsThere(OleDbConnection cnConn, String strTable, String strColumn)
	{
		String SQL;
		SQL = "";
		SQL = SQL + " SELECT [" + strTable + "].[" + strColumn + "]";
		SQL = SQL + " FROM [" + strTable + "];";

		try
		{
			OleDbCommand cmd = new OleDbCommand(SQL, cnConn);
			OleDbDataReader rd = cmd.ExecuteReader();
			cmd.Dispose();
			rd.Close();
			return true;
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	/// Get a connection string for a trusted SQL server connection
	/// </summary>
	/// <param name="strServer"></param>
	/// <param name="strDatabase"></param>
	/// <returns></returns>
	public static String GetConnStringSQLServer(String strServer, String strDatabase)
	{
		//trusted connection string
		return GetConnStringSQLServer(strServer, strDatabase, "", "");
	}

	public static String GetConnStringSQLServer(String strServer, String strDatabase, String strUser, String strPW)
	{
		string myConnString;
		//use a trusted connection
		if (strUser == "" && strPW == "")
		{
			myConnString = @"Data Source=" + strServer + ";Initial Catalog=" + strDatabase + ";Integrated Security=True";
		}
		else
		{
			myConnString = @"Data Source=" + strServer + ";Initial Catalog=" + strDatabase + ";User Id=" + strUser + ";Password=" + strPW;
		}
		return myConnString;
	}


	public static String GetConnStringAccess(String strDatabase)
	{
		return "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + strDatabase + ";";
	}

	/// <summary>
	/// tells us if the specified value exists in the specified column
	/// anywhere in the given table (case insensitive)
	/// </summary>
	/// <param name="tbl"></param>
	/// <param name="strCol"></param>
	/// <param name="strValue"></param>
	/// <returns></returns>
	public static bool ValueExists(DataTable tbl, String strCol, String strValue)
	{
		ArrayList arrRet = new ArrayList();
		foreach (DataRow dr in tbl.Rows)
		{
			if (System.DBNull.Value != dr[strCol])
			{
				if (strValue.ToUpper() == dr[strCol].ToString().ToUpper())
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Gets a list of values which are in table1  but not in table2, strCol must be a valid column in both data tables
	/// this function is case insensitive
	/// </summary>
	public static ArrayList CompareDataTables(DataTable tbl1, DataTable tbl2, String strCol)
	{
		//precondition: 
		//
		ArrayList arrRet = new ArrayList();
		String strTemp;
		foreach (DataRow dr in tbl1.Rows)
		{
			if (System.DBNull.Value != dr[strCol])
			{
				strTemp = dr[strCol].ToString();
				if (strTemp != "")
				{
					if (!ValueExists(tbl2, strCol, strTemp))
					{
						arrRet.Add(strTemp);
					}
				}
			}
		}
		return arrRet;
	}

	public static OleDbDataReader ExecuteReader(OleDbConnection cn, string SQL)
	{
		OleDbCommand cmd = new OleDbCommand(SQL, cn);
		OleDbDataReader rdRet = cmd.ExecuteReader();
		//cmd.Dispose(); dont use this.. it will mess up later and leave readers open
		return rdRet;
	}

	public static SqlDataReader ExecuteReader(SqlConnection cn, string SQL)
	{
		SqlCommand cmd = new SqlCommand(SQL, cn);
		SqlDataReader rdRet = cmd.ExecuteReader();
		//cmd.Dispose(); dont use this.. it will mess up later and leave readers open
		return rdRet;
	}

	/// <summary>
	/// Use this to execute an insert query, and obtain the latest ID for an autonumber field (SQL server only)
	/// </summary>
	/// <param name="cn"></param>
	/// <param name="strQuery"></param>
	/// <returns></returns>
	public static int ExecuteQueryGetIdent(OleDbConnection cn, String strQuery)
	{
		if (!strQuery.EndsWith(";")) strQuery += ";";

		OleDbCommand cmd = new OleDbCommand(strQuery + " SELECT @@Identity;", cn);
		OleDbDataReader rd = cmd.ExecuteReader();
		rd.Read();
		int intTemp = Convert.ToInt32(rd[0]);
		rd.Close();
		return intTemp;
	}

	/// <summary>
	/// Get the dataTable object resulting from a select query
	/// </summary>
	public static DataTable ExecuteQueryDataTable(OleDbConnection cn, String strQuery)
	{
		DataTable dTblRet = new DataTable();
		OleDbDataAdapter da = new OleDbDataAdapter(strQuery, cn);
		da.Fill(dTblRet);
		da.Dispose();
		return dTblRet;
	}

	/// <summary>
	/// Get the dataTable object resulting from a select query
	/// </summary>
	public static DataTable ExecuteQueryDataTable(System.Data.SqlClient.SqlConnection cn, String strQuery)
	{
		DataTable dTblRet = new DataTable();
		System.Data.SqlClient.SqlDataAdapter da = new System.Data.SqlClient.SqlDataAdapter(strQuery, cn);
		da.Fill(dTblRet);
		da.Dispose();
		return dTblRet;
	}

	public static int MaxID(OleDbConnection cnDB, string strTable, string strColumn)
	{
		int intRet = -1;
		OleDbDataReader rd = UtilDB.ExecuteReader(cnDB, "select max([" + strColumn + "]) from [" + strTable + "];");
		if (rd.HasRows)
		{
			rd.Read();
			if (rd[0] != DBNull.Value)
				intRet = Convert.ToInt32(rd[0]);
		}
		return intRet;
	}
}

/// <summary>
/// Send an email
/// </summary>
public class UtilEMail
{
	public static bool ValidateEmailAddress(string strEmail)
	{
		if (strEmail.Contains("@") && strEmail.Contains(".") && (strEmail.LastIndexOf(".") == strEmail.Length - 4 ||
			strEmail.LastIndexOf(".") == strEmail.Length - 3 || strEmail.LastIndexOf(".") == strEmail.Length - 5) &&
			UtilString.Count(strEmail, "@") == 1 && strEmail.IndexOf('@') < strEmail.LastIndexOf('.') &&
			UtilString.Replace(UtilString.DelimField(strEmail, "@", 2), ",;'\"!#$%^&*()+=\\/|?><`~[]{}\r\n", "") == UtilString.DelimField(strEmail, "@", 2))
		{
			return true;
		}
		return false;
	}

	public static bool SendHTMLEmail(string strServer, string strToEmail, string strFromEmail, string strFromName,
								string strSubject, string strHTMLBody, int intPort, bool boolAuthenticate, string strUser, string strPassword,
		string strCCAddressesBarDelim = "", string strBCCAddressDelim = "")
	{
		strCCAddressesBarDelim = strCCAddressesBarDelim.Replace(",", "|").Replace(";", "|").Replace(":", "|");
		bool boolTemp = false;

		MailMessage mail = new MailMessage();

		strToEmail = strToEmail.Replace(",", "|").Replace(";", "|").Replace(":", "|");

		List<string> arrTo = UtilString.Split(strToEmail, "|");

		foreach (string strto in arrTo)
		{
			if (UtilEMail.ValidateEmailAddress(strto.Trim()))
				mail.To.Add(new MailAddress(strto.Trim()));
		}

		mail.Subject = strSubject;
		mail.Body = strHTMLBody;
		mail.IsBodyHtml = true;

		if (strFromEmail.Contains("\"") && strFromEmail.Contains("<") && strFromEmail.Contains(">") && strFromName == string.Empty)
		{
			//"insider@cta.org" <insider@cta.org> the name is included in the fromemail
			string strTempFromeml = strFromEmail;
			string strTempFromName = strFromEmail;

			strTempFromeml = UtilString.DelimField(strTempFromeml, "<", 2);
			strTempFromeml = UtilString.DelimField(strTempFromeml, ">", 1);
			strTempFromName = UtilString.DelimField(strTempFromName, "\"", 2);
			mail.From = new MailAddress(strTempFromeml, strTempFromName);
		}
		else
		{
			mail.From = new MailAddress(strFromEmail, strFromName);
		}

		if (strCCAddressesBarDelim != string.Empty)
		{
			List<string> arrCC = UtilString.Split(strCCAddressesBarDelim, "|");
			foreach (string strtmp in arrCC)
			{
				if (UtilEMail.ValidateEmailAddress(strtmp.Trim()))
					mail.CC.Add(new MailAddress(strtmp.Trim()));
			}
		}

		if (strBCCAddressDelim != string.Empty)
		{
			strBCCAddressDelim = strBCCAddressDelim.Replace(",", "|").Replace(";", "|").Replace(":", "|");
			List<string> arrBCC = UtilString.Split(strBCCAddressDelim, "|");
			foreach (string strtmp in arrBCC)
			{
				if (UtilEMail.ValidateEmailAddress(strtmp.Trim()))
					mail.Bcc.Add(new MailAddress(strtmp.Trim()));
			}
		}

		SmtpClient sendMail = new SmtpClient(strServer, intPort);
		sendMail.EnableSsl = boolAuthenticate;
		if (boolAuthenticate)
		{
			sendMail.EnableSsl = true;
			sendMail.Credentials = new System.Net.NetworkCredential(strUser, strPassword);
			//sendMail.UseDefaultCredentials = true;
		}

		try
		{
#if !DEBUG
			//sendMail.Send(mail);
#endif
			boolTemp = true;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}

		return boolTemp;
	}
}

/// <summary>
/// Summary description for UtilWebsite.
/// </summary>
public class UtilWebsite
{
	public static string GetWebsiteHTML(string strURL)
	{
		string strRet = "";
		System.Net.WebClient wc = new WebClient();
		System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
		strRet = enc.GetString(wc.DownloadData(strURL));
		return strRet;
	}

	/// <summary>
	/// Another way to do the above (got from the internet - http://www.codeproject.com/csharp/spellcheckparser.asp)
	/// </summary>
	private String GetWebsiteHTMLB(string url)
	{
		string result = "";
		try
		{
			WebRequest objRequest = HttpWebRequest.Create(url);
			WebResponse objResponse = objRequest.GetResponse();
			Stream stream = objResponse.GetResponseStream();
			StreamReader sr = new StreamReader(stream);

			result = sr.ReadToEnd();
		}
		catch (Exception e)
		{
			return e.Message;
		}

		// convert the result to lower case and return it  
		return (result).ToLower();
	}

	public static string CleanURL(string strURL)
	{
		//take the duplicate spaces out
		while (strURL.IndexOf("  ") > 0)
			strURL.Replace("  ", " ");

		strURL = Regex.Replace(strURL, "a href=", "|<a href=", RegexOptions.IgnoreCase);
		strURL = Regex.Replace(strURL, "a href =", "|<a href=", RegexOptions.IgnoreCase);

		strURL = Regex.Replace(strURL, "</a>", "</a>|", RegexOptions.IgnoreCase);
		strURL = Regex.Replace(strURL, "< /a>", "</a>|", RegexOptions.IgnoreCase);
		strURL = Regex.Replace(strURL, "</ a>", "</a>|", RegexOptions.IgnoreCase);
		strURL = Regex.Replace(strURL, "< / a>", "</a>|", RegexOptions.IgnoreCase);
		strURL = Regex.Replace(strURL, "< / a >", "</a>|", RegexOptions.IgnoreCase);
		strURL = Regex.Replace(strURL, "</ a >", "</a>|", RegexOptions.IgnoreCase);
		return strURL;
	}

	/// <summary>
	/// return the URL of the href pointer
	/// </summary>
	public static string GetURLFromHTMLHREF(string strHREF)
	{
		//strHREF must look like this:
		//ie : <a href="a-z/a/abraham.htm" class='bodyText10B'>ABRAHAM</a>
		strHREF = CleanURL(strHREF);
		strHREF = UtilString.DelimField(strHREF, "href=", 2);
		//strHREF = strHREF.Replace("<a href=", "|").Trim();
		if (strHREF.StartsWith("\""))
		{
			if (strHREF.IndexOf("\"", 1) > 0)
				strHREF = strHREF.Substring(1, strHREF.IndexOf("\"", 1) - 1);
			else if (strHREF.IndexOf(" ", 1) > 0)
				strHREF = strHREF.Substring(1, strHREF.IndexOf(" ", 1) - 1);
			else
				strHREF = UtilString.DelimField(strHREF, ">", 1);
		}
		return strHREF;
	}

	/// <summary>
	/// return the Title of the href pointer
	/// </summary>
	public static string GetTitleFromHTMLHREF(string strHREF)
	{
		//strHREF must look like this:
		//ie : <a href="a-z/a/abraham.htm" class='bodyText10B'>ABRAHAM</a>
		strHREF = CleanURL(strHREF);
		strHREF = UtilString.DelimField(strHREF, ">", 2);
		strHREF = UtilString.DelimField(strHREF, "<", 1);
		return strHREF;
	}

	//public static TableRow TableGetRow(int intColumns, string strCssClass)
	//{
	//    TableRow ret = new TableRow();

	//    for (int i = 1; i <= intColumns; i++)
	//    {
	//        TableCell cell = new TableCell();
	//        ret.Cells.Add(cell);
	//    }
	//    ret.CssClass = strCssClass;
	//    return ret;
	//}
}



/// <summary>
/// An in memory representation of a file
/// </summary>

public class clsFile
{
	public List<string> arrLine;
	public String strFile;

	public clsFile(String strfile)
		: this(strfile, false)
	{
		// 
		// TODO: Add constructor logic here
	}

	public clsFile(String strfile, bool boolPopulate)
	{
		// 
		// TODO: Add constructor logic here

		arrLine = new List<string>();
		strFile = strfile;
		if (boolPopulate)
		{
			Populate();
		}
	}

	public bool Populate()
	{
		if (UtilFile.FileIsThere(strFile))
		{
			arrLine = UtilFile.SplitFile(strFile);
			return true;
		}
		else
		{
			return false;
		}
	}

	public long NumLines()
	{
		return arrLine.Count;
	}
}

/// <summary>
/// An in memory representation of a directory
/// </summary>
public class clsDirectory
{
	public String strDirectory;
	public List<clsFile> arrFile;
	public List<clsDirectory> arrDirectory;

	public clsDirectory(String strDir)
	{
		// TODO: Add constructor logic here
		strDirectory = strDir;
		arrFile = new List<clsFile>();
		arrDirectory = new List<clsDirectory>();
	}

	/// <summary>
	/// Populate the internal list of files and directories (but not the contents of the subdirectories)
	/// </summary>
	/// <returns></returns>
	public virtual bool Populate()
	{
		return Populate(false);
	}

	/// <summary>
	/// Populate the internal list of files and directories
	/// </summary>
	/// <param name="boolPopulateSubDirs"></param>
	/// <returns></returns>
	public virtual bool Populate(bool boolPopulateSubDirs)
	{
		Array strFiles;

		if (!UtilFile.DirIsThere(strDirectory))
		{
			return false;
		}

		//System.Windows.Forms.Application.DoEvents();
		strFiles = System.IO.Directory.GetFiles(strDirectory);

		clsDirectory cDir;
		clsFile cFile;

		foreach (String str in strFiles)
		{
			cFile = new clsFile(str);
			arrFile.Add(cFile);
		}

		strFiles = null;
		//System.Windows.Forms.Application.DoEvents();
		strFiles = System.IO.Directory.GetDirectories(strDirectory);

		//System.Windows.Forms.Application.DoEvents();
		foreach (String str in strFiles)
		{
			if (str != ".." && str != ".")
			{
				//we have a directory
				cDir = new clsDirectory(str);
				if (boolPopulateSubDirs)
				{
					//System.Windows.Forms.Application.DoEvents();
					cDir.Populate(true);
				}
				arrDirectory.Add(cDir);
			}
		}

		return true;
	}

	/// <summary>
	/// populate an array of matching files with all files from all directories (including sub directories if desired)
	/// </summary>
	/// <param name="arrF">the array which to add the string file paths to, or the clsFile objects</param>
	/// <param name="strExtension">the extension of the file (with or without the '.' in front)</param>
	/// <param name="boolSubDirs"></param>
	/// <returns></returns>
	public bool GetArrFile(List<clsFile> arrF, String strExtension, bool boolSubDirs)
	{
		if (strExtension.StartsWith("."))
			strExtension = strExtension.Substring(1);

		//'populate an array of matching files with all files from all directories (including sub dirs)
		foreach (clsFile cFile in arrFile)
		{
			if (strExtension == "")
			{
				arrF.Add(cFile);
			}
			else if (UtilString.LikeStr(cFile.strFile.ToUpper(), "*." + strExtension.ToUpper()))
			{
				arrF.Add(cFile);
			}
		}
		if (boolSubDirs)
		{
			foreach (clsDirectory cDir in arrDirectory)
			{
				if (!cDir.GetArrFile(arrF, strExtension, true))
				{
					return false;
				}
			}
		}

		return true;
	}

	/// <summary>
	/// Get the containing directories (clsDirectory objects)
	/// </summary>
	/// <param name="arrD">the list of directories to populate.  must be initiated already</param>
	/// <param name="boolSubD"></param>
	/// <returns></returns>
	public bool GetArrDir(List<clsDirectory> arrD, bool boolSubDirs)
	{
		if (!boolSubDirs)
		{
			foreach (clsDirectory cDir in arrDirectory)
			{
				arrD.Add(cDir);
			}

			return true;
		}

		foreach (clsDirectory cDir in arrDirectory)
		{
			//'add the directory itself, then all it's subdirs
			arrD.Add(cDir);
			cDir.GetArrDir(arrD, true);
		}

		return true;
	}
}