using FluentHub.Hub;
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
        public static IContextApplication<T> RegisterConverter<T,U>(
            this IContextApplication<T> @this
            , Func<IModelBuilder<U>, IModelConverter<U>> makeConverter)
            where U : class, T, new()
        {
            var modelBuilder = ToModelBuilder<U>();
            var converter = makeConverter(modelBuilder);
            @this.AddConverter(converter.ToBaseTypeConverter<U,T>());
            return @this;
        }

        public static IModelBuilder<T> ToModelBuilder<T>()
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
            builder.Converter.RegisterConverter(typeof(IEnumerable<byte>), m => (m as IEnumerable<byte>).ToArray(), data => data);
            builder.Converter.RegisterEqual(typeof(Array), (x, y) => Enumerable.SequenceEqual((x as Array).OfType<object>(), (y as Array).OfType<object>()));
            return builder;
        }

        public static IModelBuilder<T> ToModelBuilder<T>(this T _)
            where T : class, new()
        {
            return ToModelBuilder<T>();
        }

        public static IModelBuilder<T> ToBigEndian<T>(this IModelBuilder<T> @this)
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
        
        public static IModelBuilder<T> Init<T>(this IModelBuilder<T> @this, Action<T> init)
            where T : class, new()
        {
            @this.RegisterInit(init);
            return @this;
        }

        public static IBuildItemChain<T, ITaggedBuildItem<T>> Property<T, V, BuildItem>(
            this IBuildItemChain<T, BuildItem> @this
            , Expression<Func<T, V>> getterExpression)
            where T : class, new()
            where BuildItem : IBuildItem<T>
        {
            var getter = getterExpression.Compile();
            var setter = MakeSetter(getterExpression);

            var item =
                new PropertyBuildItem<T, V>(
                    getter
                    , setter
                    , @this.Converter);
            var newItem = @this.SetNext(item as ITaggedBuildItem<T>);
            return newItem;
        }

        public static IBuildItemChain<T, IBuildItem<T>> Property<T, V, BuildItem>(
            this IBuildItemChain<T, BuildItem> @this
            , Expression<Func<T, V>> getterExpression
            , Action<IModelBuilder<V>> childModelBuilderFactory)
            where T : class, new()
            where V : class, new()
            where BuildItem : IBuildItem<T>

        {
            var getter = getterExpression.Compile();
            var setter = MakeSetter(getterExpression);
            var childModelBuilder = @this.MakeChildModelBuilder(childModelBuilderFactory);

            var item = 
                new ProxyModelBuildItem<T, V>(
                    getter
                    , setter
                    , childModelBuilder);
            return @this.SetNext(item as IBuildItem<T>);
        }


        public static IBuildItemChain<T, ITaggedBuildItem<T>> GetProperty<T, V, BuildItem>(
            this IBuildItemChain<T, BuildItem> @this
            , Func<T, V> getter)
            where T : class, new()
            where BuildItem : IBuildItem<T>
        {
            var item = new GetPropertyBuildItem<T, V>(getter, @this.Converter) as ITaggedBuildItem<T>;
            return @this.SetNext(item);
        }

        public static IBuildItemChain<T, ITaggedBuildItem<T>> Constant<T, V, BuildItem>(
            this IBuildItemChain<T, BuildItem> @this
            , V v)
            where T : class, new()
            where BuildItem : IBuildItem<T>

        {
            var item = new ConstantBuildItem<T, V>(v, @this.Converter) as ITaggedBuildItem<T>;
            return @this.SetNext(item);
        }

        

        public static IBuildItemChain<T, IBuildItem<T>> Array<T, V, BuildItem>(
            this IBuildItemChain<T, BuildItem> @this
            , string loopCountName
            , Expression<Func<T, IEnumerable<V>>> getterExpression
            , Action<IModelBuilder<V>> childModelBuilderFactory)
            where T : class, new()
            where V : class, new()
            where BuildItem : IBuildItem<T>

        {
            var getter = getterExpression.Compile();
            var setter = MakeSetter(getterExpression);
            var childModelBuilder = @this.MakeChildModelBuilder(childModelBuilderFactory);

            var arrayMember = getterExpression.GetPropertyInfo().Last();
            var tryArrayConvert = MakeArrayConvert<V>(arrayMember.PropertyType);

            var item = 
                new ArrayBuildItem<T, V>(
                    childModelBuilder
                    , getter
                    , (m, xs) => setter(m, tryArrayConvert(xs))
                    , loopCountName) as IBuildItem<T>;
            return @this.SetNext(item);
        }

        public static IBuildItemChain<T, IBuildItem<T>> FixedArray<T, VModel, BuildItem>(
            this IBuildItemChain<T, BuildItem> @this
            , int loopCount
            , Expression<Func<T, IEnumerable<VModel>>> getterExpression
            , Action<IModelBuilder<VModel>> childModelBuilderFactory)
            where T : class, new()
            where VModel : class, new()
            where BuildItem : IBuildItem<T>

        {
            var getter = getterExpression.Compile();
            var setter = MakeSetter(getterExpression);
            var childModelBuilder = @this.MakeChildModelBuilder(childModelBuilderFactory);

            var arrayMember = getterExpression.GetPropertyInfo().Last();
            var tryArrayConvert = MakeArrayConvert<VModel>(arrayMember.PropertyType);

            var item = new FixedArrayBuildItem<T, VModel>(
                childModelBuilder
                , getter
                , (m, xs) => setter(m, tryArrayConvert(xs))
                , loopCount) as IBuildItem<T>;
            return @this.SetNext(item);
        }

        public static IBuildItemChain<T, ITaggedBuildItem<T>> AsTag<T>(
            this IBuildItemChain<T, ITaggedBuildItem<T>> @this, string tagName)
            where T : class, new()
        {
            (@this.Value as ITaggedBuildItem<T>).Tag = tagName;
            return @this;
        }

        public static IModelConverter<T> ToConverter<T, BuildItem>(
            this IBuildItemChain<T, BuildItem> @this)
            where T : class, new()
            where BuildItem : IBuildItem<T>

        {
            return new FluentModelConverter<T>(@this.Builder);
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
            else if (arrayType.Equals(typeof(Queue<VModel>)))
            {
                return xs => new Queue<VModel>(xs);
            }
            else
            {
                return xs => xs;
            }
        }

        static IModelBuilder<ChildModel> MakeChildModelBuilder<T, ChildModel, BuildItem>(
            this IBuildItemChain<T, BuildItem> @this
            , Action<IModelBuilder<ChildModel>> childModelBuilderFactory)
            where T : class, new()
            where ChildModel : class, new()
            where BuildItem : IBuildItem<T>

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
