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

		#region Methods
		/// <summary>取得此 <see cref="IParameter"/> 的複製品</summary>
		/// <returns>複製品</returns>
		IParameter Clone();
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
		/// <summary>取得此 <see cref="IMethod"/> 的複製品</summary>
		/// <returns>複製品</returns>
		IMethod Clone();
		/// <summary>將 <see cref="IParameter"/> 取代為指定的資料</summary>
		/// <param name="src">欲被取代的 <see cref="IParameter"/></param>
		/// <param name="tar">欲取代的新 <see cref="IParameter"/></param>
		void ReplaceParameter(IParameter src, IParameter tar);
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

		#region Public Operations
		/// <summary>取得此 <see cref="IParameter"/> 的複製品</summary>
		/// <returns>複製品</returns>
		public IParameter Clone() {
			return new UrParameter() {
				Label = this.Label,
				Comment = this.Comment
			};
		}
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
		[JsonConverter(typeof(UrParametersConverter))]
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
		[JsonConverter(typeof(UrSignatureConverter))]
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

		/// <summary>取得此 <see cref="IMethod"/> 的複製品</summary>
		/// <returns>複製品</returns>
		public IMethod Clone() {
			//採用 JSON 序列化
			var jsonStr = JsonConvert.SerializeObject(this);
			//反序列化並回傳之
			return JsonConvert.DeserializeObject<UrMethod>(jsonStr);
		}

		/// <summary>將 <see cref="IParameter"/> 取代為指定的資料</summary>
		/// <param name="src">欲被取代的 <see cref="IParameter"/></param>
		/// <param name="tar">欲取代的新 <see cref="IParameter"/></param>
		public void ReplaceParameter(IParameter src, IParameter tar) {
			//先記錄舊的位置
			var idx = Signature.Parameters.IndexOf(src);
			//移除舊的資料
			Signature.Parameters.Remove(src);
			//在原位插入資料
			Signature.Parameters.Insert(idx, tar);
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
	/// <summary>提供 WpfObservableCollection&lt;<see cref="IParameter"/>&gt; 的 JSON 轉換功能</summary>
	/// <remarks>先反序列化成 IList&lt;UrParameter&gt; 再塞入目標集合！ 避免型別失敗~ </remarks>
	public class UrParametersConverter : JsonConverter<WpfObservableCollection<IParameter>> {

		public override WpfObservableCollection<IParameter> ReadJson(JsonReader reader, Type objectType, WpfObservableCollection<IParameter> existingValue, bool hasExistingValue, JsonSerializer serializer) {
			//檢查是否已有 WpfObservableCollection<IParameter>，沒有則 new 一個。  理論上都會有才對...
			var result = hasExistingValue ? existingValue : new WpfObservableCollection<IParameter>();
			//因不能反序列化成 IParameter，故直接反成 UrParameter
			var obj = serializer.Deserialize<IList<UrParameter>>(reader);
			//如果反成功，加入集合。 因若直接 WpfObservableCollection<UrParameter> 塞入 WpfObservableCollection<IParameter> 會跳 IParameter 無法等同 UrParameter 的型別錯誤!!
			if (obj != null) {
				foreach (var item in obj) {
					result.Add(item);
				}
			}
			return result;
		}

		public override void WriteJson(JsonWriter writer, WpfObservableCollection<IParameter> value, JsonSerializer serializer) {
			serializer.Serialize(writer, value);
		}
	}

	/// <summary>提供適用於 <see cref="UrSignature"/> 的 <see cref="ISignature"/> JSON 轉換功能</summary>
	/// <remarks>先反序列化成 UrSignature 再回傳 ISignature! 避免型別失敗~ </remarks>
	public class UrSignatureConverter : JsonConverter<ISignature> {

		public override ISignature ReadJson(JsonReader reader, Type objectType, ISignature existingValue, bool hasExistingValue, JsonSerializer serializer) {
			//檢查是否已有 ISignature，沒有則 new 一個。  理論上都會有才對...
			var result = hasExistingValue ? existingValue : new UrSignature();
			//因不能反序列化成 ISignature，故直接反成 UrSignature
			var obj = serializer.Deserialize<UrSignature>(reader);
			//如果反成功，寫入資料。 因 IMethod 的 Signature 是唯讀，為了不想再開放 set 來破壞其權限，故這邊直接手動更新之!!
			if (obj != null) {
				result.Deprecated = obj.Deprecated;
				result.Label = obj.Label;
				result.Return = obj.Return;
				foreach (var item in obj.Parameters) {
					result.Parameters.Add(item);
				}
			}
			return result;
		}

		public override void WriteJson(JsonWriter writer, ISignature value, JsonSerializer serializer) {
			serializer.Serialize(writer, value);
		}
	}
	#endregion
}
