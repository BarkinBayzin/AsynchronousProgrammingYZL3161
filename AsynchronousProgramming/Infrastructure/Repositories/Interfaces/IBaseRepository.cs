using AsynchronousProgramming.Models.Entities.Abstract;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AsynchronousProgramming.Infrastructure.Repositories.Interfaces
{
    //Asenkron Programming (Eş zamansız programlama)
    //Bu güne kadar yaptığımız çalışmalarda senkro programlama (eş zamanlı programlama) yapıyorduk. Bu yüzden bir iş (business) yapıldığında kullanıcı arayüzü (UI - User Interface) sadece yapılan bu işe bütün eforunu sarf etmekteydi. Örneğin bir web servisten data çekmek istiyorsunuz ve request attınız, response olarak gelen datanınn listelenmesi işleminde, UI Thread'i kitledi. Böylelikle kullanıcı uygulamanın ona yan tarafta verdiği not tutma bölümünü kullanamaz hale geldi. Senkron programlama burada yetersiz kaldı. Bizim problemimizi yani data listelenirken arayüz üzerinde not tutma işini, asenkron programming ile yapabiliriz. Asenkron programming aynı anda bir birinden bağımsız olarak işlemler yapmamızı temin edecektir
    public interface IBaseRepository<T> where T : BaseEntity
    {
        //Bu projede elimizin asenkron programlaya alışması için bütün methodları asenkron yazacağım. Lakin Create, Update ve Delete işlemleri çok aksi bir business olmadığı sürece asenkron programlanmaz. Buna grerek yoktur. Asıl odaklanmamız gereken nokta Read operasyonlarıdır.

        Task Add(T entity);
        Task Update(T entity);
        Task Delete(T entity);

        // _context.Products.Where(x => x.Status != Status.Passive)
        Task<List<T>> GetByDefaults (Expression<Func<T, bool>> expression);
        Task<T> GetByDefault (Expression<Func<T, bool>> expression);

        // Read Operations
        Task<List<TResult>> GetFilteredList<TResult>(Expression<Func<T, TResult>> select,
                                                     Expression<Func<T, bool>> where = null,
                                                     Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
                                                     Func<IQueryable<T>, IIncludableQueryable<T, object>> join = null);

        Task<T> GetById(int id);
        Task<bool> Any(Expression<Func<T, bool>> expression);
    }
}
