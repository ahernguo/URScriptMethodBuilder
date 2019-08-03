using MahApps.Metro.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace URScriptMethodBuilder {

	/// <summary>URScript 函數簽章編輯視窗</summary>
	public partial class ParameterWindow : MetroWindow, INotifyPropertyChanged {

		#region Properties
		/// <summary>取得欲編輯的參數</summary>
		public IParameter Parameter { get; }
		#endregion

		#region Constructor
		/// <summary>產生空白的 <see cref="UrParameter"/> 以供使用者編輯</summary>
		public ParameterWindow() : this(new UrParameter()) {
		}

		/// <summary>使用既有的 <see cref="IParameter"/> 以供使用者編輯</summary>
		/// <param name="parameter">欲編輯的參數</param>
		public ParameterWindow(IParameter parameter) {
			InitializeComponent();

			this.DataContext = this;
			Parameter = parameter;
		}
		#endregion

		#region INotifyPropertyChanged Implements
		/// <summary>屬性變更事件</summary>
		public event PropertyChangedEventHandler PropertyChanged;
		/// <summary>觸發屬性變更事件</summary>
		/// <param name="name">屬性名稱</param>
		private void RaisePropChg([CallerMemberName]string name = "") {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
		#endregion

		#region UI Events
		private void Save_Clicked(object sender, RoutedEventArgs e) {
			//設定結果並關閉視窗
			this.Invoke(
				() => {
					this.DialogResult = true;
					this.Close();
				}
			);
		}

		private void Exit_Clicked(object sender, RoutedEventArgs e) {
			//設定結果並關閉視窗
			this.Invoke(
				() => {
					this.DialogResult = false;
					this.Close();
				}
			);
		} 
		#endregion
	}
}
