using FluentHub.ModelConverter.FluentBuilderItems;
using System;
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

        public static ModelBuilder<T> Property<T, V>(this ModelBuilder<T> @this, Expression<Func<T, V>> f)
            where T : class, new()
        {
            var getter = f.Compile();
            var chain = f.GetPropertyInfo().ToArray();
            var setter = (Action<T, V>)((T m, V v) =>
            {
                // 最後の1つ手前メンバアクセスがx.y.zだとしたらyまで進む
                var visitMemberAccess = chain.Take(chain.Length - 1);
                var y =
                   visitMemberAccess
                   .Aggregate(m as object, (x, pi) => pi.GetValue(x));

                var lastone = chain.Last();
                lastone.SetValue(y, v);
            });
            @this.AddBuildItem(new PropertyBuildItem<T, V>(getter, setter, @this.Converter));
            return @this;
        }

        public static ModelBuilder<T> GetProperty<T, V>(this ModelBuilder<T> @this, Func<T, V> getter)
            where T : class, new()
        {
            @this.AddBuildItem(new GetPropertyBuildItem<T, V>(getter, @this.Converter));
            return @this;
        }

        public static ModelBuilder<T> Constant<T, V>(this ModelBuilder<T> @this, V v)
            where T : class, new()
            where V : struct
        {
            @this.AddBuildItem(new ConstantBuildItem<T, V>(v, @this.Converter));
            return @this;
        }

        // todo 型推論が働くようにするArrayを置き換えればいいと思う。IEnumerable とIListと配列くらいに対応しておけばいいかなあ
        public static ModelBuilder<T> Array<T, VModel>(
            this ModelBuilder<T> @this
            , string loopCountName
            , Expression<Func<T, IEnumerable<VModel>>> f
            , Action<ModelBuilder<VModel>> childModelBuilderFactory)
            where T : class, new()
            where VModel : class, new()
        {
            var getter = f.Compile();
            var chain = f.GetPropertyInfo().ToArray();
            // todo 自分でsetter定義するバージョンもほしいね。
            var setter = (Action<T, IEnumerable<VModel>>)((T m, IEnumerable<VModel> array) =>
            {
                // 最後の1つ手前メンバアクセスがx.y.zだとしたらyまで進む
                var visitMemberAccess = chain.Take(chain.Length - 1);
                var y =
                   visitMemberAccess
                   .Aggregate(m as object, (x, pi) => pi.GetValue(x));

                var lastone = chain.Last();
                lastone.SetValue(y, array);
            });

            // 配列の型のビルダーを生成
            var childModelBuilder = new ModelBuilder<VModel>();
            childModelBuilder.Converter = @this.Converter;
            childModelBuilderFactory(childModelBuilder);

            @this.AddBuildItem(new ArrayBuildItem<T, VModel>(childModelBuilder, getter, setter, loopCountName));
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
