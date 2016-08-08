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
            builder.Converter.RegisterConverter<bool>(m => BitConverter.GetBytes((bool)m), data => BitConverter.ToBoolean(data, 0), () => sizeof(bool));
            builder.Converter.RegisterConverter<char>(m => BitConverter.GetBytes((char)m), data => BitConverter.ToChar(data, 0), () => sizeof(char));
            builder.Converter.RegisterConverter<byte>(m => new[] { (byte)m }, data => data.First(), () => sizeof(byte));
            builder.Converter.RegisterConverter<short>(m => BitConverter.GetBytes((short)m), data => BitConverter.ToInt16(data, 0), () => sizeof(short));
            builder.Converter.RegisterConverter<int>(m => BitConverter.GetBytes((int)m), data => BitConverter.ToInt32(data, 0), () => sizeof(int));
            builder.Converter.RegisterConverter<long>(m => BitConverter.GetBytes((long)m), data => BitConverter.ToInt64(data, 0), () => sizeof(long));
            builder.Converter.RegisterConverter<ushort>(m => BitConverter.GetBytes((ushort)m), data => BitConverter.ToUInt16(data, 0), () => sizeof(ushort));
            builder.Converter.RegisterConverter<uint>(m => BitConverter.GetBytes((uint)m), data => BitConverter.ToUInt32(data, 0), () => sizeof(uint));
            builder.Converter.RegisterConverter<ulong>(m => BitConverter.GetBytes((ulong)m), data => BitConverter.ToUInt64(data, 0), () => sizeof(ulong));
            builder.Converter.RegisterConverter<float>(m => BitConverter.GetBytes((float)m), data => BitConverter.ToSingle(data, 0), () => sizeof(float));
            builder.Converter.RegisterConverter<double>(m => BitConverter.GetBytes((double)m), data => BitConverter.ToDouble(data, 0), () => sizeof(double));
            //builder.Converter.RegisterConverter<byte[]>(m => (byte[])m, data => data);
            //builder.Converter.RegisterConverter<IEnumerable<byte>>(m => (m as IEnumerable<byte>).ToArray(), data => data);
            //builder.Converter.RegisterEqual<Array>((x, y) => Enumerable.SequenceEqual((x as Array).OfType<object>(), (y as Array).OfType<object>()));
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
            @this.Converter.RegisterConverter<bool>(m => BitConverter.GetBytes((bool)m).Reverse().ToArray(), data => BitConverter.ToBoolean(data.Reverse().ToArray(), 0), () => sizeof(bool));
            @this.Converter.RegisterConverter<char>(m => BitConverter.GetBytes((char)m).Reverse().ToArray(), data => BitConverter.ToChar(data.Reverse().ToArray(), 0), () => sizeof(char));
            @this.Converter.RegisterConverter<short>(m => BitConverter.GetBytes((short)m).Reverse().ToArray(), data => BitConverter.ToInt16(data.Reverse().ToArray(), 0), () => sizeof(short));
            @this.Converter.RegisterConverter<int>(m => BitConverter.GetBytes((int)m).Reverse().ToArray(), data => BitConverter.ToInt32(data.Reverse().ToArray(), 0), () => sizeof(int));
            @this.Converter.RegisterConverter<long>(m => BitConverter.GetBytes((long)m).Reverse().ToArray(), data => BitConverter.ToInt64(data.Reverse().ToArray(), 0), () => sizeof(long));
            @this.Converter.RegisterConverter<ushort>(m => BitConverter.GetBytes((ushort)m).Reverse().ToArray(), data => BitConverter.ToUInt16(data.Reverse().ToArray(), 0), () => sizeof(ushort));
            @this.Converter.RegisterConverter<uint>(m => BitConverter.GetBytes((uint)m).Reverse().ToArray(), data => BitConverter.ToUInt32(data.Reverse().ToArray(), 0), () => sizeof(uint));
            @this.Converter.RegisterConverter<ulong>(m => BitConverter.GetBytes((ulong)m).Reverse().ToArray(), data => BitConverter.ToUInt64(data.Reverse().ToArray(), 0), () => sizeof(ulong));
            @this.Converter.RegisterConverter<float>(m => BitConverter.GetBytes((float)m).Reverse().ToArray(), data => BitConverter.ToSingle(data.Reverse().ToArray(), 0), () => sizeof(float));
            @this.Converter.RegisterConverter<double>(m => BitConverter.GetBytes((double)m).Reverse().ToArray(), data => BitConverter.ToDouble(data.Reverse().ToArray(), 0), () => sizeof(double));
            
            return @this;
        }

        public static IModelBuilder<TModel> RegisterTypeConverter<TModel, VModel>(this IModelBuilder<TModel> @this
            , Func<VModel, byte[]> toBytes, Func<byte[], VModel> toModel, Func<int> getSize)
            where TModel : class, new()
        {
            @this.Converter.RegisterConverter<VModel>(
                m => toBytes((VModel)m)
                , bytes => toModel(bytes)
                , getSize);

            return @this;
        }

        public static IModelBuilder<T> Init<T>(this IModelBuilder<T> @this, Action<T> init)
            where T : class, new()
        {
            @this.RegisterInit(init);
            return @this;
        }

        public static IBuildItemChain<T, ITaggedBuildItem<T>> Property<T, V>(
            this IBuildItemChain<T, IBuildItem<T>> @this
            , Expression<Func<T, V>> getterExpression)
            where T : class, new()
        {
            var getter = getterExpression.Compile();
            var setter = MakeSetter(getterExpression);

            var item =
                new PropertyBuildItem<T, V>(
                    getter
                    , setter
                    , @this.Converter
                    , @this.Converter.GetTypeSize<V>());
            var newItem = @this.SetNext(item);
            return newItem;
        }

        public static IBuildItemChain<T, ITaggedBuildItem<T>> Property<T, V>(
            this IBuildItemChain<T, IBuildItem<T>> @this
            , Func<T, V> getter
            , Action<T, V> setter)
            where T : class, new()
        {
            var item =
                new PropertyBuildItem<T, V>(
                    getter
                    , setter
                    , @this.Converter
                    , @this.Converter.GetTypeSize<V>());
            var newItem = @this.SetNext(item);
            return newItem;
        }

        public static IBuildItemChain<T, IBuildItem<T>> Property<T, V>(
            this IBuildItemChain<T, IBuildItem<T>> @this
            , Expression<Func<T, V>> getterExpression
            , Action<IModelBuilder<V>> childModelBuilderFactory)
            where T : class, new()
            where V : class, new()
        {
            var getter = getterExpression.Compile();
            var setter = MakeSetter(getterExpression);
            var childModelBuilder = @this.MakeChildModelBuilder(childModelBuilderFactory);

            var item = 
                new ProxyModelBuildItem<T, V>(
                    getter
                    , setter
                    , childModelBuilder);
            return @this.SetNext(item);
        }

        public static IBuildItemChain<T, IBuildItem<T>> ArrayProperty<T, V>(
            this IBuildItemChain<T, IBuildItem<T>> @this
            , string loopCountName
            , Expression<Func<T, IEnumerable<V>>> getterExpression
            , Action<IModelBuilder<V>> childModelBuilderFactory)
            where T : class, new()
            where V : class, new()
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
                    , loopCountName);
            return @this.SetNext(item);
        }

        public static IBuildItemChain<T, IBuildItem<T>> ArrayProperty<T, V>(
            this IBuildItemChain<T, IBuildItem<T>> @this
            , string loopCountName
            , Expression<Func<T, IEnumerable<V>>> getterExpression)
            where T : class, new()
            where V : struct
        {
            var getter = getterExpression.Compile();
            var setter = MakeSetter(getterExpression);
            var childModelBuilder = @this.MakeChildModelBuilder((IModelBuilder<Box<V>> builder)=>
            {
                builder.Property(vm => vm.Value);
            });

            var arrayMember = getterExpression.GetPropertyInfo().Last();
            var tryArrayConvert = MakeArrayConvert<V>(arrayMember.PropertyType);

            var item =
                new ArrayBuildItem<T, Box<V>>(
                    childModelBuilder
                    , m => getter(m).Select(x=>new Box<V> { Value = x })
                    , (m, xs) => setter(m, tryArrayConvert(xs.Select(x=>x.Value).ToArray()))
                    , loopCountName);
            return @this.SetNext(item);
        }

        class Box<T>
            where T : struct
        {
            public T Value { get; set; }
        }

        public static IBuildItemChain<T, IBuildItem<T>> FixedArrayProperty<T, VModel>(
            this IBuildItemChain<T, IBuildItem<T>> @this
            , int loopCount
            , Expression<Func<T, IEnumerable<VModel>>> getterExpression
            , Action<IModelBuilder<VModel>> childModelBuilderFactory)
            where T : class, new()
            where VModel : class, new()
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
                , loopCount);
            return @this.SetNext(item);
        }

        // todo ArrayPropertyのBoxを流用できるんじゃない？
        public static IBuildItemChain<T, IBuildItem<T>> FixedArrayProperty<T, VModel>(
            this IBuildItemChain<T, IBuildItem<T>> @this
            , int loopCount
            , Expression<Func<T, IEnumerable<VModel>>> getterExpression)
            where T : class, new()
            where VModel : struct
        {
            var getter = getterExpression.Compile();
            var setter = MakeSetter(getterExpression);

            var arrayMember = getterExpression.GetPropertyInfo().Last();
            var tryArrayConvert = MakeArrayConvert<VModel>(arrayMember.PropertyType);

            var item = @this.MakeStructArrayBuildItem(loopCount
                , m => getter(m)
                , (m, v) => setter(m, tryArrayConvert(v))
                , (chain, i) => chain.Property(list => list[i], (list, v) => list[i] = v));
            return @this.SetNext(item);
        }
        
        public static IBuildItemChain<T, ITaggedBuildItem<T>> GetProperty<T, V>(
            this IBuildItemChain<T, IBuildItem<T>> @this
            , Func<T, V> getter)
            where T : class, new()
        {
            var item =
                new GetPropertyBuildItem<T, V>(
                    getter
                    , @this.Converter
                    , @this.Converter.GetTypeSize<V>());
            return @this.SetNext(item);
        }

        public static IBuildItemChain<T, ITaggedBuildItem<T>> Constant<T, V>(
            this IBuildItemChain<T, IBuildItem<T>> @this
            , V v)
            where T : class, new()
        {
            var item =
                new ConstantBuildItem<T, V>(v
                , @this.Converter
                , @this.Converter.GetTypeSize<V>());
            return @this.SetNext(item);
        }

        public static IBuildItemChain<T, IBuildItem<T>> Constant<T, VModel>(
            this IBuildItemChain<T, IBuildItem<T>> @this
            , VModel[] vList)
            where T : class, new()
            where VModel : struct
        {
            var loopCount = vList.Count();
            var item = @this.MakeStructArrayBuildItem(loopCount
                , m => vList
                , (m, v) => { }
                , (chain,i) => chain.Constant(vList[i]));
            return @this.SetNext(item);
        }


        public static IBuildItemChain<T, ITaggedBuildItem<T>> AsTag<T>(
            this IBuildItemChain<T, ITaggedBuildItem<T>> @this, string tagName)
            where T : class, new()
        {
            (@this.Value as ITaggedBuildItem<T>).Tag = tagName;
            return @this;
        }

        public static IModelConverter<T> ToConverter<T>(
            this IBuildItemChain<T, IBuildItem<T>> @this)
            where T : class, new()
        {
            return new FluentModelConverter<T>(@this.Builder);
        }

        public static IModelConverter<P> ToBaseTypeConverter<T,P>(
            this IModelConverter<T> @this)
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
            else if (arrayType.Equals(typeof(string)))
            {
                return xs =>
                {
                    var a = new string(xs.OfType<char>().ToArray());
                    return a.AsEnumerable() as IEnumerable<VModel>;
                };
            }
            else
            {
                return xs => xs;
            }
        }

        static IModelBuilder<ChildModel> MakeChildModelBuilder<T, ChildModel>(
            this IBuildItemChain<T, IBuildItem<T>> @this
            , Action<IModelBuilder<ChildModel>> childModelBuilderFactory)
            where T : class, new()
            where ChildModel : class, new()
        {
            var childModelBuilder = new ModelBuilder<ChildModel>();
            childModelBuilder.Converter = @this.Converter;
            childModelBuilderFactory(childModelBuilder);
            return childModelBuilder;
        }

        static IBuildItem<T> MakeStructArrayBuildItem<T, VModel>(
            this IBuildItemChain<T, IBuildItem<T>> @this
            , int loopCount
            , Func<T, IEnumerable<VModel>> getter
            , Action<T, IEnumerable<VModel>> setter
            , Func<IBuildItemChain<List<VModel>, IBuildItem<List<VModel>>>, int, IBuildItemChain<List<VModel>, IBuildItem<List<VModel>>>> build)
            where T : class, new()
            where VModel : struct
        {
            var childModelBuilder = @this.MakeChildModelBuilder((IModelBuilder<List<VModel>> builder) =>
            {
                // 固定長で初期化
                builder.Init(list => list.AddRange(Enumerable.Range(0, loopCount).Select(_ => default(VModel))));
                // 各要素のBuildItemを生成
                var seed = null as IBuildItemChain<List<VModel>, IBuildItem<List<VModel>>>;

                for (int i = 0; i < loopCount; i++)
                {
                    var li = i;
                    if (seed == null)
                    {
                        seed = build(builder, li);//.Property(list => list[li], (list, v) => list[li] = v);
                    }
                    else
                    {
                        seed = build(seed, li);//.Property(list => list[li], (list, v) => list[li] = v);
                    }
                }
            });

            var item =
                new ProxyModelBuildItem<T, List<VModel>>(
                    m => getter(m).ToList()
                    , (m, v) => setter(m, v)
                    , childModelBuilder);
            return item;
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
