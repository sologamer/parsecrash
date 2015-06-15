using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Parse;

public class TestParse : MonoBehaviour
{
	Queue<Action> dispatchers = new Queue<Action>();

	string GetStreamingAssetsPath()
	{
		string path;
		#if UNITY_EDITOR
		path = "file:" + Application.dataPath + "/StreamingAssets";
		#elif UNITY_ANDROID
		path = "jar:file://"+ Application.dataPath + "!/assets/";
		#elif UNITY_IOS
		path = "file:" + Application.dataPath + "/Raw";
		#else
		//Desktop (Mac OS or Windows)
		path = "file:"+ Application.dataPath + "/StreamingAssets";
		#endif
		
		return path;
	}

	private IEnumerator ReadFile (string file)
	{
		WWW www = new WWW (file);
		yield return www;

		
		Debug.Log ("#1 Creating ParseObject");
		var q = new ParseObject("TestObject");

		
		Debug.Log ("#2 Set data");
		q ["data"] = www.text;
		
		Debug.Log ("#3 Set length");
		q ["dataLength"] = www.text.Length;


		Debug.Log ("#4 Calling SaveAsync");
		var task = q.SaveAsync ();
	
		Action action = () => {
			Debug.Log ("Inserted to parse with data length = " + www.text.Length);
		};

		task.ContinueWith(t => { dispatchers.Enqueue(action); });
	}

	public void Update()
	{
		while (dispatchers.Count > 0) {
			dispatchers.Dequeue()();
		}
	}

	public void OnGUI()
	{
		if (GUILayout.Button ("Insert", GUILayout.Width(300), GUILayout.Height(125))) {
			string path = GetStreamingAssetsPath();
			string file = path + "/json.txt";
			StartCoroutine(ReadFile(file));
		}

		if (GUILayout.Button ("Test FindAsync", GUILayout.Width(300), GUILayout.Height(125))) {
			Debug.Log ("#1 - Start FindAsync");
			var q = new ParseQuery<ParseObject>("TestObject");
			Debug.Log ("#2 - Created Query");
			
			// This breaks when building with IL2CPP (Unity 4.6.6p1), however it works for Mono 2.x
			// Probably something with fetching a large amount of data
			var find = q.FindAsync(); 
			Debug.Log ("#3 - Called FindAsync");
			
			find.ContinueWith(task =>
			                  {
				// Never comes here because the game crashed
				Debug.Log ("Task Result: " + find.Result);
			});
		}

		if (GUILayout.Button ("Test FirstOrDefaultAsync", GUILayout.Width(300), GUILayout.Height(125))) {
			Debug.Log ("#1 - Start FirstOrDefaultAsync");
			var q = new ParseQuery<ParseObject>("TestObject");
			Debug.Log ("#2 - Created Query");
			
			// This breaks when building with IL2CPP (Unity 4.6.6p1), however it works for Mono 2.x
			// Probably something with fetching a large amount of data
			var find = q.FirstOrDefaultAsync(); 
			Debug.Log ("#3 - Called FirstOrDefaultAsync");
			
			find.ContinueWith(task =>
			                  {
				// Never comes here because the game crashed
				Debug.Log ("Task Result: " + find.Result);
			});
		}
	}
}
