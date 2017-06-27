using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SlackDotNet.WebApi
{
    internal interface IQueryBuilder
    {
        IReadOnlyList<KeyValuePair<string, string>> Items { get; }

        IQueryBuilder Append(string key, string value);
        IQueryBuilder Clear();
    }

    internal class EmptyQueryBuilder : IQueryBuilder
    {
        public static readonly IQueryBuilder Instance = new EmptyQueryBuilder();

        public IReadOnlyList<KeyValuePair<string, string>> Items => new KeyValuePair<string, string>[0];

        private EmptyQueryBuilder()
        {
        }

        public IQueryBuilder Append(string key, string value)
        {
            throw new InvalidOperationException($"The '{nameof(EmptyQueryBuilder)}' is readonly and is to remain empty.");
        }

        public IQueryBuilder Clear()
        {
            return this;
        }
    }

    internal class QueryBuilder : IQueryBuilder
    {
        public static readonly IQueryBuilder Shared = new QueryBuilder();

        private readonly List<KeyValuePair<string, string>> items;
        public IReadOnlyList<KeyValuePair<string, string>> Items { get; }

        public QueryBuilder()
            : this(16)
        {
        }

        public QueryBuilder(int capacity)
        {
            items = new List<KeyValuePair<string, string>>(capacity);
            Items = new ReadOnlyCollection<KeyValuePair<string, string>>(items);
        }

        public IQueryBuilder Append(string key, string value)
        {
            items.Add(new KeyValuePair<string, string>(key, value));
            return this;
        }

        public IQueryBuilder Clear()
        {
            items.Clear();
            return this;
        }
    }
}
