using MahApps.Metro.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace URScriptMethodBuilder {
	/// <summary>URScript 函數建構器</summary>
	public partial class MainWindow : MetroWindow, INotifyPropertyChanged {

		#region Properties
		/// <summary>取得 URScript 函數集合</summary>
		public WpfObservableCollection<IMethod> Methods { get; }
		/// <summary>取得或設定當前 <see cref="ListBox"/> 所選取的項目</summary>
		public IMethod SelectedMethod { get; set; }
		/// <summary>取得當前是否有函數於 <see cref="Methods"/> 中</summary>
		public bool HasItem => Methods.Count > 0;
		#endregion

		#region Constructor
		/// <summary>初始化視窗</summary>
		public MainWindow() {
			InitializeComponent();

			this.DataContext = this;
			Methods = new WpfObservableCollection<IMethod>();
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
		private void Load_Clicked(object sender, RoutedEventArgs e) {
			try {
				//建立 Dialog
				var diag = new OpenFileDialog() {
					Filter = "URScipt Methods | *.json"
				};
				//如果使用者有成功選擇檔案則載入之
				if (diag.ShowDialog() ?? false) {
					//先把檔案內的資料讀出來
					var jsonStr = File.ReadAllText(diag.FileName);
					//JSON 反序列化
					var mthds = JsonConvert.DeserializeObject<IList<UrMethod>>(jsonStr);
					//如果有成功反出東西，則更新集合
					if (mthds != null) {
						Methods.Clear();
						foreach (var mthd in mthds) {
							Methods.Add(mthd);
						}
						//告知結果
						MessageBox.Show(
							$"File loaded",
							"Load",
							MessageBoxButton.OK,
							MessageBoxImage.Information
						);
					} else {
						//告知結果
						MessageBox.Show(
							$"Load failed when deserializing",
							"Load",
							MessageBoxButton.OK,
							MessageBoxImage.Information
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
			} finally {
				RaisePropChg("HasItem");
			}
		}

		private void Save_Clicked(object sender, RoutedEventArgs e) {
			try {
				//建立 Dialog
				var diag = new SaveFileDialog() {
					Filter = "URScipt Methods | *.json"
				};
				//如果使用者有成功選擇檔案則儲存之
				if (diag.ShowDialog() ?? false) {
					//序列化成 JSON 字串
					var jsonStr = JsonConvert.SerializeObject(Methods, Formatting.Indented);
					//寫入檔案
					File.WriteAllText(diag.FileName, jsonStr);
					//告知結果
					MessageBox.Show(
						$"File saved",
						"Save",
						MessageBoxButton.OK,
						MessageBoxImage.Information
					);
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

		private void Add_Clicked(object sender, RoutedEventArgs e) {
			try {
				//建立視窗
				var mthdWind = new MethodWindow();
				//如果使用者有點下儲存，進行判斷
				if (mthdWind.ShowDialog() ?? false) {
					//確認名稱有打
					if (mthdWind.Method.IsValid) {
						//加入集合
						Methods.Add(mthdWind.Method);
					}
				}
			} catch (Exception ex) {
				MessageBox.Show(
					ex.ToString(),
					"Exception",
					MessageBoxButton.OK,
					MessageBoxImage.Error
				);
			} finally {
				RaisePropChg("HasItem");
			}
		}

		private void Delete_Clicked(object sender, RoutedEventArgs e) {
			try {
				//確保目前有選擇項目
				if (SelectedMethod != null) {
					//詢問是否真的要刪除
					var check = MessageBox.Show(
						$"Sure to delete '{SelectedMethod.Name}' ?",
						"Remove",
						MessageBoxButton.YesNo,
						MessageBoxImage.Question
					);
					//如果要刪除，進行動作~
					if (check == MessageBoxResult.Yes) {
						//嘗試從集合中移除
						var result = Methods.Remove(SelectedMethod);
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
			} finally {
				RaisePropChg("HasItem");
			}
		}

		private void Modify_Clicked(object sender, RoutedEventArgs e) {
			try {
				//確保目前有選擇項目
				if (SelectedMethod != null) {
					//先進行複製，避免取消還是更動到原始資料
					var cpMthd = SelectedMethod.Clone();
					//建立編輯視窗
					var mthdWind = new MethodWindow(cpMthd);
					//如果使用者有點下儲存，進行判斷
					if (mthdWind.ShowDialog() ?? false) {
						//檢查名稱是否合法
						if (mthdWind.Method.IsValid) {
							//先記錄索引位置，等等要放回來
							var idx = Methods.IndexOf(SelectedMethod);
							//移除舊的
							Methods.Remove(SelectedMethod);
							//插入原先位置
							Methods.Insert(idx, mthdWind.Method);
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
			} finally {
				RaisePropChg("HasItem");
			}
		}
		#endregion
	}
}
