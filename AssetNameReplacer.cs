using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace AssetNameReplace
{
	public class AssetNameReplacer : EditorWindow
	{
		[MenuItem("Tools/Asset Name Replace", false, 110)]
		static void OpenWindow()
		{
			var window = GetWindow<AssetNameReplacer>("AssetNameReplacer");
			window.Search();
		}

		private void OnEnable()
		{
			Search();
		}

		public class FilterData
		{
			public string name;
			public bool isEnable;
		}

		List<FilterData> filterList = new List<FilterData>() {
			new FilterData (){ name = "AnimatorController", isEnable = false },
			new FilterData (){ name = "AnimationClip", isEnable = false },
			new FilterData (){ name = "AudioClip", isEnable = false },
			new FilterData (){ name = "AudioMixer", isEnable = false },
			new FilterData (){ name = "Font", isEnable = false },
			new FilterData (){ name = "GUISkin", isEnable = false },
			new FilterData (){ name = "Material", isEnable = false },
			new FilterData (){ name = "Mesh", isEnable = false },
			new FilterData (){ name = "Model", isEnable = false },
			new FilterData (){ name = "PhysicMaterial", isEnable = false },
			new FilterData (){ name = "Prefab", isEnable = false },
			new FilterData (){ name = "Scene", isEnable = false },
			new FilterData (){ name = "Script", isEnable = false },
			new FilterData (){ name = "ScriptableObject", isEnable = false },
			new FilterData (){ name = "Shader", isEnable = false },
			new FilterData (){ name = "Sprite", isEnable = false },
			new FilterData (){ name = "Texture", isEnable = false },
			new FilterData (){ name = "TimelineAsset", isEnable = false },
			new FilterData (){ name = "VideoClip", isEnable = false },
			new FilterData (){ name = "Texture2D",isEnable = false},
		};


		private Vector2 scrollPos = Vector2.zero;

		// 検索キーワード
		private string searchWord;
		// 検索対象フォルダー
		UnityEngine.Object folderFilter;

		// 検索結果格納変数
		List<string> searchAssetPassList = new List<string>();

		// ファイル変換後名
		List<string> assetReplaceNameList = new List<string>();


		private float LineHeight { get { return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; } }

		
		private void OnGUI()
		{
			EditorGUI.BeginChangeCheck();

			var style = GUI.skin.box;

			// ツールバー
			using (new GUILayout.HorizontalScope())
			{
				using (new GUILayout.VerticalScope(style, GUILayout.Height(LineHeight * 5), GUILayout.Width(260)))
				{
					// 検索ワード
					using (new GUILayout.HorizontalScope())
					{
						EditorGUILayout.LabelField("検索ワード", GUILayout.Width(60));
						searchWord = EditorGUILayout.TextField("", searchWord, GUILayout.Width(200));
						GUILayout.ExpandWidth(true);
					}

					// 検索フォルダー
					using (new GUILayout.HorizontalScope())
					{
						EditorGUILayout.LabelField("フォルダ", GUILayout.Width(60));
						folderFilter = EditorGUILayout.ObjectField(folderFilter, typeof(UnityEngine.Object), false, GUILayout.Width(200));
						GUILayout.ExpandWidth(true);
					}

					// 変更イベント
					if (EditorGUI.EndChangeCheck())
					{

					}

					EditorGUILayout.LabelField("");
					// 検索
					if (GUILayout.Button("検索", GUILayout.Width(260),GUILayout.Height(LineHeight * 2)))
					{
						Search();
					}
				}

				using (new GUILayout.VerticalScope(style, GUILayout.Width(400)))
				{
					for(int i = 0;i < filterList.Count;)
                    {
						using(new GUILayout.HorizontalScope())
                        {
							for(int j = 0;j < 4; j++, i++)
                            {
								if (filterList.Count <= i) break;

								var filter = filterList[i];
								filter.isEnable = EditorGUILayout.ToggleLeft(filter.name, filter.isEnable, GUILayout.Width(120));
                            }
                        }
                    }
				}
			}

			// 取得数表示
			EditorGUILayout.LabelField($"取得オブジェクト数{searchAssetPassList.Count()}");
			EditorGUILayout.Space();

			GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(3));

			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
			{
				// アセットリネーム処理
				if (searchAssetPassList.Any())
				{
					// 要素表示関数
					void DrowElement(string path,int index)
					{
						// アセットを取得
						var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);

						// アイコンを取得
						Texture icon = AssetDatabase.GetCachedIcon(path);

						using (new GUILayout.HorizontalScope())
                        {
							// アイコンを表示
							if(icon != null)
                            {
								GUILayout.Box(icon, GUILayout.Height(50), GUILayout.Width(50));
                            }

							using (new GUILayout.VerticalScope())
                            {
								// オブジェクトの名前を表示
                                if (GUILayout.Button(asset.name, GUI.skin.box))
                                {
									Selection.objects = new[] { asset };
                                }

								using(new GUILayout.HorizontalScope())
                                {
									// リネームフィールドを表示
									assetReplaceNameList[index] = EditorGUILayout.TextField(assetReplaceNameList[index]);
                                    if (GUILayout.Button("Replace", GUILayout.Width(100)))
                                    {
										if (!string.IsNullOrEmpty(assetReplaceNameList[index]))
										{
											AssetDatabase.RenameAsset(path, assetReplaceNameList[index]);
											Search();
										}
									}
                                }
                            }
                        }
                    }

					using (new GUILayout.VerticalScope())
					{
						for (int i = 0; i < searchAssetPassList.Count; i++)
						{
							DrowElement(searchAssetPassList[i], i);
							GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
						}
					}
				}
			}
			EditorGUILayout.EndScrollView();
		}

		/// <summary>
		/// 設定をもとにアセットを検索
		/// </summary>
		public void Search()
		{
			searchAssetPassList.Clear();
			assetReplaceNameList.Clear();

			if (string.IsNullOrEmpty(searchWord) || !CheckFilterAllEnable()) return;

			searchAssetPassList = AssetDatabase.FindAssets(GetFilter())
				.Select(x => AssetDatabase.GUIDToAssetPath(x))
				.Where(x =>
				{
					if (folderFilter == null)
					{
						return true;
					}
					var folderPath = AssetDatabase.GetAssetPath(folderFilter);
					return x.Contains(folderPath);
				})
				.Where(x => x.Match(searchWord, false))
				.ToList();

			assetReplaceNameList = new List<string>(searchAssetPassList.Count);
			for (int i = 0; i < assetReplaceNameList.Capacity; i++)
			{
				assetReplaceNameList.Add("");
			}
		}

		/// <summary>
		/// 設定をもとにフィルター文字列を取得
		/// </summary>
		private string GetFilter()
        {
			string ret = "";
			foreach(var filter in filterList)
            {
                if (filter.isEnable)
                {
					ret += "t:" + filter.name + " ";
                }
            }
			ret += searchWord;
			return ret;
        }

		/// <summary>
		/// 設定のフィルーターが有効か?
		/// </summary>
		private bool CheckFilterAllEnable()
        {
			bool enable = false;
			foreach(var filter in filterList)
            {
				enable |= filter.isEnable;
            }
			return enable;
        }
	}

	public static class StringExtension
	{
		public static bool Match(this string path, string matchWord, bool perfectMatch, bool directoryMatch = false)
		{
			var fileName = System.IO.Path.GetFileNameWithoutExtension(path);
			if (perfectMatch)
			{
				if (directoryMatch)
				{
					return fileName == matchWord || path.StartsWith(matchWord + "/", StringComparison.Ordinal) || path.Contains("/" + matchWord + "/");
				}
				else
				{
					return fileName == matchWord;
				}
			}
			else
			{
				if (directoryMatch)
				{
					return path.Contains(matchWord);
				}
				else
				{
					return fileName.Contains(matchWord);
				}
			}
		}
	}
}