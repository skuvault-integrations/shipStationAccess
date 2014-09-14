using System.Data.Services.Client;
using System.Linq;
using System.Linq.Expressions;

namespace ShipStationAccess.V1.DataService
{
	public class DataServiceQueryProviderWrapper : IQueryProvider
	{
		private readonly IQueryProvider _wrappedProvider;

		public DataServiceQueryProviderWrapper( IQueryProvider wrappedProvider )
		{
			this._wrappedProvider = wrappedProvider;
		}

		public IQueryable CreateQuery( Expression expression )
		{
			var queryable = this._wrappedProvider.CreateQuery( expression );
			if( !( queryable is DataServiceQuery ) )
				return queryable;

			var genericTypes = queryable.GetType().GetGenericArguments();
			if( genericTypes.Length == 0 )
				return queryable;

			var wrapper = typeof( DataServiceQueryWrapper< > ).MakeGenericType( genericTypes );
			var constructor = wrapper.GetConstructors()[ 0 ];
			if( constructor == null )
				return queryable;

			return ( IQueryable )constructor.Invoke( new object[] { queryable } );
		}

		public IQueryable< TElement > CreateQuery< TElement >( Expression expression )
		{
			return new DataServiceQueryWrapper< TElement >( ( DataServiceQuery< TElement > )this._wrappedProvider.CreateQuery< TElement >( expression ) );
		}

		public object Execute( Expression expression )
		{
			return this._wrappedProvider.Execute( expression );
		}

		public TResult Execute< TResult >( Expression expression )
		{
			return this._wrappedProvider.Execute< TResult >( expression );
		}
	}
}