using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ShipStationAccess.DataService
{
	public class DataServiceQueryWrapper< TElement > : IDataServiceQuery< TElement >
	{
		private readonly DataServiceQuery< TElement > _dataServiceQuery;

		public DataServiceQueryWrapper( DataServiceQuery< TElement > dataServiceQuery )
		{
			this._dataServiceQuery = dataServiceQuery;
		}

		public IEnumerator< TElement > GetEnumerator()
		{
			return this._dataServiceQuery.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this._dataServiceQuery.GetEnumerator();
		}

		public Expression Expression
		{
			get { return this._dataServiceQuery.Expression; }
		}

		public Type ElementType
		{
			get { return this._dataServiceQuery.ElementType; }
		}

		public IQueryProvider Provider
		{
			get { return this._dataServiceQuery.Provider; }
		}

		public IDataServiceQuery< TElement > Expand< TTarget >( Expression< Func< TElement, TTarget > > navigationPropertyAccessor )
		{
			return new DataServiceQueryWrapper< TElement >( this._dataServiceQuery.Expand( navigationPropertyAccessor ) );
		}

		public IDataServiceQuery< TElement > Expand( string path )
		{
			return new DataServiceQueryWrapper< TElement >( this._dataServiceQuery.Expand( path ) );
		}

		public IDataServiceQuery< TElement > IncludeTotalCount()
		{
			return new DataServiceQueryWrapper< TElement >( this._dataServiceQuery.IncludeTotalCount() );
		}

		public IDataServiceQuery< TElement > AddQueryOption( string name, object value )
		{
			return new DataServiceQueryWrapper< TElement >( this._dataServiceQuery.AddQueryOption( name, value ) );
		}

		public IEnumerable< TElement > Execute()
		{
			return this._dataServiceQuery.Execute();
		}

		public Task< IEnumerable< TElement > > ExecuteAsync()
		{
			return Task< IEnumerable< TElement > >.Factory.FromAsync( this._dataServiceQuery.BeginExecute, this._dataServiceQuery.EndExecute, null );
		}
	}
}