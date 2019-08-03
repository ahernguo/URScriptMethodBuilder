using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace URScriptMethodBuilder {

	#region Interfaces
	/// <summary>函數簽章的參數</summary>
	public interface IParameter {

		#region Properties
		/// <summary>取得或設定名稱標籤</summary>
		string Label { get; set; }
		/// <summary>取得或設定備註的 Markdown 字串</summary>
		string Comment { get; set; }
		/// <summary>取得此參數是否合法</summary>
		bool IsValid { get; }
		#endregion

	}

	/// <summary>函數簽章</summary>
	public interface ISignature {

		#region Properties
		/// <summary>取得或設定名稱標籤</summary>
		string Label { get; set; }
		/// <summary>取得或設定回傳用的 Markdown 字串</summary>
		string Return { get; set; }
		/// <summary>取得或設定已被取代的 Markdown 字串</summary>
		string Deprecated { get; set; }
		/// <summary>取得此函數對應的參數集合</summary>
		WpfObservableCollection<IParameter> Parameters { get; }
		#endregion

	}

	/// <summary>函數</summary>
	public interface IMethod {

		#region Properties
		/// <summary>取得或設定函數名稱</summary>
		string Name { get; set; }
		/// <summary>取得或設定備註的 Markdown 字串</summary>
		string Comment { get; set; }
		/// <summary>取得此函數名稱是否合法</summary>
		bool IsValid { get; }
		/// <summary>取得或設定函數簽章</summary>
		ISignature Signature { get; }
		#endregion

		#region Methods
		/// <summary>添加一筆新 <see cref="IParameter"/> 至集合中</summary>
		/// <param name="parameter">欲添加的 <see cref="IParameter"/></param>
		void AddParameter(IParameter parameter);
		/// <summary>從集合中移除 <see cref="IParameter"/> </summary>
		/// <param name="parameter">欲移除的 <see cref="IParameter"/></param>
		bool RemoveParameter(IParameter parameter);
		/// <summary>更新函數簽章</summary>
		void UpdateSignature();
		#endregion

	}
	#endregion

	#region Implements
	/// <summary>URScript 函數簽章的參數</summary>
	public class UrParameter : IParameter {

		#region Properties
		/// <summary>取得或設定名稱標籤</summary>
		public string Label { get; set; }
		/// <summary>取得或設定備註的 Markdown 字串</summary>
		public string Comment { get; set; }
		/// <summary>取得此參數是否合法</summary>
		[JsonIgnore]
		public bool IsValid => !string.IsNullOrEmpty(Label) && !string.IsNullOrEmpty(Comment);
		#endregion

	}

	/// <summary>URScript 函數簽章</summary>
	public class UrSignature : ISignature {

		#region Properties
		/// <summary>取得或設定名稱標籤</summary>
		public string Label { get; set; }
		/// <summary>取得或設定回傳用的 Markdown 字串</summary>
		public string Return { get; set; }
		/// <summary>取得或設定已被取代的 Markdown 字串</summary>
		public string Deprecated { get; set; }
		/// <summary>取得此函數對應的參數集合</summary>
		[JsonConverter(typeof(UrConverter<WpfObservableCollection<UrParameter>>))]
		public WpfObservableCollection<IParameter> Parameters { get; }
		#endregion

		#region Constructor
		public UrSignature() {
			Parameters = new WpfObservableCollection<IParameter>();
		}
		#endregion
	}

	/// <summary>URScript 函數</summary>
	public class UrMethod : IMethod {

		#region Properties
		/// <summary>取得或設定函數名稱</summary>
		public string Name { get; set; }
		/// <summary>取得或設定備註的 Markdown 字串</summary>
		public string Comment { get; set; }
		/// <summary>取得此函數名稱是否合法</summary>
		[JsonIgnore]
		public bool IsValid => !string.IsNullOrEmpty(Name);
		/// <summary>取得或設定函數簽章</summary>
		[JsonConverter(typeof(UrConverter<UrSignature>))]
		public ISignature Signature { get; }
		#endregion

		#region Constructor
		public UrMethod() {
			Signature = new UrSignature();
		}
		#endregion

		#region Public Operations
		/// <summary>添加一筆新 <see cref="IParameter"/> 至集合中</summary>
		/// <param name="parameter">欲添加的 <see cref="IParameter"/></param>
		public void AddParameter(IParameter parameter) {
			Signature.Parameters.Add(parameter);
		}

		/// <summary>從集合中移除 <see cref="IParameter"/> </summary>
		/// <param name="parameter">欲移除的 <see cref="IParameter"/></param>
		public bool RemoveParameter(IParameter parameter) {
			return Signature.Parameters.Remove(parameter);
		}

		/// <summary>更新函數簽章</summary>
		public void UpdateSignature() {
			var sb = new StringBuilder();

			//Sub or Function
			if (string.IsNullOrEmpty(Signature.Return)) {
				sb.Append("(Sub) ");
			} else {
				sb.Append("(Function) ");
			}
			//Name
			sb.AppendFormat("{0}(", Name);
			//Parameters
			if (Signature.Parameters.Count > 0) {
				var paraList = Signature.Parameters.Select(p => p.Label);
				var paraStr = string.Join(", ", paraList);
				sb.Append(paraStr);
			}
			//End
			sb.Append(")");

			//Assign
			Signature.Label = sb.ToString();
		}
		#endregion

		#region Overides
		/// <summary>取得此函數的名稱字串</summary>
		/// <returns>名稱</returns>
		public override string ToString() {
			return Name;
		}
		#endregion

	}
	#endregion

	#region JSON Converter
	/// <summary>提供 URScript 各項類別之轉換器</summary>
	/// <typeparam name="T">IMethod, ISignature, IParameter</typeparam>
	public class UrConverter<T> : JsonConverter {

		#region Overrides
		public override bool CanConvert(Type objectType) {
			return true;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			return serializer.Deserialize<T>(reader);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			serializer.Serialize(writer, value);
		}
		#endregion

	}
	#endregion
}
