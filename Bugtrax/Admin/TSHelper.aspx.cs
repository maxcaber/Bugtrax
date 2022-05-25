using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApp.Admin
{
	public partial class TSHelper : System.Web.UI.Page
	{
		protected void bRun_Click(object sender, EventArgs e)
		{
			buildTypes(tAssemblyName.Text, tEntityNamespace.Text, "entities.ts");
			buildTypes(tAssemblyName.Text, tDTONamespace.Text, "dtos.ts");
			buildObjLiteral(tAssemblyName.Text, tEntityNamespace.Text, "obj-literals.js");
			buildObjLiteral(tAssemblyName.Text, tDTONamespace.Text, "dto-obj-literals.js");
			lMessage.Text = "Finished at " + DateTime.Now;
		}

		private void buildObjLiteral(string assemblyName, string ns, string outFileName)
		{
			try
			{
				StringBuilder sbImports = new StringBuilder();
				StringWriter swImports = new StringWriter(sbImports);
				StringBuilder sbClass = new StringBuilder();
				StringWriter swClass = new StringWriter(sbClass);

				Assembly a = Assembly.Load(assemblyName); //Assembly.GetAssembly(typeof(InvoiceLib.Entity.ActiveDirectory));

				Type[] typelist = GetTypesInNamespace(a, ns);
				for (int i = 0; i < typelist.Length; i++)
				{
					Type type = typelist[i];

					if (type.FullName.ToLower().Contains("override") || type.FullName.ToLower().Contains("<>"))
						continue;

					swClass.WriteLine("export const new" + ToUpperFirstLetter(type.Name.ToLower()) + " = {");

					foreach (PropertyInfo pi in type.GetProperties())
					{
						string tsPropLine = "'" + pi.Name + "'";
						string tsType = "";
						string val = "";
						Type piType = pi.PropertyType;

						if (IsNumericType(piType))
						{
							tsType = "number";
							val = "0";
						}
						else if (GetTypeName(piType).ToLower() == "boolean")
						{
							tsType = "boolean";
							val = "false";
						}
						else if (GetTypeName(piType).ToLower() == "string")
						{
							tsType = "string";
							val = "''";
						}
						else if (GetTypeName(piType).ToLower() == "datetime")
						{
							tsType = "Date";
							val = "new Date()";
						}
						else if (GetTypeName(piType).ToLower() == "guid")
						{
							tsType = "string";
							val = "'00000000-0000-0000-0000-000000000000'";

						}
						else if (piType.Namespace == "System.Collections.Generic")
						{
							if (piType.Name == "List`1" || piType.Name == "ICollection`1")
							{
								tsType = piType.GenericTypeArguments[0].Name + " []";
								val = "[]";
							}
						}
						else if (piType.IsClass)
						{
							tsType = piType.Name;
							val = "null";
						}
						tsPropLine += " : " + val + ", ";
						swClass.WriteLine(tsPropLine);
					}
					swClass.WriteLine("}");
					swClass.WriteLine();
				}

				string strImports = sbImports.ToString();
				swImports.Close();
				string strClasses = sbClass.ToString();
				swClass.Close();



				string p = Server.MapPath(tOutPath.Text) + "/" + outFileName;
				File.WriteAllText(p, strImports + strClasses);
			}
			catch (Exception e)
			{
				SQLLogger.LogError(e);
			}
		}


		private void buildTypes(string assemblyName, string ns, string outFileName)
		{
			try
			{
				StringBuilder sbImports = new StringBuilder();
				StringWriter swImports = new StringWriter(sbImports);
				StringBuilder sbClass = new StringBuilder();
				StringWriter swClass = new StringWriter(sbClass);

				Assembly a = Assembly.Load(assemblyName); //Assembly.GetAssembly(typeof(InvoiceLib.Entity.ActiveDirectory));

				Type[] typelist = GetTypesInNamespace(a, ns);
				for (int i = 0; i < typelist.Length; i++)
				{
					Type type = typelist[i];

					if (type.FullName.ToLower().Contains("override") || type.FullName.ToLower().Contains("<>"))
						continue;

					swClass.WriteLine("export interface " + type.Name + " {");

					foreach (PropertyInfo pi in type.GetProperties())
					{
						string tsPropLine = pi.Name;
						string tsType = "";
						Type piType = pi.PropertyType;

						if (IsNumericType(piType))
							tsType = "number";
						else if (GetTypeName(piType).ToLower() == "boolean")
							tsType = "boolean";
						else if (GetTypeName(piType).ToLower() == "string")
							tsType = "string";
						else if (GetTypeName(piType).ToLower() == "datetime")
						{
							tsType = "Date";
						}
						else if (GetTypeName(piType).ToLower() == "guid")
							tsType = "string";
						else if (piType.Namespace == "System.Collections.Generic")
						{
							if (piType.Name == "List`1" || piType.Name == "ICollection`1")
							{
								tsType = piType.GenericTypeArguments[0].Name + " []";
								swImports.WriteLine("import { " + piType.GenericTypeArguments[0].Name + " } from './entities';");
							}
						}
						else if (piType.IsClass)
						{
							tsType = piType.Name;
							swImports.WriteLine("import { " + tsType + " } from './entities';");
						}
						tsPropLine += ": " + tsType + ";";
						swClass.WriteLine(tsPropLine);
					}
					swClass.WriteLine("}");
					swClass.WriteLine();
				}

				string strImports = sbImports.ToString();
				swImports.Close();
				string strClasses = sbClass.ToString();
				swClass.Close();

				strImports = String.IsNullOrEmpty(strImports)?"": strImports + "\n";


				string p = Server.MapPath(tOutPath.Text) + "/" + outFileName;
				File.WriteAllText(p, strImports + strClasses);
			}
			catch (Exception e)
			{
				SQLLogger.LogError(e);
			}
		}

		public static string GetTypeName(Type type)
		{
			var nullableType = Nullable.GetUnderlyingType(type);

			bool isNullableType = nullableType != null;

			if (isNullableType)
				return nullableType.Name;
			else
				return type.Name;
		}

		bool IsNumericType(Type type)
		{
			if (type == null)
			{
				return false;
			}

			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Byte:
				case TypeCode.Decimal:
				case TypeCode.Double:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.SByte:
				case TypeCode.Single:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					return true;
				case TypeCode.Object:
					if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
					{
						return IsNumericType(Nullable.GetUnderlyingType(type));
					}
					return false;
			}
			return false;
		}

		private Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
		{
			return assembly.GetTypes().Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)).ToArray();
		}

		private string ToLowerFirstLetter(string source)
		{
			if (string.IsNullOrEmpty(source))
				return string.Empty;
			// convert to char array of the string
			char[] letters = source.ToCharArray();
			// upper case the first char
			letters[0] = char.ToLower(letters[0]);
			// return the array made of the new char array
			return new string(letters);
		}

		private string ToUpperFirstLetter(string source)
		{
			if (string.IsNullOrEmpty(source))
				return string.Empty;
			// convert to char array of the string
			char[] letters = source.ToCharArray();
			// upper case the first char
			letters[0] = char.ToUpper(letters[0]);
			// return the array made of the new char array
			return new string(letters);
		}
	}
}