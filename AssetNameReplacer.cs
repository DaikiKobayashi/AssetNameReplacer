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

		// �����L�[���[�h
		private string searchWord;
		// �����Ώۃt�H���_�[
		UnityEngine.Object folderFilter;

		// �������ʊi�[�ϐ�
		List<string> searchAssetPassList = new List<string>();

		// �t�@�C���ϊ��㖼
		List<string> assetReplaceNameList = new List<string>();


		private float LineHeight { get { return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; } }

		
		private void OnGUI()
		{
			EditorGUI.BeginChangeCheck();

			var style = GUI.skin.box;

			// �c�[���o�[
			using (new GUILayout.HorizontalScope())
			{
				using (new GUILayout.VerticalScope(style, GUILayout.Height(LineHeight * 5), GUILayout.Width(260)))
				{
					// �������[�h
					using (new GUILayout.HorizontalScope())
					{
						EditorGUILayout.LabelField("�������[�h", GUILayout.Width(60));
						searchWord = EditorGUILayout.TextField("", searchWord, GUILayout.Width(200));
						GUILayout.ExpandWidth(true);
					}

					// �����t�H���_�[
					using (new GUILayout.HorizontalScope())
					{
						EditorGUILayout.LabelField("�t�H���_", GUILayout.Width(60));
						folderFilter = EditorGUILayout.ObjectField(folderFilter, typeof(UnityEngine.Object), false, GUILayout.Width(200));
						GUILayout.ExpandWidth(true);
					}

					// �ύX�C�x���g
					if (EditorGUI.EndChangeCheck())
					{

					}

					EditorGUILayout.LabelField("");
					// ����
					if (GUILayout.Button("����", GUILayout.Width(260),GUILayout.Height(LineHeight * 2)))
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

			// �擾���\��
			EditorGUILayout.LabelField($"�擾�I�u�W�F�N�g��{searchAssetPassList.Count()}");
			EditorGUILayout.Space();

			GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(3));

			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
			{
				// �A�Z�b�g���l�[������
				if (searchAssetPassList.Any())
				{
					// �v�f�\���֐�
					void DrowElement(string path,int index)
					{
						// �A�Z�b�g���擾
						var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);

						// �A�C�R�����擾
						Texture icon = AssetDatabase.GetCachedIcon(path);

						using (new GUILayout.HorizontalScope())
                        {
							// �A�C�R����\��
							if(icon != null)
                            {
								GUILayout.Box(icon, GUILayout.Height(50), GUILayout.Width(50));
                            }

							using (new GUILayout.VerticalScope())
                            {
								// �I�u�W�F�N�g�̖��O��\��
                                if (GUILayout.Button(asset.name, GUI.skin.box))
                                {
									Selection.objects = new[] { asset };
                                }

								using(new GUILayout.HorizontalScope())
                                {
									// ���l�[���t�B�[���h��\��
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
		/// �ݒ�����ƂɃA�Z�b�g������
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
		/// �ݒ�����ƂɃt�B���^�[��������擾
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
		/// �ݒ�̃t�B���[�^�[���L����?
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