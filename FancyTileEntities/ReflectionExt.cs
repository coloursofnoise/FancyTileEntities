using System;
using System.Globalization;
using System.Reflection;

namespace Celeste.Mod.FancyTileEntities {
    public static partial class Extensions {

        public static FieldInfo<T> GetField<T>(this Type type, string name) {
            return new FieldInfo<T>(type.GetField(name));
        }

        public static FieldInfo<T> GetField<T>(this Type type, string name, BindingFlags bindingAttr) {
            return new FieldInfo<T>(type.GetField(name, bindingAttr));
        }

        public class FieldInfo<T> {
            protected FieldInfo _FieldInfo;

            public FieldInfo(FieldInfo field) {
                if (!typeof(T).IsAssignableFrom(field.FieldType))
                    throw new InvalidCastException($"Field of type {field.FieldType} cannot be cast to type {typeof(T)}.");

                _FieldInfo = field;
            }

            public T GetValue(object obj) {
                return (T) _FieldInfo.GetValue(obj);
            }

            public void SetValue(object obj, T value) {
                _FieldInfo.SetValue(obj, value);
            }

            public void SetValue(object obj, T value, BindingFlags invokeAttr, Binder binder, CultureInfo culture) {
                _FieldInfo.SetValue(obj, value, invokeAttr, binder, culture);
            }

            public static implicit operator FieldInfo(FieldInfo<T> field) => field._FieldInfo;

            public T this[object obj] {
                get { return GetValue(obj); }
                set { SetValue(obj, value); }
            }

        }

        public static MethodInfo<TReturn> GetMethod<TReturn>(this Type type, string name) {
            return new MethodInfo<TReturn>(type.GetMethod(name));
        }

        public static MethodInfo<TReturn> GetMethod<TReturn>(this Type type, string name, BindingFlags bindingAttr) {
            return new MethodInfo<TReturn>(type.GetMethod(name, bindingAttr));
        }

        public class MethodInfo<TReturn> {
            protected MethodInfo _MethodInfo;

            public MethodInfo(MethodInfo method) {
                if (!typeof(TReturn).IsAssignableFrom(method.ReturnType))
                    throw new InvalidCastException($"Method return of type {method.ReturnType} cannot be cast to type {typeof(TReturn)}.");

                _MethodInfo = method;
            }

            public TReturn Invoke(object obj) {
                return (TReturn) _MethodInfo.Invoke(obj, null);
            }

            public TReturn Invoke(object obj, object[] parameters) {
                return (TReturn) _MethodInfo.Invoke(obj, parameters);
            }

            public TReturn Invoke(object obj, BindingFlags invokeAttr, Binder binder, CultureInfo culture) {
                return (TReturn) _MethodInfo.Invoke(obj, invokeAttr, binder, null, culture);
            }

            public static implicit operator MethodInfo(MethodInfo<TReturn> method) => method._MethodInfo;

            public TReturn this[object obj] 
                => Invoke(obj);

        }

    }
}
