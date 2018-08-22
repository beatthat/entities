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

            using (var storedIds = ListPool<string>.Get ()) {
                model.GetAllStoredKeys (storedIds);

				var defaultColor = GUI.color;

                foreach (var id in storedIds) {
                    ResolveStatus loadStatus;
                    if (!model.GetResolveStatus (id, out loadStatus)) {
						continue;
					}

                    if (!string.IsNullOrEmpty (loadStatus.resolveError)) {
						GUI.color = ERROR;
                    } else if (loadStatus.isResolveInProgress) {
						GUI.color = PENDING;
					} else {
                        GUI.color = loadStatus.hasResolved ? defaultColor : NONE;
					}

					EditorGUILayout.LabelField (id);
					if (!string.IsNullOrEmpty (loadStatus.resolveError)) {
                        EditorGUILayout.LabelField ("error", loadStatus.resolveError);
                    } else if (loadStatus.isResolveInProgress) {
                        EditorGUILayout.LabelField ("loading for ", (DateTimeOffset.Now - loadStatus.updatedAt).TotalSeconds + "secs");
						GUI.color = PENDING;
					} else {
                        GUI.color = loadStatus.hasResolved ? defaultColor : NONE;
					}

					EditorGUILayout.Separator ();
				}

				EditorGUI.indentLevel--;
			}
        }

		#pragma warning disable 414
		private static readonly Color IN_PROGRESS = Color.cyan;
		private static readonly Color ERROR = Color.red;
		private static readonly Color PENDING = Color.yellow;
		private static readonly Color NONE = Color.gray;
		#pragma warning restore 414

    }
}




