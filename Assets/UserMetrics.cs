using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class UserMetrics {
	private static string username, emoji_start, emoji_end, face_emotion_start, face_emotion_end;
	private static double time_in_app;
	private static DateTime date;

	public static string Username {
		get {
			return username;
		}

		set {
			username = value;
		}
	}

	public static string Emoji_start {
		get {
			return emoji_start;
		}

		set {
			emoji_start = value;
		}
	}

	public static string Emoji_end {
		get {
			return emoji_end;
		}

		set {
			emoji_end = value;
		}
	}

	public static string Face_emotion_start {
		get {
			return face_emotion_start;
		}

		set {
			face_emotion_start = value;
		}
	}

	public static string Face_emotion_end {
		get {
			return face_emotion_end;
		}

		set {
			face_emotion_end = value;
		}
	}

	public static double Time_in_app {
		get {
			return time_in_app;
		}

		set {
			time_in_app = value;
		}
	}

	public static DateTime Date {
		get {
			return date;
		}

		set {
			date = value;
		}
	}
}