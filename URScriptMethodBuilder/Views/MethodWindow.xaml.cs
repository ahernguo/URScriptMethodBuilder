using MahApps.Metro.Controls;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace URScriptMethodBuilder {

	/// <summary>URScript 函數編輯視窗</summary>
	public partial class MethodWindow : MetroWindow, INotifyPropertyChanged {

		#region Properties
		/// <summary>取得欲編輯的函數</summary>
		public IMethod Method { get; }
		/// <summary>取得或設定當前選取的參數</summary>
		public IParameter SelectedParameter { get; set; }
		#endregion

		#region Constructor
		/// <summary>產生空白的 <see cref="UrMethod"/> 以供使用者編輯</summary>
		public MethodWindow() : this(new UrMethod()) {
		}

		/// <summary>使用既有的 <see cref="IMethod"/> 以供使用者編輯</summary>
		/// <param name="parameter">欲編輯的函數</param>
		public MethodWindow(IMethod method) {
			InitializeComponent();

			this.DataContext = this;
			Method = method;
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

		private void Add_Clicked(object sender, RoutedEventArgs e) {
			try {
				//建立視窗
				var paraWind = new ParameterWindow();
				//如果使用者有點下儲存，進行判斷
				if (paraWind.ShowDialog() ?? false) {
					if (paraWind.Parameter.IsValid) {
						//確認名稱和註解有打
						Method.AddParameter(paraWind.Parameter);
					}
				}
			} catch (Exception ex) {
				MessageBox.Show(
					ex.ToString(),
					"Exception",
					MessageBoxButton.OK,
					MessageBoxImage.Error
				);
			}
		}

		private void Delete_Clicked(object sender, RoutedEventArgs e) {
			try {
				//確保目前有選擇項目
				if (SelectedParameter != null) {
					//詢問是否真的要刪除
					var check = MessageBox.Show(
						$"Sure to delete '{SelectedParameter.Label}' ?",
						"Remove",
						MessageBoxButton.YesNo,
						MessageBoxImage.Question
					);
					//如果要刪除，進行動作~
					if (check == MessageBoxResult.Yes) {
						//嘗試從集合中移除
						var result = Method.RemoveParameter(SelectedParameter);
						//告知結果
						MessageBox.Show(
							$"Remove {(result ? "success" : "failed")}",
							"Remove",
							MessageBoxButton.OK,
							result ? MessageBoxImage.Information : MessageBoxImage.Error
						);
					}
				}
			} catch (Exception ex) {
				MessageBox.Show(
					ex.ToString(),
					"Exception",
					MessageBoxButton.OK,
					MessageBoxImage.Error
				);
			}
		}

		private void Modify_Clicked(object sender, RoutedEventArgs e) {
			try {
				//確保目前有選擇項目
				if (SelectedParameter != null) {
					//先進行複製，避免取消還是更動到原始資料
					var cpPara = SelectedParameter.Clone();
					//建立編輯視窗
					var paraWind = new ParameterWindow(cpPara);
					//如果使用者有點下儲存，進行判斷
					if (paraWind.ShowDialog() ?? false) {
						//檢查名稱是否合法
						if (paraWind.Parameter.IsValid) {
							//取代之
							Method.ReplaceParameter(SelectedParameter, paraWind.Parameter);
						}
					}
				}
			} catch (Exception ex) {
				MessageBox.Show(
					ex.ToString(),
					"Exception",
					MessageBoxButton.OK,
					MessageBoxImage.Error
				);
			}
		}

		private void MoveUp_Clicked(object sender, RoutedEventArgs e) {
			try {
				//確保目前有選擇項目
				if (SelectedParameter != null) {
					//將之往上移動
					Method.ParameterMoveUp(SelectedParameter);
				}
			} catch (Exception ex) {
				MessageBox.Show(
					ex.ToString(),
					"Exception",
					MessageBoxButton.OK,
					MessageBoxImage.Error
				);
			}
		}

		private void MoveDown_Clicked(object sender, RoutedEventArgs e) {
			try {
				//確保目前有選擇項目
				if (SelectedParameter != null) {
					//將之往下移動
					Method.ParameterMoveDown(SelectedParameter);
				}
			} catch (Exception ex) {
				MessageBox.Show(
					ex.ToString(),
					"Exception",
					MessageBoxButton.OK,
					MessageBoxImage.Error
				);
			}
		}
		#endregion
	}
}
