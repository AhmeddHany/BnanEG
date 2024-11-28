using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;
public class TResult2
{
    public TResult2()
    {

    }
    public object Column { get; set; }  // نوع ديناميكي للعمود المحدد
    public int RowCount { get; set; }
}
public interface IGenric<T> where T : class
{
    T GetById(string id);
    Task<T> GetByIdAsync(string id);
    IEnumerable<T> GetAll(string[] includes = null);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> GetAllAsyncAsNoTrackingAsync();
    T Find(Expression<Func<T, bool>> predicate, string[] includes = null);
    Task<T> FindAsync(Expression<Func<T, bool>> predicate, string[] includes = null);
    IEnumerable<T> FindAll(Expression<Func<T, bool>> criteria, string[] includes = null);
    Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria, string[] includes = null);
    Task<List<T>> FindAllAsNoTrackingAsync(Expression<Func<T, bool>> predicate, string[] includes = null);

    Task<List<TResult>> FindAllWithSelectAsNoTrackingAsync<TResult>(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<TResult>> selectProjection, string[] includes = null);
    Task<List<TResult2>?> FindCountByColumnAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> columnSelector, string[] includes = null);
    Task<List<TResult2>?> FindCountByColumnAsync<TResult>(Expression<Func<T, object>> columnSelector, string[] includes = null);
    T Add(T entity);
    Task<T> AddAsync(T entity);
    //IEnumerable<T> AddRange(IEnumerable<T> entities);
    T Update(T entity);
    IEnumerable<T> UpdateRange(IEnumerable<T> entities);
    bool Delete(T entity);
    //void DeleteRange(IEnumerable<T> entities);
    void Attach(T entity);
    //void AttachRange(IEnumerable<T> entities);
    int Count();
    int Count(Expression<Func<T, bool>> criteria);
    Task<int> CountAsync();
    Task<int> CountAsync(Expression<Func<T, bool>> criteria);
    Task<IDbContextTransaction> BeginTransactionAsync();
    IQueryable<T> GetTableNoTracking();
    IQueryable<T> GetTableAsTracking();

}