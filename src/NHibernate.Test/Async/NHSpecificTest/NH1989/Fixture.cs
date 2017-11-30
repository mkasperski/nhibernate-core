﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Data.Common;

using NUnit.Framework;

using NHibernate.Cache;
using NHibernate.Cfg;
using NHibernate.Criterion;
using NHibernate.Engine;

namespace NHibernate.Test.NHSpecificTest.NH1989
{
	using System.Threading.Tasks;
	using System.Threading;
	[TestFixture]
	public class FixtureAsync : BugTestCase
	{
		protected override bool AppliesTo(ISessionFactoryImplementor factory)
		{
			return factory.ConnectionProvider.Driver.SupportsMultipleQueries;
		}

		protected override void Configure(Configuration configuration)
		{
			base.Configure(configuration);
			configuration.Properties[Environment.CacheProvider] = typeof(HashtableCacheProvider).AssemblyQualifiedName;
			configuration.Properties[Environment.UseQueryCache] = "true";
		}

		protected override void OnSetUp()
		{
			// Clear cache at each test.
			RebuildSessionFactory();
		}

		protected override void OnTearDown()
		{
			using (ISession s = OpenSession())
			using (ITransaction tx = s.BeginTransaction())
			{
				s.Delete("from User");
				tx.Commit();
			}
		}

		private static async Task DeleteObjectsOutsideCacheAsync(ISession s, CancellationToken cancellationToken = default(CancellationToken))
		{
			using (var cmd = s.Connection.CreateCommand())
			{
				cmd.CommandText = "DELETE FROM UserTable";
				await (cmd.ExecuteNonQueryAsync(cancellationToken));
			}
		}

		[Test]
		public async Task SecondLevelCacheWithSingleCacheableFutureAsync()
		{
			using (ISession s = OpenSession())
			using (ITransaction tx = s.BeginTransaction())
			{
				User user = new User() { Name = "test" };
				await (s.SaveAsync(user));
				await (tx.CommitAsync());
			}

			using (ISession s = OpenSession())
			{
				// Query results should be cached
				User user =
					await (s.CreateCriteria<User>()
						.Add(Restrictions.NaturalId().Set("Name", "test"))
						.SetCacheable(true)
						.FutureValue<User>()
						.GetValueAsync());

				Assert.That(user, Is.Not.Null);

				await (DeleteObjectsOutsideCacheAsync(s));
			}

			using (ISession s = OpenSession())
			{
				User user =
					await (s.CreateCriteria<User>()
						.Add(Restrictions.NaturalId().Set("Name", "test"))
						.SetCacheable(true)
						.FutureValue<User>()
						.GetValueAsync());

				Assert.That(user, Is.Not.Null,
					"entity not retrieved from cache");
			}
		}

		[Test]
		public async Task SecondLevelCacheWithDifferentRegionsFutureAsync()
		{
			using (ISession s = OpenSession())
			using (ITransaction tx = s.BeginTransaction())
			{
				User user = new User() { Name = "test" };
				await (s.SaveAsync(user));
				await (tx.CommitAsync());
			}

			using (ISession s = OpenSession())
			{
				// Query results should be cached
				User user =
					await (s.CreateCriteria<User>()
						.Add(Restrictions.NaturalId().Set("Name", "test"))
						.SetCacheable(true)
						.SetCacheRegion("region1")
						.FutureValue<User>()
						.GetValueAsync());

				Assert.That(user, Is.Not.Null);

				await (DeleteObjectsOutsideCacheAsync(s));
			}

			using (ISession s = OpenSession())
			{
				User user =
					await (s.CreateCriteria<User>()
						.Add(Restrictions.NaturalId().Set("Name", "test"))
						.SetCacheable(true)
						.SetCacheRegion("region2")
						.FutureValue<User>()
						.GetValueAsync());

				Assert.That(user, Is.Null,
					"entity from different region should not be retrieved");
			}
		}

