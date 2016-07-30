using FluentHub.ModelConverter.FluentBuilderItems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.ModelConverter
{
    public static class FluentModelConverterBuilder
    {
        public static ModelBuilder<T> ToModelBuilder<T>(this T _)
            where T : class, new()
        {
            var builder = new ModelBuilder<T>();
            // default type converter
            builder.Converter.RegisterConverter(typeof(bool), m => BitConverter.GetBytes((bool)m), data => BitConverter.ToBoolean(data, 0));
            builder.Converter.RegisterConverter(typeof(char), m => BitConverter.GetBytes((char)m), data => BitConverter.ToChar(data, 0));
            builder.Converter.RegisterConverter(typeof(short), m => BitConverter.GetBytes((short)m), data => BitConverter.ToInt16(data, 0));
            builder.Converter.RegisterConverter(typeof(int), m => BitConverter.GetBytes((int)m), data => BitConverter.ToInt32(data, 0));
            builder.Converter.RegisterConverter(typeof(long), m => BitConverter.GetBytes((long)m), data => BitConverter.ToInt64(data, 0));
            builder.Converter.RegisterConverter(typeof(ushort), m => BitConverter.GetBytes((ushort)m), data => BitConverter.ToUInt16(data, 0));
            builder.Converter.RegisterConverter(typeof(uint), m => BitConverter.GetBytes((uint)m), data => BitConverter.ToUInt32(data, 0));
            builder.Converter.RegisterConverter(typeof(ulong), m => BitConverter.GetBytes((ulong)m), data => BitConverter.ToUInt64(data, 0));
            builder.Converter.RegisterConverter(typeof(float), m => BitConverter.GetBytes((float)m), data => BitConverter.ToSingle(data, 0));
            builder.Converter.RegisterConverter(typeof(double), m => BitConverter.GetBytes((double)m), data => BitConverter.ToDouble(data, 0));
            builder.Converter.RegisterConverter(typeof(byte[]), m => (byte[])m, data => data);
            return builder;
        }

        public static ModelBuilder<T> ToBigEndian<T>(this ModelBuilder<T> @this)
            where T : class, new()
        {
            // default type converter
            if (BitConverter.IsLittleEndian == false)
            {
                return @this;
            }
            // override
            @this.Converter.RegisterConverter(typeof(bool), m => BitConverter.GetBytes((bool)m).Reverse().ToArray(), data => BitConverter.ToBoolean(data.Reverse().ToArray(), 0));
            @this.Converter.RegisterConverter(typeof(char), m => BitConverter.GetBytes((char)m).Reverse().ToArray(), data => BitConverter.ToChar(data.Reverse().ToArray(), 0));
            @this.Converter.RegisterConverter(typeof(short), m => BitConverter.GetBytes((short)m).Reverse().ToArray(), data => BitConverter.ToInt16(data.Reverse().ToArray(), 0));
            @this.Converter.RegisterConverter(typeof(int), m => BitConverter.GetBytes((int)m).Reverse().ToArray(), data => BitConverter.ToInt32(data.Reverse().ToArray(), 0));
            @this.Converter.RegisterConverter(typeof(long), m => BitConverter.GetBytes((long)m).Reverse().ToArray(), data => BitConverter.ToInt64(data.Reverse().ToArray(), 0));
            @this.Converter.RegisterConverter(typeof(ushort), m => BitConverter.GetBytes((ushort)m).Reverse().ToArray(), data => BitConverter.ToUInt16(data.Reverse().ToArray(), 0));
            @this.Converter.RegisterConverter(typeof(uint), m => BitConverter.GetBytes((uint)m).Reverse().ToArray(), data => BitConverter.ToUInt32(data.Reverse().ToArray(), 0));
            @this.Converter.RegisterConverter(typeof(ulong), m => BitConverter.GetBytes((ulong)m).Reverse().ToArray(), data => BitConverter.ToUInt64(data.Reverse().ToArray(), 0));
            @this.Converter.RegisterConverter(typeof(float), m => BitConverter.GetBytes((float)m).Reverse().ToArray(), data => BitConverter.ToSingle(data.Reverse().ToArray(), 0));
            @this.Converter.RegisterConverter(typeof(double), m => BitConverter.GetBytes((double)m).Reverse().ToArray(), data => BitConverter.ToDouble(data.Reverse().ToArray(), 0));

            return @this;
        }
        
        public static ModelBuilder<T> Init<T>(this ModelBuilder<T> @this, Action<T> init)
            where T : class, new()
        {
            @this.RegisterInit(init);
            return @this;
        }

        public static ModelBuilder<T> Property<T, V>(
            this ModelBuilder<T> @this
            , Expression<Func<T, V>> getterExpression)
            where T : class, new()
        {
            var getter = getterExpression.Compile();
            var setter = MakeSetter(getterExpression);
            @this.AddBuildItem(
                new PropertyBuildItem<T, V>(
                    getter
                    , setter
                    , @this.Converter));
            return @this;
        }

        public static ModelBuilder<T> Property<T, V>(
            this ModelBuilder<T> @this
            , Expression<Func<T, V>> getterExpression
            , Action<ModelBuilder<V>> childModelBuilderFactory)
            where T : class, new()
            where V : class, new()
        {
            var getter = getterExpression.Compile();
            var setter = MakeSetter(getterExpression);
            var childModelBuilder = @this.MakeChildModelBuilder(childModelBuilderFactory);

            @this.AddBuildItem(
                new ProxyModelBuildItem<T, V>(
                    getter
                    , setter
                    , childModelBuilder));
            return @this;
        }


        public static ModelBuilder<T> GetProperty<T, V>(
            this ModelBuilder<T> @this
            , Func<T, V> getter)
            where T : class, new()
        {
            @this.AddBuildItem(new GetPropertyBuildItem<T, V>(getter, @this.Converter));
            return @this;
        }

        public static ModelBuilder<T> Constant<T, V>(this ModelBuilder<T> @this, V v)
            where T : class, new()
        {
            @this.AddBuildItem(new ConstantBuildItem<T, V>(v, @this.Converter));
            return @this;
        }

        

        public static ModelBuilder<T> Array<T, V>(
            this ModelBuilder<T> @this
            , string loopCountName
            , Expression<Func<T, IEnumerable<V>>> getterExpression
            , Action<ModelBuilder<V>> childModelBuilderFactory)
            where T : class, new()
            where V : class, new()
        {
            var getter = getterExpression.Compile();
            var setter = MakeSetter(getterExpression);
            var childModelBuilder = @this.MakeChildModelBuilder(childModelBuilderFactory);

            var arrayMember = getterExpression.GetPropertyInfo().Last();
            var tryArrayConvert = MakeArrayConvert<V>(arrayMember.PropertyType);

            @this.AddBuildItem(
                new ArrayBuildItem<T, V>(
                    childModelBuilder
                    , getter
                    , (m, xs) => setter(m, tryArrayConvert(xs))
                    , loopCountName));
            return @this;
        }

        public static ModelBuilder<T> FixedArray<T, VModel>(
            this ModelBuilder<T> @this
            , int loopCount
            , Expression<Func<T, IEnumerable<VModel>>> getterExpression
            , Action<ModelBuilder<VModel>> childModelBuilderFactory)
            where T : class, new()
            where VModel : class, new()
        {
            var getter = getterExpression.Compile();
            var setter = MakeSetter(getterExpression);
            var childModelBuilder = @this.MakeChildModelBuilder(childModelBuilderFactory);

            var arrayMember = getterExpression.GetPropertyInfo().Last();
            var tryArrayConvert = MakeArrayConvert<VModel>(arrayMember.PropertyType);

            @this.AddBuildItem(new FixedArrayBuildItem<T, VModel>(
                childModelBuilder
                , getter
                , (m, xs) => setter(m, tryArrayConvert(xs))
                , loopCount));
            return @this;
        }

        public static ModelBuilder<T> AsTag<T>(this ModelBuilder<T> @this, string tagName)
            where T : class, new()
        {
            @this.SetTagForLastOne(tagName);
            return @this;
        }

        public static IModelConverter<T> ToConverter<T>(this ModelBuilder<T> @this)
            where T : class, new()
        {
            return new FluentModelConverter<T>(@this);
        }

        public static IModelConverter<P> ToBaseTypeConverter<T,P>(this IModelConverter<T> @this)
            where T : P
        {
            return new BaseTypeModelConverter<P,T>(@this);
        }






        static Action<T, V> MakeSetter<T, V>(Expression<Func<T, V>> getterExpression)
        {
            var chain = getterExpression.GetPropertyInfo().ToArray();
            var setter = (Action<T, V>)((T m, V v) =>
            {
                // 最後の1つ手前メンバアクセスがx.y.zだとしたらyまで進む
                var visitMemberAccess = chain.Take(chain.Length - 1);
                var y =
                   visitMemberAccess
                   .Aggregate(m as object, (x, pi) =>
                   {
                       var value = pi.GetValue(x);
                       // 途中でnullメンバを見つけたら勝手にインスタンス作ってみる
                       if (value == null)
                       {
                           value = Activator.CreateInstance(pi.PropertyType);
                           pi.SetValue(x, value);
                       }
                       return value;
                   });

                var lastone = chain.Last();
                lastone.SetValue(y, v);
            });
            return setter;
        }


        private static Func<IEnumerable<VModel>, IEnumerable<VModel>> MakeArrayConvert<VModel>(Type arrayType)
        {
            // todo add other array
            if (arrayType.Equals(typeof(VModel[])))
            {
                return xs => xs.ToArray();
            }
            else if (arrayType.Equals(typeof(List<VModel>)))
            {
                return xs => xs.ToList();
            }
            else
            {
                return xs => xs;
            }
        }


        static ModelBuilder<ChildModel> MakeChildModelBuilder<T, ChildModel>(
            this ModelBuilder<T> @this
            , Action<ModelBuilder<ChildModel>> childModelBuilderFactory)
            where T : class, new()
            where ChildModel : class, new()
        {
            var childModelBuilder = new ModelBuilder<ChildModel>();
            childModelBuilder.Converter = @this.Converter;
            childModelBuilderFactory(childModelBuilder);
            return childModelBuilder;
        }

        public static IEnumerable<PropertyInfo> GetPropertyInfo<T, _>(this Expression<Func<T, _>> lambda)
        {
            if (lambda.Body.NodeType != ExpressionType.MemberAccess)
            {
                throw new Exception("getter body is must be MemberAccess");
            }
            return lambda.Body.GetPropertyInfo();
        }

        public static IEnumerable<PropertyInfo> GetPropertyInfo(this Expression @this)
        {
            if (@this.NodeType != ExpressionType.MemberAccess)
            {
                throw new Exception("getter body is must be MemberAccess");
            }
            var asMemberExpression = (@this as System.Linq.Expressions.MemberExpression);

            var info = asMemberExpression.Member as PropertyInfo;
            if (info == null || info.CanRead == false || info.CanWrite == false)
            {
                throw new Exception("member is must be get set Property");
            }

            var chain = new[] { info };

            if (asMemberExpression.Expression.NodeType == ExpressionType.MemberAccess)
            {
                chain = chain.Concat(asMemberExpression.Expression.GetPropertyInfo()).ToArray();
            }
            return chain.Reverse().ToArray();
        }
    }
}
