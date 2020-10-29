using System;
using System.Collections.Generic;
using BeatThat.Pools;
using UnityEditor;
using UnityEngine;

namespace BeatThat.Entities
{
    [CustomEditor(typeof(EntityStore), editorForChildClasses: true)]
    public class EntityStoreEditor : UnityEditor.Editor
    {
        public delegate string FoldoutStringFor(string entityId);
        public delegate void FoldoutPropsFor(string entityId, IList<KeyValuePair<string, string>> props);

        private bool showStored { get; set; }

        /// <summary>
        /// Set to display a specific string (e.g. the item name) 
        /// for each entity's details foldout in the inspector.
        /// When unset, displays the entity id
        /// </summary>
        /// <value>The foldout string delegate.</value>
        protected FoldoutStringFor foldoutStringDelegate { get; set; }

        /// <summary>
        /// Set to display a custom set of props for each stored entity
        /// when it's foldout is expanded in the inspector.
        /// By default displays on details about resolve status.
        /// </summary>
        /// <value>The foldout properties delegate.</value>
        protected FoldoutPropsFor foldoutPropsDelegate { get; set; }

        override public void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!Application.isPlaying)
            {
                return;
            }

            AddStoredItemsFoldout();
        }

        protected void OnEnable()
        {
            m_isExpandedById = DictionaryPool<string, bool>.Get();
            OnEnableEntityStoreEditor();
        }

        protected void OnDisable()
        {
            if (m_isExpandedById != null)
            {
                m_isExpandedById.Dispose();
                m_isExpandedById = null;
            }
            OnDisableEntityStoreEditor();
        }
        private PooledDictionary<string, bool> m_isExpandedById;


        virtual protected void OnEnableEntityStoreEditor() { }
        virtual protected void OnDisableEntityStoreEditor() { }

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

            var entityStore = this.target as EntityStore;

            using (var storedIds = ListPool<string>.Get())
            {
                entityStore.GetAllStoredKeys(storedIds);

                var defaultColor = GUI.color;
                var defaultContentColor = GUI.contentColor;

                var now = DateTimeOffset.Now;

                foreach (var id in storedIds)
                {
                    var foldoutStyle = EditorStyles.foldout;
                    ResolveStatus loadStatus;
                    if (!entityStore.GetResolveStatus(id, out loadStatus))
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

                    var foldoutString = this.foldoutStringDelegate != null ?
                                            this.foldoutStringDelegate(id): id;

                    var showDetails = EditorGUILayout.Foldout(showingDetails, new GUIContent(foldoutString), foldoutStyle);

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
                    }

                    if (loadStatus.hasResolved)
                    {
                        EditorGUILayout.LabelField("timestamp", loadStatus.timestamp.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:ss.fff"));
                        EditorGUILayout.LabelField("max age", loadStatus.maxAgeSecs.ToString());
                    }

                    if(this.foldoutPropsDelegate != null) {
                        using(var props = ListPool<KeyValuePair<string, string>>.Get()) {
                            this.foldoutPropsDelegate(id, props);
                            foreach(var p in props) {
                                EditorGUILayout.LabelField(new GUIContent(p.Key), 
                                                           new GUIContent(p.Value), 
                                                           EditorStyles.wordWrappedLabel
                                                          );
                            }
                        }
                    }
                    else {
                        try {
                            object data;
                            if(entityStore.GetDataAsObject(id, out data)) {
                                EditorGUILayout.TextArea(JsonUtility.ToJson(data, true));
                            }
                        }
                        catch(Exception e) {
                            Debug.LogError(e);
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




