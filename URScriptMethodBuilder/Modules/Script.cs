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

	/// <summary>函數</summary>
	public interface IMethod {

		#region Properties
		/// <summary>取得或設定函數名稱</summary>
		string Name { get; set; }
		/// <summary>取得或設定備註的 Markdown 字串</summary>
		string Comment { get; set; }
		/// <summary>取得或設定回傳用的 Markdown 字串</summary>
		string Return { get; set; }
		/// <summary>取得或設定已被取代的 Markdown 字串</summary>
		string Deprecated { get; set; }
		/// <summary>取得此函數名稱是否合法</summary>
		bool IsValid { get; }
		/// <summary>取得此函數對應的參數集合</summary>
		WpfObservableCollection<IParameter> Parameters { get; }
		#endregion

		#region Methods
		/// <summary>添加一筆新 <see cref="IParameter"/> 至集合中</summary>
		/// <param name="parameter">欲添加的 <see cref="IParameter"/></param>
		void AddParameter(IParameter parameter);
		/// <summary>從集合中移除 <see cref="IParameter"/> </summary>
		/// <param name="parameter">欲移除的 <see cref="IParameter"/></param>
		bool RemoveParameter(IParameter parameter);
		/// <summary>將 <see cref="IParameter"/> 索引位置往上移動一格</summary>
		/// <param name="parameter">欲移動的參數</param>
		void ParameterMoveUp(IParameter parameter);
		/// <summary>將 <see cref="IParameter"/> 索引位置往下移動一格</summary>
		/// <param name="parameter">欲移動的參數</param>
		void ParameterMoveDown(IParameter parameter);
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
		public string Label { get; set; } = string.Empty;
		/// <summary>取得或設定備註的 Markdown 字串</summary>
		public string Comment { get; set; } = string.Empty;
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

	/// <summary>URScript 函數</summary>
	public class UrMethod : IMethod {

		#region Properties
		/// <summary>取得或設定函數名稱</summary>
		[JsonProperty(Order = 0)]
		public string Name { get; set; } = string.Empty;
		/// <summary>取得或設定回傳用的 Markdown 字串</summary>
		[JsonProperty(Order = 1)]
		public string Return { get; set; } = string.Empty;
		/// <summary>取得或設定已被取代的 Markdown 字串</summary>
		[JsonProperty(Order = 2)]
		public string Deprecated { get; set; } = string.Empty;
		/// <summary>取得或設定備註的 Markdown 字串</summary>
		[JsonProperty(Order = 3)]
		public string Comment { get; set; } = string.Empty;
		/// <summary>取得此函數名稱是否合法</summary>
		[JsonIgnore]
		public bool IsValid => !string.IsNullOrEmpty(Name);
		/// <summary>取得此函數對應的參數集合</summary>
		[JsonConverter(typeof(UrParametersConverter))]
		[JsonProperty(Order = 4)]
		public WpfObservableCollection<IParameter> Parameters { get; }
		#endregion

		#region Constructor
		public UrMethod() {
			Parameters = new WpfObservableCollection<IParameter>();
		}
		#endregion

		#region Public Operations
		/// <summary>添加一筆新 <see cref="IParameter"/> 至集合中</summary>
		/// <param name="parameter">欲添加的 <see cref="IParameter"/></param>
		public void AddParameter(IParameter parameter) {
			Parameters.Add(parameter);
		}

		/// <summary>從集合中移除 <see cref="IParameter"/> </summary>
		/// <param name="parameter">欲移除的 <see cref="IParameter"/></param>
		public bool RemoveParameter(IParameter parameter) {
			return Parameters.Remove(parameter);
		}

		/// <summary>將 <see cref="IParameter"/> 索引位置往上移動一格</summary>
		/// <param name="parameter">欲移動的參數</param>
		public void ParameterMoveUp(IParameter parameter) {
			var idx = Parameters.IndexOf(parameter);
			if (idx > 0) {
				if (Parameters.Remove(parameter)) {
					Parameters.Insert(idx - 1, parameter);
				}
			}
		}

		/// <summary>將 <see cref="IParameter"/> 索引位置往下移動一格</summary>
		/// <param name="parameter">欲移動的參數</param>
		public void ParameterMoveDown(IParameter parameter) {
			if (Parameters.Count > 1) {
				var idx = Parameters.IndexOf(parameter);
				if (Parameters.Remove(parameter)) {
					Parameters.Insert(idx + 1, parameter);
				}
			}
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
			var idx = Parameters.IndexOf(src);
			//移除舊的資料
			Parameters.Remove(src);
			//在原位插入資料
			Parameters.Insert(idx, tar);
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
	#endregion
}
