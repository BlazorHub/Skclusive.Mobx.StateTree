using System;
using System.Collections.Generic;
using System.Linq;
using Skclusive.Mobx.StateTree;

namespace ClientSide.Models
{
    public class ModelTypes
    {
        public readonly static IObjectType<ITodoSnapshot, ITodo> TodoType = Types.
                        Object<ITodoSnapshot, ITodo>("Todo")
                       .Proxy(x => new TodoProxy(x))
                       .Snapshot(() => new TodoSnapshot())
                       .Mutable(o => o.Title, Types.String)
                       .Mutable(o => o.Done, Types.Boolean)
                       .Action(o => o.Toggle(), (o) => o.Done = !o.Done)
                       .Action<string>(o => o.Edit(null), (o, title) => o.Title = title)
                       .Action(o => o.Remove(), (o) => o.GetRoot<ITodoStore>().Remove(o));

        private readonly static IDictionary<string, Func<ITodo, bool>> FilterMapping = new Dictionary<string, Func<ITodo, bool>>
        {
            { "ShowAll", (_) => true },
            { "ShowActive", (todo) => !todo.Done },
            { "ShowCompleted", (todo) => todo.Done }
        };

        public readonly static IObjectType<ITodoStoreSnapshot, ITodoStore> StoreType = Types.
                        Object<ITodoStoreSnapshot, ITodoStore>("Store")
                       .Proxy(x => new TodoStoreProxy(x))
                       .Snapshot(() => new TodoStoreSnapshot())
                       .Mutable(o => o.Todos, Types.List(TodoType))
                       .Mutable(o => o.Filter, Types.Enumeration("Filter", "ShowAll", "ShowActive", "ShowCompleted"))
                       .View(o => o.TotalCount, Types.Int, (o) => o.Todos.Count())
                       .View(o => o.CompletedCount, Types.Int, (o) => o.Todos.Where(t => t.Done).Count())
                       .View(o => o.FilteredTodos, Types.List(TodoType), (o) => o.Todos.Where(FilterMapping[o.Filter]).ToList())
                       .View(o => o.ActiveCount, Types.Int, (o) => o.TotalCount - o.CompletedCount)
                       .Action((o) => o.CompleteAll(), (o) => o.Todos.Select(todo => todo.Done = true).ToList())
                       .Action((o) => o.ClearCompleted(), (o) =>
                       {
                           foreach (var completed in o.Todos.Where(todo => todo.Done).ToArray())
                               o.Todos.Remove(completed);
                       })
                       .Action<string>((o) => o.SetFilter(null), (o, filter) => o.Filter = filter)
                       .Action<ITodo>((o) => o.Remove(null), (o, x) => x.Destroy());
    }
}
