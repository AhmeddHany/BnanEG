using Bnan.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace Bnan.Inferastructure.Repository
{

    public class BaseRepository<T> : IGenric<T> where T : class
    {
        protected BnanSCContext _context;


        public BaseRepository(BnanSCContext context
           )
        {
            _context = context;
        }

        public IEnumerable<T> GetAll(string[] includes = null)
        {
            try
            {
                IQueryable<T> query = _context.Set<T>();

                if (includes != null)
                {
                    foreach (var include in includes)
                        query = query.Include(include);
                }
                return query.ToList();
            }
            catch (Exception)
            {

                return null;
            }
        }




        public async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                return await _context.Set<T>().ToListAsync();
            }
            catch (Exception)
            {

                return null;
            }
        }
        public async Task<IEnumerable<T>> GetAllAsyncAsNoTrackingAsync()
        {
            return await _context.Set<T>().AsNoTracking().ToListAsync();
        }
        public T GetById(string id)
        {
            try
            {
                return _context.Set<T>().Find(id);
            }
            catch (Exception)
            {

                return null;
            }
        }

        public async Task<T> GetByIdAsync(string id)
        {
            try
            {
                return await _context.Set<T>().FindAsync(id);
            }
            catch (Exception)
            {

                return null;
            }
        }

        public T Find(Expression<Func<T, bool>> predicate, string[] includes = null)
        {
            try
            {
                IQueryable<T> query = _context.Set<T>();
                query = query.Where(predicate);

                if (includes != null)
                {
                    foreach (var include in includes)
                        query = query.Include(include);
                }
                return query.FirstOrDefault(predicate);
            }
            catch (Exception)
            {

                return null;
            }
        }

        public IEnumerable<T> FindAll(Expression<Func<T, bool>> criteria, string[] includes = null)
        {

            try
            {
                IQueryable<T> query = _context.Set<T>();
                query = query.Where(criteria);

                if (includes != null)
                {
                    foreach (var include in includes)
                        query = query.Include(include);
                }
                return query.Where(criteria).ToList();
            }
            catch (Exception)
            {

                return null;
            }
        }


        public T Add(T entity)
        {
            try
            {

                _context.Set<T>().Add(entity);
                return entity;

            }
            catch (Exception)
            {

                return null;
            }
        }
        public async Task<T> AddAsync(T entity)
        {
            try
            {

                await _context.Set<T>().AddAsync(entity);
                return entity;

            }
            catch (Exception)
            {

                return null;
            }
        }

        public IEnumerable<T> AddRange(IEnumerable<T> entities)
        {
            try
            {

                _context.Set<T>().AddRange(entities);
                return entities;

            }
            catch (Exception)
            {

                return null;
            }
        }

        public T Update(T entity)
        {
            _context.Update(entity);
            return entity;
        }
        public IEnumerable<T> UpdateRange(IEnumerable<T> entities)
        {
            _context.UpdateRange(entities);
            return entities;
        }
        public bool Delete(T entity)
        {
            try
            {
                _context.Set<T>().Remove(entity);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            _context.Set<T>().RemoveRange(entities);
        }

        public void Attach(T entity)
        {
            _context.Set<T>().Attach(entity);
        }

        public void AttachRange(IEnumerable<T> entities)
        {
            _context.Set<T>().AttachRange(entities);
        }

        public int Count()
        {
            try
            {
                return _context.Set<T>().Count();
            }
            catch (Exception)
            {

                return 0;
            }

        }

        public int Count(Expression<Func<T, bool>> criteria)
        {
            try
            {
                return _context.Set<T>().Count(criteria);
            }
            catch (Exception)
            {

                return 0;
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.Set<T>().CountAsync();
            }
            catch (Exception)
            {
                return 0;

            }
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> criteria)
        {
            try
            {
                return await _context.Set<T>().CountAsync(criteria);
            }
            catch (Exception)
            {

                return 0;
            }
        }

        public async Task<T> FindAsync(Expression<Func<T, bool>> predicate, string[] includes = null)
        {
            try
            {
                IQueryable<T> query = _context.Set<T>();
                query = query.Where(predicate);

                if (includes != null)
                {
                    foreach (var include in includes)
                        query = query.Include(include);
                }
                return await query.FirstOrDefaultAsync(predicate);
            }
            catch (Exception)
            {

                return null;
            }
        }

        public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria, string[] includes = null)
        {
            try
            {
                IQueryable<T> query = _context.Set<T>();
                query = query.Where(criteria);

                if (includes != null)
                {
                    foreach (var include in includes)
                        query = query.Include(include);
                }
                return await query.Where(criteria).ToListAsync();
            }
            catch (Exception)
            {

                return null;
            }
        }
        public async Task<List<T>> FindAllAsNoTrackingAsync(Expression<Func<T, bool>> predicate, string[] includes = null)
        {
            IQueryable<T> query = _context.Set<T>();

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            // Use AsNoTracking for this specific method
            query = query.AsNoTracking();
            return await query.Where(predicate).ToListAsync();
        }

        public async Task<List<TResult>> FindAllWithSelectAsNoTrackingAsync<TResult>(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IQueryable<TResult>> selectProjection,
            string[] includes = null)
        {
            IQueryable<T> query = _context.Set<T>().AsNoTracking(); 

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include); 
                }
            }

            // تطبيق دالة Select لتحديد الأعمدة المطلوبة
            var resultQuery = selectProjection(query);

            return await resultQuery.ToListAsync(); 
        }

        public async Task<List<TResult2>> FindCountByColumnAsync<TResult>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, object>> columnSelector,  // العمود الذي سيتم التجميع عليه
            string[] includes = null)
        {
            IQueryable<T> query = _context.Set<T>().AsNoTracking(); 

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include); 
                }
            }

            // التجميع بناءً على العمود المحدد
            var resultQuery = query
                .GroupBy(columnSelector) // تجميع النتائج بناءً على العمود المحدد
                .Select(g => new TResult2
                {
                    Column = g.Key,           // إرجاع القيمة من العمود المحدد
                    RowCount = g.Count()      // حساب عدد الصفوف في هذه المجموعة
                });

            return await resultQuery.ToListAsync(); 
        }


        public async Task<List<TResult2>> FindCountByColumnAsync<TResult>(
            Expression<Func<T, object>> columnSelector,  // العمود الذي سيتم التجميع عليه
            string[] includes = null)
        {
            IQueryable<T> query = _context.Set<T>().AsNoTracking();

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            // التجميع بناءً على العمود المحدد
            var resultQuery = query
                .GroupBy(columnSelector) // تجميع النتائج بناءً على العمود المحدد
                .Select(g => new TResult2
                {
                    Column = g.Key,           // إرجاع القيمة من العمود المحدد
                    RowCount = g.Count()      // حساب عدد الصفوف في هذه المجموعة
                });

            return await resultQuery.ToListAsync();
        }

        public IQueryable<T> GetTableAsTracking()
        {
            return _context.Set<T>().AsQueryable();

        }
        public IQueryable<T> GetTableNoTracking()
        {
            return _context.Set<T>().AsNoTracking().AsQueryable();
        }
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }



    }
}
