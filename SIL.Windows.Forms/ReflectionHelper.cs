using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace SIL.Windows.Forms
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
					dllPath = Path.Combine(Application.StartupPath, dllFile);
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
					BindingFlags.CreateInstance, null, args, null, null);

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
							BindingFlags.CreateInstance, null, args, null, null);
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
			flags |= (BindingFlags.NonPublic | BindingFlags.Public);

			//if (CanInvoke(binding, name, flags))
			{
				try
				{
					// If binding is a Type then assume invoke on a static method, property or field.
					// Otherwise invoke on an instance method, property or field.
					if (binding is Type)
					{
						return ((binding as Type).InvokeMember(name,
							flags | BindingFlags.Static, null, binding, args));
					}

					return binding.GetType().InvokeMember(name,
						flags | BindingFlags.Instance, null, binding, args);
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
			const BindingFlags flags =
				(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.InvokeMethod);

			// If binding is a Type then assume invoke on a static method, property or field.
			// Otherwise invoke on an instance method, property or field.
			if (binding is Type)
			{
				return ((binding as Type).InvokeMember(name,
					flags | BindingFlags.Static, null, binding, args));
			}

			return binding.GetType().InvokeMember(name,
				flags | BindingFlags.Instance, null, binding, args);
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
	}
}
