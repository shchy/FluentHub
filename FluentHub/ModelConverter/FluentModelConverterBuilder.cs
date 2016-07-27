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
            return new ModelBuilder<T>();
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
            @this.AddBuildItem(new PropertyBuildItem<T, V>(getter, setter));
            return @this;
        }

        public static ModelBuilder<T> GetProperty<T, V>(this ModelBuilder<T> @this, Func<T, V> getter)
            where T : class, new()
        {
            @this.AddBuildItem(new GetPropertyBuildItem<T, V>(getter));
            return @this;
        }

        public static ModelBuilder<T> Constant<T, V>(this ModelBuilder<T> @this, V v)
            where T : class, new()
            where V : struct
        {
            @this.AddBuildItem(new ConstantBuildItem<T, V>(v));
            return @this;
        }

        // todo 型推論が働くようにするArrayを置き換えればいいと思う。IEnumerable とIListと配列くらいに対応しておけばいいかなあ
        public static ModelBuilder<T> Array<T, Array, VModel>(
            this ModelBuilder<T> @this
            , string loopCountName
            , Expression<Func<T, Array>> f
            , Action<ModelBuilder<VModel>> childModelBuilderFactory)
            where T : class, new()
            where Array : IEnumerable<VModel>
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
            childModelBuilderFactory(childModelBuilder);

            @this.AddBuildItem(new ArrayBuildItem<T, Array, VModel>(childModelBuilder, getter, setter, loopCountName));
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
