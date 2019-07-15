using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* * developed by tetr4lab. @ http://tetr4lab.nyanta.jp/
 * */
namespace SQLiteUnity {

	/// <summary>パーサのデリゲート</summary>
	public delegate T Parser<T> (string str);

	/// <summary>SQLiteユーティリティ</summary>
	public static partial class SQLiteUtility {

		/// <summary>TableCell → 文字列 (なければ空文字列)</summary>
		public static string GetColumn (this SQLiteRow row, string name) {
			if (!string.IsNullOrEmpty (name) && row != null && row [name] != null) {
				var val = row [name] as string;
				if (!string.IsNullOrEmpty (val)) {
					return string.Copy (val);
				}
			}
			return string.Empty;
		}

		/// <summary>TableCell → インスタンスまたはEnum (なければ指定値)</summary>
		public static T GetColumn<T> (this SQLiteRow row, string name, T defaultValue) {
			if (!string.IsNullOrEmpty (name) && row != null && row [name] != null) {
				if (typeof (T).IsEnum) {
					var val = row [name] as string;
					if (!string.IsNullOrEmpty (val)) {
						return (T) System.Enum.Parse (typeof (T), val);
					}
				} else {
					var cell = row [name];
					return cell.IsScalar () ? (T) cell : (T) Activator.CreateInstance (typeof (T), cell as string);
				}
			}
			return defaultValue;
		}

		/// <summary>TableCell → Parse → 値 (なければ指定値)</summary>
		public static T GetColumn<T> (this SQLiteRow row, Parser<T> parser, string name, T defaultValue = default) {
			if (!string.IsNullOrEmpty (name) && row != null && row [name] != null && parser != null) {
				var val = row [name] as string;
				if (!string.IsNullOrEmpty (val)) {
					return parser (val);
				}
			}
			parser = null;
			return defaultValue;
		}

		/// <summary>拡張バインド (トランザクション用)</summary>
		public static string SQLiteBind (this string query, SQLiteRow param) {
			foreach (string key in param.Keys) {
				object val = param [key];
				string name = (key [0] == ':' || key [0] == '@' || key [0] == '$') ? key : $":{key}";
				string str;
				if (val == null) {
					str = "NULL";
				} else if (val.IsScalar ()) {
					str = val.ToString ();
				} else if (val.Equals (val.GetType ().GetDefaultValue ())) {
					str = "NULL";
				} else if (val is DateTime) {
					str = $"'{((DateTime)val).ToString ("yyyy-MM-dd HH:mm:ss")}'";
				} else {
					str = $"'{val.ToString ().Replace ("'", "''")}'";
				}
				query = query.Replace (name, str);
			}
			return query;
		}

		/// <summary>スカラー型か判定</summary>
		public static bool IsScalar<T> (this T val) {
			return (val is int || val is uint || val is short || val is ushort || val is long || val is ulong || val is byte || val is sbyte || val is float || val is double);
		}

	}

}
