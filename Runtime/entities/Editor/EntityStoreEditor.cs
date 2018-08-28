using System;
using BeatThat.Pools;
using UnityEditor;
using UnityEngine;

namespace BeatThat.Entities
{
    [CustomEditor(typeof(EntityStore), editorForChildClasses: true)]
    public class EntityStoreEditor : UnityEditor.Editor
    {
        private bool showStored { get; set; }

        override public void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!Application.isPlaying)
            {
                return;
            }

            AddStoredItemsFoldout();
        }

        void OnEnable()
        {
            m_isExpandedById = DictionaryPool<string, bool>.Get();
        }

        void OnDisable()
        {
            if (m_isExpandedById != null)
            {
                m_isExpandedById.Dispose();
                m_isExpandedById = null;
            }
        }
        private PooledDictionary<string, bool> m_isExpandedById;

        protected void AddStoredItemsFoldout()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            this.showStored = EditorGUILayout.Foldout(this.showStored, "Stored Items");
            if (!this.showStored)
            {
                return;
            }

            EditorGUI.indentLevel++;

            var model = this.target as EntityStore;

            using (var storedIds = ListPool<string>.Get())
            {
                model.GetAllStoredKeys(storedIds);

                var defaultColor = GUI.color;
                var defaultContentColor = GUI.contentColor;

                var now = DateTimeOffset.Now;


                foreach (var id in storedIds)
                {
                    var foldoutStyle = EditorStyles.foldout;
                    ResolveStatus loadStatus;
                    if (!model.GetResolveStatus(id, out loadStatus))
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(loadStatus.resolveError))
                    {
                        GUI.color = ERROR;
                        GUI.contentColor = ERROR;

                    }
                    else if (loadStatus.isResolveInProgress)
                    {
                        GUI.color = IN_PROGRESS;
                        GUI.contentColor = IN_PROGRESS;

                    }
                    else if (loadStatus.IsExpiredAt(now))
                    {
                        GUI.color = EXPIRED;
                        GUI.contentColor = EXPIRED;

                        foldoutStyle = new GUIStyle(foldoutStyle);
                        foldoutStyle.normal.textColor = EXPIRED;

                    }
                    else
                    {
                        GUI.color = loadStatus.hasResolved ? defaultColor : NONE;
                        GUI.contentColor = GUI.color;
                    }

                    bool showingDetails = false;
                    m_isExpandedById.TryGetValue(id, out showingDetails);

                    var showDetails = EditorGUILayout.Foldout(showingDetails, new GUIContent(id), foldoutStyle);

                    if (showDetails != showingDetails)
                    {
                        m_isExpandedById[id] = showDetails;
                    }

                    if (!showDetails)
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(loadStatus.resolveError))
                    {
                        EditorGUILayout.LabelField("error", loadStatus.resolveError);
                    }
                    else if (loadStatus.isResolveInProgress)
                    {
                        EditorGUILayout.LabelField("loading for ", (DateTimeOffset.Now - loadStatus.updatedAt).TotalSeconds + "secs");
                        GUI.color = IN_PROGRESS;
                    }
                    else if (loadStatus.IsExpiredAt(now))
                    {
                        EditorGUILayout.LabelField("expired at",
                                                   loadStatus.timestamp.AddSeconds(loadStatus.maxAgeSecs).ToLocalTime().ToString("yyyy-MM-ddTHH:mm:ss.fff"));
                    }
                    else
                    {
                        GUI.color = loadStatus.hasResolved ? defaultColor : NONE;

                        if (loadStatus.hasResolved)
                        {
                            EditorGUILayout.LabelField("timestamp", loadStatus.timestamp.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:ss.fff"));
                            EditorGUILayout.LabelField("max age", loadStatus.maxAgeSecs.ToString());
                        }
                    }

                    GUI.contentColor = defaultContentColor;

                    EditorGUILayout.Separator();
                }

                EditorGUI.indentLevel--;
            }
        }

#pragma warning disable 414
        private static readonly Color IN_PROGRESS = Color.cyan;
        private static readonly Color ERROR = Color.red;
        private static readonly Color EXPIRED = Color.yellow;
        private static readonly Color NONE = Color.gray;
#pragma warning restore 414

    }
}