		[Test]
		public async Task SecondLevelCacheWithMixedCacheableAndNonCacheableFutureAsync()
		{
			using (ISession s = OpenSession())
			using (ITransaction tx = s.BeginTransaction())
			{
				User user = new User() { Name = "test" };
				await (s.SaveAsync(user));
				await (tx.CommitAsync());
			}

			using (ISession s = OpenSession())
			{
				// cacheable Future, not evaluated yet
				IFutureValue<User> userFuture =
					s.CreateCriteria<User>()
						.Add(Restrictions.NaturalId().Set("Name", "test"))
						.SetCacheable(true)
						.FutureValue<User>();

				// non cacheable Future causes batch to be non-cacheable
				int count =
					await (s.CreateCriteria<User>()
						.SetProjection(Projections.RowCount())
						.FutureValue<int>()
						.GetValueAsync());

				Assert.That(await (userFuture.GetValueAsync()), Is.Not.Null);
				Assert.That(count, Is.EqualTo(1));

				await (DeleteObjectsOutsideCacheAsync(s));
			}

			using (ISession s = OpenSession())
			{
				IFutureValue<User> userFuture =
					s.CreateCriteria<User>()
						.Add(Restrictions.NaturalId().Set("Name", "test"))
						.SetCacheable(true)
						.FutureValue<User>();

				int count =
					await (s.CreateCriteria<User>()
						.SetProjection(Projections.RowCount())
						.FutureValue<int>()
						.GetValueAsync());

				Assert.That(await (userFuture.GetValueAsync()), Is.Null,
					"query results should not come from cache");
			}
		}

		[Test]
		public async Task SecondLevelCacheWithMixedCacheRegionsFutureAsync()
		{
			using (ISession s = OpenSession())
			using (ITransaction tx = s.BeginTransaction())
			{
				User user = new User() { Name = "test" };
				await (s.SaveAsync(user));
				await (tx.CommitAsync());
			}

			using (ISession s = OpenSession())
			{
				// cacheable Future, not evaluated yet
				IFutureValue<User> userFuture =
					s.CreateCriteria<User>()
						.Add(Restrictions.NaturalId().Set("Name", "test"))
						.SetCacheable(true)
						.SetCacheRegion("region1")
						.FutureValue<User>();

				// different cache-region causes batch to be non-cacheable
				int count =
					await (s.CreateCriteria<User>()
						.SetProjection(Projections.RowCount())
						.SetCacheable(true)
						.SetCacheRegion("region2")
						.FutureValue<int>()
						.GetValueAsync());

				Assert.That(await (userFuture.GetValueAsync()), Is.Not.Null);
				Assert.That(count, Is.EqualTo(1));

				await (DeleteObjectsOutsideCacheAsync(s));
			}

			using (ISession s = OpenSession())
			{
				IFutureValue<User> userFuture =
					s.CreateCriteria<User>()
						.Add(Restrictions.NaturalId().Set("Name", "test"))
						.SetCacheable(true)
						.SetCacheRegion("region1")
						.FutureValue<User>();

				int count =
					await (s.CreateCriteria<User>()
						.SetProjection(Projections.RowCount())
						.SetCacheable(true)
						.SetCacheRegion("region2")
						.FutureValue<int>()
						.GetValueAsync());

				Assert.That(await (userFuture.GetValueAsync()), Is.Null,
					"query results should not come from cache");
			}
		}

		[Test]
		public async Task SecondLevelCacheWithSingleCacheableQueryFutureAsync()
		{
			using (ISession s = OpenSession())
			using (ITransaction tx = s.BeginTransaction())
			{
				User user = new User() { Name = "test" };
				await (s.SaveAsync(user));
				await (tx.CommitAsync());
			}

			using (ISession s = OpenSession())
			{
				// Query results should be cached
				User user =
					await (s.CreateQuery("from User u where u.Name='test'")
						.SetCacheable(true)
						.FutureValue<User>()
						.GetValueAsync());

				Assert.That(user, Is.Not.Null);

				await (DeleteObjectsOutsideCacheAsync(s));
			}

			using (ISession s = OpenSession())
			{
				User user =
					await (s.CreateQuery("from User u where u.Name='test'")
						.SetCacheable(true)
						.FutureValue<User>()
						.GetValueAsync());

				Assert.That(user, Is.Not.Null,
					"entity not retrieved from cache");
			}
		}
	}
}