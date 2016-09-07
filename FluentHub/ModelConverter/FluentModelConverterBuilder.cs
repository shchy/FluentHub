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
        public static IModelBuilder<T> ToModelBuilder<T>(Func<T> defaultModel)
        {
            var builder = new ModelBuilder<T>(defaultModel);
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
            return builder;
        }

        public static IModelBuilder<T> ToModelBuilder<T>(this T _)
            where T : class, new()
        {
            return ToModelBuilder<T>(()=>new T());
        }

        public static IModelBuilder<T> ToBigEndian<T>(this IModelBuilder<T> @this)
        {
            // default type converter
            if (BitConverter.IsLittleEndian == false)
            {
                return @this;
            }
            // override
            @this.Converter.RegisterConverter<short>(m => BitConverter.GetBytes((short)m).Reverse().ToArray(), data => BitConverter.ToInt16(data.Reverse().ToArray(), 0), () => sizeof(short));
            @this.Converter.RegisterConverter<int>(m => BitConverter.GetBytes((int)m).Reverse().ToArray(), data => BitConverter.ToInt32(data.Reverse().ToArray(), 0), () => sizeof(int));
            @this.Converter.RegisterConverter<long>(m => BitConverter.GetBytes((long)m).Reverse().ToArray(), data => BitConverter.ToInt64(data.Reverse().ToArray(), 0), () => sizeof(long));
            @this.Converter.RegisterConverter<ushort>(m => BitConverter.GetBytes((ushort)m).Reverse().ToArray(), data => BitConverter.ToUInt16(data.Reverse().ToArray(), 0), () => sizeof(ushort));
            @this.Converter.RegisterConverter<uint>(m => BitConverter.GetBytes((uint)m).Reverse().ToArray(), data => BitConverter.ToUInt32(data.Reverse().ToArray(), 0), () => sizeof(uint));
            @this.Converter.RegisterConverter<ulong>(m => BitConverter.GetBytes((ulong)m).Reverse().ToArray(), data => BitConverter.ToUInt64(data.Reverse().ToArray(), 0), () => sizeof(ulong));
            
            return @this;
        }

        public static IModelBuilder<TModel> RegisterTypeConverter<TModel, VModel>(this IModelBuilder<TModel> @this
            , Func<VModel, byte[]> toBytes, Func<byte[], VModel> toModel, Func<int> getSize)
        {
            @this.Converter.RegisterConverter<VModel>(
                m => toBytes((VModel)m)
                , bytes => toModel(bytes)
                , getSize);

            return @this;
        }

        public static IModelBuilder<T> Init<T>(this IModelBuilder<T> @this, Action<T> init)
        {
            @this.RegisterInit(init);
            return @this;
        }

        public static IBuildItemChain<T, ITaggedBuildItem<T>> Property<T, V>(
            this IBuildItemChain<T, IBuildItem<T>> @this
            , Expression<Func<T, V>> getterExpression)
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
            , Action<IModelBuilder<V>> childModelBuilderFactory
            , Func<V> defaultModel)
        {
            var getter = getterExpression.Compile();
            var setter = MakeSetter(getterExpression);
            var childModelBuilder = @this.MakeChildModelBuilder(childModelBuilderFactory, defaultModel);

            var item = 
                new ProxyModelBuildItem<T, V>(
                    getter
                    , setter
                    , childModelBuilder);
            return @this.SetNext(item);
        }

        public static IBuildItemChain<T, IBuildItem<T>> Property<T, V>(
            this IBuildItemChain<T, IBuildItem<T>> @this
            , Expression<Func<T, V>> getterExpression
            , Action<IModelBuilder<V>> childModelBuilderFactory)
            where V : class, new()
        {
            return @this.Property(getterExpression, childModelBuilderFactory, () => new V());
        }

        public static IBuildItemChain<T, IBuildItem<T>> ArrayProperty<T, V>(
            this IBuildItemChain<T, IBuildItem<T>> @this
            , string loopCountName
            , Expression<Func<T, IEnumerable<V>>> getterExpression
            , Action<IModelBuilder<V>> childModelBuilderFactory
            , Func<V> defaultModel)
        {
            var getter = getterExpression.Compile();
            var setter = MakeSetter(getterExpression);
            var childModelBuilder = @this.MakeChildModelBuilder(childModelBuilderFactory, defaultModel);

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
            , Expression<Func<T, IEnumerable<V>>> getterExpression
            , Action<IModelBuilder<V>> childModelBuilderFactory)
            where V : class, new()
        {
            return @this.ArrayProperty(loopCountName, getterExpression, childModelBuilderFactory, () => new V());
        }

        public static IBuildItemChain<T, IBuildItem<T>> ArrayProperty<T, V>(
            this IBuildItemChain<T, IBuildItem<T>> @this
            , string loopCountName
            , Expression<Func<T, IEnumerable<V>>> getterExpression)
            where V : struct
        {
            var getter = getterExpression.Compile();
            var setter = MakeSetter(getterExpression);
            var childModelBuilder = @this.MakeChildModelBuilder((IModelBuilder<Box<V>> builder)=>
            {
                builder.Property(vm => vm.Value);
            }, ()=> new Box<V>());

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
            , Action<IModelBuilder<VModel>> childModelBuilderFactory
            , Func<VModel> defaultModel)
        {
            var getter = getterExpression.Compile();
            var setter = MakeSetter(getterExpression);
            var childModelBuilder = @this.MakeChildModelBuilder(childModelBuilderFactory, defaultModel);

            var arrayMember = getterExpression.GetPropertyInfo().Last();
            var tryArrayConvert = MakeArrayConvert<VModel>(arrayMember.PropertyType);

            var item = new FixedArrayBuildItem<T, VModel>(
                childModelBuilder
                , getter
                , (m, xs) => setter(m, tryArrayConvert(xs))
                , loopCount
                , defaultModel);
            return @this.SetNext(item);
        }


        public static IBuildItemChain<T, IBuildItem<T>> FixedArrayProperty<T, VModel>(
            this IBuildItemChain<T, IBuildItem<T>> @this
            , int loopCount
            , Expression<Func<T, IEnumerable<VModel>>> getterExpression
            , Action<IModelBuilder<VModel>> childModelBuilderFactory)
            where VModel : class, new()
        {
            return @this.FixedArrayProperty(loopCount, getterExpression, childModelBuilderFactory, () => new VModel());
        }

        public static IBuildItemChain<T, IBuildItem<T>> FixedArrayProperty<T, VModel>(
            this IBuildItemChain<T, IBuildItem<T>> @this
            , int loopCount
            , Expression<Func<T, IEnumerable<VModel>>> getterExpression)
            where VModel : struct
        {
            var getter = getterExpression.Compile();
            var setter = MakeSetter(getterExpression);

            var childModelBuilder = @this.MakeChildModelBuilder((IModelBuilder<Box<VModel>> builder) =>
            {
                builder.Property(vm => vm.Value);
            }, ()=> new Box<VModel>());
            var arrayMember = getterExpression.GetPropertyInfo().Last();
            var tryArrayConvert = MakeArrayConvert<VModel>(arrayMember.PropertyType);

            var item =
                new FixedArrayBuildItem<T, Box<VModel>>(
                    childModelBuilder
                    , m => getter(m).Select(x => new Box<VModel> { Value = x })
                    , (m, xs) => setter(m, tryArrayConvert(xs.Select(x => x.Value).ToArray()))
                    , loopCount
                    , () => new Box<VModel>());
            return @this.SetNext(item);
        }
        
        public static IBuildItemChain<T, ITaggedBuildItem<T>> GetProperty<T, V>(
            this IBuildItemChain<T, IBuildItem<T>> @this
            , Func<T, V> getter)
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
            where VModel : struct
        {
            var loopCount = vList.Count();
            var childModelBuilder = @this.MakeChildModelBuilder((IModelBuilder<Box<VModel>> builder) =>
            {
                builder.Property(vm => vm.Value);
            }, ()=>new Box<VModel>());

            var item =
                new FixedArrayBuildItem<T, Box<VModel>>(
                    childModelBuilder
                    , m => vList.Select(x => new Box<VModel> { Value = x })
                    , (m, xs) => { }
                    , loopCount
                    , ()=> new Box<VModel>() );
            
            return @this.SetNext(item);
        }


        public static IBuildItemChain<T, ITaggedBuildItem<T>> AsTag<T>(
            this IBuildItemChain<T, ITaggedBuildItem<T>> @this, string tagName)
        {
            (@this.Value as ITaggedBuildItem<T>).Tag = tagName;
            return @this;
        }

        public static IModelConverter<T> ToConverter<T>(
            this IBuildItemChain<T, IBuildItem<T>> @this)
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
            , Action<IModelBuilder<ChildModel>> childModelBuilderFactory
            , Func<ChildModel> defaultModel)
        {
            var childModelBuilder = new ModelBuilder<ChildModel>(defaultModel);
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
