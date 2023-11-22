#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace KrisDevelopment
{
    public class OrganizedShaderGUI : ShaderGUI
    {
        private class ByRef<T>
        {
            public T value;

            public ByRef(T @default)
            {
                value = @default;
            }
        }

        private static string[] commonPassNames = new[] {
            "ALWAYS",
            "FORWARD",
            "FORWARD_DELTA",
            "DEFERRED",
            "SHADOWCASTER",
            "META",
        };



        private static string _groupConvention = @"\[Group (\S+)\].*"; //match [Group name]
        
        /// <summary> <see cref="_groupConvention"/> </summary>
        private static string _trimGroupConvention = "[Group {0}]"; // what to trim from the property name, should match the regex

        private GUIStyle _separatorStyle;
        private string _search = string.Empty;

		private Dictionary<string, bool> _foldout = new();


        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            /*
             * Init
             * Draw "open shader" option (integrate with Amplify?)
             * Collect groups from property names that follow the convention
             * Collect groups from property names that dont follow the convention
             * Draw some extra info
             */

            // Init
            if(_separatorStyle == null)
            {
                _separatorStyle = new GUIStyle("box");
                _separatorStyle.fontSize = 18;
                _separatorStyle.fontStyle = FontStyle.Bold;
				_separatorStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
				_separatorStyle.alignment = TextAnchor.MiddleCenter;
                _separatorStyle.stretchWidth = true;
            }

            // Tools
            var _materialTarget = materialEditor.target as Material;
            if (_materialTarget && materialEditor.targets.Length == 1)
            {
                if (GUILayout.Button("Open Shader"))
                {
                    AssetDatabase.OpenAsset(_materialTarget.shader);
                }

				GUILayout.BeginHorizontal(EditorStyles.toolbar);
				GUILayout.Label("Search", EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
				GUILayout.Space(6);
				_search = EditorGUILayout.TextField(_search, EditorStyles.textField);
				GUILayout.EndHorizontal();

				// draw shader warnings (since when there are errors no GUI is drawn at all)
				if (ShaderUtil.ShaderHasWarnings(_materialTarget.shader))
				{
					EditorGUILayout.HelpBox("Shader generates warnings.", MessageType.Warning);
				}

                GUILayout.Space(5);
            }

            // Group

            var _ungroupedProperties = new List<MaterialProperty>(properties);
            var _groupedProperties = new Dictionary<string, List<MaterialProperty>>();

            Action<string, MaterialProperty> _addToGroup = (s, m) =>
            {
                if (!_groupedProperties.ContainsKey(s))
                {
                    _groupedProperties[s] = new List<MaterialProperty>();
                }
                _groupedProperties[s].Add(m);
            };

            foreach (var _prop in _ungroupedProperties)
            {
				if(_prop.flags == MaterialProperty.PropFlags.HideInInspector)
				{
					continue;
				}

                Match match = Regex.Match(_prop.displayName, _groupConvention);
                if (match.Success)
                {
                    // group by convention
                    string _groupContent = match.Groups[1].Value;
                    _addToGroup(_groupContent, _prop);
                }
                else
                {
                    // group by type
                    _addToGroup($"{_prop.type}s", _prop);
                }
            }

            // Draw the GUI

            // get the current keywords from the material
            foreach (var _propKV in _groupedProperties)
			{
				bool foldout = GetKey(_propKV);
				GUILayout.BeginHorizontal();
				{
					if (string.IsNullOrEmpty(_search))
					{
						foldout = GUILayout.Toggle(foldout, (foldout ? "v" : ">"), _separatorStyle, GUILayout.Width(32));
					}

					foldout = GUILayout.Toggle(foldout, _propKV.Key, _separatorStyle);
				}
				GUILayout.EndHorizontal();

				_foldout[_propKV.Key] = foldout;

				if (GetKey(_propKV) || !string.IsNullOrEmpty(_search))
				{
					EditorGUI.indentLevel++;
					DrawGroup(materialEditor, _propKV.Key, _propKV.Value);
					EditorGUI.indentLevel--;
				}
				
				GUILayout.Space(5);
			}

			GUILayout.Label("Settings", _separatorStyle);
			materialEditor.EnableInstancingField();
			materialEditor.RenderQueueField();

			if (materialEditor.targets.Length == 1)
			{
				if (_materialTarget == null || !materialEditor.isVisible)
				{
					return;
				}

				// inform the user about disabled passes.
				DrawPasses(_materialTarget);
			}
		}

		private bool GetKey(KeyValuePair<string, List<MaterialProperty>> propKV)
		{
			return _foldout.ContainsKey(propKV.Key) ? _foldout[propKV.Key] : true;
		}

		protected virtual void DrawGroup(MaterialEditor materialEditor, string group, List<MaterialProperty> properties)
		{
			foreach (var prop in properties)
			{
				bool _disabled = false;
				switch (prop.flags)
				{
					case MaterialProperty.PropFlags.Gamma:
					case MaterialProperty.PropFlags.HDR:
					case MaterialProperty.PropFlags.Normal:
					case MaterialProperty.PropFlags.NoScaleOffset:
					case MaterialProperty.PropFlags.None:
						break;
					case MaterialProperty.PropFlags.HideInInspector:
						continue;
					case MaterialProperty.PropFlags.PerRendererData:
					case MaterialProperty.PropFlags.NonModifiableTextureData:
						_disabled = true;
						break;
				}

				var _name = prop.displayName.Replace(string.Format(_trimGroupConvention, group), string.Empty);
				var _searchable = $"{_name} {prop.name}";

				// match a search
				if (!string.IsNullOrEmpty(_search) && !_search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Any(a => _searchable.IndexOf(a, StringComparison.OrdinalIgnoreCase) >= 0))
					continue;

				EditorGUI.BeginDisabledGroup(_disabled);
				DrawProperty(materialEditor, prop, _name);
				EditorGUI.EndDisabledGroup();
			}
		}

		protected virtual void DrawProperty(MaterialEditor materialEditor, MaterialProperty prop, string name)
		{
			materialEditor.ShaderProperty(prop, new GUIContent(name));
		}

		private void DrawPasses(Material material)
        {
			var allPassesDisabled = new ByRef<bool>(true);
            var passes = new Dictionary<string, (bool enabled, int amount)>();

            for (int i = 0; i < material.passCount; i++)
            {
                AddPass(passes, material, material.GetPassName(i), allPassesDisabled);
            }

            foreach (var pass in commonPassNames)
            {
                AddPass(passes, material, pass, allPassesDisabled);
            }

            GUILayout.BeginVertical("Pass Info", "Window");
            {
                foreach (var pass in passes)
                {
                    var _clr = GUI.color;
                    GUI.color = pass.Value.enabled ? Color.white : Color.grey;
                    {
                        GUILayout.Label($"PASS {(string.IsNullOrEmpty(pass.Key) ? "-" : pass.Key)} (x{pass.Value.amount}): {pass.Value.enabled}");
                    }
                    GUI.color = _clr;
                }
            }
            GUILayout.EndVertical();

            if (allPassesDisabled.value)
            {
                EditorGUILayout.HelpBox("ALL PASSES DISABLED!", MessageType.Error);
            }
        }

        private void AddPass(Dictionary<string, (bool enabled, int amount)> passes, Material mat, string passName, ByRef<bool> allPassesDisabled)
        {
            var pass = passName.ToUpper();

            if (mat.FindPass(pass) < 0 && pass != "ALWAYS")
            {
                return;
            }

            bool enabled = mat.GetShaderPassEnabled(pass);

            if (enabled)
            {
                allPassesDisabled.value = false;
            }

            if (!passes.ContainsKey(pass))
            {
                passes.Add(pass, (enabled, 1));
            }
            else if (!commonPassNames.Contains(pass))
            {
                passes[pass] = (enabled, passes[pass].amount + 1);
            }
        }
    }
}
#endif