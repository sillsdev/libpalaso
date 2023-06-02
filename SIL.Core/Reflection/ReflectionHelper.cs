using SIL.IO;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Unix.Native;

namespace SIL.Reflection
{
	public static class ReflectionHelper
	{
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Loads a DLL.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static Assembly LoadAssembly(string dllPath)
		{
			try
			{
				if (!File.Exists(dllPath))
				{
					string dllFile = Path.GetFileName(dllPath);
					string startingDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
					dllPath = Path.Combine(startingDir, dllFile);
					if (!File.Exists(dllPath))
						return null;
				}

				return Assembly.LoadFrom(dllPath);
			}
			catch (Exception)
			{
				return null;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static object CreateClassInstance(Assembly assembly, string className)
		{
			return CreateClassInstance(assembly, className, null);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static object CreateClassInstance(Assembly assembly, string className, object[] args)
		{
			try
			{
				// First, take a stab at creating the instance with the specified name.
				object instance = assembly.CreateInstance(className, false,
					BindingFlags.Instance | BindingFlags.CreateInstance | BindingFlags.NonPublic, null, args, null, null);

				if (instance != null)
					return instance;

				Type[] types = assembly.GetTypes();

				// At this point, we know we failed to instantiate a class with the
				// specified name, so try to find a type with that name and attempt
				// to instantiate the class using the full namespace.
				foreach (Type type in types)
				{
					if (type.Name == className)
					{
						return assembly.CreateInstance(type.FullName, false,
							BindingFlags.Instance | BindingFlags.CreateInstance | BindingFlags.NonPublic, null, args, null, null);
					}
				}
			}
			catch { }

			return null;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns a string value returned from a call to a private method.
		/// </summary>
		/// <param name="binding">This is either the Type of the object on which the method
		/// is called or an instance of that type of object. When the method being called
		/// is static then binding should be a type.</param>
		/// <param name="methodName">Name of the method to call.</param>
		/// <param name="args">An array of arguments to pass to the method call.</param>
		/// ------------------------------------------------------------------------------------
		public static string GetStrResult(object binding, string methodName, object[] args)
		{
			return (GetResult(binding, methodName, args) as string);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns a string value returned from a call to a private method.
		/// </summary>
		/// <param name="binding">This is either the Type of the object on which the method
		/// is called or an instance of that type of object. When the method being called
		/// is static then binding should be a type.</param>
		/// <param name="methodName">Name of the method to call.</param>
		/// <param name="args">An single arguments to pass to the method call.</param>
		/// ------------------------------------------------------------------------------------
		public static string GetStrResult(object binding, string methodName, object args)
		{
			return (GetResult(binding, methodName, args) as string);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns a integer value returned from a call to a private method.
		/// </summary>
		/// <param name="binding">This is either the Type of the object on which the method
		/// is called or an instance of that type of object. When the method being called
		/// is static then binding should be a type.</param>
		/// <param name="methodName">Name of the method to call.</param>
		/// <param name="args">An single arguments to pass to the method call.</param>
		/// ------------------------------------------------------------------------------------
		public static int GetIntResult(object binding, string methodName, object args)
		{
			return ((int)GetResult(binding, methodName, args));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns a integer value returned from a call to a private method.
		/// </summary>
		/// <param name="binding">This is either the Type of the object on which the method
		/// is called or an instance of that type of object. When the method being called
		/// is static then binding should be a type.</param>
		/// <param name="methodName">Name of the method to call.</param>
		/// <param name="args">An array of arguments to pass to the method call.</param>
		/// ------------------------------------------------------------------------------------
		public static int GetIntResult(object binding, string methodName, object[] args)
		{
			return ((int)GetResult(binding, methodName, args));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns a float value returned from a call to a private method.
		/// </summary>
		/// <param name="binding">This is either the Type of the object on which the method
		/// is called or an instance of that type of object. When the method being called
		/// is static then binding should be a type.</param>
		/// <param name="methodName">Name of the method to call.</param>
		/// <param name="args">An single arguments to pass to the method call.</param>
		/// ------------------------------------------------------------------------------------
		public static float GetFloatResult(object binding, string methodName, object args)
		{
			return ((float)GetResult(binding, methodName, args));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns a float value returned from a call to a private method.
		/// </summary>
		/// <param name="binding">This is either the Type of the object on which the method
		/// is called or an instance of that type of object. When the method being called
		/// is static then binding should be a type.</param>
		/// <param name="methodName">Name of the method to call.</param>
		/// <param name="args">An array of arguments to pass to the method call.</param>
		/// ------------------------------------------------------------------------------------
		public static float GetFloatResult(object binding, string methodName, object[] args)
		{
			return ((float)GetResult(binding, methodName, args));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns a boolean value returned from a call to a private method.
		/// </summary>
		/// <param name="binding">This is either the Type of the object on which the method
		/// is called or an instance of that type of object. When the method being called
		/// is static then binding should be a type.</param>
		/// <param name="methodName">Name of the method to call.</param>
		/// <param name="args">An single arguments to pass to the method call.</param>
		/// ------------------------------------------------------------------------------------
		public static bool GetBoolResult(object binding, string methodName, object args)
		{
			return ((bool)GetResult(binding, methodName, args));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns a boolean value returned from a call to a private method.
		/// </summary>
		/// <param name="binding">This is either the Type of the object on which the method
		/// is called or an instance of that type of object. When the method being called
		/// is static then binding should be a type.</param>
		/// <param name="methodName">Name of the method to call.</param>
		/// <param name="args">An array of arguments to pass to the method call.</param>
		/// ------------------------------------------------------------------------------------
		public static bool GetBoolResult(object binding, string methodName, object[] args)
		{
			return ((bool)GetResult(binding, methodName, args));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Calls a method specified on the specified binding.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static void CallMethod(object binding, string methodName)
		{
			CallMethod(binding, methodName, null);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Calls a method specified on the specified binding.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static void CallMethod(object binding, string methodName, object args)
		{
			GetResult(binding, methodName, args);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Calls a method specified on the specified binding.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static void CallMethod(object binding, string methodName, object arg1, object arg2)
		{
			object[] args = new[] { arg1, arg2 };
			GetResult(binding, methodName, args);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Calls a method specified on the specified binding.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static void CallMethod(object binding, string methodName, object arg1,
			object arg2, object arg3)
		{
			object[] args = new[] { arg1, arg2, arg3 };
			GetResult(binding, methodName, args);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Calls a method specified on the specified binding.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static void CallMethod(object binding, string methodName, object arg1,
			object arg2, object arg3, object arg4)
		{
			object[] args = new[] { arg1, arg2, arg3, arg4 };
			GetResult(binding, methodName, args);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Calls a method specified on the specified binding.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static void CallMethod(object binding, string methodName, object[] args)
		{
			GetResult(binding, methodName, args);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns the result of calling a method on the specified binding.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static object GetResult(object binding, string methodName, object args)
		{
			return Invoke(binding, methodName, new[] { args }, BindingFlags.InvokeMethod);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns the result of calling a method on the specified binding.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static object GetResult(object binding, string methodName, object[] args)
		{
			return Invoke(binding, methodName, args, BindingFlags.InvokeMethod);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the specified property on the specified binding.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static void SetProperty(object binding, string propertyName, object args)
		{
			Invoke(binding, propertyName, new[] { args }, BindingFlags.SetProperty);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the specified field (i.e. member variable) on the specified binding.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static void SetField(object binding, string fieldName, object args)
		{
			Invoke(binding, fieldName, new[] { args }, BindingFlags.SetField);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the specified property on the specified binding.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static object GetProperty(object binding, string propertyName)
		{
			return Invoke(binding, propertyName, null, BindingFlags.GetProperty);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the specified field (i.e. member variable) on the specified binding.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static object GetField(object binding, string fieldName)
		{
			return Invoke(binding, fieldName, null, BindingFlags.GetField);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the specified member variable or property (specified by name) on the
		/// specified binding.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static object Invoke(object binding, string name, object[] args, BindingFlags flags)
		{
			//if (CanInvoke(binding, name, flags))
			{
				try
				{
					return InvokeWithError(binding, name, args, flags);
				}
				catch { }
			}

			return null;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Calls a method specified on the specified binding, throwing any exceptions that
		/// may occur.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static object CallMethodWithThrow(object binding, string name, params object[] args)
		{
			return InvokeWithError(binding, name, args, BindingFlags.InvokeMethod);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the specified member variable or property (specified by name) on the
		/// specified binding.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static object InvokeWithError(object binding, string name, object[] args, BindingFlags flags)
		{
			// If binding is a Type then assume invoke on a static method, property or field.
			// Otherwise invoke on an instance method, property or field.
			flags |= BindingFlags.NonPublic | BindingFlags.Public |
					 (binding is Type ? BindingFlags.Static : BindingFlags.Instance);

			// If necessary, go up the inheritance chain until the name
			// of the method, property or field is found.
			Type type = binding is Type ? (Type)binding : binding.GetType();
			while (type.GetMember(name, flags).Length == 0 && type.BaseType != null)
				type = type.BaseType;

			return type.InvokeMember(name, flags, null, binding, args);
		}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// Gets a value indicating whether or not the specified binding contains the field,
		///// property or method indicated by name and having the specified flags.
		///// </summary>
		///// ------------------------------------------------------------------------------------
		//private static bool CanInvoke(object binding, string name, BindingFlags flags)
		//{
		//	var srchFlags = (BindingFlags.Public | BindingFlags.NonPublic);
		//	Type bindingType = null;

		//	if (binding is Type)
		//	{
		//		bindingType = (Type)binding;
		//		srchFlags |= BindingFlags.Static;
		//	}
		//	else
		//	{
		//		binding.GetType();
		//		srchFlags |= BindingFlags.Instance;
		//	}

		//	if (((flags & BindingFlags.GetProperty) == BindingFlags.GetProperty) ||
		//		((flags & BindingFlags.SetProperty) == BindingFlags.SetProperty))
		//	{
		//		return (bindingType.GetProperty(name, srchFlags) != null);
		//	}

		//	if (((flags & BindingFlags.GetField) == BindingFlags.GetField) ||
		//		((flags & BindingFlags.SetField) == BindingFlags.SetField))
		//	{
		//		return (bindingType.GetField(name, srchFlags) != null);
		//	}

		//	if ((flags & BindingFlags.InvokeMethod) == BindingFlags.InvokeMethod)
		//		return (bindingType.GetMethod(name, srchFlags) != null);

		//	return false;
		//}

		public static string VersionNumberString
		{
			get
			{
				/*                object attr = GetAssemblyAttribute(typeof (AssemblyFileVersionAttribute));
								if (attr != null)
								{
									return ((AssemblyFileVersionAttribute) attr).Version;
								}
								return Application.ProductVersion;
				 */
				var ver = Assembly.GetEntryAssembly().GetName().Version;
				return string.Format("Version {0}.{1}.{2}", ver.Major, ver.Minor, ver.Build);
			}
		}

		public static string LongVersionNumberString
		{
			get
			{
				Assembly assembly = Assembly.GetEntryAssembly();
				if (assembly != null)
				{
					string version = VersionNumberString;

					version += " (apparent build date: ";
					try
					{
						string path = PathHelper.StripFilePrefix(assembly.CodeBase);
						version += File.GetLastWriteTimeUtc(path).ToString("dd-MMM-yyyy") + ")";
					}
					catch
					{
						version += "???";
					}

#if DEBUG
					version += "  (Debug version)";
#endif
					return version;
				}
				return "unknown";
			}
		}

		private static readonly bool RunningFromUnitTest = AppDomain.CurrentDomain.GetAssemblies()
			.Any(assem => 
				assem.FullName.StartsWith("nunit.framework", StringComparison.OrdinalIgnoreCase)
				|| assem.FullName.StartsWith("Microsoft.VisualStudio.QualityTools.UnitTestFramework", StringComparison.OrdinalIgnoreCase)
				|| assem.FullName.StartsWith("Microsoft.VisualStudio.TestPlatform.TestFramework", StringComparison.OrdinalIgnoreCase)
				|| assem.FullName.StartsWith("xunit.core", StringComparison.OrdinalIgnoreCase)
			);

		public static string DirectoryOfTheApplicationExecutable
		{
			get
			{
				string path;
				// checking for assembly == null is for backwards compatibility with old code that may unintentionally call take the unit test path
				if (RunningFromUnitTest || Assembly.GetEntryAssembly() == null)
				{
					path = new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath;
					path = Uri.UnescapeDataString(path);
				}
				else
				{
					path = EntryAssembly.Location;
				}
				return Directory.GetParent(path).FullName;
			}
		}
	}
}
