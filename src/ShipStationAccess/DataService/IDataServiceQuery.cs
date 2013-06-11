using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ShipStationAccess.DataService
{
	public interface IDataServiceQuery< TElement > : IQueryable< TElement >, IEnumerable< TElement >, IQueryable, IEnumerable
	{
		IDataServiceQuery< TElement > Expand< TTarget >( Expression< Func< TElement, TTarget > > navigationPropertyAccessor );

		IDataServiceQuery< TElement > Expand( string path );

		IDataServiceQuery< TElement > IncludeTotalCount();

		IDataServiceQuery< TElement > AddQueryOption( string name, object value );

		IEnumerable< TElement > Execute();
		Task< IEnumerable< TElement > > ExecuteAsync();
	}
}