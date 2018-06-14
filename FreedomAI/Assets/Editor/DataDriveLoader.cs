using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Excel;
using System.Data;
using System.Reflection;
using DataDrive;
/*
 * you should create a excel in the appcation path
 * you can create a Gameobject for global var
 * you can write all var in the excel
 * if you want to add a datastruct transferring, you can add method in DataDriveTransferer
 * JT
*/
namespace DataDrive
{

	/*
	 * the Method set for Transferring data
	*/
	public class DataDriveTransferer
	{
		
		public static object string2int(string s)
		{
			int i = int.Parse (s);
			return (object)i;
		}

		public static object string2float(string s)
		{
			float f = float.Parse (s);
			return (object)f;
		}

		public static object string2string(string s)
		{
			return (object)s;
		}

		public static object string2char(string s)
		{
			char c = char.Parse (s);
			return (object)c;
		}

		public static object string2double(string s)
		{
			double d = double.Parse (s);
			return (object)d;
		}

		public static object string2bool(string s)
		{
			bool b = bool.Parse (s);
			return (object)b;
		}
	}
	/*
	 * load Excel for data driving
	*/
	public class DataDriveLoader : EditorWindow 
	{
		// tagName or GameObjectName
		static string[] KeyName;
		// Find GameObject by name or tag?
		static string[] MatchMode;
		// class name
		static string[] ClassName;
		// the property name of this class
		static string[] PropertyName;
		// data type
		static string[] DataType;
		// data value
		static string[] DataValue;
		// dafault path
		static string ExcelName = "GameData.xls";

		public enum STATECODE
		{
			STATE_SUCCEED = 0,
			STATE_PATH_ERROR = -1,
			STATE_FORMAT_ERROR = -2,
			STATE_NOGAMEOBJECT_ERROR = -3,
			STATE_NOCLASS_ERROR = -4,
			STATE_NOPROPERTY_ERROR = -5,
			STATE_VALUETRANSFER_ERROR = -6
		};

		[MenuItem("DataDrive/Load")]
		static void Load()
		{
			STATECODE statecode = 0;
			statecode = LoadExcel (ExcelName);
			if (statecode != STATECODE.STATE_SUCCEED) 
			{
				LogError (statecode);
				return;
			}
			for(int i=0;i<KeyName.Length;i++)
			{
				GameObject[] tempResult=null;
				statecode = FindGameObject (ref tempResult,KeyName[i],MatchMode[i]);
				if (statecode != STATECODE.STATE_SUCCEED) 
				{
					LogError (statecode);
					return;
				}
				for (int j = 0; j < tempResult.Length; j++) 
				{
					MonoBehaviour[] monothis = tempResult [j].GetComponents<MonoBehaviour> ();
					for(int k=0;k<monothis.Length;k++)
					{
						if (monothis [k].GetType ().ToString () == ClassName [i])
						{
							FieldInfo fInfo = monothis [k].GetType ().GetField (PropertyName[i]);
							if (fInfo == null)
							{
								LogError (STATECODE.STATE_NOPROPERTY_ERROR);
								return;
							}
							object tempTargetResult = null;
							statecode = DataTransfer (DataType[i],DataValue[i],ref tempTargetResult);
							if (statecode != STATECODE.STATE_SUCCEED)
							{
								LogError (statecode);
								return;
							}
							fInfo.SetValue (monothis[k],tempTargetResult);
							break;
						}
						if (k == monothis.Length - 1)
						{
							LogError (STATECODE.STATE_NOCLASS_ERROR);
							return;
						}
					}
				}
			}
		}

		static STATECODE DataTransfer(string type,string data,ref object result)
		{
			Type _Type = typeof(DataDriveTransferer);
			MethodInfo MI = _Type.GetMethod (type);
			object[] para = new object[1];
			para [0] = data;
			result = (object)MI.Invoke (null,para);
			if (result == null)
				return STATECODE.STATE_VALUETRANSFER_ERROR;
			else
				return STATECODE.STATE_SUCCEED;
		}

		static STATECODE FindGameObject(ref GameObject[] result,string key,string type)
		{
			if (type == "name") 
			{
				GameObject temp = GameObject.Find (key);
				if (temp!=null) 
				{
					result = new GameObject[1];
					result [0] = temp;
					return STATECODE.STATE_SUCCEED;
				}
				return STATECODE.STATE_NOGAMEOBJECT_ERROR;
			}
			else
			{
				result = GameObject.FindGameObjectsWithTag (key);
				if (result.Length != 0)
					return STATECODE.STATE_SUCCEED;
				return STATECODE.STATE_NOGAMEOBJECT_ERROR;
			}
		}

		static STATECODE LoadExcel(string s)
		{
			FileStream stream = File.Open(Application.dataPath + "/"+s, FileMode.Open, FileAccess.Read);
			if (stream == null)
				return STATECODE.STATE_PATH_ERROR;
			IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

			DataSet result = excelReader.AsDataSet();

			int columns = result.Tables[0].Columns.Count;
			int rows = result.Tables[0].Rows.Count;
		//	Debug.Log (columns);
			if (columns < 6)
				return STATECODE.STATE_FORMAT_ERROR;
			InitArray (rows);
			for(int i = 0;  i< rows; i++)
			{
				KeyName [i] = result.Tables [0].Rows [i] [0].ToString();
				MatchMode [i] = result.Tables [0].Rows [i] [1].ToString ();
				ClassName [i] = result.Tables [0].Rows [i] [2].ToString ();
				PropertyName [i] = result.Tables [0].Rows [i] [3].ToString ();
				DataType [i] = result.Tables [0].Rows [i] [4].ToString ();
				DataValue [i] = result.Tables [0].Rows [i] [5].ToString ();
			}
			return STATECODE.STATE_SUCCEED;
		}

		static void InitArray(int length)
		{
			KeyName = new string[length];
			MatchMode = new string[length];
			ClassName = new string[length];
			PropertyName = new string[length];
			DataType = new string[length];
			DataValue = new string[length];
		}

		static void LogError(STATECODE _state)
		{
			switch (_state) 
			{
			case STATECODE.STATE_PATH_ERROR:
				Debug.LogError ("excel is not exist");
				break;
			case STATECODE.STATE_FORMAT_ERROR:
				Debug.LogError ("Data format is not standard");
				break;
			case STATECODE.STATE_NOCLASS_ERROR:
				Debug.LogError ("this class is not exist");
				break;
			case STATECODE.STATE_NOGAMEOBJECT_ERROR:
				Debug.LogError ("GameObject can not be found");
				break;
			case STATECODE.STATE_NOPROPERTY_ERROR:
				Debug.LogError ("this class have not this property");
				break;
			case STATECODE.STATE_VALUETRANSFER_ERROR:
				Debug.LogError ("Data can not be Transferred");
				break;
			}
		}
	}
}