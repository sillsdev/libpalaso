using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SIL.IO.FileLock.FileSys
{
	internal static class LockIO
	{
		public static string GetFilePath(string lockName)
		{
			return Path.Combine(Path.GetTempPath(), lockName);
		}

		public static bool LockExists(string lockFilePath)
		{
			return File.Exists(lockFilePath);
		}

		public static FileLockContent ReadLock(string lockFilePath)
		{
			try
			{
				using (var streamReader = new StreamReader(File.OpenRead(lockFilePath)))
				{
					JObject obj = JObject.Load(new JsonTextReader(streamReader));
					return new FileLockContent
					{
						PID = obj["PID"].ToObject<long>(),
						ProcessName = obj["ProcessName"].ToString(),
						Timestamp = obj["Timestamp"].ToObject<long>()
					};
				}
			}
			catch (FileNotFoundException)
			{
				return new MissingFileLockContent();
			}
			catch (IOException)
			{
				return new OtherProcessOwnsFileLockContent();
			}
			catch (Exception) //We have no idea what went wrong - reacquire this lock
			{
				return new MissingFileLockContent();
			}
		}

		public static bool WriteLock(string lockFilePath, FileLockContent lockContent)
		{
			try
			{
				var obj = new JObject();
				// type is only required for forward compatibility.
				// older versions of this library used DataContractJsonSerializer to read/write the lock file contents.
				// by writing out the old type value, it allows older versions of this library to continue to be able
				// to read the lock files correctly.
				obj["__type"] = "FileLockContent:#Palaso.IO.FileLock";
				obj["PID"] = lockContent.PID;
				obj["ProcessName"] = lockContent.ProcessName;
				obj["Timestamp"] = lockContent.Timestamp;
				using (StreamWriter streamWriter = new StreamWriter(File.Create(lockFilePath)))
				{
					obj.WriteTo(new JsonTextWriter(streamWriter));
				}
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public static void DeleteLock(string lockFilePath)
		{
			try
			{
				File.Delete(lockFilePath);
			}
			catch (Exception)
			{
				
			}
		}
	}
}
